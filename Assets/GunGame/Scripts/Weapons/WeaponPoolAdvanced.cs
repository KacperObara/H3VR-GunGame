using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using FistVR;
using UnityEngine;

namespace GunGame.Scripts.Weapons
{
    [Serializable]
    public class WeaponPoolAdvanced : WeaponPoolInterface
    {
        public string Name = "";
        public string Description = "";
        public OrderType OrderType = OrderType.Fixed;
        public string EnemyType = "M_Swat_Scout";
        public int CurrentIndex = 0;
        public string WeaponPoolType = "";
        public KillProgressionType EnemyProgressionType = KillProgressionType.Count;

        public List<GunData> Guns = new List<GunData>();
        public List<EnemyData> Enemies = new List<EnemyData>();

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
            return Guns[CurrentIndex];
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
            return EnemyProgressionType;
        }

        public List<EnemyData> GetEnemies()
        {
            return Enemies;
        }

        public bool IncrementProgress()
        {
            CurrentIndex++;
            //game is complete, return true
            if (CurrentIndex == Guns.Count)
            {
                return true;
            }
            return false;
        }

        public void DecrementProgress()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
        }

        public void Initialize()
        {
            SetGunOrder();
            SeedRandomMagazines();
            CurrentIndex = 0;
            AutofillEnemyData();
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

        //This is necessary to make the json be human readable, we need to convert from strings to enums
        private void AutofillEnemyData()
        {
            foreach(EnemyData enemy in Enemies){
                //If the enemy data has a string, we will overwrite the enum with the converted string value
                //This both allows for you to only provide a human readable name, and solve the problem of having the enum and the string not match in the json
                if (!enemy.EnemyNameString.IsNullOrWhiteSpace())
                {
                    enemy.EnemyName = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), enemy.EnemyNameString, true);
                }
            }
        }


        //For simplicity's sake, we randomly decide which provided ammo each gun will use once at the beginning.
        private void SeedRandomMagazines()
        {
            //grab a random number to start
            UnityEngine.Random.InitState(Convert.ToInt16(Time.time));
            int counter = UnityEngine.Random.Range(0,10);
            foreach (GunData gun in Guns)
            {
                //If there is no mags in the names list, we skip and don't set any mag, which the progression code should handle properly
                if (gun.MagNames.Count > 0) {
                    //set the mag to be one instance inside of the mag names list
                    gun.MagName = gun.MagNames[counter % gun.MagNames.Count];
                
                }
                //iterate the counter
                counter++;
            }
        }

    }

}