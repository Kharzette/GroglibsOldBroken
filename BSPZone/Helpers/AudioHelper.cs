﻿using System;
using System.Text;
using System.Collections.Generic;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using UtilityLib;
using AudioLib;


namespace BSPZone
{
	public class AudioHelper
	{
		Zone	mZone;
		Audio	mAudio;

		List<ZoneEntity>				mAudioEntities	=new List<ZoneEntity>();
		Dictionary<ZoneEntity, Emitter>	mEmitters		=new Dictionary<ZoneEntity, Emitter>();

		bool	mbMiscListening;


		public void Initialize(Zone zone, TriggerHelper th, Audio aud)
		{
			if(mZone != null)
			{
				//if changing level, release previous instances
				foreach(KeyValuePair<ZoneEntity, Emitter> ems in mEmitters)
				{
					mAudio.StopEmitter(ems.Value);
				}
			}
			mZone	=zone;
			mAudio	=aud;

			//be sure to only wire this up once!
			//can cause double toggling
			if(!mbMiscListening)
			{
				th.eMisc		+=OnTriggerMisc;
				mbMiscListening	=true;
			}

			mAudioEntities.Clear();
			mEmitters.Clear();

			//grab out audio emitters
			List<ZoneEntity>	sounds	=mZone.GetEntitiesStartsWith("misc_sound");
			foreach(ZoneEntity ze in sounds)
			{
				mAudioEntities.Add(ze);

				Vector3	pos;
				ze.GetOrigin(out pos);

				Emitter	em	=Audio.MakeEmitter(pos);

				mEmitters.Add(ze, em);

				string	fxName;
				fxName	=ze.GetValue("effect_name");

				int		looping;
				ze.GetInt("looping", out looping);

				float	volume;
				ze.GetFloat("volume", out volume);

				string	sOn	=ze.GetValue("activated");
				if(sOn != "0")
				{
					mAudio.PlayAtLocation(fxName, volume, (looping != 0), em);
				}
			}
		}


		void OnTriggerMisc(object sender, EventArgs ea)
		{
			ZoneEntity	ze	=sender as ZoneEntity;
			if(ze == null)
			{
				return;
			}

			string	className	=ze.GetValue("classname");
			if(!className.StartsWith("misc_sound"))
			{
				return;
			}

			if(!mEmitters.ContainsKey(ze))
			{
				return;
			}

			string	fxName;
			fxName	=ze.GetValue("effect_name");

			int		looping;
			ze.GetInt("looping", out looping);

			float	volume;
			ze.GetFloat("volume", out volume);

			if(ze.IsActivated())
			{
				mAudio.StopEmitter(mEmitters[ze]);
			}
			else
			{
				mAudio.PlayAtLocation(fxName, volume, (looping != 0), mEmitters[ze]);
			}

			ze.ToggleEntityActivated();
		}
	}
}
