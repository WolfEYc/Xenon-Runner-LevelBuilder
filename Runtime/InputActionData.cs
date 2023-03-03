#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)

namespace HeathenEngineering.SteamworksIntegration
{
    [System.Serializable]
    public struct InputActionData
    {
        public Steamworks.InputHandle_t controller;
        public InputActionType type;
        public bool active;
        public Steamworks.EInputSourceMode mode;
        public bool state;
        public float x;
        public float y;

        public override string ToString()
        {
            if(type == InputActionType.Analog)
            {
                if(active)
                {
                    return "Active: X[" + x + "] Y[" + y + "]";
                }
                else
                {
                    return "Inactive";
                }
            }
            else
            {
                if (active)
                {
                    return "Active: " + (state ? "Engaged" : "Idle");
                }
                else
                {
                    return "Inactive";
                }
            }
        }
    }
}
#endif