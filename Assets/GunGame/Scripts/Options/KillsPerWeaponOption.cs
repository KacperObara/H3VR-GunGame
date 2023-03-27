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
		public static String Description;

		[SerializeField] private Text _counterText;
        [SerializeField] private Text _descriptionText;

        private void Awake()
		{
			OptionChanged += UpdateUI;
            GameSettings.WeaponPoolChanged += ResetClicked;
        }

		private void Start()
		{
			ResetClicked();
		}

		public void ArrowLeftClicked()
		{
			KillsPerWeaponCount--;
			//check if the progression type is tiered, to allow 0 values
			KillProgressionType progressionType = Progression.Instance.ProgressionType;
			if (KillsPerWeaponCount <= 1 && progressionType != KillProgressionType.Tiers)
				KillsPerWeaponCount = 1;
			else if (KillsPerWeaponCount <= 0)
			{
				KillsPerWeaponCount = 0;
			}

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
            //Debug.Log("ResetClicked!");
			//Change the text on the enemy count to reflect different progression type's behavior
			UIData defaultValues = Progression.Instance.GetProgressionTypeUIDefaults();
			KillsPerWeaponCount = defaultValues.Value;
			Description = defaultValues.Text;

            if (OptionChanged != null)
				OptionChanged.Invoke();
		}

		private void UpdateUI()
		{
            //Debug.Log("UIUpdate!");
			_counterText.text = KillsPerWeaponCount.ToString();
			_descriptionText.text = Description;
        }

		private void OnDestroy()
		{
			OptionChanged -= UpdateUI;
            GameSettings.WeaponPoolChanged -= ResetClicked;
        }
	}
}
