#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    public class InputAction : Events.GameEvent<InputActionUpdate>
    {
        public InputActionType Type
        {
            get => type;
#if UNITY_EDITOR
            set => type = value;
#endif
        }
        public string ActionName
        {
            get => actionName;
#if UNITY_EDITOR
            set => actionName = value;
#endif
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [SerializeField]
        private InputActionType type;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [SerializeField]
        private string actionName;

        public Steamworks.InputAnalogActionHandle_t AnalogHandle => analogHandle;
        public Steamworks.InputDigitalActionHandle_t DigitalHandle => digitalHandle;

        private Steamworks.InputAnalogActionHandle_t analogHandle = new Steamworks.InputAnalogActionHandle_t(0);
        private Steamworks.InputDigitalActionHandle_t digitalHandle = new Steamworks.InputDigitalActionHandle_t(0);
        private Dictionary<Steamworks.InputHandle_t, InputActionData> controllerMapping = null;

        public void UpdateStatus(Steamworks.InputHandle_t controller)
        {
            if (controllerMapping == null)
            {
                controllerMapping = new Dictionary<Steamworks.InputHandle_t, InputActionData>();
                controllerMapping.Add(controller, new InputActionData() { controller = controller, type = type });
            }
            else if (!controllerMapping.ContainsKey(controller))
            {
                controllerMapping.Add(controller, new InputActionData() { controller = controller, type = type });
            }


            if (API.Input.Client.Initialized)
            {
                var current = controllerMapping[controller];

                if (type == InputActionType.Analog)
                {
                    if (analogHandle.m_InputAnalogActionHandle == 0)
                        analogHandle = API.Input.Client.GetAnalogActionHandle(actionName);

                    if (analogHandle.m_InputAnalogActionHandle != 0)
                    {
                        var rawData = API.Input.Client.GetAnalogActionData(controller, analogHandle);

                        var update = new InputActionUpdate
                        {
                            controller = controller,
                            mode = current.mode,
                            type = current.type,
                            wasActive = current.active,
                            wasState = current.state,
                            wasX = current.x,
                            wasY = current.y,
                            isActive = rawData.bActive != 0,
                            isState = rawData.x != 0 || rawData.y != 0,
                            isX = rawData.x,
                            isY = rawData.y,
                        };

                        var change = current.x != rawData.x || current.y != rawData.y;

                        current = update.Data;

                        controllerMapping[controller] = current;
                        if (change)
                        {
                            Raise(this, update);
                        }
                    }
                }
                else
                {
                    if (digitalHandle.m_InputDigitalActionHandle == 0)
                        digitalHandle = API.Input.Client.GetDigitalActionHandle(actionName);

                    if (digitalHandle.m_InputDigitalActionHandle != 0)
                    {
                        var rawData = API.Input.Client.GetDigitalActionData(controller, digitalHandle);

                        var update = new InputActionUpdate
                        {
                            controller = controller,
                            mode = Steamworks.EInputSourceMode.k_EInputSourceMode_None,
                            type = current.type,
                            wasActive = current.active,
                            wasState = current.state,
                            wasX = current.x,
                            wasY = current.y,
                            isActive = rawData.bActive != 0,
                            isState = rawData.bState != 0,
                            isX = rawData.bState,
                            isY = rawData.bState,
                        };

                        var change = rawData.bState != 0 != current.state;

                        current = update.Data;

                        controllerMapping[controller] = current;
                        if (change)
                        {
                            Raise(this, update);
                        }
                    }
                }
            }
        }
        public InputActionData this[Steamworks.InputHandle_t controller]
        {
            get
            {
                if (controllerMapping != null && controllerMapping.ContainsKey(controller))
                    return controllerMapping[controller];
                else
                    return new InputActionData
                    {
                        controller = controller,
                        active = false,
                        mode = Steamworks.EInputSourceMode.k_EInputSourceMode_None,
                        state = false,
                        type = type,
                        x = 0,
                        y = 0,
                    };
            }
        }

        public Texture2D[] GetInputGlyphs(Steamworks.InputHandle_t controller, InputActionSet set) => GetInputGlyphs(controller, set.Handle);
        public Texture2D[] GetInputGlyphs(Steamworks.InputHandle_t controller, InputActionSetLayer set) => GetInputGlyphs(controller, set.Handle);
        public Texture2D[] GetInputGlyphs(Steamworks.InputHandle_t controller, Steamworks.InputActionSetHandle_t set)
        {
            if (type == InputActionType.Analog)
            {
                var origns = API.Input.Client.GetAnalogActionOrigins(controller, set, analogHandle);

                var textArray = new Texture2D[origns.Length];
                for (int i = 0; i < origns.Length; i++)
                {
                    textArray[i] = API.Input.Client.GetGlyphActionOrigin(origns[i]);
                }

                return textArray;
            }
            else
            {
                var origns = API.Input.Client.GetDigitalActionOrigins(controller, set, digitalHandle);

                var textArray = new Texture2D[origns.Length];
                for (int i = 0; i < origns.Length; i++)
                {
                    textArray[i] = API.Input.Client.GetGlyphActionOrigin(origns[i]);
                }

                return textArray;
            }
        }

        public string[] GetInputNames(Steamworks.InputHandle_t controller, InputActionSet set) => GetInputNames(controller, set.Handle);
        public string[] GetInputNames(Steamworks.InputHandle_t controller, InputActionSetLayer set) => GetInputNames(controller, set.Handle);
        public string[] GetInputNames(Steamworks.InputHandle_t controller, Steamworks.InputActionSetHandle_t set)
        {
            if (type == InputActionType.Analog)
            {
                var origns = API.Input.Client.GetAnalogActionOrigins(controller, set, analogHandle);

                var stringArray = new string[origns.Length];
                for (int i = 0; i < origns.Length; i++)
                {
                    stringArray[i] = API.Input.Client.GetStringForActionOrigin(origns[i]);
                }

                return stringArray;
            }
            else
            {
                var origns = API.Input.Client.GetDigitalActionOrigins(controller, set, digitalHandle);

                var stringArray = new string[origns.Length];
                for (int i = 0; i < origns.Length; i++)
                {
                    stringArray[i] = API.Input.Client.GetStringForActionOrigin(origns[i]);
                }

                return stringArray;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(InputAction))]
    public class InputActionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        { }
    }
#endif
}
#endif