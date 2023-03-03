#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    public struct ExchangeEntry
    {
        public SteamItemInstanceID_t instance;
        public uint quantity;
    }
#endif
}
#endif