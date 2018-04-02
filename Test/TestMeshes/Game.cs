﻿using System;
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


namespace TestMeshes
{
	class Game
	{
		//data
		string		mGameRootDir;
		StuffKeeper	mSKeeper;

		Random	mRand	=new Random();

		//helpers
		IDKeeper	mKeeper	=new IDKeeper();

		//static stuff
		MatLib						mStaticMats;
		Dictionary<string, IArch>	mStatics	=new Dictionary<string, IArch>();
		List<StaticMesh>			mMeshes		=new List<StaticMesh>();

		//static transform details
		List<Vector3>	mMeshPositions	=new List<Vector3>();
		List<Vector3>	mMeshRotations	=new List<Vector3>();
		List<Vector3>	mMeshScales		=new List<Vector3>();

		//submesh bounds
		List<Dictionary<Mesh, BoundingBox>>	mMeshBounds	=new List<Dictionary<Mesh, BoundingBox>>();

		//test characters
		Dictionary<string, IArch>		mCharArchs	=new Dictionary<string, IArch>();
		Dictionary<Character, IArch>	mCharToArch	=new Dictionary<Character,IArch>();
		List<Character>					mCharacters	=new List<Character>();
		List<string>					mAnims		=new List<string>();
		float[]							mAnimTimes;
		int[]							mCurAnims;
		MatLib							mCharMats;
		AnimLib							mCharAnims;
		int								mCurChar;
		float							mInvertInterval	=0.15f;	//default 150ms

		//fontery
		ScreenText		mST;
		MatLib			mFontMats;
		Matrix			mTextProj;
		int				mResX, mResY;
		List<string>	mFonts	=new List<string>();

		//2d stuff
		ScreenUI	mSUI;

		//gpu
		GraphicsDevice	mGD;

		//collision debuggery
		CommonPrims	mCPrims;
		Vector4		mHitColor, mTextColor;
		int			mFrameCheck, mCurStatic;
		int[]		mCBone;
		Mesh		mPartHit;
		StaticMesh	mMeshHit;

		//collision bones
		Dictionary<int, Matrix>[]	mCBones;

		//constants
		const float	InvertIncrement	=0.01f;	//10ms


		internal Game(GraphicsDevice gd, string gameRootDir)
		{
			mGD				=gd;
			mGameRootDir	=gameRootDir;
			mResX			=gd.RendForm.ClientRectangle.Width;
			mResY			=gd.RendForm.ClientRectangle.Height;

			mSKeeper	=new StuffKeeper();

			mSKeeper.eCompileNeeded	+=SharedForms.ShaderCompileHelper.CompileNeededHandler;
			mSKeeper.eCompileDone	+=SharedForms.ShaderCompileHelper.CompileDoneHandler;

			mSKeeper.Init(mGD, gameRootDir);

			mFontMats	=new MatLib(gd, mSKeeper);
			mCPrims		=new CommonPrims(gd, mSKeeper);

			mFonts	=mSKeeper.GetFontList();

			mFontMats.CreateMaterial("Text");
			mFontMats.SetMaterialEffect("Text", "2D.fx");
			mFontMats.SetMaterialTechnique("Text", "Text");

			mST		=new ScreenText(gd.GD, mFontMats, mFonts[0], 1000);
			mSUI	=new ScreenUI(gd.GD, mFontMats, 100);

			mTextProj	=Matrix.OrthoOffCenterLH(0, mResX, mResY, 0, 0.1f, 5f);

			//load avail static stuff
			if(Directory.Exists(mGameRootDir + "/Statics"))
			{
				DirectoryInfo	di	=new DirectoryInfo(mGameRootDir + "/Statics");

				FileInfo[]	fi	=di.GetFiles("*.MatLib", SearchOption.TopDirectoryOnly);

				if(fi.Length > 0)
				{
					mStaticMats	=new MatLib(gd, mSKeeper);
					mStaticMats.ReadFromFile(fi[0].DirectoryName + "\\" + fi[0].Name);

					mStaticMats.InitCelShading(1);
					mStaticMats.GenerateCelTexturePreset(gd.GD,
						(gd.GD.FeatureLevel == FeatureLevel.Level_9_3),
						false, 0);
					mStaticMats.SetCelTexture(0);
					mKeeper.AddLib(mStaticMats);
				}

				mStatics	=Mesh.LoadAllStaticMeshes(mGameRootDir + "\\Statics", gd.GD);

				foreach(KeyValuePair<string, IArch> arch in mStatics)
				{
					arch.Value.UpdateBounds();
				}

				fi	=di.GetFiles("*.StaticInstance", SearchOption.TopDirectoryOnly);
				foreach(FileInfo f in fi)
				{
					string	archName	=FileUtil.StripExtension(f.Name);
					if(archName.Contains('_'))
					{
						archName	=archName.Substring(0, f.Name.IndexOf('_'));
					}

					archName	+=".Static";

					if(!mStatics.ContainsKey(archName))
					{
						continue;
					}

					StaticMesh	sm	=new StaticMesh(mStatics[archName]);

					sm.ReadFromFile(f.DirectoryName + "\\" + f.Name);

					mMeshes.Add(sm);

					sm.UpdateBounds();
					sm.SetMatLib(mStaticMats);
					Vector3	randPos	=Mathery.RandomPosition(mRand,
							Vector3.UnitX * 100f +
							Vector3.UnitZ * 100f);
					mMeshPositions.Add(randPos);
					mMeshRotations.Add(Vector3.Zero);
					mMeshScales.Add(Vector3.One);
					UpdateStaticTransform(mMeshes.Count - 1);
				}
				AddStaticCollision();
			}

			//skip hair stuff when computing bone bounds
			//hits to hair usually wouldn't activate much
			List<string>	skipMats	=new List<string>();
			skipMats.Add("Hair");

			//load character stuff if any around
			if(Directory.Exists(mGameRootDir + "/Characters"))
			{
				DirectoryInfo	di	=new DirectoryInfo(mGameRootDir + "/Characters");

				FileInfo[]	fi	=di.GetFiles("*.AnimLib", SearchOption.TopDirectoryOnly);
				if(fi.Length > 0)
				{
					mCharAnims	=new AnimLib();
					mCharAnims.ReadFromFile(fi[0].DirectoryName + "\\" + fi[0].Name);

					List<Anim>	anims	=mCharAnims.GetAnims();
					foreach(Anim a in anims)
					{
						mAnims.Add(a.Name);
					}
				}

				fi	=di.GetFiles("*.MatLib", SearchOption.TopDirectoryOnly);
				if(fi.Length > 0)
				{
					mCharMats	=new MatLib(mGD, mSKeeper);
					mCharMats.ReadFromFile(fi[0].DirectoryName + "\\" + fi[0].Name);
					mCharMats.InitCelShading(1);
					mCharMats.GenerateCelTexturePreset(gd.GD,
						gd.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);
					mCharMats.SetCelTexture(0);
					mKeeper.AddLib(mCharMats);
				}

				fi	=di.GetFiles("*.Character", SearchOption.TopDirectoryOnly);
				foreach(FileInfo f in fi)
				{
					IArch	arch	=new CharacterArch();
					arch.ReadFromFile(f.DirectoryName + "\\" + f.Name, mGD.GD, true);

					mCharArchs.Add(FileUtil.StripExtension(f.Name), arch);
				}

				fi	=di.GetFiles("*.CharacterInstance", SearchOption.TopDirectoryOnly);
				foreach(FileInfo f in fi)
				{
					string	archName	=f.Name;
					if(archName.Contains('_'))
					{
						archName	=f.Name.Substring(0, f.Name.IndexOf('_'));
					}

					if(!mCharArchs.ContainsKey(archName))
					{
						continue;
					}

					Character	c	=new Character(mCharArchs[archName], mCharAnims);

					//map this to an arch
					mCharToArch.Add(c, mCharArchs[archName]);

					c.ReadFromFile(f.DirectoryName + "\\" + f.Name);

					c.SetMatLib(mCharMats);

					c.SetTransform(Matrix.Translation(
						Mathery.RandomPosition(mRand,
							Vector3.UnitX * 100f +
							Vector3.UnitZ * 100f)));

					c.ComputeBoneBounds(skipMats);

					c.AutoInvert(true, mInvertInterval);

					mCharacters.Add(c);
				}

				if(mCharacters.Count > 0)
				{
					mAnimTimes	=new float[mCharacters.Count];
					mCurAnims	=new int[mCharacters.Count];
					mCBone		=new int[mCharacters.Count];
					mCBones		=new Dictionary<int,Matrix>[mCharacters.Count];
				}

				foreach(KeyValuePair<string, IArch> arch in mCharArchs)
				{
					//build draw data for bone bounds
					(arch.Value as CharacterArch).BuildDebugBoundDrawData(mGD.GD, mCPrims);
				}
			}

			//typical material group for characters
			//or at least it works with the ones
			//I have right now
			//TODO: way to define these in the asset?
			List<string>	skinMats	=new List<string>();

			skinMats.Add("Face");
			skinMats.Add("Skin");
			skinMats.Add("EyeWhite");
			skinMats.Add("EyeLiner");
			skinMats.Add("IrisLeft");
			skinMats.Add("PupilLeft");
			skinMats.Add("IrisRight");
			skinMats.Add("PupilRight");
			skinMats.Add("Nails");
			mKeeper.AddMaterialGroup("SkinGroup", skinMats);

			mTextColor	=Vector4.UnitY + (Vector4.UnitW * 0.15f);
			mHitColor	=Vector4.One * 0.9f;
			mHitColor.Y	=mHitColor.Z	=0f;

			mSUI.AddGump("UI\\CrossHair", "CrossHair", Vector4.One,
				Vector2.UnitX * ((mResX / 2) - 16)
				+ Vector2.UnitY * ((mResY / 2) - 16),
				Vector2.One);

			//string indicators for various statusy things
			mST.AddString(mFonts[0], "", "StaticStatus",
				mTextColor, Vector2.UnitX * 20f + Vector2.UnitY * 460f, Vector2.One);
			mST.AddString(mFonts[0], "", "InvertStatus",
				mTextColor, Vector2.UnitX * 20f + Vector2.UnitY * 480f, Vector2.One);
			mST.AddString(mFonts[0], "", "AnimStatus",
				mTextColor, Vector2.UnitX * 20f + Vector2.UnitY * 500f, Vector2.One);
			mST.AddString(mFonts[0], "", "CharStatus",
				mTextColor, Vector2.UnitX * 20f + Vector2.UnitY * 520f, Vector2.One);
			mST.AddString(mFonts[0], "", "PosStatus",
				mTextColor, Vector2.UnitX * 20f + Vector2.UnitY * 540f, Vector2.One);
			mST.AddString(mFonts[0], "", "HitStatus",
				mTextColor, Vector2.UnitX * 20f + Vector2.UnitY * 560f, Vector2.One);
			mST.AddString(mFonts[0], "", "ThreadStatus",
				mTextColor, Vector2.UnitX * 20f + Vector2.UnitY * 580f, Vector2.One);

			UpdateCAStatus();
			UpdateInvertStatus();
			UpdateStaticStatus();
		}


		internal void Update(UpdateTimer time, List<Input.InputAction> actions)
		{
			mFrameCheck++;

			Vector3	startPos	=mGD.GCam.Position;
			Vector3	endPos		=startPos + mGD.GCam.Forward * -2000f;

			float	deltaMS		=time.GetUpdateDeltaMilliSeconds();
			float	deltaSec	=time.GetUpdateDeltaSeconds();

			//animate characters
			for(int i=0;i < mCharacters.Count;i++)
			{
				Character	c	=mCharacters[i];

				c.Update(deltaSec);

				float	totTime	=mCharAnims.GetAnimTime(mAnims[mCurAnims[i]]);
				float	strTime	=mCharAnims.GetAnimStartTime(mAnims[mCurAnims[i]]);
				float	endTime	=totTime + strTime;

				mAnimTimes[i]	+=deltaSec;
				if(mAnimTimes[i] > endTime)
				{
					mAnimTimes[i]	=strTime + (mAnimTimes[i] - endTime);
				}

				c.Animate(mAnims[mCurAnims[i]], mAnimTimes[i]);

				mCBones[i]	=(mCharToArch[c] as CharacterArch).GetBoneTransforms(mCharAnims.GetSkeleton());
			}

			//check for keys
			foreach(Input.InputAction act in actions)
			{
				if(act.mAction.Equals(Program.MyActions.NextCharacter))
				{
					mCurChar++;
					if(mCurChar >= mCharacters.Count)
					{
						mCurChar	=0;
					}
					UpdateCAStatus();
				}
				else if(act.mAction.Equals(Program.MyActions.NextStatic))
				{
					mCurStatic++;
					if(mCurStatic >= mMeshes.Count)
					{
						mCurStatic	=0;
					}
					UpdateStaticStatus();
				}
				else if(act.mAction.Equals(Program.MyActions.NextAnim))
				{
					if(mCharacters.Count > 0)
					{
						mCurAnims[mCurChar]++;
						if(mCurAnims[mCurChar] >= mAnims.Count)
						{
							mCurAnims[mCurChar]	=0;
						}
						UpdateCAStatus();
					}
				}
				else if(act.mAction.Equals(Program.MyActions.IncreaseInvertInterval))
				{
					mInvertInterval	+=InvertIncrement;
					foreach(Character c in mCharacters)
					{
						c.AutoInvert(true, mInvertInterval);
					}
					UpdateInvertStatus();
				}
				else if(act.mAction.Equals(Program.MyActions.DecreaseInvertInterval))
				{
					mInvertInterval	-=InvertIncrement;
					if(mInvertInterval < InvertIncrement)
					{
						mInvertInterval	=InvertIncrement;
					}
					foreach(Character c in mCharacters)
					{
						c.AutoInvert(true, mInvertInterval);
					}
					UpdateInvertStatus();
				}
				else if(act.mAction.Equals(Program.MyActions.RandRotateStatic))
				{
					if(mMeshes.Count > 0)
					{
						//make a random rotation
						mMeshRotations[mCurStatic]	=new Vector3(
							Mathery.RandomFloatNext(mRand, 0, MathUtil.TwoPi),
							Mathery.RandomFloatNext(mRand, 0, MathUtil.TwoPi),
							Mathery.RandomFloatNext(mRand, 0, MathUtil.TwoPi));
						UpdateStaticTransform(mCurStatic);
					}
				}
				else if(act.mAction.Equals(Program.MyActions.RandScaleStatic))
				{
					if(mMeshes.Count > 0)
					{
						//make a random scale
						mMeshScales[mCurStatic]	=new Vector3(
							Mathery.RandomFloatNext(mRand, 0.25f, 5f),
							Mathery.RandomFloatNext(mRand, 0.25f, 5f),
							Mathery.RandomFloatNext(mRand, 0.25f, 5f));
						UpdateStaticTransform(mCurStatic);
					}
				}
			}
			
			mPartHit	=null;
			mMeshHit	=null;

			float		bestDist	=float.MaxValue;
			for(int i=0;i < mMeshes.Count;i++)
			{
				StaticMesh	sm	=mMeshes[i];

				float	?bHit	=sm.RayIntersect(startPos, endPos, true);
				if(bHit != null)
				{
					Mesh	partHit	=null;

					bHit	=sm.RayIntersect(startPos, endPos, true, out partHit);
					if(bHit != null)
					{
						if(bHit.Value < bestDist)
						{
							bestDist	=bHit.Value;
							mPartHit	=partHit;
							mMeshHit	=sm;
						}
					}
				}
			}
			
			if(mStaticMats != null)
			{
				mStaticMats.UpdateWVP(Matrix.Identity, mGD.GCam.View, mGD.GCam.Projection, mGD.GCam.Position);
			}
			if(mCharMats != null)
			{
				mCharMats.UpdateWVP(Matrix.Identity, mGD.GCam.View, mGD.GCam.Projection, mGD.GCam.Position);
			}

			mCPrims.Update(mGD.GCam, Vector3.Down);
			
			for(int i=0;i < mCharacters.Count;i++)
			{
				Character	c	=mCharacters[i];
				float?	bHit	=c.RayIntersect(startPos, endPos);
				if(bHit != null)
				{
					c.RayIntersectBones(startPos, endPos, false, out mCBone[i]);
				}
				else
				{
					mCBone[i]	=0;
				}
			}

			if(mFrameCheck == 10)
			{
				mFrameCheck	=0;
				UpdateThreadStatus();
			}

			UpdatePosStatus();
			UpdateHitStatus();

			//this has to behind any text changes
			//otherwise the offsets will be messed up
			mST.Update(mGD.DC);
			mSUI.Update(mGD.DC);
		}


		internal void Render(DeviceContext dc)
		{
			foreach(Character c in mCharacters)
			{
				c.Draw(dc, mCharMats);
			}

			foreach(StaticMesh sm in mMeshes)
			{
				sm.Draw(dc, mStaticMats);
			}

			for(int i=0;i < mCharacters.Count;i++)
			{
				foreach(KeyValuePair<int, Matrix> bone in mCBones[i])
				{
					Matrix	boneTrans	=bone.Value;

					if(bone.Key == mCBone[i])
					{
						mCPrims.DrawBox(dc, bone.Key, boneTrans * mCharacters[i].GetTransform(), mHitColor);
					}
					else
					{
						mCPrims.DrawBox(dc, bone.Key, boneTrans * mCharacters[i].GetTransform(), Vector4.One * 0.5f);
					}
				}
			}

			int	idx	=10000;
			for(int i=0;i < mMeshes.Count;i++)
			{
				StaticMesh	sm	=mMeshes[i];

				Matrix	mat	=sm.GetTransform();

				Dictionary<Mesh, BoundingBox>	bnd	=mMeshBounds[i];

				foreach(KeyValuePair<Mesh, BoundingBox> b in bnd)
				{
					if(b.Key == mPartHit && mMeshHit == sm)
					{
						mCPrims.DrawBox(dc, idx++, b.Key.GetTransform() * mat, mHitColor);
					}
					else
					{
						mCPrims.DrawBox(dc, idx++, b.Key.GetTransform() * mat, Vector4.One * 0.5f);
					}
				}
			}

			mCPrims.DrawAxis(dc);

			mSUI.Draw(dc, Matrix.Identity, mTextProj);
			mST.Draw(dc, Matrix.Identity, mTextProj);
		}


		internal void FreeAll()
		{
			if(mStaticMats != null)
			{
				mStaticMats.FreeAll();
			}
			mKeeper.Clear();
			if(mCharMats != null)
			{
				mCharMats.FreeAll();
			}

			mSKeeper.FreeAll();
		}


		void UpdateStaticTransform(int index)
		{
			StaticMesh	sm	=mMeshes[index];

			Matrix	trans	=Matrix.Translation(mMeshPositions[index]);
			Matrix	rot		=Matrix.RotationYawPitchRoll(
				mMeshRotations[index].X,
				mMeshRotations[index].Y,
				mMeshRotations[index].Z);
			Matrix	scl		=Matrix.Scaling(
				mMeshScales[index].X,
				mMeshScales[index].Y,
				mMeshScales[index].Z);

			sm.SetTransform(scl * rot * trans);
		}


		void AddStaticCollision()
		{
			int	statIndex	=10000;
			foreach(StaticMesh sm in mMeshes)
			{
				Dictionary<Mesh, BoundingBox>	bd	=sm.GetBoundData();

				foreach(KeyValuePair<Mesh, BoundingBox> box in bd)
				{
					mCPrims.AddBox(mGD.GD, statIndex++, box.Value);
				}

				mMeshBounds.Add(bd);
			}
		}


		void UpdateStaticStatus()
		{
			mST.ModifyStringText(mFonts[0], "(,) CurStatic: " + mCurStatic
				+ " (Y) To Random Rotate, (U) To Randomly Scale", "StaticStatus");
		}


		void UpdateCAStatus()
		{
			if(mAnims.Count == 0 || mCharacters.Count == 0)
			{
				return;
			}
			mST.ModifyStringText(mFonts[0], "(C) CurCharacter: " + mCurChar, "CharStatus");
			mST.ModifyStringText(mFonts[0], "(N) CurAnim: " + mAnims[mCurAnims[mCurChar]], "AnimStatus");
		}


		void UpdatePosStatus()
		{
			mST.ModifyStringText(mFonts[0], "(WASD) :"
				+ (int)mGD.GCam.Position.X + ", "
				+ (int)mGD.GCam.Position.Y + ", "
				+ (int)mGD.GCam.Position.Z, "PosStatus");
		}


		void UpdateHitStatus()
		{
			bool	bAnyHit	=false;
			for(int i=0;i < mCharacters.Count;i++)
			{
				if(mCBone[i] > 0)
				{
					bAnyHit	=true;
				}
			}

			if(!bAnyHit)
			{
				mST.ModifyStringText(mFonts[0], "No Collisions", "HitStatus");
				mST.ModifyStringColor("HitStatus", mTextColor);
				return;
			}

			mST.ModifyStringColor("HitStatus", mHitColor);

			string	hitString	="Hit";
			for(int i=0;i < mCharacters.Count;i++)
			{
				if(mCBone[i] <= 0)
				{
					continue;
				}

				hitString	+=" Character " + i + " in bone " +
					mCharAnims.GetSkeleton().GetBoneName(mCBone[i]);
			}

			mST.ModifyStringText(mFonts[0], hitString, "HitStatus");
		}


		void UpdateThreadStatus()
		{
			string	threadString	="Thread Misses: ";
			for(int i=0;i < mCharacters.Count;i++)
			{
				threadString	+="Char" + i + ": " + mCharacters[i].GetThreadMisses() + " ";
			}
			mST.ModifyStringText(mFonts[0], threadString, "ThreadStatus");
		}


		void UpdateInvertStatus()
		{
			mST.ModifyStringText(mFonts[0], "(PGUP/PGDN) Invert Interval: " + mInvertInterval, "InvertStatus");
		}
	}
}
