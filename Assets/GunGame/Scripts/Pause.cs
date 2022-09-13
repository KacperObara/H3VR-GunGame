using System;
using System.Collections;
using System.Collections.Generic;
using FistVR;
using GunGame.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [SerializeField] private Transform PauseWaypoint;
    [SerializeField] private Transform UnPauseWaypoint;

    [SerializeField] private Text TimeText;
    [SerializeField] private Text KillsText;
    [SerializeField] private Text DeathsText;

    private float _pauseTimer;

    public void PauseTheGame()
    {
        GM.CurrentMovementManager.TeleportToPoint(PauseWaypoint.position, true, transform.position + transform.forward);

        KillsText.text = "Kills: " + GameManager.Instance.Kills;
        DeathsText.text = "Deaths: " + GameManager.Instance.Deaths;

        TimeSpan time = TimeSpan.FromSeconds(GameManager.Instance.GameTime);
        TimeText.text = "Time: " + time.Hours.ToString("00") + ":" + time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");

        _pauseTimer = Time.time;
    }

    public void UnpauseTheGame()
    {
        GM.CurrentMovementManager.TeleportToPoint(UnPauseWaypoint.position, true, transform.position + transform.forward);

        float timePaused = Time.time - _pauseTimer;
        GameManager.Instance.RemovePausedTime(timePaused);
    }
}
