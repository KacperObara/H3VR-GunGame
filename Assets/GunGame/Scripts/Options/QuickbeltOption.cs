using System.Collections;
using FistVR;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace GunGame.Scripts.Options
{
	public class QuickbeltOption : MonoBehaviour
	{
		public static FVRQuickBeltSlot AmmoQuickbeltSlot;
		public static FVRQuickBeltSlot ExtraQuickbeltSlot;
		private FVRPhysicalObject MagObject;
		private FVRPhysicalObject ExtraObject;
		public Transform MagSpawnPos;

        private string _magName = "MagazineStanag5rnd";
		private string _extraName = "ReflexGamepointRDS";


        private void Awake()
		{
			GameManager.BeforeGameStartedEvent += OnGameStart;
		}

		private IEnumerator Start()
		{
			yield return new WaitForSeconds(0.2f);
            SpawnMag();
            SpawnExtra();
		}

		private void OnGameStart()
		{
            AmmoQuickbeltSlot = GM.CurrentPlayerBody.QBSlots_Internal[0];
            ExtraQuickbeltSlot = GM.CurrentPlayerBody.QBSlots_Internal[1];
            if (MagObject.QuickbeltSlot)
			{
                Debug.Log("Found mag slot item");
				AmmoQuickbeltSlot = MagObject.QuickbeltSlot;
			}
			//the extra is in the default ammo slot, but the ammo isn't anywhere else
			else if (ExtraObject.QuickbeltSlot == GM.CurrentPlayerBody.QBSlots_Internal[0])
			{
                Debug.Log("Default mag slot");
                //Set the ammo to the normal extra default slot
                AmmoQuickbeltSlot = GM.CurrentPlayerBody.QBSlots_Internal[1];
            }
			if (ExtraObject.QuickbeltSlot)
			{
                Debug.Log("Found extra slot item");
                ExtraQuickbeltSlot = ExtraObject.QuickbeltSlot;
			}
            //the mag is in the default extra slot, but the extra isn't anywhere else
            else if (MagObject.QuickbeltSlot == GM.CurrentPlayerBody.QBSlots_Internal[1])
            {
                Debug.Log("Default extra slot");
                //Set the extra to the normal ammo default slot
                ExtraQuickbeltSlot = GM.CurrentPlayerBody.QBSlots_Internal[0];
            }
            //clear slots, just in case
            if (AmmoQuickbeltSlot.CurObject)
            {
                AmmoQuickbeltSlot.CurObject.ClearQuickbeltState();
            }
            if (ExtraQuickbeltSlot.CurObject)
			{
				ExtraQuickbeltSlot.CurObject.ClearQuickbeltState();
			}
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

			//Move the mag over a bit, to add room for the extra
            Vector3 MagPosition = MagSpawnPos.transform.position;
            MagPosition.z -= 0.2f;
            GameObject mag = Instantiate(callback, MagPosition, transform.rotation);
			mag.SetActive(true);
			MagObject = mag.GetComponent<FVRPhysicalObject>();
		}

        private void SpawnExtra()
        {
            FVRObject obj = null;
            if (!IM.OD.TryGetValue(_extraName, out obj))
            {
                Debug.LogError("No object found with id: " + _extraName);
                return;
            }

            var callback = obj.GetGameObject();
			Vector3 ExtraPosition = MagSpawnPos.transform.position;
			ExtraPosition.z += 0.2f;
            //Move the extra over a bit, to add room for the mag
            GameObject extra = Instantiate(callback, ExtraPosition, transform.rotation);
            extra.SetActive(true);
            ExtraObject = extra.GetComponent<FVRPhysicalObject>();
        }

        private void OnDestroy()
		{
			GameManager.BeforeGameStartedEvent -= OnGameStart;
		}
	}
}
