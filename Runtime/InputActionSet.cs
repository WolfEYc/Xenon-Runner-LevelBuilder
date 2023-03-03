#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    public class InputActionSet : ScriptableObject
    {
        public string setName;
        public Steamworks.InputActionSetHandle_t Handle => handle;

        private Steamworks.InputActionSetHandle_t handle;

        public bool IsActive(Steamworks.InputHandle_t controller)
        {
            if (handle.m_InputActionSetHandle == 0)
                handle = API.Input.Client.GetActionSetHandle(setName);

            if (handle.m_InputActionSetHandle != 0)
            {
                var layers = API.Input.Client.GetCurrentActionSet(controller);
                if (layers.m_InputActionSetHandle == handle.m_InputActionSetHandle)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public void Activate(Steamworks.InputHandle_t controller)
        {
            if (handle.m_InputActionSetHandle == 0)
                handle = API.Input.Client.GetActionSetHandle(setName);

            if (handle.m_InputActionSetHandle != 0)
            {
                API.Input.Client.ActivateActionSet(controller, handle);
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(InputActionSet))]
    public class InputActionSetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        { }
    }
#endif
}
#endif