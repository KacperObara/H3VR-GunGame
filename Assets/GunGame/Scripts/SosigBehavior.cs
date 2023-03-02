using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FistVR;
using GunGame.Scripts.Options;
using GunGame.Scripts.Weapons;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GunGame.Scripts
{
	public class SosigBehavior : MonoBehaviourSingleton<SosigBehavior>
	{
		public int MaxSosigCount = 8;
		[HideInInspector] public int RealSosigCount;

		[Header("Don't spawn sosigs too close or too far from the player")]
		public int IgnoredSpawnersCloseToPlayer = 2;
		public int IgnoredSpawnersFarFromPlayer = 0;

		public bool DespawnDistantSosigs = false;
		public float MaxSosigDistanceFromPlayer = 0;

		public float HearRangeMultiplier = 1f;
		public float SightRangeMultiplier = 1f;

		public List<Transform> Waypoints;
		public List<CustomSosigSpawner> SosigSpawners;

		[HideInInspector] public Dictionary<Sosig, SosigEnemyID> Sosigs;

		public override void Awake()
		{
			base.Awake();

			Sosigs = new Dictionary<Sosig, SosigEnemyID>();
			GameManager.GameStartedEvent += OnGameStarted;
		}

		private void OnGameStarted()
		{
            Progression.Instance.InitEnemyProgression();
            RealSosigCount = GameSettings.MaxSosigCount;

			for (int i = 0; i < RealSosigCount; i++)
			{
				//grab the next sosig type and spawn it
				SpawnSosigRandomPlace(Progression.GetNextSosigType());
			}

			StartCoroutine(UpdateWaypoints());
		}

		[HarmonyPatch(typeof(Sosig), "Start")]
		[HarmonyPostfix]
		private static void SosigSpawned(Sosig __instance)
		{
			// I'm reducing bleed rate multiplier to avoid situations, when player shoots sosig and it dies sometime later, randomly changing player's weapon.
			// But there is a problem with small weapons that just don't have enough damage to kill sosig by other means. That's why I'm increasing damage multiplier.
			__instance.CanBeKnockedOut = false;
			__instance.m_maxUnconsciousTime = 0;
			__instance.BleedRateMult = 0.1f;

			for (int i = 0; i < __instance.Links.Count; i++)
			{
				__instance.Links[i].SetIntegrity(__instance.Links[i].m_integrity * .65f);
			}

			__instance.Links[0].DamMult = 13.5f;
			__instance.Links[1].DamMult = 6f;
			__instance.Links[2].DamMult = 5f;
			__instance.Links[3].DamMult = 4f;

			__instance.MaxHearingRange *= Instance.HearRangeMultiplier;
			__instance.MaxSightRange *= Instance.SightRangeMultiplier;

			if (Instance.DespawnDistantSosigs)
				Instance.StartCoroutine(Instance.CheckSosigDistance(__instance));
		}

		private IEnumerator UpdateWaypoints()
		{
			while (true)
			{
				for (int i = 0; i < Sosigs.Count; i++)
				{
					if (i < 0 || i >= Sosigs.Count)
					{
						continue;
					}
					Sosig sosigdata = Sosigs.ElementAt(i).Key;
					//TODO quick temporary cleanup
					if (sosigdata == null)
					{
						continue;
					}

                    sosigdata.SetCurrentOrder(Sosig.SosigOrder.Assault);
                    sosigdata.CommandAssaultPoint(Waypoints[Random.Range(0, Waypoints.Count)].position);
				}

				yield return new WaitForSeconds(Random.Range(12, 25));
			}
		}

		private IEnumerator CheckSosigDistance(Sosig sosig)
		{
			WaitForSeconds delay = new WaitForSeconds(6f);
			while (sosig != null)
			{
				yield return delay;

				if (Vector3.Distance(GM.CurrentPlayerBody.transform.position, sosig.transform.position) > MaxSosigDistanceFromPlayer)
				{
					if (Progression.DeadSosigs.Contains(sosig.GetInstanceID()))
						break;

					//grab sosig's type, then remove from list and despawn
					SosigEnemyID sosigtype = Sosigs[sosig];
					Sosigs.Remove(sosig);
					sosig.DeSpawnSosig();
					//spawn new instance of same type of sosig
					Instance.SpawnSosigRandomPlace(sosigtype);

					break;
				}
			}
		}

		public void SpawnSosigRandomPlace(SosigEnemyID sosigtype)
		{
			// ignore two closest spawners to the player
			List<CustomSosigSpawner> availableSpawners = SosigSpawners
				.OrderBy(spawner => Vector3.Distance(spawner.transform.position, GM.CurrentPlayerBody.transform.position))
				.ToList();

			if (IgnoredSpawnersCloseToPlayer > availableSpawners.Count
			|| IgnoredSpawnersFarFromPlayer > availableSpawners.Count
			|| IgnoredSpawnersCloseToPlayer + IgnoredSpawnersFarFromPlayer > availableSpawners.Count)
			{
				Debug.LogError("Ignoring more spawners than available, aborting");
			}

			int random = Random.Range(IgnoredSpawnersCloseToPlayer, availableSpawners.Count - IgnoredSpawnersFarFromPlayer);

            SpawnedSosigInfo sosigdata = availableSpawners[random].Spawn(sosigtype);
			Sosigs.Add(sosigdata.SpawnedSosig, sosigdata.SosigType);
		}

		public void OnSosigKilled(Sosig sosig)
		{
            Sosigs.Remove(sosig);
        }

		public void ClearSosigs()
		{
			List<Sosig> TempSosigsList = new List<Sosig>();
            foreach (Sosig sosig in Sosigs.Keys)
			{
				TempSosigsList.Add(sosig);
			}
            foreach (Sosig sosig in TempSosigsList)
			{
				Sosigs.Remove(sosig);
                sosig.DeSpawnSosig();
				SosigEnemyID sosigType = Progression.GetNextSosigType();
				//Add a delay to hopefully avoid clipping
                yield return new WaitForSeconds(1f);
                Instance.SpawnSosigRandomPlace(sosigType);
            }
		}

		private void OnDestroy()
		{
			GameManager.GameStartedEvent -= OnGameStarted;
		}
	}
}
