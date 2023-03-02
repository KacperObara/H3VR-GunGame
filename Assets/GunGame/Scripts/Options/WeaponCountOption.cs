using System;
using GunGame.Scripts.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace GunGame.Scripts.Options
{
    public class WeaponCountOption : MonoBehaviour
    {
        public Action OptionChanged;

        //public int DefaultCount;
        public static int WeaponCount;

        [SerializeField] private Text _counterText;

        private void Awake()
        {
            OptionChanged += UpdateUI;
            GameSettings.WeaponPoolChanged += ResetClicked;
        }

        // private void Start()
        // {
        //     ResetClicked();
        // }

        public void ArrowLeftClicked()
        {
            WeaponCount--;

            if (WeaponCount <= 1)
                WeaponCount = 1;

            if (OptionChanged != null)
                OptionChanged.Invoke();
        }

        public void ArrowRightClicked()
        {
            WeaponCount++;

            if (WeaponCount > GameSettings.CurrentPool.GetWeaponCount())
                WeaponCount = GameSettings.CurrentPool.GetWeaponCount();

            if (OptionChanged != null)
                OptionChanged.Invoke();
        }

        public void ArrowRight10Clicked()
        {
            WeaponCount += 20;

            if (WeaponCount > GameSettings.CurrentPool.GetWeaponCount())
                WeaponCount = GameSettings.CurrentPool.GetWeaponCount();

            if (OptionChanged != null)
                OptionChanged.Invoke();
        }

        public void ArrowLeft10Clicked()
        {
            WeaponCount -= 20;

            if (WeaponCount <= 1)
                WeaponCount = 1;

            if (OptionChanged != null)
                OptionChanged.Invoke();
        }

        public void ResetClicked()
        {
            WeaponCount = GameSettings.CurrentPool.GetWeaponCount();

            if (OptionChanged != null)
                OptionChanged.Invoke();
        }

        private void UpdateUI()
        {
            _counterText.text = WeaponCount.ToString();
        }

        private void OnDestroy()
        {
            OptionChanged -= UpdateUI;
            GameSettings.WeaponPoolChanged -= ResetClicked;
        }
    }
}
