using System;
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


namespace TestUI
{
	internal static class Program
	{
		internal enum MyActions
		{
			MoveForwardBack, MoveForward, MoveBack,
			MoveLeftRight, MoveLeft, MoveRight,
			MoveForwardFast, MoveBackFast,
			MoveLeftFast, MoveRightFast,
			Turn, TurnLeft, TurnRight,
			Pitch, PitchUp, PitchDown,
			ToggleMouseLookOn, ToggleMouseLookOff,
			ExitMenu, MouseSelect,
			MenuUp, MenuDown, MenuSelect,
			MenuLeft, MenuRight,
			MenuUpDown, MenuLeftRight
		};

		const float	MaxTimeDelta	=0.1f;


		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			GraphicsDevice	gd	=new GraphicsDevice("UI Test Program",
				FeatureLevel.Level_11_0, 0.1f, 3000f);

			//save renderform position
			gd.RendForm.DataBindings.Add(new Binding("Location",
				Settings.Default, "MainWindowPos", true,
				DataSourceUpdateMode.OnPropertyChanged));

			//set title of progress window
			SharedForms.ShaderCompileHelper.mTitle	="Compiling Shaders...";

			int	borderWidth		=gd.RendForm.Size.Width - gd.RendForm.ClientSize.Width;
			int	borderHeight	=gd.RendForm.Size.Height - gd.RendForm.ClientSize.Height;

			gd.RendForm.Location	=Settings.Default.MainWindowPos;
			gd.RendForm.Size		=new System.Drawing.Size(
				1280 + borderWidth,
				720 + borderHeight);

			gd.CheckResize();

			UserSettings	userSettings	=new UserSettings();

			StuffKeeper	sk		=new StuffKeeper();

			sk.eCompileNeeded	+=SharedForms.ShaderCompileHelper.CompileNeededHandler;
			sk.eCompileDone		+=SharedForms.ShaderCompileHelper.CompileDoneHandler;

			sk.Init(gd, "C:\\Games\\CurrentGame");

			gd.SetFullScreen(userSettings.mbFullScreen);

			UpdateTimer	time	=new UpdateTimer(true, true);

			time.SetFixedTimeStepSeconds(1f / 60f);	//60fps update rate
			time.SetMaxDeltaSeconds(MaxTimeDelta);

			PlayerSteering	pSteering		=SetUpSteering();
			Input			inp				=SetUpInput();
			Random			rand			=new Random();
			CommonPrims		comPrims		=new CommonPrims(gd, sk);
			bool			bMouseLookOn	=false;
			Vector3			pos				=Vector3.One * 5f;
			Vector3			lightDir		=-Vector3.UnitY;

			EventHandler	actHandler	=new EventHandler(
				delegate(object s, EventArgs ea)
				{	inp.ClearInputs();	});

			EventHandler<EventArgs>	deActHandler	=new EventHandler<EventArgs>(
				delegate(object s, EventArgs ea)
				{
					gd.SetCapture(false);
				});

			gd.RendForm.Activated		+=actHandler;
			gd.RendForm.AppDeactivated	+=deActHandler;

			MainMenu	menu	=new MainMenu(gd, sk, inp, userSettings);
			Gumps		gumps	=new Gumps(gd, sk);

			SwitchKeyControls(inp, userSettings.mbESDF);

			EventHandler	esdfHandler	=new EventHandler(
				delegate(object s, EventArgs ea)
				{	SwitchKeyControls(inp, (bool)s);	});

			menu.eESDF	+=esdfHandler;

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
					acts	=UpdateInput(inp, gd, time.GetUpdateDeltaSeconds());
					if(!gd.RendForm.Focused)
					{
						acts.Clear();
						bMouseLookOn	=false;
						gd.SetCapture(false);
						inp.UnMapAxisAction(Input.MoveAxis.MouseYAxis);
						inp.UnMapAxisAction(Input.MoveAxis.MouseXAxis);
					}
					menu.Update(time, acts);

					UserScaleActions(userSettings, acts);

					if(!menu.IsActive())
					{
						UpdateMouseLook(acts, inp, gd, ref bMouseLookOn);
						Vector3	deltaMove	=pSteering.Update(pos, gd.GCam.Forward, gd.GCam.Left, gd.GCam.Up, acts);

						//scale speed up a bit
						deltaMove	*=10f;

						pos	-=deltaMove;
					}
					else
					{
						if(bMouseLookOn)
						{
							//kill mouselook if menu pops up
							bMouseLookOn	=false;
							gd.SetCapture(false);
							inp.UnMapAxisAction(Input.MoveAxis.MouseYAxis);
							inp.UnMapAxisAction(Input.MoveAxis.MouseXAxis);
						}
					}
					time.UpdateDone();
				}

				gumps.Update(gd.DC);

				MenuBindings(inp, menu.IsActive());

				gd.GCam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

				comPrims.Update(gd.GCam, lightDir);

				comPrims.DrawAxis(gd.DC);

				gumps.Draw(gd.DC);
				menu.Render(gd.DC);
				
				gd.Present();

				acts.Clear();
			}, true);	//true here is slow but needed for winforms events

			Settings.Default.Save();

			gd.RendForm.Activated		-=actHandler;
			gd.RendForm.AppDeactivated	-=deActHandler;

			inp.FreeAll();
			comPrims.FreeAll();

			menu.eESDF	-=esdfHandler;

			menu.FreeAll();
			
			sk.eCompileDone		-=SharedForms.ShaderCompileHelper.CompileDoneHandler;
			sk.eCompileNeeded	-=SharedForms.ShaderCompileHelper.CompileNeededHandler;

			sk.FreeAll();

			//Release all resources
			gd.ReleaseAll();

			userSettings.SaveSettings();
		}

		//multiply by user preferences
		static void UserScaleActions(UserSettings settings, List<Input.InputAction> actions)
		{
			//analogs are backwards by default according to what
			//standards I could find on the goog (in Y)
			foreach(Input.InputAction act in actions)
			{
				if(act.mDevice != Input.InputAction.DeviceType.ANALOG)
				{
					continue;
				}

				if(act.mAction.Equals(MyActions.PitchUp)
					|| act.mAction.Equals(MyActions.PitchDown)
					|| act.mAction.Equals(MyActions.Pitch))
				{
					act.mMultiplier	=-act.mMultiplier;
				}
			}

			int	turnSpeed	=settings.mTurnSensitivity;

			float	turnMultiplier	=turnSpeed / 5f;

			//square curve
			turnMultiplier	*=turnMultiplier;

			//turn scaling
			foreach(Input.InputAction act in actions)
			{
				if(act.mAction.Equals(MyActions.Turn)
					|| act.mAction.Equals(MyActions.TurnLeft)
					|| act.mAction.Equals(MyActions.TurnRight)
					|| act.mAction.Equals(MyActions.PitchUp)
					|| act.mAction.Equals(MyActions.PitchDown)
					|| act.mAction.Equals(MyActions.Pitch))
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
					act.mMultiplier	*=turnMultiplier;
				}
			}

			//axis inversion
			if(settings.mbYAxisInverted)
			{
				foreach(Input.InputAction act in actions)
				{
					if(act.mAction.Equals(MyActions.PitchUp)
						|| act.mAction.Equals(MyActions.PitchDown)
						|| act.mAction.Equals(MyActions.Pitch))
					{
						act.mMultiplier	=-act.mMultiplier;
					}
				}
			}

			if(settings.mbXAxisInverted)
			{
				foreach(Input.InputAction act in actions)
				{
					if(act.mAction.Equals(MyActions.TurnLeft)
						|| act.mAction.Equals(MyActions.TurnRight)
						|| act.mAction.Equals(MyActions.Turn))
					{
						act.mMultiplier	=-act.mMultiplier;
					}
				}
			}
		}

		static List<Input.InputAction> UpdateInput(
			Input inp, GraphicsDevice gd, float delta)
		{
			List<Input.InputAction>	actions	=inp.GetAction();

			//delta scale analogs, since there's no timestamp stuff in gamepad code
			foreach(Input.InputAction act in actions)
			{
				if(!act.mbTime && act.mDevice == Input.InputAction.DeviceType.ANALOG)
				{
					//analog needs a time scale applied
					act.mMultiplier	*=delta;
				}
			}
			return	actions;
		}

		static void UpdateMouseLook(List<Input.InputAction> actions,
			Input inp, GraphicsDevice gd, ref bool bMouseLookOn)
		{
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
		}

		static void MenuBindings(Input inp, bool bMenuOn)
		{
			//clear existing
			inp.UnMapAction(System.Windows.Forms.Keys.Up, Modifiers.None);
			inp.UnMapAction(System.Windows.Forms.Keys.Down, Modifiers.None);
			inp.UnMapAction(System.Windows.Forms.Keys.Left, Modifiers.None);
			inp.UnMapAction(System.Windows.Forms.Keys.Right, Modifiers.None);
			inp.UnMapAxisAction(Input.MoveAxis.GamePadLeftXAxis);
			inp.UnMapAxisAction(Input.MoveAxis.GamePadLeftYAxis);

			if(bMenuOn)
			{
				inp.MapAction(MyActions.MenuUp, ActionTypes.ContinuousHold,
					Modifiers.None, System.Windows.Forms.Keys.Up);
				inp.MapAction(MyActions.MenuDown, ActionTypes.ContinuousHold,
					Modifiers.None, System.Windows.Forms.Keys.Down);
				inp.MapAction(MyActions.MenuLeft, ActionTypes.ContinuousHold,
					Modifiers.None, System.Windows.Forms.Keys.Left);
				inp.MapAction(MyActions.MenuRight, ActionTypes.ContinuousHold,
					Modifiers.None, System.Windows.Forms.Keys.Right);

				inp.MapAxisAction(MyActions.MenuUpDown, Input.MoveAxis.GamePadLeftYAxis);
				inp.MapAxisAction(MyActions.MenuLeftRight, Input.MoveAxis.GamePadLeftXAxis);
			}
			else
			{
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

				inp.MapAxisAction(MyActions.MoveLeftRight, Input.MoveAxis.GamePadLeftXAxis);
				inp.MapAxisAction(MyActions.MoveForwardBack, Input.MoveAxis.GamePadLeftYAxis);
			}
		}

		static void SwitchKeyControls(Input inp, bool bESDF)
		{
			if(bESDF)
			{
				inp.UnMapAction(System.Windows.Forms.Keys.W, Modifiers.None);
				inp.UnMapAction(System.Windows.Forms.Keys.A, Modifiers.None);
				inp.UnMapAction(System.Windows.Forms.Keys.S, Modifiers.None);
				inp.UnMapAction(System.Windows.Forms.Keys.D, Modifiers.None);
				inp.UnMapAction(System.Windows.Forms.Keys.W, Modifiers.ShiftHeld);
				inp.UnMapAction(System.Windows.Forms.Keys.A, Modifiers.ShiftHeld);
				inp.UnMapAction(System.Windows.Forms.Keys.S, Modifiers.ShiftHeld);
				inp.UnMapAction(System.Windows.Forms.Keys.D, Modifiers.ShiftHeld);

				inp.MapAction(MyActions.MoveForward, ActionTypes.ContinuousHold,
					Modifiers.None, System.Windows.Forms.Keys.E);
				inp.MapAction(MyActions.MoveLeft, ActionTypes.ContinuousHold,
					Modifiers.None, System.Windows.Forms.Keys.S);
				inp.MapAction(MyActions.MoveBack, ActionTypes.ContinuousHold,
					Modifiers.None, System.Windows.Forms.Keys.D);
				inp.MapAction(MyActions.MoveRight, ActionTypes.ContinuousHold,
					Modifiers.None, System.Windows.Forms.Keys.F);
				inp.MapAction(MyActions.MoveForwardFast, ActionTypes.ContinuousHold,
					Modifiers.ShiftHeld, System.Windows.Forms.Keys.E);
				inp.MapAction(MyActions.MoveLeftFast, ActionTypes.ContinuousHold,
					Modifiers.ShiftHeld, System.Windows.Forms.Keys.S);
				inp.MapAction(MyActions.MoveBackFast, ActionTypes.ContinuousHold,
					Modifiers.ShiftHeld, System.Windows.Forms.Keys.D);
				inp.MapAction(MyActions.MoveRightFast, ActionTypes.ContinuousHold,
					Modifiers.ShiftHeld, System.Windows.Forms.Keys.F);
			}
			else
			{
				inp.UnMapAction(System.Windows.Forms.Keys.E, Modifiers.None);
				inp.UnMapAction(System.Windows.Forms.Keys.S, Modifiers.None);
				inp.UnMapAction(System.Windows.Forms.Keys.D, Modifiers.None);
				inp.UnMapAction(System.Windows.Forms.Keys.F, Modifiers.None);
				inp.UnMapAction(System.Windows.Forms.Keys.E, Modifiers.ShiftHeld);
				inp.UnMapAction(System.Windows.Forms.Keys.S, Modifiers.ShiftHeld);
				inp.UnMapAction(System.Windows.Forms.Keys.D, Modifiers.ShiftHeld);
				inp.UnMapAction(System.Windows.Forms.Keys.F, Modifiers.ShiftHeld);

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
				inp.MapAction(MyActions.MoveLeftFast, ActionTypes.ContinuousHold,
					Modifiers.ShiftHeld, System.Windows.Forms.Keys.A);
				inp.MapAction(MyActions.MoveBackFast, ActionTypes.ContinuousHold,
					Modifiers.ShiftHeld, System.Windows.Forms.Keys.S);
				inp.MapAction(MyActions.MoveRightFast, ActionTypes.ContinuousHold,
					Modifiers.ShiftHeld, System.Windows.Forms.Keys.D);
			}
		}

		static Input SetUpInput()
		{
			Input	inp	=new InputLib.Input(1f / Stopwatch.Frequency);
		
			inp.MapAction(MyActions.PitchUp, ActionTypes.ContinuousHold, Modifiers.None, 16);
			inp.MapAction(MyActions.PitchDown, ActionTypes.ContinuousHold, Modifiers.None, 18);

			inp.MapToggleAction(MyActions.ToggleMouseLookOn,
				MyActions.ToggleMouseLookOff, Modifiers.None,
				Input.VariousButtons.RightMouseButton);

			inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.GamePadRightYAxis);
			inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.GamePadRightXAxis);

			//mouseselect
			inp.MapAction(MyActions.MouseSelect, ActionTypes.PressAndRelease,
				Modifiers.None, Input.VariousButtons.LeftMouseButton);

			//menu stuff for keyboard
			inp.MapAction(MyActions.MenuSelect, ActionTypes.PressAndRelease,
				Modifiers.None, System.Windows.Forms.Keys.Enter);
			inp.MapAction(MyActions.ExitMenu, ActionTypes.PressAndRelease,
				Modifiers.None, System.Windows.Forms.Keys.Escape);

			//menu stuff for dpad
			inp.MapAction(MyActions.MenuUp, ActionTypes.ContinuousHold,
				Modifiers.None, Input.VariousButtons.GamePadDPadUp);
			inp.MapAction(MyActions.MenuDown, ActionTypes.ContinuousHold,
				Modifiers.None, Input.VariousButtons.GamePadDPadDown);
			inp.MapAction(MyActions.MenuLeft, ActionTypes.ContinuousHold,
				Modifiers.None, Input.VariousButtons.GamePadDPadLeft);
			inp.MapAction(MyActions.MenuRight, ActionTypes.ContinuousHold,
				Modifiers.None, Input.VariousButtons.GamePadDPadRight);
			inp.MapAction(MyActions.MenuSelect, ActionTypes.PressAndRelease,
				Modifiers.None, Input.VariousButtons.GamePadA);
			inp.MapAction(MyActions.ExitMenu, ActionTypes.PressAndRelease,
				Modifiers.None, Input.VariousButtons.GamePadBack);

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
	}
}
