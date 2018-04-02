﻿using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using SharpDX;


namespace ColladaConvert
{
	public class Animation
	{
		public enum KeyPartsUsed
		{
			TranslateX	=1,
			TranslateY	=2,
			TranslateZ	=4,
			RotateX		=8,
			RotateY		=16,
			RotateZ		=32,
			ScaleX		=64,
			ScaleY		=128,
			ScaleZ		=256,
			All			=511
		}

		string	mName;

		List<SubAnimation>	mSubAnims	=new List<SubAnimation>();


		public Animation(animation anim)
		{
			if(anim.Items.OfType<animation>().Count() > 0)
			{
				foreach(object anObj in anim.Items)
				{
					animation	anm	=anObj as animation;
					if(anm == null)
					{
						continue;
					}

					mName	=anim.name;

					SubAnimation	sa	=new SubAnimation(anm);
					mSubAnims.Add(sa);
				}
			}
			else
			{
				SubAnimation	sa	=new SubAnimation(anim);
				mSubAnims.Add(sa);
			}
		}


		public string GetName()
		{
			return	mName;
		}


		internal MeshLib.SubAnim	GetAnims(MeshLib.Skeleton skel, library_visual_scenes lvs, out KeyPartsUsed parts)
		{
			parts	=0;

			//grab full list of bones
			List<string>	boneNames	=new List<string>();

			skel.GetBoneNames(boneNames);

			//for each bone, find any keyframe times
			foreach(string bone in boneNames)
			{
				List<float>	times	=new List<float>();

				foreach(SubAnimation sa in mSubAnims)
				{
					List<float>	saTimes	=sa.GetTimesForBone(bone, lvs);

					foreach(float t in saTimes)
					{
						if(times.Contains(t))
						{
							continue;
						}
						times.Add(t);
					}
				}

				if(times.Count == 0)
				{
					continue;
				}

				times.Sort();

				//build list of keys for times
				List<MeshLib.KeyFrame>	keys	=new List<MeshLib.KeyFrame>();
				foreach(float t in times)
				{
					keys.Add(new MeshLib.KeyFrame());
				}

				//track axis angle style keys
				List<MeshLib.KeyFrame>	axisAngleKeys	=new List<MeshLib.KeyFrame>();

				//set keys
				foreach(SubAnimation sa in mSubAnims)
				{
					parts	|=sa.SetKeys(bone, times, keys, lvs, axisAngleKeys);
				}

				//fix axis angle keyframes
				foreach(MeshLib.KeyFrame kf in axisAngleKeys)
				{
					Matrix	mat	=Matrix.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(kf.mRotation.X));
					mat	*=Matrix.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(kf.mRotation.Y));
					mat	*=Matrix.RotationAxis(Vector3.UnitZ, MathUtil.DegreesToRadians(kf.mRotation.Z));

					kf.mRotation	=Quaternion.RotationMatrix(mat);
				}
				return	new MeshLib.SubAnim(bone, times, keys);
			}

			return	null;
		}
	}
}