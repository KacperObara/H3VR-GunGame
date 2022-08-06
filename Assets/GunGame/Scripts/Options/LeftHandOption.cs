using System;
using UnityEngine;
using UnityEngine.UI;

namespace GunGame.Scripts.Options
{
	public class LeftHandOption : MonoBehaviour
	{
		public Action OptionChanged;

		public static bool LeftHandModeEnabled;

		[SerializeField] private Image EnabledImage;

		private void Awake()
		{
			OptionChanged += UpdateUI;

			LeftHandModeEnabled = false;
			EnabledImage.enabled = false;
		}

		public void ToggleClicked()
		{
			LeftHandModeEnabled = !LeftHandModeEnabled;

			if (OptionChanged != null)
				OptionChanged.Invoke();
		}

		private void UpdateUI()
		{
			EnabledImage.enabled = LeftHandModeEnabled;
		}

		private void OnDestroy()
		{
			OptionChanged -= UpdateUI;
		}
	}
}
