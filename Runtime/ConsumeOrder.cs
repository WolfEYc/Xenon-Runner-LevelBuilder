#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    public struct ConsumeOrder
    {
        public Steamworks.SteamItemDetails_t detail;
        public uint quantity;
    }
#elif FACEPUNCH
    public struct ConsumeOrder
    {
        public Steamworks.InventoryItem detail;
        public int quantity;
    }
#endif
}
#endif