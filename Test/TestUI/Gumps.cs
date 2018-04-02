using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.IO;

using MeshLib;
using UtilityLib;
using MaterialLib;
using AudioLib;
using InputLib;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using MatLib	=MaterialLib.MaterialLib;


namespace TestUI
{
	class Gumps
	{
		Random	mRand	=new Random();

		//2d stuff
		ScreenUI	mSUI;

		//Fonts / UI
		ScreenText		mST;
		MatLib			mFontMats;
		Matrix			mTextProj;
		Mover2			mTextMover	=new Mover2();
		List<string>	mFonts	=new List<string>();


		internal Gumps(GraphicsDevice gd, StuffKeeper sk)
		{
			int	resX	=gd.RendForm.ClientRectangle.Width;
			int	resY	=gd.RendForm.ClientRectangle.Height;

			mFontMats	=new MatLib(gd, sk);

			mFontMats.CreateMaterial("Text");
			mFontMats.SetMaterialEffect("Text", "2D.fx");
			mFontMats.SetMaterialTechnique("Text", "Text");

			mFonts	=sk.GetFontList();

			mST		=new ScreenText(gd.GD, mFontMats, mFonts[0], 1000);
			mSUI	=new ScreenUI(gd.GD, mFontMats, 100);

			mTextProj	=Matrix.OrthoOffCenterLH(0, resX, resY, 0, 0.1f, 5f);

			//grab UI textures to show how to do gumpery
			List<string>	texs	=sk.GetTexture2DList();
			List<string>	uiTex	=new List<string>();
			foreach(string tex in texs)
			{
				if(tex.StartsWith("UI"))
				{
					uiTex.Add(tex);
				}
			}

			//draw some ui bits
			if(uiTex.Count > 1)
			{
				int	idx	=0;
				foreach(string tex in uiTex)
				{
					int	randX	=mRand.Next(resX);
					int	randY	=mRand.Next(resY);

					mSUI.AddGump(tex, "TestGump" + idx, Vector4.One,
						Vector2.UnitX * randX + Vector2.UnitY * randY, Vector2.One);

					idx++;
				}
			}
		}


		internal void Update(DeviceContext dc)
		{
			mST.Update(dc);
			mSUI.Update(dc);
		}


		internal void Draw(DeviceContext dc)
		{
			mSUI.Draw(dc, Matrix.Identity, mTextProj);
			mST.Draw(dc, Matrix.Identity, mTextProj);
		}
	}
}
