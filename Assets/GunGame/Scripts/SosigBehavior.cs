using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FistVR;
using GunGame.Scripts.Options;
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

		[HideInInspector] public List<Sosig> Sosigs;

		public override void Awake()
		{
			base.Awake();

			Sosigs = new List<Sosig>();
			GameManager.GameStartedEvent += OnGameStarted;
		}

		private void OnGameStarted()
		{
			RealSosigCount = GameSettings.MaxSosigCount;

			for (int i = 0; i < RealSosigCount; i++)
			{
				SpawnSosigRandomPlace();
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
						continue;

					// TODO quick temporary cleanup
					if (Sosigs[i] == null)
					{
						Sosigs.RemoveAt(i);
						--i;
						if (i < 0)
							continue;
					}

					Sosigs[i].SetCurrentOrder(Sosig.SosigOrder.Assault);
					Sosigs[i].CommandAssaultPoint(Waypoints[Random.Range(0, Waypoints.Count)].position);
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

					sosig.DeSpawnSosig();
					Instance.SpawnSosigRandomPlace();

					break;
				}
			}
		}

		public void SpawnSosigRandomPlace()
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

			Sosig sosig = availableSpawners[random].Spawn();
			Sosigs.Add(sosig);
		}

		private void OnDestroy()
		{
			GameManager.GameStartedEvent -= OnGameStarted;
		}
	}
}
