using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MeshLib;
using UtilityLib;
using MaterialLib;
using InputLib;
using AudioLib;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.X3DAudio;

using MatLib	=MaterialLib.MaterialLib;


namespace TestUI
{
	class MainMenu
	{
		//reference for querying 360 pads
		Input	mInput;

		//user preferences reference
		UserSettings	mUserSettings;

		//menu data
		Mover2	mHighLightMover		=new Mover2();
		int		mHighLightPosition	=0;
		int		mNumMenuStops		=3;
		int		mMenuPage			=0;
		bool	mbMenuOn			=true;
		bool	mbDelaying			=false;	//delay between actions
		float	mDelayTime			=0;
		float	mDelayTarget		=0;	//amount to delay

		//list of current menu choices
		List<string>	mPageChoices	=new List<string>();

		//fontery
		ScreenText		mST;
		MatLib			mFontMats;
		Matrix			mTextProj;
		int				mResX, mResY;
		List<string>	mFonts	=new List<string>();

		//2d stuff
		ScreenUI	mSUI;

		//shader compile progress indicator
		SharedForms.ThreadedProgress	mSProg;

		//gpu
		GraphicsDevice	mGD;

		//events
		internal event EventHandler	eESDF;

		//constants
		const int	VerticalMenuSpace	=100;
		const int	FontYFudge			=5;
		const float	MenuFontScale		=2f;
		const float	MenuTravelTime		=0.1f;
		const float	MenuEaseIn			=0.2f;
		const float	MenuEaseOut			=0.2f;
		const float	HighLightMoveDelay	=0.05f;	//time between up/down movements
		const float	MenuAdjustDelay		=0.2f;	//time between adjustments
		const int	HighLightWidth		=768;
		const int	HighLightHeight		=101;


		internal MainMenu(GraphicsDevice gd, StuffKeeper sk,
			Input inp, UserSettings settings)
		{
			mGD				=gd;
			mInput			=inp;
			mUserSettings	=settings;

			mResX	=gd.RendForm.ClientRectangle.Width;
			mResY	=gd.RendForm.ClientRectangle.Height;

			mFontMats	=new MatLib(gd, sk);

			mFonts	=sk.GetFontList();

			mFontMats.CreateMaterial("Text");
			mFontMats.SetMaterialEffect("Text", "2D.fx");
			mFontMats.SetMaterialTechnique("Text", "Text");

			string	bigFont	=mFonts[mFonts.Count - 1];
			string	lilFont	=mFonts[0];

			mST		=new ScreenText(gd.GD, mFontMats, bigFont, 1000);
			mSUI	=new ScreenUI(gd.GD, mFontMats, 100);

			mTextProj	=Matrix.OrthoOffCenterLH(0, mResX, mResY, 0, 0.1f, 5f);

			mSUI.AddGump("UI\\HighLight", "HighLight", Vector4.One,
				Vector2.UnitX * ((mResX / 2) - 384)
				+ Vector2.UnitY * ((mResY / 2) - 50)
				- Vector2.UnitY * VerticalMenuSpace,
				Vector2.One);

			SetMenuPage(0);

			//string indicators for various statusy things
			mST.AddString(lilFont, "", "Status",
				Vector4.One, Vector2.UnitX * 20f + Vector2.UnitY * 460f, Vector2.One);
		}


		internal bool IsActive()
		{
			return	mbMenuOn;
		}


		internal void Update(UpdateTimer time, List<Input.InputAction> actions)
		{
			float	secDelta	=time.GetUpdateDeltaSeconds();

			if(mbDelaying)
			{
				mDelayTime	+=secDelta;
				if(mDelayTime > mDelayTarget)
				{
					mbDelaying	=false;
					mDelayTime	=0;
				}
			}

			if(mbDelaying)
			{
				mST.Update(mGD.DC);
				mSUI.Update(mGD.DC);
				return;
			}

			//check for keys
			foreach(Input.InputAction act in actions)
			{
				if(act.mAction.Equals(Program.MyActions.MenuUp))
				{
					MoveHighLight(true);
				}
				else if(act.mAction.Equals(Program.MyActions.MenuDown))
				{
					MoveHighLight(false);
				}
				else if(act.mAction.Equals(Program.MyActions.MenuLeft))
				{
					Adjust(true);
				}
				else if(act.mAction.Equals(Program.MyActions.MenuRight))
				{
					Adjust(false);
				}
				else if(act.mAction.Equals(Program.MyActions.MenuSelect))
				{
					Select();
				}
				else if(act.mAction.Equals(Program.MyActions.MenuUpDown))
				{
					MoveHighLight(act.mMultiplier > 0f);
				}
				else if(act.mAction.Equals(Program.MyActions.MenuLeftRight))
				{
					Adjust(act.mMultiplier < 0f);
				}
				else if(act.mAction.Equals(Program.MyActions.ExitMenu))
				{
					UpMenu();
				}
			}

			if(!mHighLightMover.Done())
			{
				Debug.WriteLine("Delta:" + secDelta);
				mHighLightMover.Update(secDelta);

				mSUI.ModifyGumpPosition("HighLight", mHighLightMover.GetPos());

				if(mHighLightMover.Done())
				{
					mbDelaying		=true;
					mDelayTarget	=HighLightMoveDelay;
				}
			}
			
			//this has to behind any text changes
			//otherwise the offsets will be messed up
			mST.Update(mGD.DC);
			mSUI.Update(mGD.DC);
		}


		internal void Render(DeviceContext dc)
		{
			if(mbMenuOn)
			{
				mSUI.Draw(dc, Matrix.Identity, mTextProj);
				mST.Draw(dc, Matrix.Identity, mTextProj);
			}
		}


		internal void FreeAll()
		{
			mSUI.FreeAll();
			mST.FreeAll();

			mGD	=null;
		}


		Vector2	PositionString(string fontName, string theString)
		{
			Vector2	size	=mST.MeasureString(fontName, theString);

			//scale
			size	*=MenuFontScale;

			//center
			Vector2	pos;
			pos.X	=(mResX - size.X) * 0.5f;
			pos.Y	=(mResY - size.Y) * 0.5f;

			//adjust down
			pos.Y	+=FontYFudge * MenuFontScale;

			return	pos;
		}


		Vector2	GetHighLightPos()
		{
			Vector2	middle	=Vector2.UnitX * ((mResX - HighLightWidth) * 0.5f)
				+ Vector2.UnitY * ((mResY - HighLightHeight) * 0.5f);

			Vector2	top	=Vector2.UnitY * (VerticalMenuSpace * ((mNumMenuStops - 1) / 2));

			Vector2	ret	=middle - top + Vector2.UnitY * VerticalMenuSpace * mHighLightPosition;

			return	ret;
		}


		void SetMenuPage(int page)
		{
			string	bigFont	=mFonts[mFonts.Count - 1];

			mPageChoices.Clear();

			if(page == 0)
			{
				//nuke other pages if there
				mST.DeleteString("Controls");
				mST.DeleteString("Video");
				mST.DeleteString("Exit");
				mST.DeleteString("Sensitivity");
				mST.DeleteString("XAxis");
				mST.DeleteString("YAxis");
				mST.DeleteString("MSamp");
				mST.DeleteString("Windowed");
				for(int i=0;i < 6;i++)
				{
					mST.DeleteString("Control" + i);
				}

				//main
				mST.AddString(bigFont, "Controls", "Controls", Vector4.One,
					PositionString(bigFont, "Controls") -
					Vector2.UnitY * VerticalMenuSpace,
					Vector2.One * MenuFontScale);

				mST.AddString(bigFont, "Video", "Video", Vector4.One,
					PositionString(bigFont, "Video"),
					Vector2.One * MenuFontScale);

				mST.AddString(bigFont, "Exit", "Exit", Vector4.One,
					PositionString(bigFont, "Exit") +
					Vector2.UnitY * VerticalMenuSpace,
					Vector2.One * MenuFontScale);

				mNumMenuStops		=3;
				mHighLightPosition	=0;
				mMenuPage			=0;

				//pop to
				mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());

				mPageChoices.Add("Controls");
				mPageChoices.Add("Video");
				mPageChoices.Add("Exit");
			}
			else if(page == 1)
			{
				//controls
				mST.DeleteString("Controls");
				mST.DeleteString("Video");
				mST.DeleteString("Exit");
				mST.DeleteString("Sensitivity");
				mST.DeleteString("XAxis");
				mST.DeleteString("YAxis");
				mST.DeleteString("MSamp");
				mST.DeleteString("Windowed");
				for(int i=0;i < 6;i++)
				{
					mST.DeleteString("Control" + i);
				}

				List<string>	choices	=new List<string>();

				for(int i=0;i < 4;i++)
				{
					if(mInput.IsXControllerConnected(i))
					{
						choices.Add("360Pad-Port" + i);
					}
				}

				if(mUserSettings.mbESDF)
				{
					choices.Add("WASD + Mouse");
					choices.Add("(ESDF + Mouse)");
				}
				else
				{
					choices.Add("(WASD + Mouse)");
					choices.Add("ESDF + Mouse");
				}

				mNumMenuStops	=3 + choices.Count;

				Vector2	top	=Vector2.UnitY * (VerticalMenuSpace * ((mNumMenuStops - 1) / 2));

				string	sens	="Sensitivity - " + mUserSettings.mTurnSensitivity;
				string	yaxis	="Y-Axis " + ((mUserSettings.mbYAxisInverted)? "Invert" : "Normal");
				string	xaxis	="X-Axis " + ((mUserSettings.mbXAxisInverted)? "Invert" : "Normal");

				mST.AddString(bigFont, sens,
					"Sensitivity", Vector4.One,
					PositionString(bigFont, sens) - top,
					Vector2.One * MenuFontScale);

				mST.AddString(bigFont, xaxis,
					"XAxis", Vector4.One,
					PositionString(bigFont, xaxis) - top +
					Vector2.UnitY * VerticalMenuSpace,
					Vector2.One * MenuFontScale);

				mST.AddString(bigFont, yaxis,
					"YAxis", Vector4.One,
					PositionString(bigFont, yaxis) - top +
					Vector2.UnitY * (VerticalMenuSpace * 2),
					Vector2.One * MenuFontScale);

				mPageChoices.Add("Sensitivity");
				mPageChoices.Add("XAxis");
				mPageChoices.Add("YAxis");

				int	idx	=0;
				foreach(string choice in choices)
				{
					mST.AddString(bigFont, choice, "Control" + idx, Vector4.One,
						PositionString(bigFont, choice) - top +
						Vector2.UnitY * VerticalMenuSpace * (idx + 3),
						Vector2.One * MenuFontScale);
					idx++;

					mPageChoices.Add(choice);
				}

				mHighLightPosition	=0;
				mMenuPage			=1;

				//pop to
				mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
			}
			else if(page == 2)
			{
				//video
				mST.DeleteString("Controls");
				mST.DeleteString("Video");
				mST.DeleteString("Exit");
				mST.DeleteString("MSamp");
				mST.DeleteString("Windowed");

				mNumMenuStops		=2;
				mMenuPage			=2;
				mHighLightPosition	=0;

				Vector2	top	=Vector2.UnitY * (VerticalMenuSpace * ((mNumMenuStops - 1) / 2));

				string	samp	="MultSamp " + ((mUserSettings.mbMultiSampling)? "On" : "Off");
				string	fs		="" + ((mUserSettings.mbFullScreen)? "Fullscreen" : "Windowed");

				mST.AddString(bigFont, samp, "MSamp", Vector4.One,
					PositionString(bigFont, samp) - top,
					Vector2.One * MenuFontScale);

				mST.AddString(bigFont, fs, "Windowed", Vector4.One,
					PositionString(bigFont, fs) - top +
					Vector2.UnitY * VerticalMenuSpace,
					Vector2.One * MenuFontScale);

				//pop to
				mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());

				mPageChoices.Add("MSamp");
				mPageChoices.Add("Windowed");
			}
			mbDelaying		=true;
			mDelayTarget	=HighLightMoveDelay;
		}


		void UpMenu()
		{
			if(!mHighLightMover.Done())
			{
				return;
			}

			if(!mbMenuOn)
			{
				mbMenuOn	=true;
				SetMenuPage(0);
				return;
			}

			if(mMenuPage == 0)
			{
				mbMenuOn		=!mbMenuOn;
				mbDelaying		=true;
				mDelayTarget	=HighLightMoveDelay;
			}
			else if(mMenuPage == 1 || mMenuPage == 2)
			{
				//switch to main
				SetMenuPage(0);
			}
		}


		void Adjust(bool bLeft)
		{
			if(!mHighLightMover.Done() || !mbMenuOn)
			{
				return;
			}

			string	bigFont	=mFonts[mFonts.Count - 1];

			if(mMenuPage == 0)
			{
				//nothing to adjust on main
				return;
			}

			if(mMenuPage == 1)
			{
				//settings menu
				if(mHighLightPosition == 0)
				{
					//adjust sens
					if(bLeft && (mUserSettings.mTurnSensitivity > 1))
					{
						mUserSettings.mTurnSensitivity--;
					}
					else if(!bLeft && (mUserSettings.mTurnSensitivity < 10))
					{
						mUserSettings.mTurnSensitivity++;
					}
					SetMenuPage(1);
				}
				else if(mHighLightPosition == 1)
				{
					//adjust x-axis
					mUserSettings.mbXAxisInverted	=!mUserSettings.mbXAxisInverted;
					SetMenuPage(1);
					mHighLightPosition	=1;
					mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
				}
				else if(mHighLightPosition == 2)
				{
					//adjust y-axis
					mUserSettings.mbYAxisInverted	=!mUserSettings.mbYAxisInverted;
					SetMenuPage(1);
					mHighLightPosition	=2;
					mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
				}
				else
				{
					//rest are control choices
					//maybe active in green with the key/mouse choice in ()?
					string	cont	=mPageChoices[mHighLightPosition];

					if(!cont.StartsWith("("))
					{
						if(cont.Contains("WASD"))
						{
							mUserSettings.mbESDF	=false;
							Misc.SafeInvoke(eESDF, false);
							int	hlp	=mHighLightPosition;
							SetMenuPage(1);
							mHighLightPosition	=hlp;
							mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
						}
						else if(cont.Contains("ESDF"))
						{
							mUserSettings.mbESDF	=true;
							Misc.SafeInvoke(eESDF, true);
							int	hlp	=mHighLightPosition;
							SetMenuPage(1);
							mHighLightPosition	=hlp;
							mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
						}
					}
				}
			}
			else if(mMenuPage == 2)
			{
				//video menu
				if(mHighLightPosition == 0)
				{
					//adjust msamp
					mUserSettings.mbMultiSampling	=!mUserSettings.mbMultiSampling;
					SetMenuPage(2);
				}
				else if(mHighLightPosition == 1)
				{
					//adjust fullscreen/windowed
					mUserSettings.mbFullScreen	=!mUserSettings.mbFullScreen;
					SetMenuPage(2);
					mHighLightPosition	=1;
					mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
					mGD.SetFullScreen(mUserSettings.mbFullScreen);
				}
			}
			mbDelaying		=true;
			mDelayTarget	=MenuAdjustDelay;
		}


		void Select()
		{
			if(!mHighLightMover.Done() || !mbMenuOn)
			{
				return;
			}

			string	bigFont	=mFonts[mFonts.Count - 1];

			if(mMenuPage == 0)
			{
				//main top menu
				if(mHighLightPosition == 0)
				{
					//switch to controls page
					SetMenuPage(1);
				}
				else if(mHighLightPosition == 1)
				{
					//Video
					SetMenuPage(2);
				}
				else if(mHighLightPosition == 2)
				{
					//exit
					mGD.RendForm.Close();
				}
				return;
			}
			else if(mMenuPage == 1)
			{
				//controls menu
				if(mHighLightPosition == 0)
				{
					//adjust sens
					mUserSettings.mTurnSensitivity++;
					if(mUserSettings.mTurnSensitivity > 10)
					{
						mUserSettings.mTurnSensitivity	=1;
					}
					SetMenuPage(1);
				}
				else if(mHighLightPosition == 1)
				{
					//adjust x-axis
					mUserSettings.mbXAxisInverted	=!mUserSettings.mbXAxisInverted;
					SetMenuPage(1);
					mHighLightPosition	=1;
					mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
				}
				else if(mHighLightPosition == 2)
				{
					//adjust y-axis
					mUserSettings.mbYAxisInverted	=!mUserSettings.mbYAxisInverted;
					SetMenuPage(1);
					mHighLightPosition	=2;
					mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
				}
				else
				{
					//rest are control choices
					//maybe active in green with the key/mouse choice in ()?
					string	cont	=mPageChoices[mHighLightPosition];

					if(!cont.StartsWith("("))
					{
						if(cont.Contains("WASD"))
						{
							mUserSettings.mbESDF	=false;
							Misc.SafeInvoke(eESDF, false);
							int	hlp	=mHighLightPosition;
							SetMenuPage(1);
							mHighLightPosition	=hlp;
							mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
						}
						else if(cont.Contains("ESDF"))
						{
							mUserSettings.mbESDF	=true;
							Misc.SafeInvoke(eESDF, true);
							int	hlp	=mHighLightPosition;
							SetMenuPage(1);
							mHighLightPosition	=hlp;
							mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
						}
					}
				}
			}
			else if(mMenuPage == 2)
			{
				//video menu
				if(mHighLightPosition == 0)
				{
					//adjust msamp
					mUserSettings.mbMultiSampling	=!mUserSettings.mbMultiSampling;
					SetMenuPage(2);
				}
				else if(mHighLightPosition == 1)
				{
					//adjust fullscreen/windowed
					mUserSettings.mbFullScreen	=!mUserSettings.mbFullScreen;
					SetMenuPage(2);
					mHighLightPosition	=1;
					mSUI.ModifyGumpPosition("HighLight", GetHighLightPos());
					mGD.SetFullScreen(mUserSettings.mbFullScreen);
				}
			}
			mbDelaying		=true;
			mDelayTarget	=MenuAdjustDelay;
		}


		void MoveHighLight(bool bUp)
		{
			if(!mHighLightMover.Done() || !mbMenuOn)
			{
				return;
			}

			if(bUp)
			{
				if(mHighLightPosition <= 0)
				{
					return;
				}
			}
			else
			{
				if(mHighLightPosition >= (mNumMenuStops - 1))
				{
					return;
				}
			}

			Vector2	curPos	=GetHighLightPos();

			if(bUp)
			{
				mHighLightPosition--;
			}
			else
			{
				mHighLightPosition++;
			}

			Vector2	nextPos	=GetHighLightPos();

			mHighLightMover.SetUpMove(curPos, nextPos,
				MenuTravelTime, MenuEaseIn, MenuEaseOut);
		}


		void OnCompilesNeeded(object sender, EventArgs ea)
		{
			Thread	uiThread	=new Thread(() =>
				{
					mSProg	=new SharedForms.ThreadedProgress("Compiling Shaders...");
					System.Windows.Forms.Application.Run(mSProg);
				});

			uiThread.SetApartmentState(ApartmentState.STA);
			uiThread.Start();

			while(mSProg == null)
			{
				Thread.Sleep(0);
			}

			mSProg.SetSizeInfo(0, (int)sender);
		}


		void OnCompileDone(object sender, EventArgs ea)
		{
			mSProg.SetCurrent((int)sender);

			if((int)sender == mSProg.GetMax())
			{
				mSProg.Nuke();
			}
		}
	}
}
