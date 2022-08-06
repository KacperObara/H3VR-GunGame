using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FistVR;
using UnityEngine;

namespace GunGame.Scripts.Weapons
{
    [Serializable]
    public class WeaponPool
    {
        public string Name;
        public string Description;
        public OrderType OrderType;
        public string EnemyType = "M_Swat_Scout";

        public List<GunData> Guns = new List<GunData>();

        // Stupid workaround for the GunData objects breaking for some reason when loading inside the game
        [HideInInspector] public List<string> GunNames = new List<string>();
        [HideInInspector] public List<string> MagNames = new List<string>();
        [HideInInspector] public List<int> CategoryIDs = new List<int>();

        public GunData GetWeapon(int index)
        {
            return Guns[index];
        }

        public void Initialize()
        {
            SetGunOrder();
            SetSpawners();
        }

        private void SetGunOrder()
        {
            if (OrderType == OrderType.Random)
            {
                Guns.Shuffle();
            }

            if (OrderType == OrderType.RandomWithinCategory)
            {
                Guns.Shuffle();
                Guns = Guns.OrderBy(x => x.CategoryID).ToList();
            }
        }

        private void SetSpawners()
        {
            SosigEnemyID enemyID = SosigEnemyID.M_MercWiener_Scout;
            try
            {
                enemyID = (SosigEnemyID) Enum.Parse(typeof(SosigEnemyID), EnemyType, true);
            }
            catch (Exception _)
            {
                Debug.LogError(EnemyType + " is not a valid SosigEnemyID, please check your weapon pool");
            }

            foreach (var sosigSpawner in SosigBehavior.Instance.SosigSpawners)
            {
                sosigSpawner.SosigType = enemyID;
            }
        }
    }

    public enum OrderType
    {
        Fixed,
        Random,
        RandomWithinCategory
    }
}