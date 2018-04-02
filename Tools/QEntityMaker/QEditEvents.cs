﻿using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;


namespace QEntityMaker
{
	public partial class QEEdit
	{
		OpenFileDialog	mOFD	=new OpenFileDialog();

		string	mRenameEnd	="";

		const string	EntityFolder	="GrogLibs Entities.qtxfolder";
		const string	FormsFolder		="Entity forms.fctx";


		void OnDeleteEntity(object sender, EventArgs e)
		{
			TreeNode	toNuke	=EntityTree.SelectedNode;

			//nuke the form if it is there
			DeleteFormForEntity(toNuke);

			//remove from tree
			toNuke.Remove();
		}


		void OnRenameEntity(object sender, EventArgs e)
		{
			string	name	=EntityTree.SelectedNode.Text;

			//chop off the end stuff
			int	colonPos	=name.IndexOf(':');

			mRenameEnd	=name.Substring(colonPos);

			RenameBox.Text		=name.Substring(0, colonPos);
			RenameBox.Enabled	=true;

			RenameBox.Leave	+=OnRenameDone;

			RenameBox.Focus();
		}


		void OnRenameDone(object sender, EventArgs ea)
		{
			RenameDone(RenameBox.Text);
		}


		void OnRenameBoxKey(object sender, KeyPressEventArgs e)
		{
			char	pressed	=e.KeyChar;

			if(pressed == '\r')
			{
				RenameDone(RenameBox.Text);
				e.Handled	=true;
			}
		}


		void OnTreeKeyUp(object sender, KeyEventArgs e)
		{
			if(!EntityTree.Focused)
			{
				return;
			}

			if(e.KeyCode == Keys.Delete)
			{
				OnDeleteEntity(null, null);
				e.Handled	=true;
			}
			else if(e.KeyCode == Keys.F2)
			{
				OnRenameEntity(null, null);
			}
		}


		void OnSave(object sender, EventArgs e)
		{
			if(QuarkEntityFile.Text == null || QuarkEntityFile.Text == "")
			{
				return;
			}

			FileStream	fs	=new FileStream(QuarkEntityFile.Text, FileMode.Create, FileAccess.Write);
			if(fs == null)
			{
				EntityTree.Nodes.Add("Can't open " + QuarkEntityFile.Text);
				return;
			}

			EntityTree.Enabled		=false;
			EntityFields.Enabled	=false;

			StreamWriter	sw	=new StreamWriter(fs);

			WriteRecursive(sw, EntityTree.Nodes[0], 1);

			sw.Close();
			fs.Close();

			EntityTree.Enabled		=true;
			EntityFields.Enabled	=true;
		}


		void OnAddEntity(object sender, EventArgs e)
		{
			TreeNode	tn	=new TreeNode();

			TreeNode	mom	=EntityTree.SelectedNode;

			//grab the category text
			string	cat	=mom.Text;

			cat	=cat.ToLower();

			int	catIndex	=cat.IndexOf("_");
			if(catIndex == -1)
			{
				cat	="point_new";
			}
			else
			{
				cat	=cat.Substring(0, catIndex);
				cat	+="_new";
			}

			tn.Text	=cat + ":e =";

			BindingList<EntityKVP>	kvps	=new BindingList<EntityKVP>();

			if(AutoAddOrigin.Checked)
			{
				EntityKVP	kvp	=new EntityKVP();

				kvp.Key		="origin";
				kvp.Value	="0 0 0";

				kvps.Add(kvp);
			}

			if(AutoAddMeshName.Checked)
			{
				EntityKVP	kvp	=new EntityKVP();

				kvp.Key		="meshname";
				kvp.Value	="Default.Static";

				kvps.Add(kvp);
			}

			if(AutoAddDesc.Checked)
			{
				EntityKVP	kvp	=new EntityKVP();

				kvp.Key		=";desc";
				kvp.Value	="An automatically generated description";

				kvps.Add(kvp);
			}

			if(AutoAddAngles.Checked)
			{
				EntityKVP	kvp	=new EntityKVP();

				kvp.Key		="angles";
				kvp.Value	="0 0 0";

				kvps.Add(kvp);
			}

			if(AutoAddPickUp.Checked)
			{
				EntityKVP	kvp	=new EntityKVP();

				kvp.Key		="pickup";
				kvp.Value	="true";

				kvps.Add(kvp);
			}

			tn.Tag	=kvps;

			if(AutoAddForm.Checked)
			{
				AddFormForEntity(tn, cat);
			}

			mom.Nodes.Add(tn);
		}


		void OnAfterSelect(object sender, TreeViewEventArgs e)
		{
			AddGroupBox.Enabled		=false;
			DeleteEntity.Enabled	=false;
			RenameGroupBox.Enabled	=false;
			UpdateForm.Enabled		=false;

			//see if this is an entity
			if(!IsInFolder(e.Node.Parent, EntityFolder))
			{
				//see if in forms folder
				if(!IsInFolder(e.Node.Parent, FormsFolder))
				{
					return;
				}
			}

			//make sure not a foldery thing
			//skip func and trigger as we don't have the poly stuff done yet
			if((e.Node.Text.Contains("*") || e.Node.Text == "Path & Combat entities.qtxfolder =")
				&& !e.Node.Text.StartsWith("Func")
				&& !e.Node.Text.StartsWith("Trigger"))
			{
				AddGroupBox.Enabled		=true;
				return;
			}

			//check for entity subfield
			if(e.Node.Text.Contains(":e")
				|| e.Node.Text.Contains(":b"))
			{
				PopulateFieldGrid(e.Node);
				DeleteEntity.Enabled	=true;
				RenameGroupBox.Enabled	=true;
				UpdateForm.Enabled		=true;
			}
			else if(e.Node.Text.Contains(":form")
				|| e.Node.Parent.Text.Contains(":form"))
			{
				//for editing of forms
				PopulateFieldGrid(e.Node);
			}
		}


		void OnBrowseForEntityFile(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.qrk";
			mOFD.Filter			="QuArK entity files (*.qrk)|*.qrk|All files (*.*)|*.*";
			mOFD.Multiselect	=false;

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			QuarkEntityFile.Text	=mOFD.FileName;
		}


		void OnQEFTextChanged(object sender, EventArgs ea)
		{
			RefreshTree();
		}


		void OnUpdateForm(object sender, EventArgs e)
		{
			//grab the forms root node
			TreeNode	forms	=FindNode(EntityTree.Nodes[0], FormsFolder);

			//see if a form node already exists
			string	entName	=EntityTree.SelectedNode.Text;

			int	cIndex	=entName.LastIndexOf(':');

			entName	=entName.Substring(0, cIndex);

			string	formName	=entName + ":form =";

			//nuke existing
			TreeNode	existingForm	=FindNode(forms, formName);
			if(existingForm != null)
			{
				existingForm.Remove();
				existingForm	=null;
			}

			AddFormForEntity(EntityTree.SelectedNode, entName);
		}
	}
}
