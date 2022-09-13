using System;
using FistVR;
using UnityEngine;
using UnityEngine.UI;

namespace GunGame.Scripts
{
	public class EndArea : MonoBehaviour
	{
		public Text TimeText;
		public Text KillsText;
		public Text DeathsText;

		public Transform EndPos;

		public void EndGame()
		{
			GM.CurrentMovementManager.TeleportToPoint(EndPos.position, true, transform.position + transform.forward);

			// Move all spawners to the end pos. If the player dies after game ended, he will still be respawned at the end location
			foreach (var spawner in GameManager.Instance.PlayerSpawners)
			{
				spawner.transform.position = EndPos.position;
			}

			KillsText.text = "Kills: " + GameManager.Instance.Kills;
			DeathsText.text = "Deaths: " + GameManager.Instance.Deaths;

			TimeSpan time = TimeSpan.FromSeconds(GameManager.Instance.GameTime);
			TimeText.text = "Time: " + time.Hours.ToString("00") + ":" + time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");
		}
	}
}
