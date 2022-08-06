using System;
using GunGame.Scripts.Weapons;

namespace GunGame.Scripts.Options
{
    public class GameSettings : MonoBehaviourSingleton<GameSettings>
    {
        public static Action SettingsChanged;
        public static Action WeaponPoolChanged;

        public static WeaponPool CurrentPool { get; private set; }

        // private void Start()
        // {
        //     if (CurrentPool == null)
        //         CurrentPool = WeaponPoolLoader.Instance.DebugWeaponPools[1];
        // }

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
