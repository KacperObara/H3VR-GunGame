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

namespace GunGame.Scripts
{
     public class Progression : MonoBehaviourSingleton<Progression>
     {
          public static Action SosigDied;

          public AudioSource EquipSound;

          [HideInInspector] public int KillsWithCurrentWeapon = 0;

          private static List<int> DeadSosigs = new List<int>();
          private List<GameObject> _currentEquipment = new List<GameObject>();
          private WeaponBuffer _weaponBuffer;
          private int _progressIndex;

          public override void Awake()
          {
               base.Awake();

               _weaponBuffer = GetComponent<WeaponBuffer>();
          }

          public void Demote()
          {
               _progressIndex--;
               if (_progressIndex < 0)
                    _progressIndex = 0;

               SpawnAndEquip(true);
          }

          public void Promote()
          {
               _progressIndex++;

               if (_progressIndex >= GameSettings.CurrentPool.Guns.Count || _progressIndex >= WeaponCountOption.WeaponCount)
               {
                    _progressIndex = GameSettings.CurrentPool.Guns.Count - 1;
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

               FVRPhysicalObject gun = _weaponBuffer.GetFromBuffer(ObjectType.Gun, _progressIndex, demotion);
               _currentEquipment.Add(gun.gameObject);

               // If magazine string is empty, then just equip the gun and clear the buffer
               if (GameSettings.CurrentPool.Guns[_progressIndex].MagName.IsNullOrWhiteSpace())
               {
                    EquipWeapon(gun);
                    _weaponBuffer.DestroyMagBuffer();
               }
               // Otherwise, fetch the magazines and equip the gun and magazine
               else
               {
                    FVRPhysicalObject magazineToLoad = _weaponBuffer.GetFromBuffer(ObjectType.MagazineToLoad, _progressIndex, demotion);
                    FVRPhysicalObject magazineForQuickbelt = _weaponBuffer.GetFromBuffer(ObjectType.MagazineForQuickbelt, _progressIndex, demotion);

                    _currentEquipment.Add(magazineToLoad.gameObject);
                    _currentEquipment.Add(magazineForQuickbelt.gameObject);

                    LoadTheGun(gun, magazineToLoad);
                    MoveMagazineToSlot(magazineForQuickbelt);
               }

               // Generate the next weapon
               _weaponBuffer.ClearBuffer();
               _weaponBuffer.GenerateBuffer(_progressIndex);
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
               // if (GM.CurrentPlayerBody.QBSlots_Internal[0].CurObject)
               //      GM.CurrentPlayerBody.QBSlots_Internal[0].CurObject.ClearQuickbeltState();

               //magazine.ForceObjectIntoInventorySlot(GM.CurrentPlayerBody.QBSlots_Internal[0]);
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
               else if (ammo as FVRFireArmMagazine == null && weapon.GetComponentInChildren<FVRFireArmMagazine>() == null && weapon as FVRFireArm != null)
               {
                    List<FVRFireArmChamber> chambers = weapon.GetComponent<FVRFireArm>().GetChambers();
                    foreach (var chamber in chambers)
                    {
                         FVRFireArmRound round = _weaponBuffer.SpawnImmediate(ObjectType.MagazineToLoad, GameSettings.CurrentPool.Guns[_progressIndex]).GetComponent<FVRFireArmRound>();
                         chamber.SetRound(round);
                    }

                    BreakActionWeapon breakActionWeapon = weapon.GetComponentInChildren<BreakActionWeapon>();
                    if (breakActionWeapon != null)
                    {
                         breakActionWeapon.CockAllHammers();
                    }
               }

               // Belt fed not working yet
               // if (ammo as FVRFireArmMagazine)
               // {
               //      FVRFireArmBeltGrabTrigger grabTrigger = ammo.GetComponentInChildren<FVRFireArmBeltGrabTrigger>();
               //
               //      if (grabTrigger != null)
               //      {
               //           FVRFirearmBeltDisplayData beltDisplayData = grabTrigger.Mag.FireArm.BeltDD;
               //
               //           try
               //           {
               //                beltDisplayData.PullPushBelt(ammo as FVRFireArmMagazine, beltDisplayData.BeltCapacity);
               //                beltDisplayData.Firearm.ConnectedToBox = true;
               //                beltDisplayData.Firearm.HasBelt = true;
               //                beltDisplayData.UpdateProxyRounds(0);
               //
               //                grabTrigger.Mag.UpdateBulletDisplay();
               //           }
               //           catch (Exception e)
               //           {
               //                Console.WriteLine(e);
               //           }
               //      }
               // }


               // Try to load the internal magazine. It works for tube-fed shotguns too.
               FVRFireArmMagazine insideMag = weapon.GetComponentInChildren<FVRFireArmMagazine>();
               if (insideMag)
               {
                    FireArmRoundClass roundClass = AM.SRoundDisplayDataDic[insideMag.RoundType].Classes[0].Class;
                    insideMag.ReloadMagWithType(roundClass);
               }

               EquipWeapon(weapon);
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
          private static void Postfix(Sosig __instance, Damage.DamageClass damClass, Sosig.SosigDeathType deathType)
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

               if (SosigDied != null)
                    SosigDied.Invoke();
          }
     }
}
