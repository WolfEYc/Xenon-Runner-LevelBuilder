#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using HeathenEngineering.Events;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    public class InputActionEvent : MonoBehaviour
    {
        [SerializeField]
        private InputAction action;

        public ActionDataEvent changed;

        private void Start()
        {
            if (action != null)
                action.AddListener(HandleEvent);
        }

        private void OnDestroy()
        {
            if (action != null)
                action.RemoveListener(HandleEvent);
        }

        private void HandleEvent(EventData<InputActionUpdate> arg0)
        {
            changed.Invoke(arg0.value);
        }

        [Serializable]
        public class ActionDataEvent : UnityEvent<InputActionUpdate>
        { }
    }
}
#endif