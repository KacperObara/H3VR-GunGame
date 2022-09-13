using System;
using GunGame.Scripts.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace GunGame.Scripts.Options
{
    public class GameSettings : MonoBehaviourSingleton<GameSettings>
    {
        public static Action SettingsChanged;
        public static Action WeaponPoolChanged;

        public static bool DisabledAutoLoading;
        public static bool AlwaysChamberRounds; // Unused for now
        public static bool HealOnPromotion;

        [SerializeField] private Image DisabledAutoLoadingImage;
        [SerializeField] private Image AlwaysChamberRoundsImage;
        [SerializeField] private Image HealOnPromotionImage;

        [SerializeField] private Text MaxSosigCountText;

        public static int MaxSosigCount;

        public static WeaponPool CurrentPool { get; private set; }

        private void Start()
        {
            DisabledAutoLoading = false;
            AlwaysChamberRounds = false;
            HealOnPromotion = false;

            ResetMaxSosigCount();
        }

        public void IncreaseMaxSosigCount()
        {
            MaxSosigCount++;

            MaxSosigCountText.text = MaxSosigCount.ToString();

            if (SettingsChanged != null)
                SettingsChanged.Invoke();
        }

        public void DecreaseMaxSosigCount()
        {
            MaxSosigCount--;
            if (MaxSosigCount < 1)
                MaxSosigCount = 1;

            MaxSosigCountText.text = MaxSosigCount.ToString();

            if (SettingsChanged != null)
                SettingsChanged.Invoke();
        }

        public void ResetMaxSosigCount()
        {
            MaxSosigCount = SosigBehavior.Instance.MaxSosigCount;

            MaxSosigCountText.text = MaxSosigCount.ToString();

            if (SettingsChanged != null)
                SettingsChanged.Invoke();
        }

        public void ToggleAutoLoading()
        {
            DisabledAutoLoading = !DisabledAutoLoading;
            DisabledAutoLoadingImage.enabled = DisabledAutoLoading;

            if (SettingsChanged != null)
                SettingsChanged.Invoke();
        }

        public void ToggleAlwaysChamberRounds()
        {
            AlwaysChamberRounds = !AlwaysChamberRounds;
            AlwaysChamberRoundsImage.enabled = AlwaysChamberRounds;

            if (SettingsChanged != null)
                SettingsChanged.Invoke();
        }

        public void ToggleHealOnPromotion()
        {
            HealOnPromotion = !HealOnPromotion;
            HealOnPromotionImage.enabled = HealOnPromotion;

            if (SettingsChanged != null)
                SettingsChanged.Invoke();
        }

        public static void ChangeCurrentPool(WeaponPool newPool)
        {
            CurrentPool = newPool;
            CurrentPool.Initialize();

            if (WeaponPoolChanged != null)
                WeaponPoolChanged.Invoke();

            if (SettingsChanged != null)
                SettingsChanged.Invoke();
        }
    }
}
