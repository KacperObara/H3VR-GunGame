using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GunGame.Scripts.Options
{
	public class MusicToggleOption : MonoBehaviour
	{
		public Action OptionChanged;
		public static bool MusicEnabled;

		public AudioSource MusicAudioSource;
		public bool MusicEnabledAtStart = false;

		[SerializeField] private Image EnabledImage;

		private void Awake()
		{
			OptionChanged += OnToggleMusic;

			MusicEnabled = MusicEnabledAtStart;
			EnabledImage.enabled = MusicEnabled;
			OnToggleMusic();
		}

		public void ToggleClicked()
		{
			MusicEnabled = !MusicEnabled;

			if (OptionChanged != null)
				OptionChanged.Invoke();
		}

		private void OnToggleMusic()
		{
			if (MusicEnabled)
			{
				MusicAudioSource.Play();
			}
			else
			{
				MusicAudioSource.Stop();
			}

			EnabledImage.enabled = MusicEnabled;
		}

		private void OnDestroy()
		{
			OptionChanged -= OnToggleMusic;
		}
	}
}
