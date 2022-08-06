using System;
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
        [HideInInspector] public float GameTime;

        public EndArea EndArea;

        public static Action BeforeGameStartedEvent;
        public static Action GameStartedEvent;

        public PlayerSpawner PlayerSpawner;

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
            if (BeforeGameStartedEvent != null)
                BeforeGameStartedEvent.Invoke();

            _progression.SpawnAndEquip();

            PlayerSpawner.MovePlayerToRandomSpawn();

            if (GameStartedEvent != null)
                GameStartedEvent.Invoke();

            _timer = Time.time;
        }

        public void EndGame()
        {
            GameTime = Time.time - _timer;
            EndArea.EndGame();
        }

        public void DebugTeleport()
        {
            PlayerSpawner.MovePlayerToRandomSpawn();
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}
