using System;
using System.Collections;
using System.Collections.Generic;
using FistVR;
using GunGame.Scripts.Options;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunGame.Scripts
{
    public class PlayerSpawner : MonoBehaviourSingleton<PlayerSpawner>
    {
        public static Action BeingRevivedEvent;

        public Progression Progression;

        private IEnumerator Start()
        {
            // Wait one frame so that everything is all setup
            yield return null;

            GM.CurrentSceneSettings.DeathResetPoint = transform;
            GM.CurrentMovementManager.TeleportToPoint(transform.position, true, transform.position + transform.forward);
        }

        private IEnumerator DelayedRespawn()
        {
            GameManager.Instance.Deaths++;
            Progression.Instance.KillsWithCurrentWeapon = 0;
            Progression.Instance.CurrentTier = 0;
            yield return new WaitForSeconds(3f);
            if (Progression.Instance.ProgressionType == KillProgressionType.Tiers || GameSettings.AlwaysResetSosigs)
            {
                //for tiered progression we want to reset the sosigs to match the tier (or if we selected the option)
                StartCoroutine(SosigBehavior.Instance.ClearSosigs());
            }
            Progression.Instance.Demote();
            GM.CurrentPlayerBody.ActivatePower(PowerupType.Invincibility, PowerUpIntensity.High, PowerUpDuration.VeryShort,
                false, false);
            GM.CurrentPlayerBody.HealPercent(100f);

            MovePlayerToRandomSpawn();

            if (BeingRevivedEvent != null)
            {
                BeingRevivedEvent.Invoke();
            }
        }

        [HarmonyPatch(typeof (GM), "KillPlayer")]
        [HarmonyPostfix]
        private static void Postfix(bool KilledSelf)
        {
            Instance.OnPlayerDeath();
        }

        private void OnPlayerDeath()
        {
            StartCoroutine(DelayedRespawn());
        }

        public void MovePlayerToRandomSpawn()
        {
            int randomSpawn = Random.Range(0, GameManager.Instance.PlayerSpawners.Count);
            transform.position = GameManager.Instance.PlayerSpawners[randomSpawn].position;
            GM.CurrentMovementManager.TeleportToPoint(transform.position, true, transform.position + transform.forward);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.25f);
        }
    }
}