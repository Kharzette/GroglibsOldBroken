﻿using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;
using UtilityLib;


namespace BSPZone
{
	internal class Pushable
	{
		internal Mobile			mMobile;
		internal BoundingBox	mBox;
		internal Vector3		mWorldCenter;
		internal int			mModelOn;


		internal Pushable(Mobile mob, BoundingBox box, Vector3 worldCenter, int modelOn)
		{
			mMobile			=mob;
			mBox			=box;
			mWorldCenter	=worldCenter;
			mModelOn		=modelOn;
		}
	}
}
