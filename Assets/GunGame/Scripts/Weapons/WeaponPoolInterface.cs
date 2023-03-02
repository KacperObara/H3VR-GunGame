using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FistVR;
using UnityEngine;

namespace GunGame.Scripts.Weapons
{
    //This interface allows for new types of weapon pools to be created
    //As long as your pool can perform these operations in a consistent way, the game will spawn the right weapon at the right time
    public interface WeaponPoolInterface
    {
        //public string Name;
        //public string Description;
        //public OrderType OrderType;
        //public string EnemyType = "M_Swat_Scout";

        //public List<GunData> Guns = new List<GunData>
        String GetName();

        String GetDescription();

        int GetWeaponCount();

        int GetCurrentWeaponIndex();

        GunData GetWeapon(int index);

        GunData GetCurrentWeapon();

        GunData GetNextWeapon();

        KillProgressionType GetProgressionType();

        List<EnemyData> GetEnemies();

        bool IncrementProgress();

        void DecrementProgress();

        void Initialize();        
    }

    public enum OrderType
    {
        Fixed,
        Random,
        RandomWithinCategory
    }
}
