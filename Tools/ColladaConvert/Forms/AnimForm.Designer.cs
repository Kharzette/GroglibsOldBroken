﻿namespace ColladaConvert
{
	partial class AnimForm
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.button2 = new System.Windows.Forms.Button();
			this.SaveAnimLib = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.button1 = new System.Windows.Forms.Button();
			this.LoadCharacter = new System.Windows.Forms.Button();
			this.SaveCharacter = new System.Windows.Forms.Button();
			this.SaveStatic = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.DrawAxis = new System.Windows.Forms.CheckBox();
			this.CheckSkeleton = new System.Windows.Forms.CheckBox();
			this.LoadModel = new System.Windows.Forms.Button();
			this.LoadStaticModel = new System.Windows.Forms.Button();
			this.LoadAnim = new System.Windows.Forms.Button();
			this.BoundGroup = new System.Windows.Forms.GroupBox();
			this.ShowBox = new System.Windows.Forms.CheckBox();
			this.ShowSphere = new System.Windows.Forms.CheckBox();
			this.BoundMesh = new System.Windows.Forms.Button();
			this.PauseButton = new System.Windows.Forms.Button();
			this.AnimTimeScale = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.AnimList = new System.Windows.Forms.ListView();
			this.ReCollada = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.BoundGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AnimTimeScale)).BeginInit();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox1.Controls.Add(this.button2);
			this.groupBox1.Controls.Add(this.SaveAnimLib);
			this.groupBox1.Location = new System.Drawing.Point(380, 260);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(101, 84);
			this.groupBox1.TabIndex = 17;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Anim Library";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(6, 19);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(89, 25);
			this.button2.TabIndex = 6;
			this.button2.Text = "Load AnimLib";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.OnLoadAnimLib);
			// 
			// SaveAnimLib
			// 
			this.SaveAnimLib.Location = new System.Drawing.Point(6, 50);
			this.SaveAnimLib.Name = "SaveAnimLib";
			this.SaveAnimLib.Size = new System.Drawing.Size(89, 25);
			this.SaveAnimLib.TabIndex = 5;
			this.SaveAnimLib.Text = "Save AnimLib";
			this.SaveAnimLib.UseVisualStyleBackColor = true;
			this.SaveAnimLib.Click += new System.EventHandler(this.OnSaveAnimLib);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox2.Controls.Add(this.ReCollada);
			this.groupBox2.Controls.Add(this.button1);
			this.groupBox2.Controls.Add(this.LoadCharacter);
			this.groupBox2.Controls.Add(this.SaveCharacter);
			this.groupBox2.Controls.Add(this.SaveStatic);
			this.groupBox2.Location = new System.Drawing.Point(160, 207);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(110, 177);
			this.groupBox2.TabIndex = 18;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Converted Meshes";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(6, 50);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(97, 25);
			this.button1.TabIndex = 14;
			this.button1.Text = "Load Static";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.OnLoadStatic);
			// 
			// LoadCharacter
			// 
			this.LoadCharacter.Location = new System.Drawing.Point(6, 19);
			this.LoadCharacter.Name = "LoadCharacter";
			this.LoadCharacter.Size = new System.Drawing.Size(97, 25);
			this.LoadCharacter.TabIndex = 8;
			this.LoadCharacter.Text = "Load Character";
			this.LoadCharacter.UseVisualStyleBackColor = true;
			this.LoadCharacter.Click += new System.EventHandler(this.OnLoadCharacter);
			// 
			// SaveCharacter
			// 
			this.SaveCharacter.Location = new System.Drawing.Point(6, 81);
			this.SaveCharacter.Name = "SaveCharacter";
			this.SaveCharacter.Size = new System.Drawing.Size(97, 25);
			this.SaveCharacter.TabIndex = 7;
			this.SaveCharacter.Text = "Save Character";
			this.SaveCharacter.UseVisualStyleBackColor = true;
			this.SaveCharacter.Click += new System.EventHandler(this.OnSaveCharacter);
			// 
			// SaveStatic
			// 
			this.SaveStatic.Location = new System.Drawing.Point(6, 112);
			this.SaveStatic.Name = "SaveStatic";
			this.SaveStatic.Size = new System.Drawing.Size(97, 25);
			this.SaveStatic.TabIndex = 13;
			this.SaveStatic.Text = "Save Static";
			this.SaveStatic.UseVisualStyleBackColor = true;
			this.SaveStatic.Click += new System.EventHandler(this.OnSaveStatic);
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox3.Controls.Add(this.CheckSkeleton);
			this.groupBox3.Controls.Add(this.LoadModel);
			this.groupBox3.Controls.Add(this.LoadStaticModel);
			this.groupBox3.Controls.Add(this.LoadAnim);
			this.groupBox3.Location = new System.Drawing.Point(12, 207);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(142, 137);
			this.groupBox3.TabIndex = 19;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Collada Files";
			// 
			// DrawAxis
			// 
			this.DrawAxis.AutoSize = true;
			this.DrawAxis.Checked = true;
			this.DrawAxis.CheckState = System.Windows.Forms.CheckState.Checked;
			this.DrawAxis.Location = new System.Drawing.Point(12, 359);
			this.DrawAxis.Name = "DrawAxis";
			this.DrawAxis.Size = new System.Drawing.Size(73, 17);
			this.DrawAxis.TabIndex = 25;
			this.DrawAxis.Text = "Draw Axis";
			this.DrawAxis.UseVisualStyleBackColor = true;
			// 
			// CheckSkeleton
			// 
			this.CheckSkeleton.AutoSize = true;
			this.CheckSkeleton.Location = new System.Drawing.Point(6, 112);
			this.CheckSkeleton.Name = "CheckSkeleton";
			this.CheckSkeleton.Size = new System.Drawing.Size(102, 17);
			this.CheckSkeleton.TabIndex = 24;
			this.CheckSkeleton.Text = "Check Skeleton";
			this.CheckSkeleton.UseVisualStyleBackColor = true;
			// 
			// LoadModel
			// 
			this.LoadModel.Location = new System.Drawing.Point(6, 19);
			this.LoadModel.Name = "LoadModel";
			this.LoadModel.Size = new System.Drawing.Size(130, 25);
			this.LoadModel.TabIndex = 1;
			this.LoadModel.Text = "Load DAE Char Parts";
			this.LoadModel.UseVisualStyleBackColor = true;
			this.LoadModel.Click += new System.EventHandler(this.OnLoadCharacterDAE);
			// 
			// LoadStaticModel
			// 
			this.LoadStaticModel.Location = new System.Drawing.Point(6, 50);
			this.LoadStaticModel.Name = "LoadStaticModel";
			this.LoadStaticModel.Size = new System.Drawing.Size(130, 25);
			this.LoadStaticModel.TabIndex = 12;
			this.LoadStaticModel.Text = "Load Static DAE";
			this.LoadStaticModel.UseVisualStyleBackColor = true;
			this.LoadStaticModel.Click += new System.EventHandler(this.OnOpenStaticDAE);
			// 
			// LoadAnim
			// 
			this.LoadAnim.Location = new System.Drawing.Point(6, 81);
			this.LoadAnim.Name = "LoadAnim";
			this.LoadAnim.Size = new System.Drawing.Size(130, 25);
			this.LoadAnim.TabIndex = 0;
			this.LoadAnim.Text = "Load Anim DAE";
			this.LoadAnim.UseVisualStyleBackColor = true;
			this.LoadAnim.Click += new System.EventHandler(this.OnLoadAnimDAE);
			// 
			// BoundGroup
			// 
			this.BoundGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BoundGroup.Controls.Add(this.ShowBox);
			this.BoundGroup.Controls.Add(this.ShowSphere);
			this.BoundGroup.Controls.Add(this.BoundMesh);
			this.BoundGroup.Location = new System.Drawing.Point(276, 260);
			this.BoundGroup.Name = "BoundGroup";
			this.BoundGroup.Size = new System.Drawing.Size(98, 97);
			this.BoundGroup.TabIndex = 23;
			this.BoundGroup.TabStop = false;
			this.BoundGroup.Text = "Bounds";
			// 
			// ShowBox
			// 
			this.ShowBox.AutoSize = true;
			this.ShowBox.Location = new System.Drawing.Point(6, 73);
			this.ShowBox.Name = "ShowBox";
			this.ShowBox.Size = new System.Drawing.Size(74, 17);
			this.ShowBox.TabIndex = 2;
			this.ShowBox.Text = "Show Box";
			this.ShowBox.UseVisualStyleBackColor = true;
			this.ShowBox.CheckedChanged += new System.EventHandler(this.OnShowBoxChanged);
			// 
			// ShowSphere
			// 
			this.ShowSphere.AutoSize = true;
			this.ShowSphere.Location = new System.Drawing.Point(6, 50);
			this.ShowSphere.Name = "ShowSphere";
			this.ShowSphere.Size = new System.Drawing.Size(90, 17);
			this.ShowSphere.TabIndex = 1;
			this.ShowSphere.Text = "Show Sphere";
			this.ShowSphere.UseVisualStyleBackColor = true;
			this.ShowSphere.CheckedChanged += new System.EventHandler(this.OnShowSphereChanged);
			// 
			// BoundMesh
			// 
			this.BoundMesh.Location = new System.Drawing.Point(6, 19);
			this.BoundMesh.Name = "BoundMesh";
			this.BoundMesh.Size = new System.Drawing.Size(86, 25);
			this.BoundMesh.TabIndex = 0;
			this.BoundMesh.Text = "Calc Bound";
			this.BoundMesh.UseVisualStyleBackColor = true;
			this.BoundMesh.Click += new System.EventHandler(this.OnCalcBounds);
			// 
			// PauseButton
			// 
			this.PauseButton.Location = new System.Drawing.Point(129, 19);
			this.PauseButton.Name = "PauseButton";
			this.PauseButton.Size = new System.Drawing.Size(55, 22);
			this.PauseButton.TabIndex = 5;
			this.PauseButton.Text = "Pause";
			this.PauseButton.UseVisualStyleBackColor = true;
			this.PauseButton.Click += new System.EventHandler(this.OnPauseAnim);
			// 
			// AnimTimeScale
			// 
			this.AnimTimeScale.DecimalPlaces = 2;
			this.AnimTimeScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.AnimTimeScale.Location = new System.Drawing.Point(6, 19);
			this.AnimTimeScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.AnimTimeScale.Name = "AnimTimeScale";
			this.AnimTimeScale.Size = new System.Drawing.Size(51, 20);
			this.AnimTimeScale.TabIndex = 3;
			this.AnimTimeScale.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(63, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Time Scale";
			// 
			// groupBox4
			// 
			this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox4.Controls.Add(this.PauseButton);
			this.groupBox4.Controls.Add(this.AnimTimeScale);
			this.groupBox4.Controls.Add(this.label1);
			this.groupBox4.Location = new System.Drawing.Point(276, 207);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(190, 47);
			this.groupBox4.TabIndex = 21;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Playback";
			// 
			// AnimList
			// 
			this.AnimList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AnimList.GridLines = true;
			this.AnimList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.AnimList.HideSelection = false;
			this.AnimList.LabelEdit = true;
			this.AnimList.Location = new System.Drawing.Point(12, 12);
			this.AnimList.Name = "AnimList";
			this.AnimList.Size = new System.Drawing.Size(469, 189);
			this.AnimList.TabIndex = 24;
			this.AnimList.UseCompatibleStateImageBehavior = false;
			this.AnimList.View = System.Windows.Forms.View.Details;
			this.AnimList.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.OnAnimRename);
			this.AnimList.SelectedIndexChanged += new System.EventHandler(this.OnAnimListSelectionChanged);
			this.AnimList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnAnimListKeyUp);
			// 
			// ReCollada
			// 
			this.ReCollada.Location = new System.Drawing.Point(7, 144);
			this.ReCollada.Name = "ReCollada";
			this.ReCollada.Size = new System.Drawing.Size(96, 25);
			this.ReCollada.TabIndex = 15;
			this.ReCollada.Text = "ReCollada";
			this.ReCollada.UseVisualStyleBackColor = true;
			this.ReCollada.Click += new System.EventHandler(this.OnReCollada);
			// 
			// AnimForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(493, 396);
			this.ControlBox = false;
			this.Controls.Add(this.DrawAxis);
			this.Controls.Add(this.AnimList);
			this.Controls.Add(this.BoundGroup);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AnimForm";
			this.ShowInTaskbar = false;
			this.Text = "Animation Stuff";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.BoundGroup.ResumeLayout(false);
			this.BoundGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AnimTimeScale)).EndInit();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button SaveAnimLib;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button LoadCharacter;
		private System.Windows.Forms.Button SaveCharacter;
		private System.Windows.Forms.Button SaveStatic;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox CheckSkeleton;
		private System.Windows.Forms.Button LoadModel;
		private System.Windows.Forms.Button LoadStaticModel;
		private System.Windows.Forms.Button LoadAnim;
		private System.Windows.Forms.GroupBox BoundGroup;
		private System.Windows.Forms.CheckBox ShowBox;
		private System.Windows.Forms.CheckBox ShowSphere;
		private System.Windows.Forms.Button BoundMesh;
		private System.Windows.Forms.Button PauseButton;
		private System.Windows.Forms.NumericUpDown AnimTimeScale;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.CheckBox DrawAxis;
		private System.Windows.Forms.ListView AnimList;
		private System.Windows.Forms.Button ReCollada;

	}
}

