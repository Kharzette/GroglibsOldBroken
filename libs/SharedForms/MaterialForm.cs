﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MeshLib;
using UtilityLib;


namespace SharedForms
{
	public partial class MaterialForm : Form
	{
		OpenFileDialog			mOFD	=new OpenFileDialog();
		SaveFileDialog			mSFD	=new SaveFileDialog();

//		ListBoxContainer	mLBC	=new ListBoxContainer();

		MaterialLib.MaterialLib	mMatLib;
		MaterialLib.StuffKeeper	mSKeeper;

		public event EventHandler	eNukedMeshPart;
		public event EventHandler	eStripElements;
		public event EventHandler	eGenTangents;
		public event EventHandler	eFoundSeams;
		public event EventHandler	eMatLibNotReadyToSave;


		public MaterialForm(MaterialLib.MaterialLib matLib,
			MaterialLib.StuffKeeper sk)
		{
			InitializeComponent();

			mMatLib		=matLib;
			mSKeeper	=sk;

			MaterialList.Columns.Add("Name");
			MaterialList.Columns.Add("Effect");
			MaterialList.Columns.Add("Technique");

			RefreshMaterials();

			MeshPartList.Columns.Add("Name");
			MeshPartList.Columns.Add("Material Name");
			MeshPartList.Columns.Add("Vertex Format");
			MeshPartList.Columns.Add("Visible");
		}


		public void RefreshMaterials()
		{
			Action<ListView>	clear	=lv => lv.Items.Clear();

			FormExtensions.Invoke(MaterialList, clear);

			List<string>	names	=mMatLib.GetMaterialNames();

			foreach(string name in names)
			{
				Action<ListView>	addItem	=lv => lv.Items.Add(name);

				FormExtensions.Invoke(MaterialList, addItem);
			}

			for(int i=0;i < MaterialList.Items.Count;i++)
			{
				Action<ListView>	tagAndSub	=lv =>
				{
					lv.Items[i].Tag = "MaterialName";
					lv.Items[i].SubItems.Add(
						mMatLib.GetMaterialEffect(MaterialList.Items[i].Text));
					lv.Items[i].SubItems.Add(
						mMatLib.GetMaterialTechnique(MaterialList.Items[i].Text));
					lv.Items[i].SubItems[1].Tag	="MaterialEffect";
					lv.Items[i].SubItems[2].Tag	="MaterialTechnique";
				};

				FormExtensions.Invoke(MaterialList, tagAndSub);
			}

			FormExtensions.SizeColumns(MaterialList);
		}


		public void RefreshMeshPartList()
		{
			MeshPartList.Items.Clear();

			Mesh.MeshAndArch	maa	=MeshPartList.Tag as Mesh.MeshAndArch;
			if(maa == null)
			{
				return;
			}

			int	count	=maa.mArch.GetPartCount();

			for(int i=0;i < count;i++)
			{
				string	partName	=maa.mArch.GetPartName(i);
				Type	partType	=maa.mArch.GetPartVertexType(i);

				string	matName	="";
				if(maa.mMesh is StaticMesh)
				{
					matName	=(maa.mMesh as StaticMesh).GetPartMaterialName(i);
				}
				else
				{
					matName	=(maa.mMesh as Character).GetPartMaterialName(i);
				}

				ListViewItem	lvi	=MeshPartList.Items.Add(partName);

				lvi.Tag	=i;

				lvi.SubItems.Add(matName);
				lvi.SubItems.Add(partType.ToString());

				//set the tag on this one for click detection help
				ListViewItem.ListViewSubItem	vis	=lvi.SubItems.Add("true");
				vis.Tag	=69;
			}

			FormExtensions.SizeColumns(MeshPartList);
		}


		public int GetTexCoordSet()
		{
			return	(int)TexCoordSet.Value;
		}


		//from a stacko question
		int DropDownWidth(ListBox myBox)
		{
			int maxWidth = 0, temp = 0;
			foreach(var obj in myBox.Items)
			{
				temp	=TextRenderer.MeasureText(obj.ToString(), myBox.Font).Width;
				if(temp > maxWidth)
				{
					maxWidth = temp;
				}
			}
			return maxWidth;
		}
		
		
		void SpawnEffectComboBox(string matName, ListViewItem.ListViewSubItem sub)
		{
			List<string>	effects	=mSKeeper.GetEffectList();
			if(effects.Count <= 0)
			{
				return;
			}

			ListBox				lbox	=new ListBox();
			ListBoxContainer	lbc		=new ListBoxContainer();

			Point	loc	=sub.Bounds.Location;

			lbc.Location	=MaterialList.PointToScreen(loc);

			lbox.Parent		=lbc;
			lbox.Location	=new Point(0, 0);
			lbox.Tag		=matName;

			string	current	=mMatLib.GetMaterialEffect(matName);

			foreach(string fx in effects)
			{
				lbox.Items.Add(fx);
			}

			if(current != null)
			{
				lbox.SelectedItem	=current;
			}

			int	width	=DropDownWidth(lbox);

			width	+=SystemInformation.VerticalScrollBarWidth;

			Size	fit	=new System.Drawing.Size(width, lbox.Size.Height);

			lbox.Size	=fit;
			lbc.Size	=fit;

			lbc.Visible		=true;
			lbox.Visible	=true;

			lbox.MouseClick	+=OnEffectListBoxClick;
			lbox.Leave		+=OnEffectListBoxEscaped;
			lbox.KeyPress	+=OnEffectListBoxKey;
			lbox.LostFocus	+=OnEffectLostFocus;
			lbox.Focus();
		}


		void SpawnTechniqueComboBox(string matName, ListViewItem.ListViewSubItem sub)
		{
			List<string>	techs	=mMatLib.GetMaterialTechniques(matName);
			if(techs.Count <= 0)
			{
				return;
			}

			ListBox				lbox	=new ListBox();
			ListBoxContainer	lbc		=new ListBoxContainer();

			Point	loc	=sub.Bounds.Location;

			lbc.Location	=MaterialList.PointToScreen(loc);
			lbox.Parent		=lbc;
			lbox.Location	=new Point(0, 0);
			lbox.Tag		=matName;

			foreach(string tn in techs)
			{
				lbox.Items.Add(tn);
			}

			string	current	=mMatLib.GetMaterialTechnique(matName);
			if(current != null)
			{
				lbox.SelectedItem	=current;
			}

			int	width	=DropDownWidth(lbox);

			width	+=SystemInformation.VerticalScrollBarWidth;

			Size	fit	=new System.Drawing.Size(width, lbox.Size.Height);

			lbox.Size	=fit;
			lbc.Size	=fit;

			lbox.Visible	=true;
			lbc.Visible	=true;

			lbox.Leave		+=OnTechListBoxEscaped;
			lbox.KeyPress	+=OnTechListBoxKey;
			lbox.MouseClick	+=OnTechListBoxClick;
			lbox.LostFocus	+=OnTechLostFocus;
			lbox.Focus();
		}


		void SetListEffect(string mat, string fx)
		{
			foreach(ListViewItem lvi in MaterialList.Items)
			{
				if(lvi.Text == mat)
				{
					lvi.SubItems[1].Text	=fx;
					return;
				}
			}
		}


		void SetListTechnique(string mat, string tech)
		{
			foreach(ListViewItem lvi in MaterialList.Items)
			{
				if(lvi.Text == mat)
				{
					lvi.SubItems[2].Text	=tech;
					return;
				}
			}
		}


		void OnTechListBoxKey(object sender, KeyPressEventArgs kpea)
		{
			ListBox	lb	=sender as ListBox;

			if(kpea.KeyChar == 27)	//escape
			{
				DisposeTechBox(lb);
			}
			else if(kpea.KeyChar == '\r')
			{
				if(lb.SelectedIndex != -1)
				{
					mMatLib.SetMaterialTechnique(lb.Tag as string, lb.SelectedItem as string);
					SetListTechnique(lb.Tag as string, lb.SelectedItem as string);
					OnMaterialSelectionChanged(null, null);
				}
				DisposeTechBox(lb);
			}
		}


		void OnEffectLostFocus(object sender, EventArgs ea)
		{
			DisposeEffectBox(sender as ListBox);
		}


		void OnTechLostFocus(object sender, EventArgs ea)
		{
			DisposeEffectBox(sender as ListBox);
		}


		void OnEffectListBoxKey(object sender, KeyPressEventArgs kpea)
		{
			ListBox	lb	=sender as ListBox;

			if(kpea.KeyChar == 27)	//escape
			{
				DisposeEffectBox(lb);
			}
			else if(kpea.KeyChar == '\r')
			{
				if(lb.SelectedIndex != -1)
				{
					mMatLib.SetMaterialEffect(lb.Tag as string, lb.SelectedItem as string);
					SetListEffect(lb.Tag as string, lb.SelectedItem as string);
					OnMaterialSelectionChanged(null, null);
				}
				DisposeEffectBox(lb);
			}
		}


		void OnTechListBoxClick(object sender, MouseEventArgs mea)
		{
			ListBox	lb	=sender as ListBox;

			if(lb.SelectedIndex != -1)
			{
				mMatLib.SetMaterialTechnique(lb.Tag as string, lb.SelectedItem as string);
				SetListTechnique(lb.Tag as string, lb.SelectedItem as string);
				OnMaterialSelectionChanged(null, null);
			}
			DisposeTechBox(lb);
		}


		void OnEffectListBoxClick(object sender, MouseEventArgs mea)
		{
			ListBox	lb	=sender as ListBox;

			if(lb.SelectedIndex != -1)
			{
				mMatLib.SetMaterialEffect(lb.Tag as string, lb.SelectedItem as string);
				SetListEffect(lb.Tag as string, lb.SelectedItem as string);
				OnMaterialSelectionChanged(null, null);
			}
			DisposeEffectBox(lb);
		}


		void OnTechListBoxEscaped(object sender, EventArgs ea)
		{
			ListBox	lb	=sender as ListBox;

			DisposeTechBox(lb);
		}


		void OnEffectListBoxEscaped(object sender, EventArgs ea)
		{
			ListBox	lb	=sender as ListBox;

			DisposeEffectBox(lb);
		}


		void OnMaterialSelectionChanged(object sender, EventArgs e)
		{
			if(MaterialList.SelectedIndices.Count < 1
				|| MaterialList.SelectedIndices.Count > 1)
			{
				VariableList.DataSource	=null;
				NewMaterial.Text		="New Mat";
				return;
			}
			NewMaterial.Text		="Clone Mat";

			string	matName	=MaterialList.Items[MaterialList.SelectedIndices[0]].Text;

			BindingList<MaterialLib.EffectVariableValue>	vars	=
				mMatLib.GetMaterialGUIVariables(matName);

			if(vars.Count > 0)
			{
				VariableList.DataSource	=vars;
			}
			else
			{
				VariableList.DataSource	=null;
			}
		}

		
		void OnMaterialRename(object sender, LabelEditEventArgs e)
		{
			if(!mMatLib.RenameMaterial(MaterialList.Items[e.Item].Text, e.Label))
			{
				e.CancelEdit	=true;
			}
			else
			{
				FormExtensions.SizeColumns(MaterialList);	//this doesn't work, still has the old value
			}
		}


		void OnMeshPartMouseUp(object sender, MouseEventArgs mea)
		{
			Mesh.MeshAndArch	maa	=MeshPartList.Tag as Mesh.MeshAndArch;
			if(maa == null)
			{
				return;
			}

			StaticMesh	sm	=maa.mMesh as StaticMesh;
			Character	chr	=maa.mMesh as Character;
			if(sm == null && chr == null)
			{
				return;
			}

			foreach(ListViewItem lvi in MeshPartList.Items)
			{
				if(lvi.Bounds.Contains(mea.Location))
				{
					foreach(ListViewItem.ListViewSubItem sub in lvi.SubItems)
					{
						if(sub.Bounds.Contains(mea.Location))
						{
							if(sub.Tag != null && (int)sub.Tag == 69)
							{
								int	index	=(int)lvi.Tag;

								if((string)sub.Text == "True")
								{
									sub.Text	="False";

									if(sm != null)
									{
										sm.SetPartVisible(index, false);
									}
									else
									{
										chr.SetPartVisible(index, false);
									}
								}
								else
								{
									sub.Text	="True";
									if(sm != null)
									{
										sm.SetPartVisible(index, true);
									}
									else
									{
										chr.SetPartVisible(index, true);
									}
								}
							}
						}
					}
				}
			}
		}


		void OnMatListClick(object sender, MouseEventArgs e)
		{
			foreach(ListViewItem lvi in MaterialList.Items)
			{
				if(lvi.Bounds.Contains(e.Location))
				{
					foreach(ListViewItem.ListViewSubItem sub in lvi.SubItems)
					{
						if(sub.Bounds.Contains(e.Location))
						{
							if((string)sub.Tag == "MaterialEffect")
							{
								SpawnEffectComboBox(lvi.Text, sub);
							}
							else if((string)sub.Tag == "MaterialTechnique")
							{
								SpawnTechniqueComboBox(lvi.Text, sub);
							}
						}
					}
				}
			}
		}


		//the new button becomes a clone button with a mat selected
		void OnNewMaterial(object sender, EventArgs e)
		{
			string	baseName	="default";
			bool	bClone		=false;
			if(MaterialList.SelectedIndices.Count == 1)
			{
				baseName	=MaterialList.Items[MaterialList.SelectedIndices[0]].Text;
				bClone		=true;
			}

			List<string>	names	=mMatLib.GetMaterialNames();

			string	tryName	=baseName;
			bool	bFirst	=true;
			int		cnt		=1;
			while(names.Contains(tryName))
			{
				if(bFirst)
				{
					tryName	+="000";
					bFirst	=false;
				}
				else
				{
					tryName	=baseName + String.Format("{0:000}", cnt);
					cnt++;
				}
			}

			if(bClone)
			{
				mMatLib.CloneMaterial(baseName, tryName);
			}
			else
			{
				mMatLib.CreateMaterial(tryName);
			}

			RefreshMaterials();
		}


		public void SetMesh(object sender)
		{
			Mesh.MeshAndArch	maa	=sender as Mesh.MeshAndArch;
			if(maa == null)
			{
				return;
			}

			MeshPartList.Tag	=maa;

			RefreshMeshPartList();
		}


		void OnFormSizeChanged(object sender, EventArgs e)
		{
			//get the mesh part grid out of the material
			//grid's junk
			int	adjust	=MeshPartGroup.Top - 6;

			adjust	-=(MeshPartList.Top + MeshPartList.Size.Height);

			MeshPartList.SetBounds(MeshPartList.Left,
				MeshPartList.Top + adjust,
				MeshPartList.Width,
				MeshPartList.Height);
		}


		void OnMatListKeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyValue == 46)	//delete
			{
				if(MaterialList.SelectedItems.Count < 1)
				{
					return;	//nothing to do
				}

				foreach(ListViewItem lvi in MaterialList.SelectedItems)
				{
					mMatLib.NukeMaterial(lvi.Text);
				}

				RefreshMaterials();
				NewMaterial.Text	="New Mat";
			}
		}


		void OnMeshPartListKeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyValue == 46)	//delete
			{
				if(MeshPartList.SelectedItems.Count < 1)
				{
					return;	//nothing to do
				}

				List<int>	toNuke	=new List<int>();

				foreach(ListViewItem lvi in MeshPartList.SelectedItems)
				{
					toNuke.Add((int)lvi.Tag);
				}

				Misc.SafeInvoke(eNukedMeshPart, toNuke);

				RefreshMeshPartList();
			}
			else if(e.KeyValue == 113)	//F2
			{
				if(MeshPartList.SelectedItems.Count != 1)
				{
					return;	//nothing to do
				}

				MeshPartList.SelectedItems[0].BeginEdit();
			}
		}


		void OnApplyMaterial(object sender, EventArgs e)
		{
			if(MaterialList.SelectedItems.Count != 1)
			{
				return;	//nothing to do
			}

			Mesh.MeshAndArch	maa	=MeshPartList.Tag as Mesh.MeshAndArch;
			if(maa == null)
			{
				return;
			}

			StaticMesh	sm	=maa.mMesh as StaticMesh;
			Character	chr	=maa.mMesh as Character;
			if(sm == null && chr == null)
			{
				return;
			}

			string	matName	=MaterialList.SelectedItems[0].Text;

			foreach(ListViewItem lvi in MeshPartList.SelectedItems)
			{
				int	meshIndex	=(int)lvi.Tag;

				if(chr == null)
				{
					sm.SetPartMaterialName(meshIndex, matName);
				}
				else
				{
					chr.SetPartMaterialName(meshIndex, matName);
				}

				lvi.SubItems[1].Text	=matName;
			}
		}


		void OnHideVariables(object sender, EventArgs e)
		{
			if(MaterialList.SelectedItems.Count != 1)
			{
				return;	//nothing to do
			}

			string	matName	=MaterialList.SelectedItems[0].Text;

			List<string>	selected	=new List<string>();
			foreach(DataGridViewRow dgvr in VariableList.SelectedRows)
			{
				selected.Add(dgvr.Cells[0].Value as string);
			}

			mMatLib.HideMaterialVariables(matName, selected);

			VariableList.DataSource	=mMatLib.GetMaterialGUIVariables(matName);
		}


		void OnIgnoreVariables(object sender, EventArgs e)
		{
			if(MaterialList.SelectedItems.Count != 1)
			{
				return;	//nothing to do
			}

			string	matName	=MaterialList.SelectedItems[0].Text;

			List<string>	selected	=new List<string>();
			foreach(DataGridViewRow dgvr in VariableList.SelectedRows)
			{
				selected.Add(dgvr.Cells[0].Value as string);
			}

			mMatLib.IgnoreMaterialVariables(matName, selected);

			VariableList.DataSource	=mMatLib.GetMaterialGUIVariables(matName);
		}


		void OnGuessVisibility(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection	matSel	=MaterialList.SelectedItems;
			foreach(ListViewItem lvi in matSel)
			{
				mMatLib.GuessParameterVisibility(lvi.Text);
			}

			//if there's a single set selected, refresh
			if(MaterialList.SelectedItems.Count == 1)
			{
				string	matName	=MaterialList.SelectedItems[0].Text;
				VariableList.DataSource	=mMatLib.GetMaterialGUIVariables(matName);
			}
		}


		void OnResetVisibility(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection	matSel	=MaterialList.SelectedItems;
			foreach(ListViewItem lvi in matSel)
			{
				mMatLib.ResetParameterVisibility(lvi.Text);
			}

			//if there's a single set selected, refresh
			if(MaterialList.SelectedItems.Count == 1)
			{
				string	matName	=MaterialList.SelectedItems[0].Text;
				VariableList.DataSource	=mMatLib.GetMaterialGUIVariables(matName);
			}
		}


		void OnSaveMaterialLib(object sender, EventArgs e)
		{
			mSFD.DefaultExt	="*.MatLib";
			mSFD.Filter		="Material lib files (*.MatLib)|*.MatLib|All files (*.*)|*.*";

			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			bool	bRet	=mMatLib.SaveToFile(mSFD.FileName);
			if(!bRet)
			{
				Misc.SafeInvoke(eMatLibNotReadyToSave, null);
			}
		}

		
		void OnLoadMaterialLib(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.MatLib";
			mOFD.Filter		="Material lib files (*.MatLib)|*.MatLib|All files (*.*)|*.*";

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			mMatLib.ReadFromFile(mOFD.FileName);
			RefreshMaterials();
		}


		void OnMatchAndVisible(object sender, EventArgs e)
		{
			Mesh.MeshAndArch	maa	=MeshPartList.Tag as Mesh.MeshAndArch;
			if(maa == null)
			{
				return;
			}

			StaticMesh	sm	=maa.mMesh as StaticMesh;
			Character	chr	=maa.mMesh as Character;
			if(sm == null && chr == null)
			{
				return;
			}

			foreach(ListViewItem lvi in MaterialList.Items)
			{
				string	matName	=lvi.Text;

				foreach(ListViewItem lviMesh in MeshPartList.Items)
				{
					string	meshName	=lviMesh.Text;

					if(meshName.Contains(matName))
					{
						int	meshIndex	=(int)lviMesh.Tag;

						if(chr == null)
						{
							sm.SetPartMaterialName(meshIndex, matName);
						}
						else
						{
							chr.SetPartMaterialName(meshIndex, matName);
						}

						lviMesh.SubItems[1].Text	=matName;
					}
				}
			}
		}


		void OnStripElements(object sender, EventArgs e)
		{
			if(MeshPartList.SelectedItems.Count < 1)
			{
				return;
			}

			Mesh.MeshAndArch	maa	=MeshPartList.Tag as Mesh.MeshAndArch;
			if(maa == null)
			{
				return;
			}

			List<int>	parts	=new List<int>();
			foreach(ListViewItem lviMesh in MeshPartList.SelectedItems)
			{
				parts.Add((int)lviMesh.Tag);
			}

			ArchEventArgs	aea	=new ArchEventArgs();

			aea.mArch		=maa.mArch;
			aea.mIndexes	=parts;

			Misc.SafeInvoke(eStripElements, null, aea);
		}


		void DisposeEffectBox(ListBox lb)
		{
			lb.Leave		-=OnEffectListBoxEscaped;
			lb.KeyPress		-=OnEffectListBoxKey;
			lb.MouseClick	-=OnEffectListBoxClick;
			lb.LostFocus	-=OnEffectLostFocus;
			lb.Parent.Dispose();
		}


		void DisposeTechBox(ListBox lb)
		{
			lb.Leave		-=OnTechListBoxEscaped;
			lb.KeyPress		-=OnTechListBoxKey;
			lb.MouseClick	-=OnTechListBoxClick;
			lb.LostFocus	-=OnTechLostFocus;
			lb.Parent.Dispose();
		}


		void OnMergeMatLib(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.MatLib";
			mOFD.Filter		="Material lib files (*.MatLib)|*.MatLib|All files (*.*)|*.*";

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			mMatLib.MergeFromFile(mOFD.FileName);

			RefreshMaterials();
		}


		void OnGuessTextures(object sender, EventArgs e)
		{
			mMatLib.GuessTextures();
		}


		void OnVariableValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if(MaterialList.SelectedItems.Count < 1
				|| MaterialList.SelectedItems.Count > 1)
			{
				return;	//nothing to do
			}

			mMatLib.FixTextureVariables(MaterialList.SelectedItems[0].Text);
		}


		void OnFrankenstein(object sender, EventArgs e)
		{
			Mesh.MeshAndArch	maa	=MeshPartList.Tag as Mesh.MeshAndArch;
			if(maa == null)
			{
				return;
			}

			Misc.SafeInvoke(eFoundSeams, maa.mArch.Frankenstein());
		}


		void OnMeshPartRename(object sender, LabelEditEventArgs e)
		{
			Mesh.MeshAndArch	maa	=MeshPartList.Tag as Mesh.MeshAndArch;
			if(maa == null)
			{
				return;
			}

			int	meshIndex	=(int)MeshPartList.Items[e.Item].Tag;

			if(!maa.mArch.RenamePart(meshIndex, e.Label))
			{
				e.CancelEdit	=true;
			}
			else
			{
				FormExtensions.SizeColumns(MeshPartList);	//this doesn't work, still has the old value
			}
		}


		void OnGenTangents(object sender, EventArgs e)
		{
			if(MeshPartList.SelectedItems.Count <= 0)
			{
				return;
			}

			Mesh.MeshAndArch	maa	=MeshPartList.Tag as Mesh.MeshAndArch;
			if(maa == null)
			{
				return;
			}

			List<int>	parts	=new List<int>();
			foreach(ListViewItem lviMesh in MeshPartList.SelectedItems)
			{
				parts.Add((int)lviMesh.Tag);
			}

			ArchEventArgs	aea	=new ArchEventArgs();

			aea.mArch		=maa.mArch;
			aea.mIndexes	=parts;

			Misc.SafeInvoke(eGenTangents, null, aea);

			RefreshMeshPartList();
		}
	}
}
