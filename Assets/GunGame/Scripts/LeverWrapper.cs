#if H3VR_IMPORTED
using System;
using System.Collections;
using FistVR;
using UnityEngine;
using UnityEngine.Events;

namespace CustomScripts
{
    public class LeverWrapper : MonoBehaviour
    {
        public UnityEvent LeverToggleEvent;
        public UnityEvent LeverOnEvent;
        public UnityEvent LeverOffEvent;
        public UnityEvent LeverHoldStartEvent;
        public UnityEvent LeverHoldEndEvent;

        private TrapLever _lever;

        private bool _isOn;

        private bool _isHeld;

        private void Awake()
        {
            _lever = GetComponentInChildren<TrapLever>();
            _lever.MessageTargets.Add(gameObject);
        }

        private void Update()
        {
            if (!_isHeld && _lever.IsHeld)
            {
                OnHoldStart();
            }

            else if (_isHeld && !_lever.IsHeld)
            {
                OnHoldEnd();
            }
        }

        private void OnHoldStart()
        {
            _isHeld = true;
            if (LeverHoldStartEvent != null)
                LeverHoldStartEvent.Invoke();
        }

        private void OnHoldEnd()
        {
            _isHeld = false;
            if (LeverHoldEndEvent != null)
                LeverHoldEndEvent.Invoke();
        }

        // Called by TrapLever message system
        public void ON()
        {
            if (_isOn)
                return;
            _isOn = true;

            if (LeverToggleEvent != null)
                LeverToggleEvent.Invoke();
            if (LeverOnEvent != null)
                LeverOnEvent.Invoke();
        }

        public void OFF()
        {
            if (!_isOn)
                return;
            _isOn = false;

            if (LeverToggleEvent != null)
                LeverToggleEvent.Invoke();
            if (LeverOffEvent != null)
                LeverOffEvent.Invoke();
        }
    }
}
#endif