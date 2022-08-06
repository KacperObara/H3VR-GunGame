using System.Collections;
using FistVR;
using UnityEngine;

namespace GunGame.Scripts.Options
{
	public class QuickbeltOption : MonoBehaviour
	{
		public static FVRQuickBeltSlot AmmoQuickbeltSlot;
		private FVRPhysicalObject MagObject;
		public Transform MagSpawnPos;

		private string _magName = "MagazineStanag5rnd";

		private void Awake()
		{
			GameManager.BeforeGameStartedEvent += OnGameStart;
		}

		private IEnumerator Start()
		{
			yield return new WaitForSeconds(0.2f);
			AmmoQuickbeltSlot = GM.CurrentPlayerBody.QBSlots_Internal[0];
			SpawnMag();
		}

		private void OnGameStart()
		{
			if (MagObject.QuickbeltSlot)
				AmmoQuickbeltSlot = MagObject.QuickbeltSlot;

			// for (int i = 0; i < GM.CurrentPlayerBody.QBSlots_Internal.Count; i++)
			// {
			// 	if (GM.CurrentPlayerBody.QBSlots_Internal[i].CurObject == MagObject)
			// 		AmmoQuickbeltSlot = GM.CurrentPlayerBody.QBSlots_Internal[i];
			// }
		}

		private void SpawnMag()
		{
			FVRObject obj = null;
			if (!IM.OD.TryGetValue(_magName, out obj))
			{
				Debug.LogError("No object found with id: " + _magName);
				return;
			}

			var callback = obj.GetGameObject();

			GameObject mag = Instantiate(callback, MagSpawnPos.transform.position, transform.rotation);
			mag.SetActive(true);
			MagObject = mag.GetComponent<FVRPhysicalObject>();
		}

		private void OnDestroy()
		{
			GameManager.BeforeGameStartedEvent -= OnGameStart;
		}
	}
}
