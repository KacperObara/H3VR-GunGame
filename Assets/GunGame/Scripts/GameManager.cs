using System;
using System.Collections.Generic;
using FistVR;
using GunGame.Scripts.Options;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Serialization;

namespace GunGame.Scripts
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        [HideInInspector] public int Kills;
        [HideInInspector] public int Deaths;
        public float GameTime { get { return Time.time - _timer; } }

        [HideInInspector] public bool GameEnded;

        public EndArea EndArea;

        public static Action BeforeGameStartedEvent;
        public static Action GameStartedEvent;

        public PlayerSpawner PlayerSpawner;
        public List<Transform> PlayerSpawners;

        private Progression _progression;
        private Harmony _harmony;
        private float _timer;

        public override void Awake()
        {
            base.Awake();

            _progression = GetComponent<Progression>();

            _harmony = Harmony.CreateAndPatchAll(typeof (PlayerSpawner), (string) null);
            _harmony.PatchAll(typeof (SosigBehavior));
            _harmony.PatchAll(typeof (Progression));
        }

        public void StartGame()
        {
            GameEnded = false;

            if (BeforeGameStartedEvent != null)
                BeforeGameStartedEvent.Invoke();

            _progression.SpawnAndEquip();

            PlayerSpawner.MovePlayerToRandomSpawn();

            if (GameStartedEvent != null)
                GameStartedEvent.Invoke();

            _timer = Time.time;
        }

        public void RemovePausedTime(float timePaused)
        {
            _timer += timePaused;
        }

        public void EndGame()
        {
            //GameTime = Time.time - _timer;
            EndArea.EndGame();
            GameEnded = true;
        }

        public void DebugAdvanceWeapon()
        {
            _progression.Promote();
        }

        public void DebugPreviousWeapon()
        {
            _progression.Demote();
        }

        public void DebugTeleport()
        {
            PlayerSpawner.MovePlayerToRandomSpawn();
        }

        public void DebugStart()
        {
            if (BeforeGameStartedEvent != null)
                BeforeGameStartedEvent.Invoke();

            _progression.SpawnAndEquip();

            if (GameStartedEvent != null)
                GameStartedEvent.Invoke();

            _timer = Time.time;
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}
