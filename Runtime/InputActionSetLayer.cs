#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    public class InputActionSetLayer : ScriptableObject
    {
        public string layerName;

        public Steamworks.InputActionSetHandle_t Handle => handle;

        private Steamworks.InputActionSetHandle_t handle;

        public bool IsActive(Steamworks.InputHandle_t controller)
        {
            if (handle.m_InputActionSetHandle == 0)
                handle = API.Input.Client.GetActionSetHandle(layerName);

            if (handle.m_InputActionSetHandle != 0)
            {
                var layers = API.Input.Client.GetActiveActionSetLayers(controller);
                if (layers.Any(p => p.m_InputActionSetHandle == handle.m_InputActionSetHandle))
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
                handle = API.Input.Client.GetActionSetHandle(layerName);

            if (handle.m_InputActionSetHandle != 0)
            {
                API.Input.Client.ActivateActionSetLayer(controller, handle);
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(InputActionSetLayer))]
    public class InputActionSetLayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        { }
    }
#endif
}
#endif