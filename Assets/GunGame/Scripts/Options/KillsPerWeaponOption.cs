using System;
using UnityEngine;
using UnityEngine.UI;

namespace GunGame.Scripts.Options
{
	public class KillsPerWeaponOption : MonoBehaviour
	{
		public Action OptionChanged;

		public int DefaultCount;
		public static int KillsPerWeaponCount;

		[SerializeField] private Text _counterText;

		private void Awake()
		{
			OptionChanged += UpdateUI;
		}

		private void Start()
		{
			ResetClicked();
		}

		public void ArrowLeftClicked()
		{
			KillsPerWeaponCount--;

			if (KillsPerWeaponCount <= 1)
				KillsPerWeaponCount = 1;

			if (OptionChanged != null)
				OptionChanged.Invoke();
		}

		public void ArrowRightClicked()
		{
			KillsPerWeaponCount++;

			if (OptionChanged != null)
				OptionChanged.Invoke();
		}

		public void ResetClicked()
		{
			KillsPerWeaponCount = DefaultCount;

			if (OptionChanged != null)
				OptionChanged.Invoke();
		}

		private void UpdateUI()
		{
			_counterText.text = KillsPerWeaponCount.ToString();
		}

		private void OnDestroy()
		{
			OptionChanged -= UpdateUI;
		}
	}
}
