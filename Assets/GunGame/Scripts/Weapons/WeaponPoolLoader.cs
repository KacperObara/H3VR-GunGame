using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using GunGame.Scripts.Options;
using UnityEngine;

namespace GunGame.Scripts.Weapons
{
    public class WeaponPoolLoader : MonoBehaviourSingleton<WeaponPoolLoader>
    {
        public static Action WeaponLoadedEvent;

        public Transform ChoicesListParent;
        public PoolChoice ChoicePrefab;

        // Quick way for me to create and save weapon pools.
        public List<WeaponPool> DebugWeaponPools;

        private List<string> _loadedWeaponPoolsLocations = new List<string>();
        private List<WeaponPool> _weaponPools = new List<WeaponPool>();

        // unused for now
        private List<PoolChoice> _choices = new List<PoolChoice>();

        public void SaveDebugWeaponPools()
        {
            for (int i = 0; i < DebugWeaponPools.Count; i++)
            {
                DebugWeaponPools[i].GunNames.Clear();
                DebugWeaponPools[i].MagNames.Clear();
                DebugWeaponPools[i].CategoryIDs.Clear();

                for (int j = 0; j < DebugWeaponPools[i].Guns.Count; j++)
                {
                    DebugWeaponPools[i].GunNames.Add(DebugWeaponPools[i].Guns[j].GunName);
                    DebugWeaponPools[i].MagNames.Add(DebugWeaponPools[i].Guns[j].MagName);
                    DebugWeaponPools[i].CategoryIDs.Add(DebugWeaponPools[i].Guns[j].CategoryID);
                }

                DebugWeaponPools[i].Guns.Clear();
            }

            foreach (var pool in DebugWeaponPools)
            {
                WriteWeaponPool(Application.dataPath + "/GunGame/" + "/GunGameWeaponPool_" + pool.Name + ".json", pool);
            }
        }

        public override void Awake()
        {
            base.Awake();

            //SaveDebugWeaponPools();

            _loadedWeaponPoolsLocations = GetWeaponPoolLocations();

            _weaponPools = new List<WeaponPool>();

            if (_loadedWeaponPoolsLocations.Count == 0)
            {
                Debug.LogError("No weapon pools found!");
                return;
            }

            for (int i = 0; i < _loadedWeaponPoolsLocations.Count; i++)
            {
                WeaponPool newWeaponPool = LoadWeaponPool(_loadedWeaponPoolsLocations[i]);

                if (newWeaponPool != null)
                {
                    _weaponPools.Add(newWeaponPool);
                    Debug.Log("Weapon pool loaded with name: " + newWeaponPool.Name + " and count: " + newWeaponPool.GunNames.Count);
                }
            }

            for (int i = 0; i < _weaponPools.Count; i++)
            {
                PoolChoice choice = Instantiate(ChoicePrefab, ChoicesListParent);
                choice.Initialize(_weaponPools[i]);
                _choices.Add(choice);
            }

            WeaponPool startPool = _weaponPools.FirstOrDefault(x => x.Name == "Default Weapons");
            GameSettings.ChangeCurrentPool(startPool);

            if (WeaponLoadedEvent != null)
                WeaponLoadedEvent.Invoke();
        }

        public WeaponPool LoadWeaponPool(string path)
        {
            using (StreamReader stream = new StreamReader(path))
            {
                string json = stream.ReadToEnd();
                WeaponPool newWeaponPool = JsonUtility.FromJson<WeaponPool>(json);

                if (_weaponPools.FirstOrDefault(x => x.Name == newWeaponPool.Name) != null)
                {
                    // weapon pool already exists, so don't load it again
                    return null;
                }

                // I have to rebuild the list because game hates me and clears it every time I load a weapon pool.
                // doesn't happen in the editor, but it does happen in the game.
                newWeaponPool.Guns.Clear();

                for (int i = 0; i < newWeaponPool.GunNames.Count; i++)
                {
                    GunData gunData = new GunData()
                    {
                        GunName = newWeaponPool.GunNames[i],
                        MagName = newWeaponPool.MagNames[i],
                        CategoryID = newWeaponPool.CategoryIDs[i]
                    };

                    newWeaponPool.Guns.Add(gunData);
                }

                return newWeaponPool;
            }
        }

        public void WriteWeaponPool(string path, WeaponPool weaponPool)
        {
            using (StreamWriter stream = new StreamWriter(path))
            {
                string json = JsonUtility.ToJson(weaponPool, true);
                stream.Write(json);
            }
        }

        private List<string> GetWeaponPoolLocations()
        {
            string pathToPlugins = Paths.PluginPath;
            List<string> list = Directory.GetFiles(pathToPlugins, "GunGameWeaponPool*.json", SearchOption.AllDirectories).ToList();
            return list;
        }
    }
}