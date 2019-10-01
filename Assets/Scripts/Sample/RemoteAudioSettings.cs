#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Audio;
using Extensions;
using UnityEngine;
using AudioSettings = Common.Audio.AudioSettings;

namespace Sample
{
	public class RemoteAudioSettings : AudioSettings
	{
#pragma warning disable 649
		[Header("Remote audio clips"), SerializeField]
		private AudioLocal[] _locales = new AudioLocal[0];
#pragma warning restore 649

#if UNITY_EDITOR
		private const string SettingsPath = "Assets/Bundles/Sound";

		[MenuItem("Tools/Game Settings/Remote Audio Settings")]
		private static void GetAndSelectSettingsInstance()
		{
			EditorUtility.FocusProjectWindow();
			Selection.activeObject =
				InspectorExtensions.FindOrCreateNewScriptableObject<RemoteAudioSettings>(SettingsPath);
		}
#endif

		public override Dictionary<SystemLanguage, Dictionary<string, AudioClip>> Clips
		{
			get
			{
				var res = new Dictionary<SystemLanguage, Dictionary<string, AudioClip>>();
				foreach (var locale in _locales)
				{
					if (Enum.IsDefined(typeof(SystemLanguage), (int) locale.Language))
					{
						var lang = (SystemLanguage) (int) locale.Language;
						res.Add(lang, locale.Clips.ToDictionary(record => record.Id, record => record.Clip));
					}
					else
					{
						Debug.LogErrorFormat("Can't resolve {0} Language enum value.",
							typeof(Language).GetEnumName(locale.Language));
					}
				}

				return res;
			}
		}
	}
}