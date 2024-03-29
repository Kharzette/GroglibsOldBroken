﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilityLib;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

using Device	=SharpDX.Direct3D11.Device;
using Cursor	=System.Windows.Forms.Cursor;


namespace UtilityLib
{
	public class GraphicsDevice
	{
		RenderForm	mRForm;

		Device			mGD;
		DeviceDebug		mGDD;
		DeviceContext	mDC;

		SwapChain	mSChain;

		Texture2D			mBackBuffer, mDepthBuffer;
		RenderTargetView	mBBView;
		DepthStencilView	mDSView;
		Viewport			mScreenPort, mShadowPort;

		GameCamera	mGCam;

		//keep near and far clip distances
		//need for camera rebuild on device lost or resize
		float	mClipNear, mClipFar;

		bool	mbResized;

		//keep track of mouse pos during mouse look
		System.Drawing.Point		mStoredMousePos	=System.Drawing.Point.Empty;
		System.Drawing.Rectangle	mStoredClipRect	=Cursor.Clip;

		public event EventHandler	ePreResize;
		public event EventHandler	eResized;


		public Device GD
		{
			get { return mGD; }
		}

		public DeviceContext DC
		{
			get { return mDC; }
		}

		public RenderForm RendForm
		{
			get { return mRForm; }
		}

		public GameCamera GCam
		{
			get { return mGCam; }
		}


		public GraphicsDevice(string formTitle, FeatureLevel flevel, float near, float far)
		{
			mRForm	=new RenderForm(formTitle);

			SwapChainDescription	scDesc	=new SwapChainDescription();

			scDesc.BufferCount			=1;
			scDesc.Flags				=SwapChainFlags.None;
			scDesc.IsWindowed			=true;
			scDesc.OutputHandle			=mRForm.Handle;
			scDesc.SampleDescription	=new SampleDescription(1, 0);
			scDesc.SwapEffect			=SwapEffect.Discard;
			scDesc.Usage				=Usage.RenderTargetOutput;
			scDesc.ModeDescription		=new ModeDescription(
				mRForm.ClientSize.Width, mRForm.ClientSize.Height,
				new Rational(60, 1), Format.R8G8B8A8_UNorm_SRgb);
			
			SharpDX.DXGI.Factory	fact	=new Factory();

			Adapter	adpt	=fact.GetAdapter(0);

			FeatureLevel	[]features	=new FeatureLevel[1];

			features[0]	=new FeatureLevel();

			features[0]	=flevel;

#if DEBUG
			Device.CreateWithSwapChain(adpt, DeviceCreationFlags.Debug, features,
				scDesc, out mGD, out mSChain);
			mGDD	=new DeviceDebug(mGD);
#else
			Device.CreateWithSwapChain(adpt, DeviceCreationFlags.None, features,
				scDesc, out mGD, out mSChain);
#endif


			adpt.Dispose();

			mDC	=mGD.ImmediateContext;

			//I always use this, hope it doesn't change somehow
			mDC.InputAssembler.PrimitiveTopology	=PrimitiveTopology.TriangleList;

			//Get the backbuffer from the swapchain
			mBackBuffer	=Texture2D.FromSwapChain<Texture2D>(mSChain, 0);

			//Renderview on the backbuffer
			mBBView	=new RenderTargetView(mGD, mBackBuffer);
			
			//Create the depth buffer
			Texture2DDescription	depthDesc	=new Texture2DDescription()
			{
				//pick depth format based on feature level
				Format				=(mGD.FeatureLevel != FeatureLevel.Level_9_3)?
										Format.D32_Float_S8X24_UInt : Format.D24_UNorm_S8_UInt,
				ArraySize			=1,
				MipLevels			=1,
				Width				=mRForm.ClientSize.Width,
				Height				=mRForm.ClientSize.Height,
				SampleDescription	=new SampleDescription(1, 0),
				Usage				=ResourceUsage.Default,
				BindFlags			=BindFlags.DepthStencil,
				CpuAccessFlags		=CpuAccessFlags.None,
				OptionFlags			=ResourceOptionFlags.None
			};

			mDepthBuffer	=new Texture2D(mGD, depthDesc);
			
			//Create the depth buffer view
			mDSView	=new DepthStencilView(mGD, mDepthBuffer);

			//screen viewport
			mScreenPort	=new Viewport(0, 0,
				mRForm.ClientRectangle.Width,
				mRForm.ClientRectangle.Height,
				0f, 1f);

			//shadow viewport
			mShadowPort	=new Viewport(0, 0,
				512, 512, 0f, 1f);

			//Setup targets and viewport for rendering
			mDC.Rasterizer.SetViewport(mScreenPort);

			mDC.OutputMerger.SetTargets(mDSView, mBBView);

			mRForm.UserResized	+=OnRenderFormResize;

			mClipNear	=near;
			mClipFar	=far;

			mGCam	=new UtilityLib.GameCamera(mRForm.ClientSize.Width,
				mRForm.ClientSize.Height, 16f/9f, near, far);
		}


		void HandleResize()
		{
			int	width	=mRForm.ClientRectangle.Width;
			int	height	=mRForm.ClientRectangle.Height;

			if(width == 0 || height == 0)
			{
				return;	//minimize?
			}

			//fire this event, and hopefully other code will
			//use it to let go of references to device stuff
			Misc.SafeInvoke(ePreResize, this);

			Utilities.Dispose(ref mBBView);
			Utilities.Dispose(ref mDSView);
			Utilities.Dispose(ref mBackBuffer);
			Utilities.Dispose(ref mDepthBuffer);

			DC.ClearState();

			DC.InputAssembler.PrimitiveTopology	=PrimitiveTopology.TriangleList;


			mSChain.ResizeBuffers(1, width, height, Format.Unknown, SwapChainFlags.None);

			mBackBuffer	=Texture2D.FromSwapChain<Texture2D>(mSChain, 0);
			mBBView		=new RenderTargetView(mGD, mBackBuffer);

			Texture2DDescription	depthDesc	=new Texture2DDescription()
			{
				//pick depth format based on feature level
				Format				=(mGD.FeatureLevel != FeatureLevel.Level_9_3)?
										Format.D32_Float_S8X24_UInt : Format.D24_UNorm_S8_UInt,
				ArraySize			=1,
				MipLevels			=1,
				Width				=width,
				Height				=height,
				SampleDescription	=new SampleDescription(1, 0),
				Usage				=ResourceUsage.Default,
				BindFlags			=BindFlags.DepthStencil,
				CpuAccessFlags		=CpuAccessFlags.None,
				OptionFlags			=ResourceOptionFlags.None
			};

			mDepthBuffer	=new Texture2D(mGD, depthDesc);
			mDSView			=new DepthStencilView(mGD, mDepthBuffer);

			mScreenPort	=new Viewport(0, 0, width, height, 0f, 1f);

			mDC.Rasterizer.SetViewport(mScreenPort);
			mDC.OutputMerger.SetTargets(mDSView, mBBView);

			mGCam	=new UtilityLib.GameCamera(width, height, 16f/9f, mClipNear, mClipFar);

			//other stuff can now resize rendertargets and such
			Misc.SafeInvoke(eResized, this);
		}


		public void Present()
		{
			mSChain.Present(0, PresentFlags.None);
		}


		public void ClearViews()
		{
			mDC.ClearDepthStencilView(mDSView, DepthStencilClearFlags.Depth, 1f, 0);
			mDC.ClearRenderTargetView(mBBView, Color.CornflowerBlue);
		}


		public void ClearDepth()
		{
			mDC.ClearDepthStencilView(mDSView, DepthStencilClearFlags.Depth, 1f, 0);
		}


		public void SetFullScreen(bool bFull)
		{
			mSChain.SetFullscreenState(bFull, null);
			mbResized	=true;
		}


		public void SetClip(float near, float far)
		{
			mClipNear	=near;
			mClipFar	=far;
		}


		public void ReleaseAll()
		{
			mDC.ClearState();
			mDC.Flush();

			mBBView.Dispose();
			mBackBuffer.Dispose();
			mDSView.Dispose();
			mDepthBuffer.Dispose();
			mDC.Dispose();
			mSChain.Dispose();
			mGD.Dispose();

#if DEBUG
			mGDD.ReportLiveDeviceObjects(ReportingLevel.Detail);

			mGDD.Dispose();
#endif
		}


		public void SetShadowViewPort()
		{
			mDC.Rasterizer.SetViewport(mShadowPort);
		}


		public void SetScreenViewPort()
		{
			mDC.Rasterizer.SetViewport(mScreenPort);
		}


		public Viewport GetScreenViewPort()
		{
			return	mScreenPort;
		}


		public void ResetCursorPos()
		{
			Cursor.Position	=mStoredMousePos;
		}


		public void SetCapture(bool bOn)
		{
			if(bOn)
			{
				mRForm.Capture	=true;

				Cursor.Hide();

				mStoredMousePos	=Cursor.Position;

				Cursor.Clip	=mRForm.RectangleToScreen(mRForm.ClientRectangle);
			}
			else
			{
				mRForm.Capture	=false;

				Cursor.Show();
				Cursor.Clip	=mStoredClipRect;
			}
		}


		public void CheckResize()
		{
			if(!mbResized)
			{
				return;
			}

			HandleResize();

			mbResized	=false;
		}


		void OnRenderFormResize(object sender, EventArgs ea)
		{
			mbResized	=true;
		}
	}
}
