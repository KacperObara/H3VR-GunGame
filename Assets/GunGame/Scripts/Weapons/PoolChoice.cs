using GunGame.Scripts.Options;
using UnityEngine;
using UnityEngine.UI;

namespace GunGame.Scripts.Weapons
{
	public class PoolChoice : MonoBehaviour
	{
		public Text TitleText;
		public Button Button;

		private WeaponPoolInterface _weaponPool;

		public void Initialize(WeaponPoolInterface weaponPool)
		{
			_weaponPool = weaponPool;
			TitleText.text = _weaponPool.GetName();
			Button.onClick.AddListener(OnClick);
		}

		public void OnClick()
		{
			GameSettings.ChangeCurrentPool(_weaponPool);
		}
	}
}
