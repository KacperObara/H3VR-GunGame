using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FistVR;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GunGame.Scripts
{
	public class SosigBehavior : MonoBehaviourSingleton<SosigBehavior>
	{
		public int MaxSosigCount = 8;

		[Header("Don't spawn sosigs too close to the player if possible")]
		public int IgnoredSpawnersCloseToPlayer = 2;

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
			for (int i = 0; i < MaxSosigCount; i++)
			{
				SpawnSosigRandomPlace();
			}

			StartCoroutine(UpdateWaypoints());
		}

		[HarmonyPatch(typeof(Sosig), "Start")]
		[HarmonyPostfix]
		private static void Postfix(Sosig __instance)
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

			__instance.Links[0].DamMult = 6.5f;
			__instance.Links[1].DamMult = 4.5f;
			__instance.Links[2].DamMult = 3f;
			__instance.Links[3].DamMult = 2f;
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

		public void SpawnSosigRandomPlace()
		{
			// ignore two closest spawners to the player
			List<CustomSosigSpawner> availableSpawners = SosigSpawners
				.OrderBy(spawner => Vector3.Magnitude(spawner.transform.position - GM.CurrentPlayerBody.transform.position))
				.ToList();

			if (IgnoredSpawnersCloseToPlayer > availableSpawners.Count)
			{
				Debug.LogError("Ignoring more spawners than available, aborting");
				IgnoredSpawnersCloseToPlayer = 0;
			}

			int random = Random.Range(IgnoredSpawnersCloseToPlayer, availableSpawners.Count);

			Sosig sosig = availableSpawners[random].Spawn();
			Sosigs.Add(sosig);
		}

		private void OnDestroy()
		{
			GameManager.GameStartedEvent -= OnGameStarted;
		}
	}
}
