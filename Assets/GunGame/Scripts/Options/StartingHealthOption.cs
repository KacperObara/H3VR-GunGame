using System;
using System.Collections;
using FistVR;
using UnityEngine;
using UnityEngine.UI;

namespace GunGame.Scripts.Options
{
    public class StartingHealthOption : MonoBehaviour
    {
        public Action OptionChanged;

        public int[] PossibleHealth;
        private int _optionIndex = 1;
        public static int CurrentHealth = 1000;

        [SerializeField] private Text _healthText;

        private void Awake()
        {
            PossibleHealth = new int[]
            {
                100,
                1000,
                2000,
                5000,
                10000
            };

            OptionChanged += UpdateUI;
            OptionChanged += UpdateHealth;
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(.1f);
            ResetClicked();
        }

        public void ArrowLeftClicked()
        {
            _optionIndex--;

            if (_optionIndex < 0)
                _optionIndex = 0;

            CurrentHealth = PossibleHealth[_optionIndex];

            if (OptionChanged != null)
                OptionChanged.Invoke();
        }

        public void ArrowRightClicked()
        {
            _optionIndex++;
            if (_optionIndex >= PossibleHealth.Length)
                _optionIndex = PossibleHealth.Length - 1;

            CurrentHealth = PossibleHealth[_optionIndex];

            if (OptionChanged != null)
                OptionChanged.Invoke();
        }

        public void ResetClicked()
        {
            _optionIndex = 1;
            CurrentHealth = PossibleHealth[_optionIndex];

            if (OptionChanged != null)
                OptionChanged.Invoke();
        }

        private void UpdateHealth()
        {
            GM.CurrentPlayerBody.SetHealthThreshold(CurrentHealth);
            GM.CurrentPlayerBody.ResetHealth();
        }

        private void UpdateUI()
        {
            _healthText.text = CurrentHealth.ToString();
        }

        private void OnDestroy()
        {
            OptionChanged -= UpdateUI;
            OptionChanged -= UpdateHealth;
        }
    }
}
