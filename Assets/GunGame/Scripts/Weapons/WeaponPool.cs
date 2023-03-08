using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FistVR;
using UnityEngine;

namespace GunGame.Scripts.Weapons
{
    [Serializable]
    public class WeaponPool : WeaponPoolInterface
    {
        public string Name;
        public string Description;
        public OrderType OrderType;
        public String EnemyType = "M_Swat_Scout";
        public int CurrentIndex;

        public List<GunData> Guns = new List<GunData>();
        [NonSerialized] public List<EnemyData> Enemies = new List<EnemyData>();

        // Stupid workaround for the GunData objects breaking for some reason when loading inside the game
        [HideInInspector] public List<string> GunNames = new List<string>();
        [HideInInspector] public List<string> MagNames = new List<string>();
        [HideInInspector] public List<int> CategoryIDs = new List<int>();

        public String GetName()
        {
            return Name;
        }

        public String GetDescription()
        {
            return Description;
        }

        public GunData GetNextWeapon()
        {
            if(CurrentIndex + 1 >= GunNames.Count)
            {
                return null;
            }
            return Guns[CurrentIndex + 1];
        }

        public GunData GetWeapon(int index)
        {
            return Guns[index];
        }

        public int GetWeaponCount()
        {
            return Guns.Count;
        }

        public int GetCurrentWeaponIndex()
        {
            return CurrentIndex;
        }


        public GunData GetCurrentWeapon()
        {
            return Guns[CurrentIndex];
        }

        public KillProgressionType GetProgressionType()
        {
            return KillProgressionType.Count;
        }

        public List<EnemyData> GetEnemies()
        {
            return Enemies;
        }

        public bool IncrementProgress()
        {
            CurrentIndex++;
            //game is complete, return true
            if(CurrentIndex == Guns.Count)
            {
                return true;
            }
            return false;
        }

        public void DecrementProgress()
        {
            if(CurrentIndex > 0)
            {
                CurrentIndex--;
            }
        }

        public void Initialize()
        {
            SetGunOrder();
            SeedEnemyList();
            //no longer necessary, with new enemy control method
            //SetSpawners();
            CurrentIndex = 0;
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

        private void SeedEnemyList()
        {
            Enemies.Add(new EnemyData(EnemyType, 1));
        }

        //DEPRECATED
        private void SetSpawners()
        {
            /*
            SosigEnemyID enemyID = SosigEnemyID.M_MercWiener_Scout;
            try
            {
                enemyID = EnemyType;
            }
            catch (Exception _)
            {
                Debug.LogError(EnemyType.ToString() + " is not a valid SosigEnemyID, please check your weapon pool");
            }

            foreach (var sosigSpawner in SosigBehavior.Instance.SosigSpawners)
            {
                //sosigSpawner.SosigType = enemyID;
            }
            */
        }

    }

}