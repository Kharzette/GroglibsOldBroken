﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UtilityLib;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

//ambiguous stuff
using Buffer	=SharpDX.Direct3D11.Buffer;
using Device	=SharpDX.Direct3D11.Device;


namespace MeshLib
{
	public class Mesh
	{
		public class MeshAndArch
		{
			public object	mMesh;
			public IArch	mArch;
		};

		protected string			mName;
		protected Buffer			mVerts;
		protected Buffer			mIndexs;
		protected int				mNumVerts, mNumTriangles, mVertSize;
		protected int				mTypeIndex;
		protected BoundingBox		mBoxBound;
		protected BoundingSphere	mSphereBound;
		protected Matrix			mTransform;

		Matrix	mTransInverted;

		protected VertexBufferBinding	mVBBinding;


		public string Name
		{
			get { return mName; }
			set { mName = ((value == null)? "" : value); }
		}
		public Type VertexType
		{
			get { return VertexTypes.GetTypeForIndex(mTypeIndex); }
			private set { mTypeIndex = VertexTypes.GetIndex(value); }
		}
		

		public Mesh() { }
		public Mesh(string name)
		{
			Name	=name;
		}


		public void FreeAll()
		{
			mVerts.Dispose();
			mIndexs.Dispose();
		}


		public Matrix GetTransform()
		{
			return	mTransform;
		}


		public void SetVertSize(int size)
		{
			mVertSize	=size;
		}


		public void SetNumVerts(int nv)
		{
			mNumVerts	=nv;
		}


		public void SetNumTriangles(int numTri)
		{
			mNumTriangles	=numTri;
		}


		public void SetVertexBuffer(Buffer vb)
		{
			mVerts		=vb;
			mVBBinding	=new VertexBufferBinding(mVerts, VertexTypes.GetSizeForTypeIndex(mTypeIndex), 0);
		}


		public void SetIndexBuffer(Buffer indbuf)
		{
			mIndexs	=indbuf;
		}


		public void SetTypeIndex(int idx)
		{
			mTypeIndex	=idx;
		}


		public void SetTransform(Matrix mat)
		{
			mTransform		=mat;
			mTransInverted	=mat;

			mTransInverted.Invert();
		}


		public virtual void Write(BinaryWriter bw)
		{
			bw.Write(mName);
			bw.Write(mNumVerts);
			bw.Write(mNumTriangles);
			bw.Write(mVertSize);
			bw.Write(mTypeIndex);

			//transform
			FileUtil.WriteMatrix(bw, mTransform);

			//box bound
			FileUtil.WriteVector3(bw, mBoxBound.Minimum);
			FileUtil.WriteVector3(bw, mBoxBound.Maximum);

			//sphere bound
			FileUtil.WriteVector3(bw, mSphereBound.Center);
			bw.Write(mSphereBound.Radius);
		}


		public virtual void Read(BinaryReader br, Device gd, bool bEditor)
		{
			mName			=br.ReadString();
			mNumVerts		=br.ReadInt32();
			mNumTriangles	=br.ReadInt32();
			mVertSize		=br.ReadInt32();
			mTypeIndex		=br.ReadInt32();

			mTransform	=FileUtil.ReadMatrix(br);

			SetTransform(mTransform);

			mBoxBound.Minimum	=FileUtil.ReadVector3(br);
			mBoxBound.Maximum	=FileUtil.ReadVector3(br);

			mSphereBound.Center	=FileUtil.ReadVector3(br);
			mSphereBound.Radius	=br.ReadSingle();

			if(!bEditor)
			{
				Array	vertArray;

				VertexTypes.ReadVerts(br, gd, out vertArray);

				int	indLen	=br.ReadInt32();

				UInt16	[]indArray	=new UInt16[indLen];

				for(int i=0;i < indLen;i++)
				{
					indArray[i]	=br.ReadUInt16();
				}

				mVerts	=VertexTypes.BuildABuffer(gd, vertArray, mTypeIndex);
				mIndexs	=VertexTypes.BuildAnIndexBuffer(gd, indArray);

				mVerts.DebugName	=mName;

				mVBBinding	=new VertexBufferBinding(mVerts,
					VertexTypes.GetSizeForTypeIndex(mTypeIndex), 0);
			}
		}


		internal void DrawDMN(DeviceContext dc, MeshMaterial mm)
		{
			if(!mm.mbVisible)
			{
				return;
			}

			if(mm.mMatLib == null)
			{
				return;
			}

			if(!mm.mMatLib.MaterialExists("DMN"))
			{
				return;
			}

			dc.InputAssembler.SetVertexBuffers(0, mVBBinding);
			dc.InputAssembler.SetIndexBuffer(mIndexs, Format.R16_UInt, 0);

			mm.mMatLib.SetMaterialParameter("DMN", "mMaterialID", mm.mMaterialID);
			mm.mMatLib.SetMaterialParameter("DMN", "mWorld",
				(mTransform * mm.mObjectTransform));
			
			mm.mMatLib.ApplyMaterialPass("DMN", dc, 0);

			dc.DrawIndexed(mNumTriangles * 3, 0, 0);
		}


		internal void Draw(DeviceContext dc, MeshMaterial mm, string altMaterial)
		{
			if(!mm.mbVisible)
			{
				return;
			}

			if(!mm.mMatLib.MaterialExists(altMaterial))
			{
				return;
			}

			dc.InputAssembler.SetVertexBuffers(0, mVBBinding);
			dc.InputAssembler.SetIndexBuffer(mIndexs, Format.R16_UInt, 0);

			mm.mMatLib.SetMaterialParameter(altMaterial, "mWorld",
				(mTransform * mm.mObjectTransform));
			
			mm.mMatLib.ApplyMaterialPass(altMaterial, dc, 0);

			dc.DrawIndexed(mNumTriangles * 3, 0, 0);
		}


		internal void Draw(DeviceContext dc, MeshMaterial mm)
		{
			if(!mm.mMatLib.MaterialExists(mm.mMaterialName))
			{
				return;
			}

			mm.mMatLib.SetMaterialParameter(mm.mMaterialName, "mWorld",
				(mTransform * mm.mObjectTransform));

			dc.InputAssembler.SetVertexBuffers(0, mVBBinding);
			dc.InputAssembler.SetIndexBuffer(mIndexs, Format.R16_UInt, 0);

			mm.mMatLib.ApplyMaterialPass(mm.mMaterialName, dc, 0);

			dc.DrawIndexed(mNumTriangles * 3, 0, 0);
		}


		public float? RayIntersect(Vector3 start, Vector3 end, bool bBox)
		{
			//backtransform the ray
			Vector3	backStart	=Vector3.TransformCoordinate(start, mTransInverted);
			Vector3	backEnd		=Vector3.TransformCoordinate(end, mTransInverted);

			if(bBox)
			{
				return	Mathery.RayIntersectBox(backStart, backEnd, mBoxBound);
			}
			else
			{
				return	Mathery.RayIntersectSphere(backStart, backEnd, mSphereBound);
			}
		}


		public virtual void Bound()
		{
			throw new NotImplementedException();
		}


		public BoundingBox GetBoxBounds()
		{
			return	mBoxBound;
		}


		public BoundingSphere GetSphereBounds()
		{
			return	mSphereBound;
		}


		public static Dictionary<string, IArch> LoadAllStaticMeshes(
			string dir,	Device gd)
		{
			Dictionary<string, IArch>	ret	=new Dictionary<string, IArch>();

			if(Directory.Exists(dir))
			{
				DirectoryInfo	di	=new DirectoryInfo(dir + "/");

				FileInfo[]		fi	=di.GetFiles("*.Static", SearchOption.TopDirectoryOnly);
				foreach(FileInfo f in fi)
				{
					//strip back
					string	path	=f.DirectoryName;

					IArch	smo	=new StaticArch();
					bool	bWorked	=smo.ReadFromFile(path + "\\" + f.Name, gd, true);

					if(bWorked)
					{
						ret.Add(f.Name, smo);
					}
				}
			}

			return	ret;
		}
	}
}