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
        private List<WeaponPoolInterface> _weaponPools = new List<WeaponPoolInterface>();

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

            _weaponPools = new List<WeaponPoolInterface>();

            if (_loadedWeaponPoolsLocations.Count == 0)
            {
                Debug.LogError("No weapon pools found!");
                return;
            }

            for (int i = 0; i < _loadedWeaponPoolsLocations.Count; i++)
            {
                WeaponPoolInterface newWeaponPool = LoadWeaponPool(_loadedWeaponPoolsLocations[i]);

                if (newWeaponPool != null)
                {
                    _weaponPools.Add(newWeaponPool);
                    Debug.Log("Weapon pool loaded with name: " + newWeaponPool.GetName() + " and count: " + newWeaponPool.GetWeaponCount());
                }
                else
                {
                    Debug.Log("Failed to load Weapon pool at location: " + _loadedWeaponPoolsLocations[i]);
                }
            }

            for (int i = 0; i < _weaponPools.Count; i++)
            {
                PoolChoice choice = Instantiate(ChoicePrefab, ChoicesListParent);
                choice.Initialize(_weaponPools[i]);
                _choices.Add(choice);
            }

            //WeaponPool startPool = _weaponPools.FirstOrDefault(x => x.Name == "Default Weapons");
            WeaponPoolInterface startPool = _weaponPools[0];
            GameSettings.ChangeCurrentPool(startPool);

            if (WeaponLoadedEvent != null)
                WeaponLoadedEvent.Invoke();
        }

        public WeaponPoolInterface LoadWeaponPool(string path)
        {
            using (StreamReader stream = new StreamReader(path))
            {
                string json = stream.ReadToEnd();
                //we need to check if the file has the optional WeaponPoolType value, which means it's an advanced pool (ooh fancy)
                WeaponPoolAdvanced newWeaponPoolAdvanced = null;
                try
                {
                    newWeaponPoolAdvanced = JsonUtility.FromJson<WeaponPoolAdvanced>(json);
                }
                catch(Exception e)
                {
                    Debug.Log(e.Message);
                    return null;
                }
                if (newWeaponPoolAdvanced.WeaponPoolType == "Advanced")
                {
                    if (_weaponPools.FirstOrDefault(x => x.GetName() == newWeaponPoolAdvanced.GetName()) != null)
                    {
                        // weapon pool already exists, so don't load it again
                        return null;
                    }
                    Debug.Log("Loaded advanced weapon pool: " + newWeaponPoolAdvanced.GetName());
                    return newWeaponPoolAdvanced;
                }
                //Original Weapon Pool
                else
                {
                    WeaponPool newWeaponPool = JsonUtility.FromJson<WeaponPool>(json);

                    if (_weaponPools.FirstOrDefault(x => x.GetName() == newWeaponPool.GetName()) != null)
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
                    Debug.Log("Loaded basic weapon pool: " + newWeaponPoolAdvanced.GetName());
                    return newWeaponPool;
                }
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
            //Debug.Log(pathToPlugins);
            List<string> list = Directory.GetFiles(pathToPlugins, "GunGameWeaponPool*.json", SearchOption.AllDirectories).ToList();
            return list;
        }
    }
}