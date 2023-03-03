#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)

namespace HeathenEngineering.SteamworksIntegration.Enums
{
    /// <summary>
    /// The type applied to a Steam Inventory Item
    /// </summary>
    public enum InventoryItemType
    {
        item,
        bundle,
        generator,
        playtimegenerator,
        tag_generator,
    }
}
#endif