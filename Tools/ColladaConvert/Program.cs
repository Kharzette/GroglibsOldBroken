﻿using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InputLib;
using MaterialLib;
using UtilityLib;
using MeshLib;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer	=SharpDX.Direct3D11.Buffer;
using Device	=SharpDX.Direct3D11.Device;
using MapFlags	=SharpDX.Direct3D11.MapFlags;
using MatLib	=MaterialLib.MaterialLib;


namespace ColladaConvert
{
	internal static class Program
	{
		enum MyActions
		{
			MoveForwardBack, MoveForward, MoveBack,
			MoveLeftRight, MoveLeft, MoveRight,
			MoveForwardFast, MoveBackFast,
			MoveLeftFast, MoveRightFast,
			Turn, TurnLeft, TurnRight,
			Pitch, PitchUp, PitchDown,
			LightX, LightY, LightZ,
			ToggleMouseLookOn, ToggleMouseLookOff
		};

		const float	MaxTimeDelta	=0.1f;


		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			GraphicsDevice	gd	=new GraphicsDevice("Collada Conversion Tool",
				FeatureLevel.Level_11_0, 0.1f, 3000f);

			//save renderform position
			gd.RendForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					Settings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			gd.RendForm.Location	=Settings.Default.MainWindowPos;

			SharedForms.ShaderCompileHelper.mTitle	="Compiling Shaders...";

			StuffKeeper	sk		=new StuffKeeper();

			sk.eCompileNeeded	+=SharedForms.ShaderCompileHelper.CompileNeededHandler;
			sk.eCompileDone		+=SharedForms.ShaderCompileHelper.CompileDoneHandler;

			sk.Init(gd, "E:\\Games\\GrogLibsGame");

			MatLib		matLib	=new MatLib(gd, sk);

			matLib.InitCelShading(1);
			matLib.GenerateCelTexturePreset(gd.GD,
				gd.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);
			matLib.SetCelTexture(0);

			//set up post processing module
			PostProcess	post	=new PostProcess(gd, matLib, "Post.fx");

			PlayerSteering	pSteering		=SetUpSteering();
			Input			inp				=SetUpInput();
			Random			rand			=new Random();
			CommonPrims		comPrims		=new CommonPrims(gd, sk);
			bool			bMouseLookOn	=false;

			EventHandler	actHandler	=new EventHandler(
				delegate(object s, EventArgs ea)
				{	inp.ClearInputs();	});

			EventHandler<EventArgs>	deActHandler	=new EventHandler<EventArgs>(
				delegate(object s, EventArgs ea)
				{
					gd.SetCapture(false);
					bMouseLookOn	=false;
				});

			gd.RendForm.Activated		+=actHandler;
			gd.RendForm.AppDeactivated	+=deActHandler;

			int	resx	=gd.RendForm.ClientRectangle.Width;
			int	resy	=gd.RendForm.ClientRectangle.Height;

			AnimForm	ss	=SetUpForms(gd.GD, matLib, sk, comPrims);

			Vector3	pos			=Vector3.One * 5f;
			Vector3	lightDir	=-Vector3.UnitY;

			UpdateTimer	time	=new UpdateTimer(true, false);

			time.SetFixedTimeStepSeconds(1f / 60f);	//60fps update rate
			time.SetMaxDeltaSeconds(MaxTimeDelta);

			List<Input.InputAction>	acts	=new List<Input.InputAction>();

			RenderLoop.Run(gd.RendForm, () =>
			{
				if(!gd.RendForm.Focused)
				{
					Thread.Sleep(33);
				}

				gd.CheckResize();

				if(bMouseLookOn && gd.RendForm.Focused)
				{
					gd.ResetCursorPos();
				}

				//Clear views
				gd.ClearViews();

				time.Stamp();
				while(time.GetUpdateDeltaSeconds() > 0f)
				{
					acts	=UpdateInput(inp, gd,
						time.GetUpdateDeltaSeconds(), ref bMouseLookOn);
					if(!gd.RendForm.Focused)
					{
						acts.Clear();
						bMouseLookOn	=false;
						gd.SetCapture(false);
						inp.UnMapAxisAction(Input.MoveAxis.MouseYAxis);
						inp.UnMapAxisAction(Input.MoveAxis.MouseXAxis);
					}

					Vector3	deltaMove	=pSteering.Update(pos,
						gd.GCam.Forward, gd.GCam.Left, gd.GCam.Up, acts);

					deltaMove	*=200f;
					pos			-=deltaMove;
					
					ChangeLight(acts, ref lightDir);

					time.UpdateDone();
				}

				//light direction is backwards now for some strange reason
				matLib.SetParameterForAll("mLightDirection", -lightDir);				
				
				gd.GCam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

				matLib.UpdateWVP(Matrix.Identity, gd.GCam.View, gd.GCam.Projection, gd.GCam.Position);

				comPrims.Update(gd.GCam, lightDir);

				ss.RenderUpdate(time.GetRenderUpdateDeltaSeconds());

				post.SetTargets(gd, "BackColor", "BackDepth");

				post.ClearTarget(gd, "BackColor", Color.CornflowerBlue);
				post.ClearDepth(gd, "BackDepth");

				ss.Render(gd.DC);

				if(ss.GetDrawAxis())
				{
					comPrims.DrawAxis(gd.DC);
				}

				if(ss.GetDrawBox())
				{
					comPrims.DrawBox(gd.DC, Matrix.Identity);
				}

				if(ss.GetDrawSphere())
				{
					comPrims.DrawSphere(gd.DC, Matrix.Identity);
				}

				gd.Present();

				acts.Clear();
			}, true);	//true here is slow but needed for winforms events

			Settings.Default.Save();

			gd.RendForm.Activated		-=actHandler;
			gd.RendForm.AppDeactivated	-=deActHandler;

			comPrims.FreeAll();
			inp.FreeAll();
			post.FreeAll();
			matLib.FreeAll();

			sk.eCompileDone		-=SharedForms.ShaderCompileHelper.CompileDoneHandler;
			sk.eCompileNeeded	-=SharedForms.ShaderCompileHelper.CompileNeededHandler;

			sk.FreeAll();
			
			//Release all resources
			gd.ReleaseAll();
		}

		static AnimForm SetUpForms(Device gd, MatLib matLib, StuffKeeper sk, CommonPrims ep)
		{
			MeshLib.AnimLib	animLib	=new MeshLib.AnimLib();
			AnimForm		af		=new AnimForm(gd, matLib, animLib);
			StripElements	se		=new StripElements();
			SkeletonEditor	skel	=new SkeletonEditor();

			SharedForms.MaterialForm	matForm	=new SharedForms.MaterialForm(matLib, sk);
			SharedForms.CelTweakForm	celForm	=new SharedForms.CelTweakForm(gd, matLib);

			//save positions
			matForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "MaterialFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			af.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "AnimFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			skel.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "SkeletonEditorFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			skel.DataBindings.Add(new System.Windows.Forms.Binding("Size",
				Settings.Default, "SkeletonEditorFormSize", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			celForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "CelTweakFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			SeamEditor	seam	=null;
			MakeSeamForm(ref seam);

			af.eMeshChanged			+=(sender, args) => matForm.SetMesh(sender);
			matForm.eNukedMeshPart	+=(sender, args) => af.NukeMeshPart(sender as List<int>);
			matForm.eStripElements	+=(sender, args) =>
				{	if(se.Visible){	return;	}
					se.Populate(args as ArchEventArgs);	};
			matForm.eGenTangents	+=(sender, args) =>
				{	ArchEventArgs	aea	=args as ArchEventArgs;
					if(aea != null)
					{
						aea.mArch.GenTangents(gd, aea.mIndexes, matForm.GetTexCoordSet());
					}	};
			matForm.eFoundSeams		+=(sender, args) =>
				{	if(seam.IsDisposed)
					{
						MakeSeamForm(ref seam);
					}
					seam.Initialize(gd);
					seam.AddSeams(sender as List<EditorMesh.WeightSeam>);
					seam.SizeColumns();
					seam.Visible	=true;	};
			se.eDeleteElement		+=(sender, args) =>
				{	List<int>	elements	=sender as List<int>;
					af.NukeVertexElement(se.GetIndexes(), elements);
					se.Populate(null);	se.Visible	=false;
					matForm.RefreshMeshPartList();	};
			se.eEscape				+=(sender, args) =>
				{	se.Populate(null);	se.Visible	=false;	};
			af.eSkeletonChanged		+=(sender, args) => skel.Initialize(sender as MeshLib.Skeleton);
			af.eBoundsChanged		+=(sender, args) => ep.ReBuildBoundsDrawData(gd, sender);			

			af.Visible		=true;
			matForm.Visible	=true;
			skel.Visible	=true;
			celForm.Visible	=true;

			return	af;
		}

		static Input SetUpInput()
		{
			Input	inp	=new InputLib.Input(1f / Stopwatch.Frequency);
			
			inp.MapAction(MyActions.MoveForward, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.W);
			inp.MapAction(MyActions.MoveLeft, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.A);
			inp.MapAction(MyActions.MoveBack, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.S);
			inp.MapAction(MyActions.MoveRight, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.D);
			inp.MapAction(MyActions.MoveForwardFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.W);
			inp.MapAction(MyActions.MoveBackFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.S);
			inp.MapAction(MyActions.MoveLeftFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.A);
			inp.MapAction(MyActions.MoveRightFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.D);

			//arrow keys
			inp.MapAction(MyActions.MoveForward, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.Up);
			inp.MapAction(MyActions.MoveBack, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.Down);
			inp.MapAction(MyActions.MoveForwardFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.Up);
			inp.MapAction(MyActions.MoveBackFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.Down);
			inp.MapAction(MyActions.TurnLeft, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.Left);
			inp.MapAction(MyActions.TurnRight, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.Right);
			inp.MapAction(MyActions.PitchUp, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.Q);
			inp.MapAction(MyActions.PitchDown, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.E);

			inp.MapAction(MyActions.PitchUp, ActionTypes.ContinuousHold, Modifiers.None, 16);
			inp.MapAction(MyActions.PitchDown, ActionTypes.ContinuousHold, Modifiers.None, 18);
			inp.MapAction(MyActions.LightX, ActionTypes.ContinuousHold, Modifiers.None, 36);
			inp.MapAction(MyActions.LightY, ActionTypes.ContinuousHold, Modifiers.None, 37);
			inp.MapAction(MyActions.LightZ, ActionTypes.ContinuousHold, Modifiers.None, 38);

			inp.MapToggleAction(MyActions.ToggleMouseLookOn,
				MyActions.ToggleMouseLookOff, Modifiers.None,
				Input.VariousButtons.RightMouseButton);

			inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.GamePadRightYAxis);
			inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.GamePadRightXAxis);
			inp.MapAxisAction(MyActions.MoveLeftRight, Input.MoveAxis.GamePadLeftXAxis);
			inp.MapAxisAction(MyActions.MoveForwardBack, Input.MoveAxis.GamePadLeftYAxis);

			inp.MapAction(MyActions.LightX, ActionTypes.ContinuousHold, Modifiers.None, Input.VariousButtons.GamePadDPadLeft);
			inp.MapAction(MyActions.LightY, ActionTypes.ContinuousHold, Modifiers.None, Input.VariousButtons.GamePadDPadDown);
			inp.MapAction(MyActions.LightZ, ActionTypes.ContinuousHold, Modifiers.None, Input.VariousButtons.GamePadDPadRight);

			return	inp;
		}

		static PlayerSteering SetUpSteering()
		{
			PlayerSteering	pSteering	=new PlayerSteering();
			pSteering.Method			=PlayerSteering.SteeringMethod.Fly;

			pSteering.SetMoveEnums(MyActions.MoveForwardBack, MyActions.MoveLeftRight,
				MyActions.MoveForward, MyActions.MoveBack, MyActions.MoveLeft,
				MyActions.MoveRight, MyActions.MoveForwardFast, MyActions.MoveBackFast,
				MyActions.MoveLeftFast, MyActions.MoveRightFast);

			pSteering.SetTurnEnums(MyActions.Turn, MyActions.TurnLeft, MyActions.TurnRight);

			pSteering.SetPitchEnums(MyActions.Pitch, MyActions.PitchUp, MyActions.PitchDown);

			return	pSteering;
		}

		static void MakeSeamForm(ref SeamEditor seam)
		{
			seam	=new SeamEditor();

			seam.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "SeamEditorFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			seam.DataBindings.Add(new System.Windows.Forms.Binding("Size",
				Settings.Default, "SeamEditorFormSize", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
		}

		static List<Input.InputAction> UpdateInput(Input inp,
			GraphicsDevice gd, float delta, ref bool bMouseLookOn)
		{
			List<Input.InputAction>	actions	=inp.GetAction();

			foreach(Input.InputAction act in actions)
			{
				if(act.mAction.Equals(MyActions.ToggleMouseLookOn))
				{
					bMouseLookOn	=true;
					gd.SetCapture(true);
					inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
					inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
				}
				else if(act.mAction.Equals(MyActions.ToggleMouseLookOff))
				{
					bMouseLookOn	=false;
					gd.SetCapture(false);
					inp.UnMapAxisAction(Input.MoveAxis.MouseYAxis);
					inp.UnMapAxisAction(Input.MoveAxis.MouseXAxis);
				}
			}

			//delta scale analogs, since there's no timestamp stuff in gamepad code
			foreach(Input.InputAction act in actions)
			{
				if(!act.mbTime && act.mDevice == Input.InputAction.DeviceType.ANALOG)
				{
					//analog needs a time scale applied
					act.mMultiplier	*=delta;
				}
			}

			//scale inputs to user prefs
			foreach(Input.InputAction act in actions)
			{
				if(act.mAction.Equals(MyActions.Turn)
					|| act.mAction.Equals(MyActions.TurnLeft)
					|| act.mAction.Equals(MyActions.TurnRight)
					|| act.mAction.Equals(MyActions.Pitch)
					|| act.mAction.Equals(MyActions.PitchDown)
					|| act.mAction.Equals(MyActions.PitchUp))
				{
					if(act.mDevice == Input.InputAction.DeviceType.MOUSE)
					{
						act.mMultiplier	*=UserSettings.MouseTurnMultiplier;
					}
					else if(act.mDevice == Input.InputAction.DeviceType.ANALOG)
					{
						act.mMultiplier	*=UserSettings.AnalogTurnMultiplier;
					}
					else if(act.mDevice == Input.InputAction.DeviceType.KEYS)
					{
						act.mMultiplier	*=UserSettings.KeyTurnMultiplier;
					}
				}
			}
			return	actions;
		}


		static void ChangeLight(List<Input.InputAction> acts, ref Vector3 lightDir)
		{
			foreach(Input.InputAction act in acts)
			{
				if(act.mAction.Equals(MyActions.LightX))
				{
					Matrix	rot	=Matrix.RotationX(act.mMultiplier);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
				else if(act.mAction.Equals(MyActions.LightY))
				{
					Matrix	rot	=Matrix.RotationY(act.mMultiplier);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
				else if(act.mAction.Equals(MyActions.LightZ))
				{
					Matrix	rot	=Matrix.RotationZ(act.mMultiplier);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
			}
		}

		static void DeleteVertElement(Device gd, List<int> inds, List<Mesh> meshes)
		{
			Type	firstType	=meshes[0].VertexType;

			foreach(Mesh m in meshes)
			{
				if(m.VertexType == firstType)
				{
					EditorMesh	em	=m as EditorMesh;

					em.NukeVertexElement(inds, gd);
				}
			}
		}
	}
}
