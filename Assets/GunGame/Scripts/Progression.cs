using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using FistVR;
using GunGame.Scripts.Options;
using GunGame.Scripts.Weapons;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

               if (CurrentWeaponId >= GameSettings.CurrentPool.Guns.Count || CurrentWeaponId >= WeaponCountOption.WeaponCount)
               {
                    CurrentWeaponId = GameSettings.CurrentPool.Guns.Count - 1;
                    GameManager.Instance.EndGame();
               }
               else
               {
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
               if (GameSettings.CurrentPool.Guns[CurrentWeaponId].MagName.IsNullOrWhiteSpace())
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

          private void LoadTheGun(FVRPhysicalObject weapon, FVRPhysicalObject ammo)
          {
               // If ammo is a magazine, then simply load it into the gun
               if (ammo as FVRFireArmMagazine != null)
               {
                    ((FVRFireArmMagazine)ammo).Load(weapon as FVRFireArm);
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
                    List<FVRFireArmChamber> chambers = weapon.GetComponent<FVRFireArm>().GetChambers();
                    foreach (var chamber in chambers)
                    {
                         FVRFireArmRound round = _weaponBuffer.SpawnImmediate(ObjectType.MagazineToLoad, GameSettings.CurrentPool.Guns[CurrentWeaponId]).GetComponent<FVRFireArmRound>();
                         chamber.SetRound(round);
                    }

                    BreakActionWeapon breakActionWeapon = weapon.GetComponentInChildren<BreakActionWeapon>();
                    if (breakActionWeapon != null)
                    {
                         breakActionWeapon.CockAllHammers();
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

          private static void OnSosigKilledByPlayer()
          {
               GameManager.Instance.Kills++;
               Instance.KillsWithCurrentWeapon++;

               if (Instance.KillsWithCurrentWeapon % KillsPerWeaponOption.KillsPerWeaponCount == 0)
               {
                    Instance.Promote();
               }
          }

          [HarmonyPatch(typeof (Sosig), "SosigDies")]
          [HarmonyPostfix]
          private static void OnSosigDied(Sosig __instance, Damage.DamageClass damClass, Sosig.SosigDeathType deathType)
          {
               // Caching the dead sosig to avoid duplicates that happen for some reason
               if (DeadSosigs.Contains(__instance.GetInstanceID()))
                    return;
               DeadSosigs.Add(__instance.GetInstanceID());

               if (__instance.GetDiedFromIFF() == GM.CurrentPlayerBody.GetPlayerIFF())
               {
                    OnSosigKilledByPlayer();
               }

               __instance.DeSpawnSosig();
               SosigBehavior.Instance.SpawnSosigRandomPlace();
          }
     }
}
