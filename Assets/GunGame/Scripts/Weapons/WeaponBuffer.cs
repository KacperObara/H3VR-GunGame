using System.Collections;
using FistVR;
using GunGame.Scripts.Options;
using UnityEngine;

namespace GunGame.Scripts.Weapons
{
	/// <summary>
	/// Helper class, used to spawn weapons and their ammo and keep them in a buffer. Before I created it, everytime a weapon spawned, there was a fps drop.
	/// Now I'm trying to create weapon asynchronously to avoid that, but I can't be sure when everything will be spawned. So I'm trying to get weapon from a buffer,
	/// but when it is not there, I'm spawning it.
	/// I know it's overcomplicated, but it's the first solution that worked in H3VR.
	/// </summary>
	public class WeaponBuffer : MonoBehaviour
	{
		public Transform BufferSpawnPos;

		private FVRPhysicalObject _gun;
		private FVRPhysicalObject _magazineToLoad;
		private FVRPhysicalObject _magazineForQuickbelt;

		private FVRPhysicalObject GetBufferObject(ObjectType type)
		{
			switch (type)
			{
				case ObjectType.Gun:
					return _gun;
				case ObjectType.MagazineToLoad:
					return _magazineToLoad;
				case ObjectType.MagazineForQuickbelt:
					return _magazineForQuickbelt;
			}
			return null;
		}

		// Get next Gun / MagazineToLoad / MagazineForQuickbelt, from buffer or spawn new one
		public FVRPhysicalObject GetFromBuffer(ObjectType type, int index, bool demoted)
		{
            //Debug.Log("Grabbing asset:" + GameSettings.CurrentPool.GetWeapon(index).ToString() + " of type: " + type.ToString());
            FVRPhysicalObject buffer = GetBufferObject(type);
			if (demoted)
			{
				if (buffer)
					Destroy(buffer.gameObject);

				return SpawnImmediate(type, GameSettings.CurrentPool.GetWeapon(index));
			}

			FVRPhysicalObject newObject = buffer;
			if (newObject == null)
				newObject = SpawnImmediate(type, GameSettings.CurrentPool.GetWeapon(index));

			return newObject;
		}

		public void ClearBuffer()
		{
			_gun = null;
			_magazineToLoad = null;
			_magazineForQuickbelt = null;
		}

		// Destroy mag buffer when magazine string is empty, because otherwise previous one remains
		public void DestroyMagBuffer()
		{
			if (_magazineToLoad)
				Destroy(_magazineToLoad.gameObject);
			if (_magazineForQuickbelt)
				Destroy(_magazineForQuickbelt.gameObject);
		}

		public void GenerateBuffer(int currentIndex)
		{
			if (currentIndex + 1 < GameSettings.CurrentPool.GetWeaponCount())
			{
				StartCoroutine(SpawnAsync(ObjectType.Gun, GameSettings.CurrentPool.GetWeapon(currentIndex + 1)));
				StartCoroutine(SpawnAsync(ObjectType.MagazineToLoad, GameSettings.CurrentPool.GetWeapon(currentIndex + 1)));
				StartCoroutine(SpawnAsync(ObjectType.MagazineForQuickbelt, GameSettings.CurrentPool.GetWeapon(currentIndex + 1)));
			}
		}

		// If possible, spawn the object asynchronously. Otherwise, spawn it immediately.
		private IEnumerator SpawnAsync(ObjectType type, GunData gunData)
		{
			string weaponString = "";

			if (type == ObjectType.Gun)
				weaponString = gunData.GunName;
			if (type == ObjectType.MagazineToLoad || type == ObjectType.MagazineForQuickbelt)
				weaponString = gunData.MagName;

			// Get the object and wait for it to load
			FVRObject newObj = null;
			if (!IM.OD.TryGetValue(weaponString, out newObj))
			{
				Debug.LogError("No object found with id: " + weaponString);
				yield break;
			}

			var callback = newObj.GetGameObjectAsync();
			yield return callback;

			// spawn objects in different positions, so the gun won't be loaded by mistake when spawning in the same place
			Vector3 spawnOffset = Vector3.left * (int)type;
			switch (type)
			{
				case ObjectType.Gun:
					_gun = Instantiate(callback.Result, BufferSpawnPos.position + spawnOffset, BufferSpawnPos.rotation).GetComponent<FVRPhysicalObject>();
					_gun.gameObject.SetActive(true);
					break;
				case ObjectType.MagazineToLoad:
					_magazineToLoad = Instantiate(callback.Result, BufferSpawnPos.position + spawnOffset, BufferSpawnPos.rotation).GetComponent<FVRPhysicalObject>();
					_magazineToLoad.gameObject.SetActive(true);
					_magazineToLoad.UsesGravity = false;
					_magazineToLoad.RootRigidbody.isKinematic = true;
					break;
				case ObjectType.MagazineForQuickbelt:
					_magazineForQuickbelt = Instantiate(callback.Result, BufferSpawnPos.position + spawnOffset, BufferSpawnPos.rotation).GetComponent<FVRPhysicalObject>();
					_magazineForQuickbelt.gameObject.SetActive(true);
					break;
			}
		}

		// Spawn object immediately, but it can cause stutter.
		public FVRPhysicalObject SpawnImmediate(ObjectType objectType, GunData gunData)
		{
			string weaponString = "";

			if (objectType == ObjectType.Gun)
				weaponString = gunData.GunName;
			if (objectType == ObjectType.MagazineToLoad || objectType == ObjectType.MagazineForQuickbelt)
				weaponString = gunData.MagName;
			if (objectType == ObjectType.Extra)
				weaponString = gunData.Extra;

			FVRObject obj = null;
			if (!IM.OD.TryGetValue(weaponString, out obj))
			{
				Debug.LogError("No object found with id: " + weaponString);
				return null;
			}

			var callback = obj.GetGameObject();

			GameObject physicalObject = Instantiate(callback, transform.position + Vector3.up, transform.rotation);
			physicalObject.SetActive(true);
			return physicalObject.GetComponent<FVRPhysicalObject>();
		}
	}

	public enum ObjectType
	{
		Gun = 0,
		MagazineToLoad,
		MagazineForQuickbelt,
		Extra
	}
}