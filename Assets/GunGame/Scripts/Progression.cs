using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using BepInEx;
using FistVR;
using GunGame.Scripts.Options;
using GunGame.Scripts.Weapons;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GunGame.Scripts
{
    public class Progression : MonoBehaviourSingleton<Progression>
    {
        public static Action SosigDespawnedEvent;
        public static Action WeaponChangedEvent;

        public AudioSource EquipSound;

        [HideInInspector] public int KillsWithCurrentWeapon = 0;

        public static List<int> DeadSosigs = new List<int>();
        private List<GameObject> _currentEquipment = new List<GameObject>();
        private WeaponBuffer _weaponBuffer;
        public int CurrentWeaponId { get; private set; }

        [HideInInspector] public KillProgressionType ProgressionType = KillProgressionType.Count;
        [HideInInspector] private List<EnemyData> Enemies = new List<EnemyData>();
        //only needed for the tiered enemy progression type
        [HideInInspector] public int CurrentTier = 0;
        [HideInInspector] private int ProbabilityTotal = 0;
        [HideInInspector] private int InverseProbabilityTotal = 0;



        public override void Awake()
        {
            base.Awake();

            _weaponBuffer = GetComponent<WeaponBuffer>();
        }

        public void Demote()
        {
            if (GameManager.Instance.GameEnded)
                return;

            CurrentWeaponId--;
            if (CurrentWeaponId < 0)
                CurrentWeaponId = 0;
            SpawnAndEquip(true);
        }

        public void Promote()
        {
            if (GameManager.Instance.GameEnded)
                return;

            if (GameSettings.HealOnPromotion)
                GM.CurrentPlayerBody.HealPercent(100f);

            CurrentWeaponId++;
            KillsWithCurrentWeapon = 0;

            if (CurrentWeaponId >= GameSettings.CurrentPool.GetWeaponCount() || CurrentWeaponId >= WeaponCountOption.WeaponCount)
            {
                //end game code
                CurrentWeaponId = GameSettings.CurrentPool.GetWeaponCount() - 1;
                GameManager.Instance.EndGame();
            }
            else
            {
                //promotion code
                EquipSound.Play();
                SpawnAndEquip();
            }
        }

        public void SpawnAndEquip(bool demotion = false)
        {
            DestroyOldEq();

            if (CustomDebug.Instance.DebugWeaponText.gameObject.activeInHierarchy)
            {
                CustomDebug.Instance.DebugWeaponText.text = "Weapon " + GameSettings.CurrentPool.GetWeapon(CurrentWeaponId).GunName;
                CustomDebug.Instance.DebugAmmoText.text = "Ammo " + GameSettings.CurrentPool.GetWeapon(CurrentWeaponId).MagName;
            }

            //Debug.Log("Trying to spawn Weapon: " + GameSettings.CurrentPool.GetWeapon(_progressIndex).GunName);
            //Debug.Log("Trying to spawn Ammo: " + GameSettings.CurrentPool.GetWeapon(_progressIndex).MagName);

            FVRPhysicalObject gun = _weaponBuffer.GetFromBuffer(ObjectType.Gun, CurrentWeaponId, demotion);

            if (gun == null)
            {
                Debug.LogError("Trying to equip null gun! Probably the ObjectId is invalid. Attempting a fix by promoting the player");
                Promote();
                return;
            }
            _currentEquipment.Add(gun.gameObject);

            // If magazine string is empty, then just equip the gun and clear the buffer
            if (GameSettings.CurrentPool.GetWeapon(CurrentWeaponId).MagName.IsNullOrWhiteSpace())
            {
                EquipWeapon(gun);
                _weaponBuffer.DestroyMagBuffer();
            }
            // Otherwise, fetch the magazines and equip the gun and magazine
            else
            {
                FVRPhysicalObject magazineToLoad = _weaponBuffer.GetFromBuffer(ObjectType.MagazineToLoad, CurrentWeaponId, demotion);

                if (magazineToLoad == null)
                {
                    Debug.LogError("Trying to equip null magazine! Probably the ObjectId is invalid. Attempting a fix by promoting the player");
                    Promote();
                    return;
                }

                magazineToLoad.UsesGravity = false;
                magazineToLoad.RootRigidbody.isKinematic = true;

                FVRPhysicalObject magazineForQuickbelt = _weaponBuffer.GetFromBuffer(ObjectType.MagazineForQuickbelt, CurrentWeaponId, demotion);
                _currentEquipment.Add(magazineToLoad.gameObject);
                _currentEquipment.Add(magazineForQuickbelt.gameObject);

                if (!GameSettings.DisabledAutoLoading)
                    LoadTheGun(gun, magazineToLoad);
                EquipWeapon(gun);
                FixAmmoRotation();

                MoveMagazineToSlot(magazineForQuickbelt);
            }
            //check if the extra string is empty, and if it isn't, spawn the thing and throw it in the right slot
            if (!GameSettings.CurrentPool.GetWeapon(CurrentWeaponId).Extra.IsNullOrWhiteSpace())
            {
                FVRPhysicalObject extraForQuickbelt = _weaponBuffer.GetFromBuffer(ObjectType.Extra, CurrentWeaponId, demotion);

                _currentEquipment.Add(extraForQuickbelt.gameObject);

                MoveExtraToSlot(extraForQuickbelt);
            }

            // Generate the next weapon
            _weaponBuffer.ClearBuffer();
            _weaponBuffer.GenerateBuffer(CurrentWeaponId);

            if (WeaponChangedEvent != null)
                WeaponChangedEvent.Invoke();
        }

        // Break interaction with all previously created objects and destroy them.
        private void DestroyOldEq()
        {
            for (int i = 0; i < _currentEquipment.Count; i++)
            {
                if (_currentEquipment[i] && _currentEquipment[i].GetComponent<FVRPhysicalObject>())
                    _currentEquipment[i].GetComponent<FVRPhysicalObject>().ForceBreakInteraction();

                Destroy(_currentEquipment[i]);
            }
            _currentEquipment.Clear();
        }

        private void MoveMagazineToSlot(FVRPhysicalObject magazine)
        {
            // TODO make quickbelt slot selectable
            if (QuickbeltOption.AmmoQuickbeltSlot.CurObject)
                QuickbeltOption.AmmoQuickbeltSlot.CurObject.ClearQuickbeltState();

            magazine.ForceObjectIntoInventorySlot(QuickbeltOption.AmmoQuickbeltSlot);
            magazine.m_isSpawnLock = true;
        }

        private void MoveExtraToSlot(FVRPhysicalObject extra)
        {
            if (QuickbeltOption.ExtraQuickbeltSlot.CurObject)
                QuickbeltOption.ExtraQuickbeltSlot.CurObject.ClearQuickbeltState();
            extra.ForceObjectIntoInventorySlot(QuickbeltOption.ExtraQuickbeltSlot);
        }

        private void LoadTheGun(FVRPhysicalObject weapon, FVRPhysicalObject ammo)
        {
            // If ammo is a magazine, then simply load it into the gun
            if (ammo as FVRFireArmMagazine != null)
            {
                try
                {
                    ((FVRFireArmMagazine)ammo).Load(weapon as FVRFireArm);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Weapon failed to load magazine for gun " + weapon.ObjectWrapper.DisplayName + " with magazine " + ammo.ObjectWrapper.DisplayName);
                }
            }
            // If the weapon is a Revolver, load the ammo into the cylinder
            else if (weapon.GetComponentInChildren<RevolverCylinder>() && ammo as Speedloader != null)
            {
                weapon.GetComponentInChildren<RevolverCylinder>().LoadFromSpeedLoader((Speedloader)ammo);
            }
            // If there is no other option, then try to fill all the chambers instead.
            // It could possibly load the chamber for every weapon, but I want Player to load it manually for now.
            else if (GameSettings.AlwaysChamberRounds || (ammo as FVRFireArmMagazine == null && weapon.GetComponentInChildren<FVRFireArmMagazine>() == null && weapon as FVRFireArm != null))
            {
                try
                {
                    List<FVRFireArmChamber> chambers = weapon.GetComponent<FVRFireArm>().GetChambers();
                    foreach (var chamber in chambers)
                    {
                        FVRFireArmRound round = _weaponBuffer.SpawnImmediate(ObjectType.MagazineToLoad, GameSettings.CurrentPool.GetWeapon(CurrentWeaponId)).GetComponent<FVRFireArmRound>();
                        chamber.SetRound(round);
                    }

                    BreakActionWeapon breakActionWeapon = weapon.GetComponentInChildren<BreakActionWeapon>();
                    if (breakActionWeapon != null)
                    {
                        breakActionWeapon.CockAllHammers();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error while trying to load gun chambers manually for a gun: " + weapon.name + " and ammo: " + ammo.name);
                }
                //weapon.GetComponentInChildren<Handgun>().CockHammer(false);
            }

            // // Belt fed not working yet
            // if (ammo as FVRFireArmMagazine)
            // {
            //      FVRFireArmBeltGrabTrigger grabTrigger = ammo.GetComponentInChildren<FVRFireArmBeltGrabTrigger>();
            //      if (grabTrigger != null)
            //      {
            //           Debug.Log("grabtrigger found");
            //           try
            //           {
            //                FVRFirearmBeltDisplayData beltDisplayData = grabTrigger.Mag.FireArm.BeltDD;
            //                Debug.Log("beltDisplayData " + beltDisplayData);
            //                FVRFireArmBeltSegment fireArmBeltSegment = beltDisplayData.StripBeltSegment(beltDisplayData.GrabPoint_Gun.position);
            //                Debug.Log("fireArmBeltSegment " + fireArmBeltSegment);
            //                FVRFireArmBeltRemovalTrigger removalTrigger = fireArmBeltSegment.m_trig;
            //                Debug.Log("removalTrigger " + removalTrigger);
            //                beltDisplayData.PullPushBelt(ammo as FVRFireArmMagazine, beltDisplayData.BeltCapacity);
            //                Debug.Log("grabTrigger.Mag " + grabTrigger.Mag);
            //                grabTrigger.Mag.UpdateBulletDisplay();
            //
            //                beltDisplayData.PullPushBelt(ammo as FVRFireArmMagazine, beltDisplayData.BeltCapacity);
            //                Debug.Log("beltDisplayData.Firearm " + beltDisplayData.Firearm);
            //                beltDisplayData.Firearm.PlayAudioEvent(FirearmAudioEventType.BeltSeat);
            //                beltDisplayData.Firearm.ConnectedToBox = true;
            //                beltDisplayData.Firearm.HasBelt = true;
            //                beltDisplayData.UpdateProxyRounds(0);
            //
            //                // Debug.Log("removalTrigger.FireArm " + removalTrigger.FireArm);
            //                // Debug.Log("removalTrigger.FireArm.BeltDD " + removalTrigger.FireArm.BeltDD);
            //                // removalTrigger.FireArm.BeltDD.MountBeltSegment(fireArmBeltSegment);
            //                Destroy(fireArmBeltSegment.gameObject);
            //           }
            //           catch (Exception e)
            //           {
            //                Console.WriteLine(e);
            //                Debug.Log("Error mounting belt");
            //           }
            //      }
            //
            //      // if (grabTrigger != null)
            //      // {
            //      //      grabTrigger.Mag.FireArm.BeltDD.m_isBeltGrabbed = true;
            //      //
            //      //      //FVRFirearmBeltDisplayData beltDisplayData = grabTrigger.Mag.FireArm.BeltDD;
            //      //
            //      //      try
            //      //      {
            //      //           beltDisplayData.PullPushBelt(ammo as FVRFireArmMagazine, beltDisplayData.BeltCapacity);
            //      //           beltDisplayData.Firearm.ConnectedToBox = true;
            //      //           beltDisplayData.Firearm.HasBelt = true;
            //      //           beltDisplayData.UpdateProxyRounds(0);
            //      //
            //      //           grabTrigger.Mag.UpdateBulletDisplay();
            //      //      }
            //      //      catch (Exception e)
            //      //      {
            //      //           Console.WriteLine(e);
            //      //      }
            //      // }
            // }


            // Try to load the internal magazine. It works for tube-fed shotguns too.
            FVRFireArmMagazine insideMag = weapon.GetComponentInChildren<FVRFireArmMagazine>();
            if (insideMag)
            {
                FireArmRoundClass roundClass = AM.SRoundDisplayDataDic[insideMag.RoundType].Classes[0].Class;
                insideMag.ReloadMagWithType(roundClass);
            }
        }

        public void FixAmmoRotation()
        {
            if (_currentEquipment[0] && _currentEquipment[1] &&
                _currentEquipment[0].GetComponent<FVRFireArm>() &&
                _currentEquipment[0].GetComponent<FVRFireArm>().MagazineMountPos)
            {
                _currentEquipment[1].transform.rotation = _currentEquipment[0].GetComponent<FVRFireArm>().MagazineMountPos.transform.rotation;
            }
        }

        private void EquipWeapon(FVRPhysicalObject weapon)
        {
            FVRViveHand hand = GM.CurrentMovementManager.Hands[1];
            if (LeftHandOption.LeftHandModeEnabled)
                hand = GM.CurrentMovementManager.Hands[0];

            hand.RetrieveObject(weapon);
        }

        private static void OnSosigKilledByPlayer(Sosig killedSosig)
        {
            GameManager.Instance.Kills++;
            //Get the type of the just killed sosig
            SosigEnemyID killedEnemyID = SosigBehavior.Instance.Sosigs[killedSosig];
            
            //If we're using points, we increment by the point value of the enemy type
            if(Instance.ProgressionType == KillProgressionType.Points)
            {
                //default value is one, if this isn't overwritten an unmanaged enemy has been killed, so we count it and move on I guess
                int killvalue = 1;
                //find the entry for this type
                foreach(EnemyData data in Instance.Enemies)
                {
                    if(data.EnemyName == killedEnemyID)
                    {
                        killvalue = data.Value;
                        //Debug.Log("Killed enemy of point value: " + killvalue.ToString());
                        break;
                    }
                }
                Instance.KillsWithCurrentWeapon += killvalue;
                //Debug.Log("New point total: " + Instance.KillsWithCurrentWeapon.ToString() + "/" + KillsPerWeaponOption.KillsPerWeaponCount.ToString());
                if (Instance.KillsWithCurrentWeapon >= KillsPerWeaponOption.KillsPerWeaponCount)
                {
                    Instance.Promote();
                }
            }
            //If we're using tiers, we don't care about kills, but specifically kills in the current tier
            else if (Instance.ProgressionType == KillProgressionType.Tiers)
            {
                int foundindex = -1;
                int currentindex = -1;
                //find the entry for this type
                foreach (EnemyData data in Instance.Enemies)
                {
                    currentindex++;
                    if (data.EnemyName == killedEnemyID)
                    {
                        foundindex = currentindex;
                        break;
                    }
                }
                //We have killed an unmanaged enemy(?)
                if (foundindex == -1)
                {
                    //We're just gonna promote and call it a day
                    Instance.KillsWithCurrentWeapon = 0;
                    Instance.CurrentTier = 0;
                    Instance.Promote();
                    return;
                }
                //otherwise we found the enemy we killed
                //We gotta check if it's an enemy type we care about, if it isn't we completely disregard
                if (foundindex == Instance.CurrentTier)
                {
                    Instance.KillsWithCurrentWeapon++;
                    //Debug.Log("Kill count: " + Instance.KillsWithCurrentWeapon.ToString() + "/" + (Instance.Enemies[Instance.CurrentTier].Value + Instance.KillsWithCurrentWeapon));
                    //We've killed enough of this type, go to the next type and then spawn
                    if (Instance.KillsWithCurrentWeapon >= (Instance.Enemies[Instance.CurrentTier].Value + KillsPerWeaponOption.KillsPerWeaponCount))
                    {
                        //Debug.Log("Next Tier!");
                        Instance.CurrentTier++;
                        //reset kills
                        Instance.KillsWithCurrentWeapon = 0;
                        //We increment and then check if we've maxed out our tiers
                        if (Instance.CurrentTier == Instance.Enemies.Count)
                        {
                            Instance.CurrentTier = 0;
                            Instance.Promote();
                        }
                    }
                }
            }
            //If we're using count progression, we use default behavior, so this is also the fall-through default
            else
            {
                Instance.KillsWithCurrentWeapon++;
                if (Instance.KillsWithCurrentWeapon >= KillsPerWeaponOption.KillsPerWeaponCount)
                {
                    Instance.Promote();
                }
            }
        }

        [HarmonyPatch(typeof(Sosig), "SosigDies")]
        [HarmonyPostfix]
        private static void OnSosigDied(Sosig __instance, Damage.DamageClass damClass, Sosig.SosigDeathType deathType)
        {
            // Caching the dead sosig to avoid duplicates that happen for some reason
            if (DeadSosigs.Contains(__instance.GetInstanceID()))
                return;
            DeadSosigs.Add(__instance.GetInstanceID());

            if (__instance.GetDiedFromIFF() == GM.CurrentPlayerBody.GetPlayerIFF())
            {
                OnSosigKilledByPlayer(__instance);
            }

            __instance.DeSpawnSosig();
            SosigBehavior.Instance.OnSosigKilled(__instance);
            SosigEnemyID nextSosig = GetNextSosigType();
            SosigBehavior.Instance.SpawnSosigRandomPlace(nextSosig);
            }

        public void InitEnemyProgression()
        {
            ProbabilityTotal = 0;
            InverseProbabilityTotal = 0;
            ProgressionType = GameSettings.CurrentPool.GetProgressionType();
            Enemies = GameSettings.CurrentPool.GetEnemies();
            foreach (EnemyData data in Instance.Enemies)
            {
                //we cannot allow 0 values, so just change them right now
                if (data.Value == 0)
                {
                    data.Value = 1;
                }
            }
            //if we're using count we want to proportional spawning based on the value, but with points we want an inverse relationship
            //Because of this I (hackily) invert the number value and multiply by a big constant to roughly get an integer instead of a float
            if (Instance.ProgressionType == KillProgressionType.Points)
            {
                foreach (EnemyData data in Instance.Enemies)
                {
                    //Using the constant '100' should cover all normal cases, with any values over 100 having their inverse rounded up to 1
                    InverseProbabilityTotal += Math.Max(1, (int)((1.0f / data.Value) * 100.0f));
                }
            }
            //Set up total for probability generations, which all progressions need
            foreach (EnemyData data in Instance.Enemies)
            {
                ProbabilityTotal += data.Value;
            }

        }

        public static SosigEnemyID GetNextSosigType()
        {
            if(Instance.Enemies.Count == 0)
            {
                //We have no sosig enemies loaded, so send a default and print an error
                Debug.Log("No enemies loaded, make sure that your weaponpool is formatted correctly!");
                return SosigEnemyID.Junkbot_Broken;
            }
            //For either of these types of progressions, we want to spawn probabilistically, based on the value
            //For count, higher numbers means that more spawn
            if (Instance.ProgressionType == KillProgressionType.Count)
            {

                if(Instance.ProbabilityTotal == 0)
                {
                    //Some joker put all 0 probabilities, so select one at random
                    Debug.Log("All probabilities set to 0!");
                    return Instance.Enemies[Random.Range(0, Instance.Enemies.Count-1)].EnemyName;
                }
                int rng = Random.Range(0, Instance.ProbabilityTotal);
                //Debug.Log("Random Number: " + rng);
                //This is a hacky way to normalize various probability values.
                int total = 0;
                foreach (EnemyData data in Instance.Enemies)
                {
                    total += data.Value;
                    if (rng < total)
                    {
                        return data.EnemyName;
                    }
                }
                //We didn't find it (somehow?), so return last
                Debug.Log("Default enemy selected");
                return Instance.Enemies[Instance.Enemies.Count-1].EnemyName;
            }
            //For points, higher numbers means that fewer spawn, since they're more valuable, we handled that in the init function
            if (Instance.ProgressionType == KillProgressionType.Points)
            {

                if (Instance.ProbabilityTotal == 0)
                {
                    //Some joker put all 0 probabilities, so select one at random
                    Debug.Log("All probabilities set to 0!");
                    return Instance.Enemies[Random.Range(0, Instance.Enemies.Count-1)].EnemyName;
                }
                int rng = Random.Range(0, Instance.InverseProbabilityTotal);
                //This is a hacky way to normalize various probability values.
                int total = 0;
                foreach (EnemyData data in Instance.Enemies)
                {
                    //Also, we once again have the super hacky way to generate inverses!
                    total += Math.Max(1, (int)((1.0f / data.Value) * 100.0f));
                    if (rng < total)
                    {
                        return data.EnemyName;
                    }
                }
                //We didn't find it (somehow?), so return last
                Debug.Log("Default enemy selection");
                return Instance.Enemies[Instance.Enemies.Count-1].EnemyName;
            }
            //For this type of progression, we spawn the current tier
            else if (Instance.ProgressionType == KillProgressionType.Tiers)
            {
                return Instance.Enemies[Instance.CurrentTier].EnemyName;
            }
            //there are no other progression types, but just in case, if we don't hit any of them...
            Debug.Log("Progression type isn't handled correctly somehow, check that your weaponpool file is formatted correctly!");
            return SosigEnemyID.Junkbot_ElfBroken;
        }

        public UIData GetProgressionTypeUIDefaults()
        {
            UIData outputData = new UIData();
            KillProgressionType newProgressionType = GameSettings.CurrentPool.GetProgressionType();
            if(newProgressionType == KillProgressionType.Points)
            {
                outputData.Text = "Points to advance";
                int pointSum = 0;
                //assume that players will want kill about one of each enemy to progress by default
                foreach(EnemyData data in GameSettings.CurrentPool.GetEnemies())
                {
                    pointSum += data.Value;
                }
                outputData.Value = pointSum;
            }
            else if(newProgressionType == KillProgressionType.Tiers)
            {
                outputData.Text = "Extra kills per tier";
                //assume that the weaponpool tiers are designed intentionally
                outputData.Value = 0;
            }
            else
            {
                //default behavior
                outputData.Text = "Kills to advance";
                outputData.Value = 3;
            }
            return outputData;
        }


    }

    //Enum for types of sosig kill progression
    //Count is standard get X number of kills
    //Points requires X number of points, with different targets worth different amounts
    //Tiers requires a specific number of each level of Sosig to progress
    public enum KillProgressionType
    {
        Count,
        Points,
        Tiers
    }

    //A generic struct to pass bundled information to UI components, extend if necessary
    public struct UIData
    {
        public String Text;
        public int Value;
    }
}
