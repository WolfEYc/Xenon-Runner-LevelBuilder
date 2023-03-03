#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    public class SteamInputManager : MonoBehaviour
    {
        public static SteamInputManager current;

        [Tooltip("If set to true then we will attempt to force Steam to use input for this app on start.\nThis is generally only needed in editor testing.")]
        [SerializeField]
        private bool forceInput = true;
        [Tooltip("If set to true the system will update every inptu action every frame for every controller found")]
        public bool autoUpdate = true;
        public static bool AutoUpdate
        {
            get => current != null ? current.autoUpdate : false;
            set 
            { 
                if(current != null)
                    current.autoUpdate = value;
            }
        }

        private static Steamworks.InputHandle_t[] controllers = null;
        public static Steamworks.InputHandle_t[] Controllers => controllers;

        private void Start()
        {
            current = this;

            if (forceInput)
                Application.OpenURL($"steam://forceinputappid/{SteamSettings.ApplicationId}");

            SteamworksIntegration.API.Input.Client.RunFrame();
            RefreshControllers();
        }

        private void OnDestroy()
        {
            if(current == this)
                current = null;

            if(forceInput)
                Application.OpenURL("steam://forceinputappid/0");
        }

        private void Update()
        {
            if (autoUpdate)
            {
                if (controllers != null && controllers.Length > 0)
                {
                    foreach (var controller in controllers)
                        SteamSettings.Client.UpdateAllActions(controller);
                }
            }
        }

        public static void UpdateAll()
        {
            if (controllers != null && controllers.Length > 0)
            {
                foreach (var controller in controllers)
                    SteamSettings.Client.UpdateAllActions(controller);
            }
        }

        public static void RefreshControllers()
        {
            controllers = API.Input.Client.ConnectedControllers;
        }
    }
}
#endif