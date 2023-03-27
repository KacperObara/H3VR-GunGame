using UnityEngine;
using UnityEngine.UI;

namespace GunGame.Scripts.Options
{
	public class SelectedPoolUI : MonoBehaviour
	{
		public Text TitleText;
		public Text DescriptionText;

		private void Awake()
		{
			GameSettings.SettingsChanged += OnSettingsChanged;
		}

		private void OnSettingsChanged()
		{
			TitleText.text = GameSettings.CurrentPool.GetName();
			DescriptionText.text = GameSettings.CurrentPool.GetDescription();
		}

		private void OnDestroy()
		{
			GameSettings.SettingsChanged -= OnSettingsChanged;
		}
	}
}
