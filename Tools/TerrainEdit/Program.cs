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
using TerrainLib;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;


namespace TerrainEdit
{
	static class Program
	{
		enum MyActions
		{
			MoveForwardBack, MoveForward, MoveBack,
			MoveLeftRight, MoveLeft, MoveRight,
			MoveForwardFast, MoveBackFast,
			MoveLeftFast, MoveRightFast,
			Turn, TurnLeft, TurnRight,
			Pitch, PitchUp, PitchDown,
			ToggleMouseLookOn, ToggleMouseLookOff,
			Exit
		};

		const float	MaxTimeDelta	=0.1f;


		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			GraphicsDevice	gd	=new GraphicsDevice("Terrain Editor",
				FeatureLevel.Level_11_0, 0.1f, 3000f);

			//save form positions
			gd.RendForm.DataBindings.Add(new Binding("Location",
				Settings.Default, "MainWindowPos", true,
				DataSourceUpdateMode.OnPropertyChanged));

			gd.RendForm.Location	=Settings.Default.MainWindowPos;

			//set this to whereever the game data is stored during
			//development.  Release ver will look in .
#if DEBUG
			string	rootDir	="C:\\Games\\CurrentGame";
#else
			string	rootDir	=".";
#endif

			StuffKeeper	sk	=new StuffKeeper();

			SharedForms.ShaderCompileHelper.mTitle	="Compiling Shaders...";

			sk.eCompileNeeded	+=SharedForms.ShaderCompileHelper.CompileNeededHandler;
			sk.eCompileDone		+=SharedForms.ShaderCompileHelper.CompileDoneHandler;

			sk.Init(gd, rootDir);

			TerrainAtlas	ta	=new TerrainAtlas(gd, sk);

			ta.DataBindings.Add(new Binding("Location",
				Settings.Default, "AtlasFormPos", true,
				DataSourceUpdateMode.OnPropertyChanged));
			ta.DataBindings.Add(new Binding("Size",
				Settings.Default, "AtlasFormSize", true,
				DataSourceUpdateMode.OnPropertyChanged));

			ta.Location		=Settings.Default.AtlasFormPos;
			ta.Size			=Settings.Default.AtlasFormSize;
			ta.Visible		=true;

			TerrainForm	tf	=new TerrainForm();

			tf.DataBindings.Add(new Binding("Location",
				Settings.Default, "TerrainFormPos", true,
				DataSourceUpdateMode.OnPropertyChanged));

			tf.Location		=Settings.Default.TerrainFormPos;
			tf.Visible		=true;

			TerrainShading	ts	=new TerrainShading();

			ts.DataBindings.Add(new Binding("Location",
				Settings.Default, "ShadingFormPos", true,
				DataSourceUpdateMode.OnPropertyChanged));

			ts.Location	=Settings.Default.ShadingFormPos;
			ts.Visible	=true;

			PlayerSteering	pSteering		=SetUpSteering();
			Input			inp				=SetUpInput();
			Random			rand			=new Random();
			Vector3			pos				=Vector3.One * 5f;
			Vector3			lightDir		=-Vector3.UnitY;
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

			GameLoop	gLoop	=new GameLoop(gd, sk, rootDir);

			EventHandler	buildHandler	=new EventHandler(
				delegate(object s, EventArgs ea)
				{	ListEventArgs<HeightMap.TexData>	lea	=ea as ListEventArgs<HeightMap.TexData>;
					gLoop.Texture((TexAtlas)s, lea.mList, ta.GetTransitionHeight());	});

			EventHandler	applyHandler	=new EventHandler(
				delegate(object s, EventArgs ea)
				{
					TerrainShading.ShadingInfo	si	=s as TerrainShading.ShadingInfo;
					gLoop.ApplyShadingInfo(si);
				});

			ta.eReBuild	+=buildHandler;
			ts.eApply	+=applyHandler;

			EventHandler	tBuildHandler	=new EventHandler(
				delegate(object s, EventArgs ea)
				{	int		gridSize, chunkSize, tilingIterations, threads;
					int		erosionIterations, polySize, smoothPasses, seed;
					float	medianHeight, variance, borderSize;
					float	rainFall, solubility, evaporation;
					tf.GetBuildData(out gridSize, out chunkSize,
						out medianHeight, out variance,	out polySize,
						out tilingIterations, out borderSize, out smoothPasses,
						out seed, out erosionIterations, out rainFall,
						out solubility, out evaporation, out threads);
					gLoop.TBuild(gridSize, chunkSize, medianHeight, variance,
						polySize, tilingIterations, borderSize,
						smoothPasses, seed, erosionIterations,
						rainFall, solubility, evaporation, threads);	});
			EventHandler	tLoadHandler	=new EventHandler(
				delegate(object s, EventArgs ea)
				{
					gLoop.TLoad(s as string);
					ta.LoadAtlasInfo(s as string);
				});
			EventHandler	tSaveHandler	=new EventHandler(
				delegate(object s, EventArgs ea)
				{
					if(gLoop.TSave(s as string))
					{
						ta.SaveAtlasInfo(s as string);
						ts.SaveShadingInfo(s as string);
					}
				});

			tf.eBuild	+=tBuildHandler;
			tf.eLoad	+=tLoadHandler;
			tf.eSave	+=tSaveHandler;

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
					gLoop.Update(time, acts, pSteering);
					time.UpdateDone();
				}

				gLoop.RenderUpdate(time.GetRenderUpdateDeltaMilliSeconds());

				gLoop.Render();
				
				gd.Present();

				acts.Clear();
			}, true);	//true here is slow but needed for winforms events

			Settings.Default.Save();

			sk.eCompileNeeded	-=SharedForms.ShaderCompileHelper.CompileNeededHandler;
			sk.eCompileDone		-=SharedForms.ShaderCompileHelper.CompileDoneHandler;

			gd.RendForm.Activated		-=actHandler;
			gd.RendForm.AppDeactivated	-=deActHandler;
			ta.eReBuild					-=buildHandler;
			tf.eBuild					-=tBuildHandler;
			ts.eApply					-=applyHandler;

			gLoop.FreeAll();
			inp.FreeAll();
			ta.FreeAll();
			sk.FreeAll();
			
			//Release all resources
			gd.ReleaseAll();
		}

		static List<Input.InputAction> UpdateInput(Input inp,
			GraphicsDevice gd, float delta, ref bool bMouseLookOn)
		{
			List<Input.InputAction>	actions	=inp.GetAction();

			//check for exit
			foreach(Input.InputAction act in actions)
			{
				if(act.mAction.Equals(MyActions.Exit))
				{
					gd.RendForm.Close();
					return	actions;
				}
			}

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


		static Input SetUpInput()
		{
			Input	inp	=new InputLib.Input(1f / Stopwatch.Frequency);
			
			//wasd
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

			inp.MapToggleAction(MyActions.ToggleMouseLookOn,
				MyActions.ToggleMouseLookOff, Modifiers.None,
				Input.VariousButtons.RightMouseButton);
			inp.MapToggleAction(MyActions.ToggleMouseLookOn,
				MyActions.ToggleMouseLookOff, Modifiers.ShiftHeld,
				Input.VariousButtons.RightMouseButton);

			inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.GamePadRightYAxis);
			inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.GamePadRightXAxis);
			inp.MapAxisAction(MyActions.MoveLeftRight, Input.MoveAxis.GamePadLeftXAxis);
			inp.MapAxisAction(MyActions.MoveForwardBack, Input.MoveAxis.GamePadLeftYAxis);

			//exit
			inp.MapAction(MyActions.Exit, ActionTypes.PressAndRelease,
				Modifiers.None, System.Windows.Forms.Keys.Escape);
			inp.MapAction(MyActions.Exit, ActionTypes.PressAndRelease,
				Modifiers.None, Input.VariousButtons.GamePadBack);

			return	inp;
		}

		static PlayerSteering SetUpSteering()
		{
			PlayerSteering	pSteering	=new PlayerSteering();
			pSteering.Method			=PlayerSteering.SteeringMethod.Fly;

			pSteering.SetMoveEnums(MyActions.MoveForwardBack, MyActions.MoveLeftRight,
				MyActions.MoveForward, MyActions.MoveBack,
				MyActions.MoveLeft, MyActions.MoveRight,
				MyActions.MoveForwardFast, MyActions.MoveBackFast,
				MyActions.MoveLeftFast, MyActions.MoveRightFast);

			pSteering.SetTurnEnums(MyActions.Turn, MyActions.TurnLeft, MyActions.TurnRight);

			pSteering.SetPitchEnums(MyActions.Pitch, MyActions.PitchUp, MyActions.PitchDown);

			return	pSteering;
		}
	}
}
