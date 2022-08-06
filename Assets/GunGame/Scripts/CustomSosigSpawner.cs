using FistVR;
using Sodalite.Api;
using UnityEngine;

namespace GunGame.Scripts
{
	public class CustomSosigSpawner : MonoBehaviour
	{
		public SosigEnemyID SosigType;
		public Sosig.SosigOrder SpawnState;
		public int IFF;

		public Sosig Spawn()
		{
			SosigAPI.SpawnOptions spawnOptions = new SosigAPI.SpawnOptions()
			{
				SpawnActivated = true,
				SpawnState = this.SpawnState,
				IFF = this.IFF,
				SpawnWithFullAmmo = true,
				EquipmentMode = SosigAPI.SpawnOptions.EquipmentSlots.All,
				SosigTargetPosition = this.transform.position,
				SosigTargetRotation = this.transform.eulerAngles
			};

			SosigEnemyTemplate template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[this.SosigType];

			Sosig newSosig = SosigAPI.Spawn(template, spawnOptions, this.transform.position, this.transform.rotation);

			return newSosig;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.5f);
			Gizmos.DrawSphere(this.transform.position, 0.1f);
			Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 0.25f);
		}
	}
}
