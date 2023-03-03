#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    public class InventoryManager : MonoBehaviour
    {
        public Currency.Code CurrencyCode => SteamSettings.Client.inventory.LocalCurrencyCode;
        public string CurrencySymbol => SteamSettings.Client.inventory.LocalCurrencySymbol;
        public List<ItemDefinition> Items => SteamSettings.Client.inventory.items;

        public InventoryChangedEvent evtChanged;
        public SteamMicroTransactionAuthorizationResponce evtTransactionResponce;

        private void OnEnable()
        {
            SteamSettings.Client.inventory.EventChanged.AddListener(evtChanged.Invoke);
            API.Inventory.Client.EventSteamMicroTransactionAuthorizationResponce.AddListener(evtTransactionResponce.Invoke);
        }

        private void OnDisable()
        {
            SteamSettings.Client.inventory.EventChanged.RemoveListener(evtChanged.Invoke);
            API.Inventory.Client.EventSteamMicroTransactionAuthorizationResponce.RemoveListener(evtTransactionResponce.Invoke);
        }

        /// <summary>
        /// Returns the sub set of items that have a price and are not hidden.
        /// These should be the same items visible in Steam's store
        /// </summary>
        /// <returns></returns>
        public ItemDefinition[] GetStoreItems()
        {
            return Items.Where(i => !i.Hidden && !i.StoreHidden && i.item_price.Valid).ToArray();
        }
    }
}
#endif