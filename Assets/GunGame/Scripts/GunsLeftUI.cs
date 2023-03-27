using System;
using System.Collections;
using System.Collections.Generic;
using GunGame.Scripts;
using GunGame.Scripts.Options;
using UnityEngine;
using UnityEngine.UI;

public class GunsLeftUI : MonoBehaviour
{
    private Text _gunsLeftText;

    private void Awake()
    {
        _gunsLeftText = GetComponent<Text>();

        Progression.WeaponChangedEvent += UpdateUI;
    }

    private void UpdateUI()
    {
        int weaponCount = Mathf.Min(GameSettings.CurrentPool.GetWeaponCount(), WeaponCountOption.WeaponCount);
        int currentWeaponId = Progression.Instance.CurrentWeaponId + 1;

        int gunsLeft = weaponCount - currentWeaponId;
        _gunsLeftText.text = "Guns left: " + gunsLeft;
    }

    private void OnDestroy()
    {
        Progression.WeaponChangedEvent -= UpdateUI;
    }
}
