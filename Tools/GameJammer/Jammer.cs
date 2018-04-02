using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GameJammer
{
	public partial class Jammer : Form
	{
		FolderBrowserDialog	mFBD	=new FolderBrowserDialog();

		public Jammer()
		{
			InitializeComponent();

			//add data bindings for positions of forms
			DataBindings.Add(new Binding("Location",
				global::GameJammer.Properties.Settings.Default,
				"JammerFormPos", true,
				DataSourceUpdateMode.OnPropertyChanged));

			//text fields
			LevelsDir.DataBindings.Add(new Binding("Text",
				Properties.Settings.Default, "GameLevelsDir", true,
				DataSourceUpdateMode.OnPropertyChanged));
			MapWorkDir.DataBindings.Add(new Binding("Text",
				Properties.Settings.Default, "MapWorkDir", true,
				DataSourceUpdateMode.OnPropertyChanged));
			QuarkAddOnDir.DataBindings.Add(new Binding("Text",
				Properties.Settings.Default, "QuarkAddOnDir", true,
				DataSourceUpdateMode.OnPropertyChanged));
			QuarkSrcDir.DataBindings.Add(new Binding("Text",
				Properties.Settings.Default, "QuarkSrcDir", true,
				DataSourceUpdateMode.OnPropertyChanged));
			GameAudioDir.DataBindings.Add(new Binding("Text",
				Properties.Settings.Default, "GameAudioDir", true,
				DataSourceUpdateMode.OnPropertyChanged));
			AudioSrcDir.DataBindings.Add(new Binding("Text",
				Properties.Settings.Default, "AudioSrcDir", true,
				DataSourceUpdateMode.OnPropertyChanged));
			LevelBaseName.DataBindings.Add(new Binding("Text",
				Properties.Settings.Default, "LevelBaseName", true,
				DataSourceUpdateMode.OnPropertyChanged));

			//update level bools
			UpdateZone.DataBindings.Add(new Binding("Checked",
				Properties.Settings.Default, "UpdateZone", true,
				DataSourceUpdateMode.OnPropertyChanged));
			UpdateZoneDraw.DataBindings.Add(new Binding("Checked",
				Properties.Settings.Default, "UpdateZoneDraw", true,
				DataSourceUpdateMode.OnPropertyChanged));
			UpdateMatLib.DataBindings.Add(new Binding("Checked",
				Properties.Settings.Default, "UpdateMatLib", true,
				DataSourceUpdateMode.OnPropertyChanged));
			UpdateVisData.DataBindings.Add(new Binding("Checked",
				Properties.Settings.Default, "UpdateVisData", true,
				DataSourceUpdateMode.OnPropertyChanged));
			UpdatePortalFile.DataBindings.Add(new Binding("Checked",
				Properties.Settings.Default, "UpdatePortalFile", true,
				DataSourceUpdateMode.OnPropertyChanged));
		}


		string FetchDir(string title, string prevDir)
		{
			mFBD.Description	=title;
			mFBD.SelectedPath	=prevDir;

			DialogResult	dr	=mFBD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return	prevDir;
			}

			return	mFBD.SelectedPath;
		}


		void OnSetGameLevelsDir(object sender, EventArgs e)
		{
			LevelsDir.Text	=FetchDir("Game Content Levels Directory", LevelsDir.Text);
		}


		void OnSetMapWorkDir(object sender, EventArgs e)
		{
			MapWorkDir.Text	=FetchDir("Map Work Directory", MapWorkDir.Text);
		}


		void OnSetQuarkAddOnDir(object sender, EventArgs e)
		{
			QuarkAddOnDir.Text	=FetchDir("QuArk Addon Directory", QuarkAddOnDir.Text);
		}


		void OnSetAddOnSrcDir(object sender, EventArgs e)
		{
			QuarkSrcDir.Text	=FetchDir("QuArk Addon Source Directory", QuarkSrcDir.Text);
		}


		void OnSetGameAudioDir(object sender, EventArgs e)
		{
			GameAudioDir.Text	=FetchDir("Game Content Audio Directory", GameAudioDir.Text);
		}


		void OnSetAudioSrcDir(object sender, EventArgs e)
		{
			AudioSrcDir.Text	=FetchDir("Game Audio Source Directory", AudioSrcDir.Text);
		}


		void OnUpdateLevel(object sender, EventArgs e)
		{
			if(!Directory.Exists(LevelsDir.Text))
			{
				return;
			}

			if(!Directory.Exists(MapWorkDir.Text))
			{
				return;
			}

			if(UpdateZone.Checked)
			{
				if(File.Exists(MapWorkDir.Text + "\\" + LevelBaseName.Text + ".Zone"))
				{
					File.Copy(MapWorkDir.Text + "/" + LevelBaseName.Text + ".Zone",
						LevelsDir.Text + "/" +  LevelBaseName.Text + ".Zone", true);
				}
			}

			if(UpdateZoneDraw.Checked)
			{
				if(File.Exists(MapWorkDir.Text + "/" + LevelBaseName.Text + ".ZoneDraw"))
				{
					File.Copy(MapWorkDir.Text + "/" + LevelBaseName.Text + ".ZoneDraw",
						LevelsDir.Text + "/" +  LevelBaseName.Text + ".ZoneDraw", true);
				}
			}

			if(UpdateMatLib.Checked)
			{
				if(File.Exists(MapWorkDir.Text + "/" + LevelBaseName.Text + ".MatLib"))
				{
					File.Copy(MapWorkDir.Text + "/" + LevelBaseName.Text + ".MatLib",
						LevelsDir.Text + "/" +  LevelBaseName.Text + ".MatLib", true);
				}
			}

			if(UpdateVisData.Checked)
			{
				if(File.Exists(MapWorkDir.Text + "/" + LevelBaseName.Text + ".VisData"))
				{
					File.Copy(MapWorkDir.Text + "/" + LevelBaseName.Text + ".VisData",
						LevelsDir.Text + "/" +  LevelBaseName.Text + ".VisData", true);
				}
			}

			if(UpdatePortalFile.Checked)
			{
				if(File.Exists(MapWorkDir.Text + "/" + LevelBaseName.Text + ".gpf"))
				{
					File.Copy(MapWorkDir.Text + "/" + LevelBaseName.Text + ".gpf",
						LevelsDir.Text + "/" +  LevelBaseName.Text + ".gpf", true);
				}
			}
		}


		void OnUpdateQuarkAddOn(object sender, EventArgs e)
		{
			if(!Directory.Exists(QuarkAddOnDir.Text))
			{
				return;
			}

			if(!Directory.Exists(QuarkSrcDir.Text))
			{
				return;
			}

			string	[]fileNames	=Directory.GetFiles(QuarkSrcDir.Text);
			foreach(string fileName in fileNames)
			{
				if(!File.Exists(fileName))
				{
					continue;
				}

				string	justTheName	=fileName.Substring(fileName.LastIndexOf('\\') + 1);

				//make sure the file exists in the addons dir (skips stuff like defaults.qrk)
				if(!File.Exists(QuarkAddOnDir.Text + "/" + justTheName))
				{
					continue;
				}

				File.Copy(fileName, QuarkAddOnDir.Text + "/" + justTheName, true);
			}
		}


		void OnUpdateAudio(object sender, EventArgs e)
		{
			if(!Directory.Exists(GameAudioDir.Text))
			{
				return;
			}

			if(!Directory.Exists(AudioSrcDir.Text))
			{
				return;
			}

			string	[]fileNames	=Directory.GetFiles(AudioSrcDir.Text);
			foreach(string fileName in fileNames)
			{
				if(!File.Exists(fileName))
				{
					continue;
				}

				string	justTheName	=fileName.Substring(fileName.LastIndexOf('\\') + 1);

				//make sure the file exists in the addons dir (skips stuff like defaults.qrk)
				if(!File.Exists(GameAudioDir.Text + "/" + justTheName))
				{
					continue;
				}

				File.Copy(fileName, GameAudioDir.Text + "/" + justTheName, true);
			}
		}
	}
}
