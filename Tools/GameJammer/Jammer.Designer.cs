namespace GameJammer
{
	partial class Jammer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Jammer));
			this.UpdateLevel = new System.Windows.Forms.Button();
			this.SetLevelsPath = new System.Windows.Forms.Button();
			this.LevelsDir = new System.Windows.Forms.TextBox();
			this.MapWorkDir = new System.Windows.Forms.TextBox();
			this.SetMapWorkDir = new System.Windows.Forms.Button();
			this.QuarkAddOnDir = new System.Windows.Forms.TextBox();
			this.SetQuarkAddOnDir = new System.Windows.Forms.Button();
			this.QuarkSrcDir = new System.Windows.Forms.TextBox();
			this.SetQuarkSrcDir = new System.Windows.Forms.Button();
			this.GameAudioDir = new System.Windows.Forms.TextBox();
			this.SetGameAudioDir = new System.Windows.Forms.Button();
			this.AudioSrcDir = new System.Windows.Forms.TextBox();
			this.SetAudioSrcDir = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.UpdateZone = new System.Windows.Forms.CheckBox();
			this.UpdateZoneDraw = new System.Windows.Forms.CheckBox();
			this.UpdateMatLib = new System.Windows.Forms.CheckBox();
			this.UpdateVisData = new System.Windows.Forms.CheckBox();
			this.UpdatePortalFile = new System.Windows.Forms.CheckBox();
			this.LevelBaseName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.UpdateAudio = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// UpdateLevel
			// 
			this.UpdateLevel.Location = new System.Drawing.Point(178, 61);
			this.UpdateLevel.Name = "UpdateLevel";
			this.UpdateLevel.Size = new System.Drawing.Size(87, 23);
			this.UpdateLevel.TabIndex = 0;
			this.UpdateLevel.Text = "Update Level";
			this.UpdateLevel.UseVisualStyleBackColor = true;
			this.UpdateLevel.Click += new System.EventHandler(this.OnUpdateLevel);
			// 
			// SetLevelsPath
			// 
			this.SetLevelsPath.Location = new System.Drawing.Point(12, 12);
			this.SetLevelsPath.Name = "SetLevelsPath";
			this.SetLevelsPath.Size = new System.Drawing.Size(112, 23);
			this.SetLevelsPath.TabIndex = 1;
			this.SetLevelsPath.Text = "Game Levels Dir";
			this.SetLevelsPath.UseVisualStyleBackColor = true;
			this.SetLevelsPath.Click += new System.EventHandler(this.OnSetGameLevelsDir);
			// 
			// LevelsDir
			// 
			this.LevelsDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LevelsDir.Location = new System.Drawing.Point(130, 14);
			this.LevelsDir.Name = "LevelsDir";
			this.LevelsDir.Size = new System.Drawing.Size(339, 20);
			this.LevelsDir.TabIndex = 2;
			// 
			// MapWorkDir
			// 
			this.MapWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MapWorkDir.Location = new System.Drawing.Point(130, 43);
			this.MapWorkDir.Name = "MapWorkDir";
			this.MapWorkDir.Size = new System.Drawing.Size(339, 20);
			this.MapWorkDir.TabIndex = 4;
			// 
			// SetMapWorkDir
			// 
			this.SetMapWorkDir.Location = new System.Drawing.Point(12, 41);
			this.SetMapWorkDir.Name = "SetMapWorkDir";
			this.SetMapWorkDir.Size = new System.Drawing.Size(112, 23);
			this.SetMapWorkDir.TabIndex = 3;
			this.SetMapWorkDir.Text = "Map Work Dir";
			this.SetMapWorkDir.UseVisualStyleBackColor = true;
			this.SetMapWorkDir.Click += new System.EventHandler(this.OnSetMapWorkDir);
			// 
			// QuarkAddOnDir
			// 
			this.QuarkAddOnDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QuarkAddOnDir.Location = new System.Drawing.Point(130, 72);
			this.QuarkAddOnDir.Name = "QuarkAddOnDir";
			this.QuarkAddOnDir.Size = new System.Drawing.Size(339, 20);
			this.QuarkAddOnDir.TabIndex = 6;
			// 
			// SetQuarkAddOnDir
			// 
			this.SetQuarkAddOnDir.Location = new System.Drawing.Point(12, 70);
			this.SetQuarkAddOnDir.Name = "SetQuarkAddOnDir";
			this.SetQuarkAddOnDir.Size = new System.Drawing.Size(112, 23);
			this.SetQuarkAddOnDir.TabIndex = 5;
			this.SetQuarkAddOnDir.Text = "QuArk Addon Dir";
			this.SetQuarkAddOnDir.UseVisualStyleBackColor = true;
			this.SetQuarkAddOnDir.Click += new System.EventHandler(this.OnSetQuarkAddOnDir);
			// 
			// QuarkSrcDir
			// 
			this.QuarkSrcDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QuarkSrcDir.Location = new System.Drawing.Point(130, 101);
			this.QuarkSrcDir.Name = "QuarkSrcDir";
			this.QuarkSrcDir.Size = new System.Drawing.Size(339, 20);
			this.QuarkSrcDir.TabIndex = 8;
			// 
			// SetQuarkSrcDir
			// 
			this.SetQuarkSrcDir.Location = new System.Drawing.Point(12, 99);
			this.SetQuarkSrcDir.Name = "SetQuarkSrcDir";
			this.SetQuarkSrcDir.Size = new System.Drawing.Size(112, 23);
			this.SetQuarkSrcDir.TabIndex = 7;
			this.SetQuarkSrcDir.Text = "Addon Src Dir";
			this.SetQuarkSrcDir.UseVisualStyleBackColor = true;
			this.SetQuarkSrcDir.Click += new System.EventHandler(this.OnSetAddOnSrcDir);
			// 
			// GameAudioDir
			// 
			this.GameAudioDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GameAudioDir.Location = new System.Drawing.Point(130, 130);
			this.GameAudioDir.Name = "GameAudioDir";
			this.GameAudioDir.Size = new System.Drawing.Size(339, 20);
			this.GameAudioDir.TabIndex = 10;
			// 
			// SetGameAudioDir
			// 
			this.SetGameAudioDir.Location = new System.Drawing.Point(12, 128);
			this.SetGameAudioDir.Name = "SetGameAudioDir";
			this.SetGameAudioDir.Size = new System.Drawing.Size(112, 23);
			this.SetGameAudioDir.TabIndex = 9;
			this.SetGameAudioDir.Text = "Game Audio Dir";
			this.SetGameAudioDir.UseVisualStyleBackColor = true;
			this.SetGameAudioDir.Click += new System.EventHandler(this.OnSetGameAudioDir);
			// 
			// AudioSrcDir
			// 
			this.AudioSrcDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AudioSrcDir.Location = new System.Drawing.Point(130, 159);
			this.AudioSrcDir.Name = "AudioSrcDir";
			this.AudioSrcDir.Size = new System.Drawing.Size(339, 20);
			this.AudioSrcDir.TabIndex = 12;
			// 
			// SetAudioSrcDir
			// 
			this.SetAudioSrcDir.Location = new System.Drawing.Point(12, 157);
			this.SetAudioSrcDir.Name = "SetAudioSrcDir";
			this.SetAudioSrcDir.Size = new System.Drawing.Size(112, 23);
			this.SetAudioSrcDir.TabIndex = 11;
			this.SetAudioSrcDir.Text = "Audio Src Dir";
			this.SetAudioSrcDir.UseVisualStyleBackColor = true;
			this.SetAudioSrcDir.Click += new System.EventHandler(this.OnSetAudioSrcDir);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.LevelBaseName);
			this.groupBox1.Controls.Add(this.UpdatePortalFile);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.UpdateVisData);
			this.groupBox1.Controls.Add(this.UpdateLevel);
			this.groupBox1.Controls.Add(this.UpdateMatLib);
			this.groupBox1.Controls.Add(this.UpdateZoneDraw);
			this.groupBox1.Controls.Add(this.UpdateZone);
			this.groupBox1.Location = new System.Drawing.Point(12, 186);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(271, 109);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Update Level";
			// 
			// UpdateZone
			// 
			this.UpdateZone.AutoSize = true;
			this.UpdateZone.Location = new System.Drawing.Point(6, 38);
			this.UpdateZone.Name = "UpdateZone";
			this.UpdateZone.Size = new System.Drawing.Size(51, 17);
			this.UpdateZone.TabIndex = 0;
			this.UpdateZone.Text = "Zone";
			this.UpdateZone.UseVisualStyleBackColor = true;
			// 
			// UpdateZoneDraw
			// 
			this.UpdateZoneDraw.AutoSize = true;
			this.UpdateZoneDraw.Location = new System.Drawing.Point(6, 61);
			this.UpdateZoneDraw.Name = "UpdateZoneDraw";
			this.UpdateZoneDraw.Size = new System.Drawing.Size(76, 17);
			this.UpdateZoneDraw.TabIndex = 1;
			this.UpdateZoneDraw.Text = "ZoneDraw";
			this.UpdateZoneDraw.UseVisualStyleBackColor = true;
			// 
			// UpdateMatLib
			// 
			this.UpdateMatLib.AutoSize = true;
			this.UpdateMatLib.Location = new System.Drawing.Point(6, 84);
			this.UpdateMatLib.Name = "UpdateMatLib";
			this.UpdateMatLib.Size = new System.Drawing.Size(58, 17);
			this.UpdateMatLib.TabIndex = 2;
			this.UpdateMatLib.Text = "MatLib";
			this.UpdateMatLib.UseVisualStyleBackColor = true;
			// 
			// UpdateVisData
			// 
			this.UpdateVisData.AutoSize = true;
			this.UpdateVisData.Location = new System.Drawing.Point(88, 61);
			this.UpdateVisData.Name = "UpdateVisData";
			this.UpdateVisData.Size = new System.Drawing.Size(63, 17);
			this.UpdateVisData.TabIndex = 3;
			this.UpdateVisData.Text = "VisData";
			this.UpdateVisData.UseVisualStyleBackColor = true;
			// 
			// UpdatePortalFile
			// 
			this.UpdatePortalFile.AutoSize = true;
			this.UpdatePortalFile.Location = new System.Drawing.Point(88, 84);
			this.UpdatePortalFile.Name = "UpdatePortalFile";
			this.UpdatePortalFile.Size = new System.Drawing.Size(72, 17);
			this.UpdatePortalFile.TabIndex = 4;
			this.UpdatePortalFile.Text = "Portal File";
			this.UpdatePortalFile.UseVisualStyleBackColor = true;
			// 
			// LevelBaseName
			// 
			this.LevelBaseName.Location = new System.Drawing.Point(103, 19);
			this.LevelBaseName.Name = "LevelBaseName";
			this.LevelBaseName.Size = new System.Drawing.Size(162, 20);
			this.LevelBaseName.TabIndex = 5;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 14;
			this.label1.Text = "Level Base Name";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(6, 19);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(137, 23);
			this.button1.TabIndex = 14;
			this.button1.Text = "Update QuArk Addon";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.OnUpdateQuarkAddOn);
			// 
			// UpdateAudio
			// 
			this.UpdateAudio.Location = new System.Drawing.Point(6, 48);
			this.UpdateAudio.Name = "UpdateAudio";
			this.UpdateAudio.Size = new System.Drawing.Size(137, 23);
			this.UpdateAudio.TabIndex = 15;
			this.UpdateAudio.Text = "Update Audio";
			this.UpdateAudio.UseVisualStyleBackColor = true;
			this.UpdateAudio.Click += new System.EventHandler(this.OnUpdateAudio);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.button1);
			this.groupBox2.Controls.Add(this.UpdateAudio);
			this.groupBox2.Location = new System.Drawing.Point(289, 187);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(158, 83);
			this.groupBox2.TabIndex = 16;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Update Existing Assets";
			// 
			// Jammer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(481, 305);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.AudioSrcDir);
			this.Controls.Add(this.SetAudioSrcDir);
			this.Controls.Add(this.GameAudioDir);
			this.Controls.Add(this.SetGameAudioDir);
			this.Controls.Add(this.QuarkSrcDir);
			this.Controls.Add(this.SetQuarkSrcDir);
			this.Controls.Add(this.QuarkAddOnDir);
			this.Controls.Add(this.SetQuarkAddOnDir);
			this.Controls.Add(this.MapWorkDir);
			this.Controls.Add(this.SetMapWorkDir);
			this.Controls.Add(this.LevelsDir);
			this.Controls.Add(this.SetLevelsPath);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Jammer";
			this.Text = "Game Jammer";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button UpdateLevel;
		private System.Windows.Forms.Button SetLevelsPath;
		private System.Windows.Forms.TextBox LevelsDir;
		private System.Windows.Forms.TextBox MapWorkDir;
		private System.Windows.Forms.Button SetMapWorkDir;
		private System.Windows.Forms.TextBox QuarkAddOnDir;
		private System.Windows.Forms.Button SetQuarkAddOnDir;
		private System.Windows.Forms.TextBox QuarkSrcDir;
		private System.Windows.Forms.Button SetQuarkSrcDir;
		private System.Windows.Forms.TextBox GameAudioDir;
		private System.Windows.Forms.Button SetGameAudioDir;
		private System.Windows.Forms.TextBox AudioSrcDir;
		private System.Windows.Forms.Button SetAudioSrcDir;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox UpdatePortalFile;
		private System.Windows.Forms.CheckBox UpdateVisData;
		private System.Windows.Forms.CheckBox UpdateMatLib;
		private System.Windows.Forms.CheckBox UpdateZoneDraw;
		private System.Windows.Forms.CheckBox UpdateZone;
		private System.Windows.Forms.TextBox LevelBaseName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button UpdateAudio;
		private System.Windows.Forms.GroupBox groupBox2;
	}
}

