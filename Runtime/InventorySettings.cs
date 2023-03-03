#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [Serializable]
    public class InventorySettings
    {
        public Currency.Code LocalCurrencyCode
        {
            get;
            private set;
        }
        public string LocalCurrencySymbol => Currency.GetSymbol(LocalCurrencyCode);
        public List<ItemDefinition> items = new List<ItemDefinition>();
        public bool runTimeUpdateItemDefinitions = false;

        public InventoryChangedEvent EventChanged
        {
            get
            {
                if (eventChanged == null)
                    eventChanged = new InventoryChangedEvent();

                return eventChanged;
            }
        }
        private InventoryChangedEvent eventChanged = new InventoryChangedEvent();

        public void Load()
        {
            API.Inventory.Client.EventSteamInventoryDefinitionUpdate.AddListener(HandleDefinitionLoad);
            API.Inventory.Client.EventSteamInventoryResultReady.AddListener(HandleItemResults);
            if (items.Count > 0)
            {
                API.Inventory.Client.GetAllItems();
                API.Inventory.Client.RequestPrices((responce, ioError) =>
                {
                    if (ioError || responce.m_result != EResult.k_EResultOK)
                    {
                        LocalCurrencyCode = Currency.Code.Unknown;
                        Debug.LogWarning("Failed to fetch current prices for the list of available inventory items.\nSteam Responce: " + responce.m_result.ToString());
                    }
                    else
                    {
                        LocalCurrencyCode = (Currency.Code)Enum.Parse(typeof(Currency.Code), responce.m_rgchCurrency.ToUpper());
                    }
                });
            }
        }

#if UNITY_EDITOR
        public void UpdateItemDefinitions()
        {
            if (API.Inventory.Client.GetItemDefinitionIDs(out SteamItemDef_t[] results))
            {
                List<ItemDefinition> bundles = new List<ItemDefinition>();
                Dictionary<ItemDefinition, string> craftable = new Dictionary<ItemDefinition, string>();
                Dictionary<ItemDefinition, string> generators = new Dictionary<ItemDefinition, string>();

                for (int i = 0; i < results.Length; i++)
                {
                    try
                    {
                        var itemDefId = results[i];
                        var target = items.FirstOrDefault(p => p.item_itemdefid == itemDefId.m_SteamItemDef);
                        var created = false;
                        if (target == null)
                        {
                            created = true;
                            target = ScriptableObject.CreateInstance<ItemDefinition>();
                        }

                        target.item_itemdefid = itemDefId.m_SteamItemDef;

                        target.item_name.Populate(itemDefId);

                        var namePart = "";

                        if (!string.IsNullOrEmpty(target.item_name.value))
                            namePart = itemDefId.ToString() + " " + target.item_name.value;
                        else if (target.item_name.variants.Count > 0)
                            namePart = itemDefId.ToString() + " " + target.item_name.variants[0].value;
                        else
                            namePart = itemDefId.ToString() + " UNKNOWN";

                        target.name = "[Inv] " + namePart;

                        target.item_description.Populate(itemDefId);
                        target.item_display_type.Populate(itemDefId);

                        var type = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "type");
                        switch (type)
                        {
                            case "item":
                                target.item_type = Enums.InventoryItemType.item;
                                break;
                            case "bundle":
                                target.item_type = Enums.InventoryItemType.bundle;
                                bundles.Add(target);
                                break;
                            case "generator":
                                target.item_type = Enums.InventoryItemType.generator;
                                bundles.Add(target);
                                Debug.LogWarning("Importing an Item Generator from Steam API ...\nValve deliberately omits the bundle node when importing items from the Steam API. As such the item pool (aka Items, aka Bundle) of this generator will be blank. You must manually reset this value before you export the JSON otherwise the generator will have an empty bundle.\nPlease let Valve know that this is a problem for you and that you would like to see this changed. This is a limitation from Valve and not something Heathen can effect.");
                                break;
                            case "playtimegenerator":
                                target.item_type = Enums.InventoryItemType.playtimegenerator;
                                bundles.Add(target);
                                break;
                            case "tag_generator":
                                target.item_type = Enums.InventoryItemType.tag_generator;
                                break;
                            default:
                                UnityEngine.Debug.LogWarning("Unknown Item Type: " + target.name);
                                break;
                        }

                        target.item_promo.Populate(itemDefId);
                        target.item_drop_start_time = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "drop_start_time");

                        var excahnge = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "exchange");
                        if (!string.IsNullOrEmpty(excahnge) && !craftable.ContainsKey(target))
                            craftable.Add(target, excahnge);

                        target.item_price.Populate(itemDefId);

                        var bgColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "background_color");
                        if (!string.IsNullOrEmpty(bgColor))
                        {
                            if (UnityEngine.ColorUtility.TryParseHtmlString(bgColor, out Color color))
                                target.item_background_color.color = color;
                            else
                                target.item_background_color.use = false;
                        }
                        else
                            target.item_background_color.use = false;

                        var nColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "name_color");
                        if (!string.IsNullOrEmpty(nColor))
                        {
                            if (UnityEngine.ColorUtility.TryParseHtmlString(nColor, out Color color))
                                target.item_name_color.color = color;
                            else
                                target.item_name_color.use = false;
                        }
                        else
                            target.item_name_color.use = false;

                        target.item_icon_url = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url");
                        target.item_icon_url_large = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url_large");

                        var returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "marketable");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool marketable))
                            target.item_marketable = marketable;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradable");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool tradable))
                            target.item_tradable = tradable;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generators");
                        if (!string.IsNullOrEmpty(returnString))
                            generators.Add(target, returnString);

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_hidden");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool store_hidden))
                            target.item_store_hidden = store_hidden;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_limit");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool use_drop_limit))
                            target.item_use_drop_limit = use_drop_limit;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_window");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_drop_window))
                            target.item_tradable = item_use_drop_window;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "granted_manually");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_granted_manually))
                            target.item_tradable = item_granted_manually;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_bundle_price");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_bundle_price))
                            target.item_tradable = item_use_bundle_price;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "auto_stack");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_auto_stack))
                            target.item_tradable = item_auto_stack;

                        target.item_tag_generator_name = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generator_name");
                        target.item_tag_generator_values.Populate(itemDefId);
                        target.item_tags.Populate(itemDefId);

                        target.item_store_tags = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_tags").Split(';'));
                        target.item_store_images = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_images").Split(';'));

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradabitem_drop_limitle");
                        if (!string.IsNullOrEmpty(returnString) && uint.TryParse(returnString, out uint item_drop_limit))
                            target.item_drop_limit = item_drop_limit;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_interval");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_interval))
                            target.item_tradable = item_drop_interval;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_window");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_window))
                            target.item_tradable = item_drop_window;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_max_per_window");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_max_per_window))
                            target.item_tradable = item_drop_max_per_window;

                        //TODO: check for and parse extended properties

                        if (created)
                        {
                            items.Add(target);
#if UNITY_EDITOR
                            UnityEditor.AssetDatabase.AddObjectToAsset(target, SteamSettings.current);
#endif
                        }
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(target);
                        UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                        UnityEditor.EditorUtility.SetDirty(target);
#endif

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to parse item definition load from Valve: " + ex.Message);
                    }
                }

                for (int i = 0; i < bundles.Count; i++)
                {
                    try
                    {
                        var target = bundles[i];
                        target.item_bundle.entries = new List<ItemDefinition.Bundle.Entry>();
                        var bundleData = API.Inventory.Client.GetItemDefinitionProperty(target.Id, "bundle");
                        if (!string.IsNullOrEmpty(bundleData))
                        {
                            var bundleString = bundleData.Split(';');
                            for (int ii = 0; ii < bundleString.Length; ii++)
                            {
                                var bundleItem = bundleString[ii];
                                if (bundleItem.Contains("x"))
                                {
                                    var kvp = bundleItem.Split('x');
                                    var id = int.Parse(kvp[0]);
                                    var count = int.Parse(kvp[1]);
                                    target.item_bundle.entries.Add(new ItemDefinition.Bundle.Entry
                                    {
                                        count = count,
                                        item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                    });
                                }
                                else
                                {
                                    var id = int.Parse(bundleItem);
                                    target.item_bundle.entries.Add(new ItemDefinition.Bundle.Entry
                                    {
                                        count = 1,
                                        item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                    });
                                }
                            }
                        }

#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(target);
                        UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                        UnityEditor.EditorUtility.SetDirty(target);
#endif
                    }
                    catch (Exception ex)
                    {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                        Debug.LogError("Failed to process bundle information for " + bundles[i]?.ToString() + "\nException: " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                    }
                }

                foreach (var keyValuePair in craftable)
                {

                    var target = keyValuePair.Key;
                    var schema = keyValuePair.Value;
                    try
                    {
                        target.item_exchange = new ItemDefinition.ExchangeCollection();
                        target.item_exchange.recipe = new List<ItemDefinition.ExchangeRecipe>();
                        var recipies = schema.Split(';');
                        foreach (var recipe in recipies)
                        {
                            var recipieObject = new ItemDefinition.ExchangeRecipe();
                            var materials = recipe.Split(',');
                            foreach (var material in materials)
                            {
                                if (material.Contains(":"))
                                {
                                    //Tag
                                    var tagCat = material.Split(':');
                                    var catigory = tagCat[0];
                                    var tag = string.Empty;
                                    uint count = 1;
                                    if (tagCat[1].Contains("*"))
                                    {
                                        var tagCount = tagCat[1].Split('*');
                                        tag = tagCount[0];
                                        count = uint.Parse(tagCount[1]);
                                    }
                                    else
                                        tag = tagCat[1];

                                    if (recipieObject.materials == null)
                                        recipieObject.materials = new List<ItemDefinition.ExchangeRecipe.Material>();

                                    recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                    {
                                        item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor { item = null, count = 0 },
                                        tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                        {
                                            name = catigory,
                                            value = tag,
                                            count = count
                                        }
                                    });
                                }
                                else
                                {
                                    //Item
                                    if (material.Contains("x"))
                                    {
                                        //Has count
                                        var itemCount = material.Split('x');
                                        var itemID = int.Parse(itemCount[0]);
                                        var count = uint.Parse(itemCount[1]);
                                        var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                        if (recipieObject.materials == null)
                                            recipieObject.materials = new List<ItemDefinition.ExchangeRecipe.Material>();

                                        recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                        {
                                            item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor
                                            {
                                                item = itemTarget,
                                                count = count
                                            },
                                            tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                            {
                                                name = string.Empty,
                                                value = string.Empty,
                                                count = 0
                                            }
                                        });
                                    }
                                    else
                                    {
                                        var itemID = int.Parse(material);
                                        var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                        if (recipieObject.materials == null)
                                            recipieObject.materials = new List<ItemDefinition.ExchangeRecipe.Material>();

                                        recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                        {
                                            item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor
                                            {
                                                item = itemTarget,
                                                count = 1
                                            },
                                            tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                            {
                                                name = string.Empty,
                                                value = string.Empty,
                                                count = 0
                                            }
                                        });
                                    }
                                }
                            }

                            target.item_exchange.recipe.Add(recipieObject);
                        }
                    }
                    catch (Exception ex)
                    {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                        Debug.LogError("Failed to parse excahnge schema for " + target?.ToString() + "; schema = " + schema + "; \n Exception = " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                    }
                }

                foreach (var keyValuePair in generators)
                {
                    var target = keyValuePair.Key;
                    var schema = keyValuePair.Value;

                    if (schema.Contains(";"))
                    {
                        target.item_tag_generators = new List<ItemDefinition>();
                        var gens = schema.Split(';');
                        foreach (var idString in gens)
                        {
                            var id = int.Parse(idString);
                            var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                            if (targetTagGen != null)
                            {
                                target.item_tag_generators.Add(targetTagGen);
                            }
                        }
                    }
                    else
                    {
                        // Only 1
                        var id = int.Parse(schema);
                        target.item_tag_generators = new List<ItemDefinition>();
                        var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                        if (targetTagGen != null)
                        {
                            target.item_tag_generators.Add(targetTagGen);
                        }
                    }
                }

#if !UNITY_EDITOR
                API.Inventory.Client.GetAllItems(HandleItemResults);
#endif
            }
        }
#endif

        private void HandleDefinitionLoad()
        {
            SteamSettings.behaviour.StartCoroutine(AsyncDefinitionLoad());
        }

        private IEnumerator AsyncDefinitionLoad()
        {
            if (runTimeUpdateItemDefinitions)
            {
                yield return null;

                if (API.Inventory.Client.GetItemDefinitionIDs(out SteamItemDef_t[] results))
                {
                    List<ItemDefinition> bundles = new List<ItemDefinition>();
                    Dictionary<ItemDefinition, string> craftable = new Dictionary<ItemDefinition, string>();
                    Dictionary<ItemDefinition, string> generators = new Dictionary<ItemDefinition, string>();

                    var counter = 0;

                    for (int i = 0; i < results.Length; i++)
                    {
                        counter++;
                        if (counter > 100)
                        {
                            counter = 0;
                            yield return new WaitForEndOfFrame();
                        }

                        try
                        {
                            var itemDefId = results[i];
                            var target = items.FirstOrDefault(p => p.item_itemdefid == itemDefId.m_SteamItemDef);
                            var created = false;
                            if (target == null)
                            {
                                created = true;
                                target = ScriptableObject.CreateInstance<ItemDefinition>();
                            }

                            target.item_itemdefid = itemDefId.m_SteamItemDef;

                            target.item_name.Populate(itemDefId);

                            var namePart = "";

                            if (!string.IsNullOrEmpty(target.item_name.value))
                                namePart = itemDefId.ToString() + " " + target.item_name.value;
                            else if (target.item_name.variants.Count > 0)
                                namePart = itemDefId.ToString() + " " + target.item_name.variants[0].value;
                            else
                                namePart = itemDefId.ToString() + " UNKNOWN";

                            target.name = "[Inv] " + namePart;

                            target.item_description.Populate(itemDefId);
                            target.item_display_type.Populate(itemDefId);

                            var type = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "type");
                            switch (type)
                            {
                                case "item":
                                    target.item_type = Enums.InventoryItemType.item;
                                    break;
                                case "bundle":
                                    target.item_type = Enums.InventoryItemType.bundle;
                                    bundles.Add(target);
                                    break;
                                case "generator":
                                    target.item_type = Enums.InventoryItemType.generator;
                                    bundles.Add(target);
                                    Debug.LogWarning("Importing an Item Generator from Steam API ...\nValve deliberately omits the bundle node when importing items from the Steam API. As such the item pool (aka Items, aka Bundle) of this generator will be blank. You must manually reset this value before you export the JSON otherwise the generator will have an empty bundle.\nPlease let Valve know that this is a problem for you and that you would like to see this changed. This is a limitation from Valve and not something Heathen can effect.");
                                    break;
                                case "playtimegenerator":
                                    target.item_type = Enums.InventoryItemType.playtimegenerator;
                                    bundles.Add(target);
                                    break;
                                case "tag_generator":
                                    target.item_type = Enums.InventoryItemType.tag_generator;
                                    break;
                                default:
                                    UnityEngine.Debug.LogWarning("Unknown Item Type: " + target.name);
                                    break;
                            }

                            target.item_promo.Populate(itemDefId);
                            target.item_drop_start_time = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "drop_start_time");

                            var excahnge = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "exchange");
                            if (!string.IsNullOrEmpty(excahnge) && !craftable.ContainsKey(target))
                                craftable.Add(target, excahnge);

                            target.item_price.Populate(itemDefId);

                            var bgColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "background_color");
                            if (!string.IsNullOrEmpty(bgColor))
                            {
                                if (UnityEngine.ColorUtility.TryParseHtmlString(bgColor, out Color color))
                                    target.item_background_color.color = color;
                                else
                                    target.item_background_color.use = false;
                            }
                            else
                                target.item_background_color.use = false;

                            var nColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "name_color");
                            if (!string.IsNullOrEmpty(nColor))
                            {
                                if (UnityEngine.ColorUtility.TryParseHtmlString(nColor, out Color color))
                                    target.item_name_color.color = color;
                                else
                                    target.item_name_color.use = false;
                            }
                            else
                                target.item_name_color.use = false;

                            target.item_icon_url = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url");
                            target.item_icon_url_large = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url_large");

                            var returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "marketable");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool marketable))
                                target.item_marketable = marketable;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradable");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool tradable))
                                target.item_tradable = tradable;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generators");
                            if (!string.IsNullOrEmpty(returnString))
                                generators.Add(target, returnString);

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_hidden");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool store_hidden))
                                target.item_store_hidden = store_hidden;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_limit");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool use_drop_limit))
                                target.item_use_drop_limit = use_drop_limit;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_window");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_drop_window))
                                target.item_tradable = item_use_drop_window;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "granted_manually");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_granted_manually))
                                target.item_tradable = item_granted_manually;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_bundle_price");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_bundle_price))
                                target.item_tradable = item_use_bundle_price;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "auto_stack");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_auto_stack))
                                target.item_tradable = item_auto_stack;

                            target.item_tag_generator_name = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generator_name");
                            target.item_tag_generator_values.Populate(itemDefId);
                            target.item_tags.Populate(itemDefId);

                            target.item_store_tags = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_tags").Split(';'));
                            target.item_store_images = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_images").Split(';'));

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradabitem_drop_limitle");
                            if (!string.IsNullOrEmpty(returnString) && uint.TryParse(returnString, out uint item_drop_limit))
                                target.item_drop_limit = item_drop_limit;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_interval");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_interval))
                                target.item_tradable = item_drop_interval;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_window");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_window))
                                target.item_tradable = item_drop_window;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_max_per_window");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_max_per_window))
                                target.item_tradable = item_drop_max_per_window;

                            //TODO: check for and parse extended properties

                            if (created)
                            {
                                items.Add(target);
#if UNITY_EDITOR
                                UnityEditor.AssetDatabase.AddObjectToAsset(target, SteamSettings.current);
#endif
                            }
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(target);
                            UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                            UnityEditor.EditorUtility.SetDirty(target);
#endif

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Failed to parse item definition load from Valve: " + ex.Message);
                        }
                    }

                    counter = 0;

                    for (int i = 0; i < bundles.Count; i++)
                    {
                        counter++;
                        if (counter > 100)
                        {
                            counter = 0;
                            yield return new WaitForEndOfFrame();
                        }

                        try
                        {
                            var target = bundles[i];
                            target.item_bundle.entries = new List<ItemDefinition.Bundle.Entry>();
                            var bundleData = API.Inventory.Client.GetItemDefinitionProperty(target.Id, "bundle");
                            if (!string.IsNullOrEmpty(bundleData))
                            {
                                var bundleString = bundleData.Split(';');
                                for (int ii = 0; ii < bundleString.Length; ii++)
                                {
                                    var bundleItem = bundleString[ii];
                                    if (bundleItem.Contains("x"))
                                    {
                                        var kvp = bundleItem.Split('x');
                                        var id = int.Parse(kvp[0]);
                                        var count = int.Parse(kvp[1]);
                                        target.item_bundle.entries.Add(new ItemDefinition.Bundle.Entry
                                        {
                                            count = count,
                                            item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                        });
                                    }
                                    else
                                    {
                                        var id = int.Parse(bundleItem);
                                        target.item_bundle.entries.Add(new ItemDefinition.Bundle.Entry
                                        {
                                            count = 1,
                                            item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                        });
                                    }
                                }
                            }

#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(target);
                            UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                            UnityEditor.EditorUtility.SetDirty(target);
#endif
                        }
                        catch (Exception ex)
                        {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                            Debug.LogError("Failed to process bundle information for " + bundles[i]?.ToString() + "\nException: " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                        }
                    }

                    foreach (var keyValuePair in craftable)
                    {

                        var target = keyValuePair.Key;
                        var schema = keyValuePair.Value;
                        try
                        {
                            target.item_exchange = new ItemDefinition.ExchangeCollection();
                            target.item_exchange.recipe = new List<ItemDefinition.ExchangeRecipe>();
                            var recipies = schema.Split(';');
                            foreach (var recipe in recipies)
                            {
                                var recipieObject = new ItemDefinition.ExchangeRecipe();
                                var materials = recipe.Split(',');
                                foreach (var material in materials)
                                {
                                    if (material.Contains(":"))
                                    {
                                        //Tag
                                        var tagCat = material.Split(':');
                                        var catigory = tagCat[0];
                                        var tag = string.Empty;
                                        uint count = 1;
                                        if (tagCat[1].Contains("*"))
                                        {
                                            var tagCount = tagCat[1].Split('*');
                                            tag = tagCount[0];
                                            count = uint.Parse(tagCount[1]);
                                        }
                                        else
                                            tag = tagCat[1];

                                        if (recipieObject.materials == null)
                                            recipieObject.materials = new List<ItemDefinition.ExchangeRecipe.Material>();

                                        recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                        {
                                            item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor { item = null, count = 0 },
                                            tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                            {
                                                name = catigory,
                                                value = tag,
                                                count = count
                                            }
                                        });
                                    }
                                    else
                                    {
                                        //Item
                                        if (material.Contains("x"))
                                        {
                                            //Has count
                                            var itemCount = material.Split('x');
                                            var itemID = int.Parse(itemCount[0]);
                                            var count = uint.Parse(itemCount[1]);
                                            var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                            if (recipieObject.materials == null)
                                                recipieObject.materials = new List<ItemDefinition.ExchangeRecipe.Material>();

                                            recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                            {
                                                item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor
                                                {
                                                    item = itemTarget,
                                                    count = count
                                                },
                                                tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                                {
                                                    name = string.Empty,
                                                    value = string.Empty,
                                                    count = 0
                                                }
                                            });
                                        }
                                        else
                                        {
                                            var itemID = int.Parse(material);
                                            var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                            if (recipieObject.materials == null)
                                                recipieObject.materials = new List<ItemDefinition.ExchangeRecipe.Material>();

                                            recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                            {
                                                item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor
                                                {
                                                    item = itemTarget,
                                                    count = 1
                                                },
                                                tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                                {
                                                    name = string.Empty,
                                                    value = string.Empty,
                                                    count = 0
                                                }
                                            });
                                        }
                                    }
                                }

                                target.item_exchange.recipe.Add(recipieObject);
                            }
                        }
                        catch (Exception ex)
                        {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                            Debug.LogError("Failed to parse excahnge schema for " + target?.ToString() + "; schema = " + schema + "; \n Exception = " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                        }
                    }

                    foreach (var keyValuePair in generators)
                    {
                        var target = keyValuePair.Key;
                        var schema = keyValuePair.Value;

                        if (schema.Contains(";"))
                        {
                            target.item_tag_generators = new List<ItemDefinition>();
                            var gens = schema.Split(';');
                            foreach (var idString in gens)
                            {
                                var id = int.Parse(idString);
                                var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                                if (targetTagGen != null)
                                {
                                    target.item_tag_generators.Add(targetTagGen);
                                }
                            }
                        }
                        else
                        {
                            // Only 1
                            var id = int.Parse(schema);
                            target.item_tag_generators = new List<ItemDefinition>();
                            var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                            if (targetTagGen != null)
                            {
                                target.item_tag_generators.Add(targetTagGen);
                            }
                        }
                    }

#if !UNITY_EDITOR
                API.Inventory.Client.GetAllItems(HandleItemResults);
#endif
                }
            }
        }

        private void HandleItemResults(InventoryResult results)
        {
            //Create the existing record
            Dictionary<ItemDefinition, List<ItemDetail>> currentState = new Dictionary<ItemDefinition, List<ItemDetail>>();
            Dictionary<ItemDefinition, List<ItemDetail>> newState = new Dictionary<ItemDefinition, List<ItemDetail>>();

            foreach (var item in items)
            {
                currentState.Add(item, new List<ItemDetail>(item.Details.ToArray()));
            }

            foreach (var detail in results.items)
            {
                var def = items.FirstOrDefault(p => p.item_itemdefid == detail.Definition.m_SteamItemDef);
                if (def != null)
                {
                    def.Details.RemoveAll(p => p.ItemId == detail.ItemId);
                    def.Details.Add(detail);
                }
            }

            foreach (var item in items)
            {
                newState.Add(item, new List<ItemDetail>(item.Details.ToArray()));
            }

            //Compare
            List<ItemChangeRecord> changes = new List<ItemChangeRecord>();

            foreach (var kvp in currentState)
            {
                var item = kvp.Key;
                var before = kvp.Value;
                var after = newState[item];

                //Was in before is not in after
                var removed = before.Where(b => !after.Any(a => a.ItemId == b.ItemId));

                //Is in after was not in before
                var added = after.Where(a => !before.Any(b => b.ItemId == a.ItemId));

                //Is in both but count doesn't match
                var bChange = before.Where(b => after.Any(a => a.ItemId == b.ItemId) && after.FirstOrDefault(a => a.ItemId == b.ItemId).Quantity != b.Quantity);
                var aChange = after.Where(a => before.Any(b => b.ItemId == a.ItemId) && before.FirstOrDefault(b => b.ItemId == a.ItemId).Quantity != a.Quantity);

                if (removed.Count() > 0
                    || added.Count() > 0
                    || bChange.Count() > 0)
                {
                    //We have some change so record it
                    List<ItemInstanceChangeRecord> changeRecords = new List<ItemInstanceChangeRecord>();

                    foreach (var r in removed)
                    {
                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = false,
                            changed = false,
                            removed = true,
                            quantityBefore = r.Quantity,
                            quantityAfter = 0,
                            instance = r.ItemId
                        });
                    }

                    foreach (var r in added)
                    {
                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = true,
                            changed = false,
                            removed = false,
                            quantityBefore = 0,
                            quantityAfter = r.Quantity,
                            instance = r.ItemId
                        });
                    }

                    foreach (var r in bChange)
                    {
                        var match = aChange.FirstOrDefault(a => a.ItemId == r.ItemId);

                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = false,
                            changed = true,
                            removed = false,
                            quantityBefore = r.Quantity,
                            quantityAfter = match.Quantity,
                            instance = match.ItemId
                        });
                    }

                    var iChanged = new ItemChangeRecord
                    {
                        item = item,
                        changes = changeRecords.ToArray()
                    };

                    changes.Add(iChanged);

                    try
                    {
                        iChanged.item.EventChanged.Invoke(iChanged);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            if (changes.Count > 0)
            {
                EventChanged.Invoke(new InventoryChangeRecord
                {
                    changes = changes.ToArray()
                });
            }
        }
    }
#elif FACEPUNCH
    [Serializable]
    public class InventorySettings
    {
        public Currency.Code LocalCurrencyCode
        {
            get;
            private set;
        }
        public string LocalCurrencySymbol => Currency.GetSymbol(LocalCurrencyCode);
        public List<ItemDefinition> items = new List<ItemDefinition>();
        public bool runTimeUpdateItemDefinitions = false;

        public InventoryChangedEvent EventChanged
        {
            get
            {
                if (eventChanged == null)
                    eventChanged = new InventoryChangedEvent();

                return eventChanged;
            }
        }
        private InventoryChangedEvent eventChanged = new InventoryChangedEvent();

        public void Load()
        {
            API.Inventory.Client.EventSteamInventoryDefinitionUpdate.AddListener(HandleDefinitionLoad);
            API.Inventory.Client.EventSteamInventoryResultReady.AddListener(HandleItemResults);
            if (items.Count > 0)
            {
                API.Inventory.Client.GetAllItems();
            }
        }

#if UNITY_EDITOR
        public void UpdateItemDefinitions()
        {
            API.Inventory.Client.LoadItemDefinitions();
        }
#endif

        private void HandleDefinitionLoad()
        {
            var results = SteamInventory.Definitions;

            List<ItemDefinition> bundles = new List<ItemDefinition>();
            Dictionary<ItemDefinition, string> craftable = new Dictionary<ItemDefinition, string>();
            Dictionary<ItemDefinition, string> generators = new Dictionary<ItemDefinition, string>();

            var counter = 0;

            for (int i = 0; i < results.Length; i++)
            {
                counter++;
                if (counter > 100)
                {
                    counter = 0;
                    //yield return new WaitForEndOfFrame();
                }

                try
                {
                    var itemDefId = results[i];
                    var target = items.FirstOrDefault(p => p.item_itemdefid == itemDefId.Id);
                    var created = false;
                    if (target == null)
                    {
                        created = true;
                        target = ScriptableObject.CreateInstance<ItemDefinition>();
                    }

                    target.item_itemdefid = itemDefId.Id;

                    target.item_name.Populate(itemDefId);

                    var namePart = "";

                    if (!string.IsNullOrEmpty(target.item_name.value))
                        namePart = itemDefId.ToString() + " " + target.item_name.value;
                    else if (target.item_name.variants.Count > 0)
                        namePart = itemDefId.ToString() + " " + target.item_name.variants[0].value;
                    else
                        namePart = itemDefId.ToString() + " UNKNOWN";

                    target.name = "[Inv] " + namePart;

                    target.item_description.Populate(itemDefId);
                    target.item_display_type.Populate(itemDefId);

                    var type = itemDefId.GetProperty("type");
                    switch (type)
                    {
                        case "item":
                            target.item_type = Enums.InventoryItemType.item;
                            break;
                        case "bundle":
                            target.item_type = Enums.InventoryItemType.bundle;
                            bundles.Add(target);
                            break;
                        case "generator":
                            target.item_type = Enums.InventoryItemType.generator;
                            bundles.Add(target);
                            Debug.LogWarning("Importing an Item Generator from Steam API ...\nValve deliberately omits the bundle node when importing items from the Steam API. As such the item pool (aka Items, aka Bundle) of this generator will be blank. You must manually reset this value before you export the JSON otherwise the generator will have an empty bundle.\nPlease let Valve know that this is a problem for you and that you would like to see this changed. This is a limitation from Valve and not something Heathen can effect.");
                            break;
                        case "playtimegenerator":
                            target.item_type = Enums.InventoryItemType.playtimegenerator;
                            bundles.Add(target);
                            break;
                        case "tag_generator":
                            target.item_type = Enums.InventoryItemType.tag_generator;
                            break;
                        default:
                            UnityEngine.Debug.LogWarning("Unknown Item Type: " + target.name);
                            break;
                    }

                    target.item_promo.Populate(itemDefId);
                    target.item_drop_start_time = itemDefId.GetProperty("drop_start_time");

                    var excahnge = itemDefId.GetProperty("exchange");
                    if (!string.IsNullOrEmpty(excahnge) && !craftable.ContainsKey(target))
                        craftable.Add(target, excahnge);

                    target.item_price.Populate(itemDefId);

                    var bgColor = itemDefId.GetProperty("background_color");
                    if (!string.IsNullOrEmpty(bgColor))
                    {
                        if (UnityEngine.ColorUtility.TryParseHtmlString(bgColor, out Color color))
                            target.item_background_color.color = color;
                        else
                            target.item_background_color.use = false;
                    }
                    else
                        target.item_background_color.use = false;

                    var nColor = itemDefId.GetProperty("name_color");
                    if (!string.IsNullOrEmpty(nColor))
                    {
                        if (UnityEngine.ColorUtility.TryParseHtmlString(nColor, out Color color))
                            target.item_name_color.color = color;
                        else
                            target.item_name_color.use = false;
                    }
                    else
                        target.item_name_color.use = false;

                    target.item_icon_url = itemDefId.GetProperty("icon_url");
                    target.item_icon_url_large = itemDefId.GetProperty("icon_url_large");

                    var returnString = itemDefId.GetProperty("marketable");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool marketable))
                        target.item_marketable = marketable;

                    returnString = itemDefId.GetProperty("tradable");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool tradable))
                        target.item_tradable = tradable;

                    returnString = itemDefId.GetProperty("tag_generators");
                    if (!string.IsNullOrEmpty(returnString))
                        generators.Add(target, returnString);

                    returnString = itemDefId.GetProperty("store_hidden");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool store_hidden))
                        target.item_store_hidden = store_hidden;

                    returnString = itemDefId.GetProperty("use_drop_limit");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool use_drop_limit))
                        target.item_use_drop_limit = use_drop_limit;

                    returnString = itemDefId.GetProperty("use_drop_window");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_drop_window))
                        target.item_tradable = item_use_drop_window;

                    returnString = itemDefId.GetProperty("granted_manually");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_granted_manually))
                        target.item_tradable = item_granted_manually;

                    returnString = itemDefId.GetProperty("use_bundle_price");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_bundle_price))
                        target.item_tradable = item_use_bundle_price;

                    returnString = itemDefId.GetProperty("auto_stack");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_auto_stack))
                        target.item_tradable = item_auto_stack;

                    target.item_tag_generator_name = itemDefId.GetProperty("tag_generator_name");
                    target.item_tag_generator_values.Populate(itemDefId);
                    target.item_tags.Populate(itemDefId);

                    target.item_store_tags = new List<string>(itemDefId.GetProperty("store_tags").Split(';'));
                    target.item_store_images = new List<string>(itemDefId.GetProperty("store_images").Split(';'));

                    returnString = itemDefId.GetProperty("tradabitem_drop_limitle");
                    if (!string.IsNullOrEmpty(returnString) && uint.TryParse(returnString, out uint item_drop_limit))
                        target.item_drop_limit = item_drop_limit;

                    returnString = itemDefId.GetProperty("item_drop_interval");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_interval))
                        target.item_tradable = item_drop_interval;

                    returnString = itemDefId.GetProperty("item_drop_window");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_window))
                        target.item_tradable = item_drop_window;

                    returnString = itemDefId.GetProperty("item_drop_max_per_window");
                    if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_max_per_window))
                        target.item_tradable = item_drop_max_per_window;

                    //TODO: check for and parse extended properties

                    if (created)
                    {
                        items.Add(target);
#if UNITY_EDITOR
                        UnityEditor.AssetDatabase.AddObjectToAsset(target, SteamSettings.current);
#endif
                    }
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(target);
                    UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                    UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                    UnityEditor.EditorUtility.SetDirty(target);
#endif

                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to parse item definition load from Valve: " + ex.Message);
                }
            }

            counter = 0;

            for (int i = 0; i < bundles.Count; i++)
            {
                counter++;
                if (counter > 100)
                {
                    counter = 0;
                    //yield return new WaitForEndOfFrame();
                }

                try
                {
                    var target = bundles[i];
                    target.item_bundle.entries = new List<ItemDefinition.Bundle.Entry>();
                    var bundleData = API.Inventory.Client.GetItemDefinitionProperty(target.Id, "bundle");
                    if (!string.IsNullOrEmpty(bundleData))
                    {
                        var bundleString = bundleData.Split(';');
                        for (int ii = 0; ii < bundleString.Length; ii++)
                        {
                            var bundleItem = bundleString[ii];
                            if (bundleItem.Contains("x"))
                            {
                                var kvp = bundleItem.Split('x');
                                var id = int.Parse(kvp[0]);
                                var count = int.Parse(kvp[1]);
                                target.item_bundle.entries.Add(new ItemDefinition.Bundle.Entry
                                {
                                    count = count,
                                    item = items.FirstOrDefault(p => p.Id.Value == id)
                                });
                            }
                            else
                            {
                                var id = int.Parse(bundleItem);
                                target.item_bundle.entries.Add(new ItemDefinition.Bundle.Entry
                                {
                                    count = 1,
                                    item = items.FirstOrDefault(p => p.Id.Value == id)
                                });
                            }
                        }
                    }

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(target);
                    UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                    UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                    UnityEditor.EditorUtility.SetDirty(target);
#endif
                }
                catch (Exception ex)
                {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                    Debug.LogError("Failed to process bundle information for " + bundles[i]?.ToString() + "\nException: " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                }
            }

            foreach (var keyValuePair in craftable)
            {

                var target = keyValuePair.Key;
                var schema = keyValuePair.Value;
                try
                {
                    target.item_exchange = new ItemDefinition.ExchangeCollection();
                    target.item_exchange.recipe = new List<ItemDefinition.ExchangeRecipe>();
                    var recipies = schema.Split(';');
                    foreach (var recipe in recipies)
                    {
                        var recipieObject = new ItemDefinition.ExchangeRecipe();
                        var materials = recipe.Split(',');
                        foreach (var material in materials)
                        {
                            if (material.Contains(":"))
                            {
                                //Tag
                                var tagCat = material.Split(':');
                                var catigory = tagCat[0];
                                var tag = string.Empty;
                                uint count = 1;
                                if (tagCat[1].Contains("*"))
                                {
                                    var tagCount = tagCat[1].Split('*');
                                    tag = tagCount[0];
                                    count = uint.Parse(tagCount[1]);
                                }
                                else
                                    tag = tagCat[1];

                                if (recipieObject.materials == null)
                                    recipieObject.materials = new List<ItemDefinition.ExchangeRecipe.Material>();

                                recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                {
                                    item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor { item = null, count = 0 },
                                    tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                    {
                                        name = catigory,
                                        value = tag,
                                        count = count
                                    }
                                });
                            }
                            else
                            {
                                //Item
                                if (material.Contains("x"))
                                {
                                    //Has count
                                    var itemCount = material.Split('x');
                                    var itemID = int.Parse(itemCount[0]);
                                    var count = int.Parse(itemCount[1]);
                                    var itemTarget = items.FirstOrDefault(p => p.Id.Value == itemID);

                                    if (recipieObject.materials == null)
                                        recipieObject.materials = new List<ItemDefinition.ExchangeRecipe.Material>();

                                    recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                    {
                                        item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor
                                        {
                                            item = itemTarget,
                                            count = count
                                        },
                                        tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                        {
                                            name = string.Empty,
                                            value = string.Empty,
                                            count = 0
                                        }
                                    });
                                }
                                else
                                {
                                    var itemID = int.Parse(material);
                                    var itemTarget = items.FirstOrDefault(p => p.Id.Value == itemID);

                                    if (recipieObject.materials == null)
                                        recipieObject.materials = new List<ItemDefinition.ExchangeRecipe.Material>();

                                    recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                    {
                                        item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor
                                        {
                                            item = itemTarget,
                                            count = 1
                                        },
                                        tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                        {
                                            name = string.Empty,
                                            value = string.Empty,
                                            count = 0
                                        }
                                    });
                                }
                            }
                        }

                        target.item_exchange.recipe.Add(recipieObject);
                    }
                }
                catch (Exception ex)
                {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                    Debug.LogError("Failed to parse excahnge schema for " + target?.ToString() + "; schema = " + schema + "; \n Exception = " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                }
            }

            foreach (var keyValuePair in generators)
            {
                var target = keyValuePair.Key;
                var schema = keyValuePair.Value;

                if (schema.Contains(";"))
                {
                    target.item_tag_generators = new List<ItemDefinition>();
                    var gens = schema.Split(';');
                    foreach (var idString in gens)
                    {
                        var id = int.Parse(idString);
                        var targetTagGen = items.FirstOrDefault(p => p.Id.Value == id);
                        if (targetTagGen != null)
                        {
                            target.item_tag_generators.Add(targetTagGen);
                        }
                    }
                }
                else
                {
                    // Only 1
                    var id = int.Parse(schema);
                    target.item_tag_generators = new List<ItemDefinition>();
                    var targetTagGen = items.FirstOrDefault(p => p.Id.Value == id);
                    if (targetTagGen != null)
                    {
                        target.item_tag_generators.Add(targetTagGen);
                    }
                }
            }

#if !UNITY_EDITOR
                API.Inventory.Client.GetAllItems(HandleItemResults);
#endif

        }

        private void HandleItemResults(InventoryItem[] results)
        {
            //Create the existing record
            Dictionary<ItemDefinition, List<InventoryItem>> currentState = new Dictionary<ItemDefinition, List<InventoryItem>>();
            Dictionary<ItemDefinition, List<InventoryItem>> newState = new Dictionary<ItemDefinition, List<InventoryItem>>();

            foreach (var item in items)
            {
                currentState.Add(item, new List<InventoryItem>(item.Details.ToArray()));
            }

            foreach (var detail in results)
            {
                var def = items.FirstOrDefault(p => p.item_itemdefid == detail.DefId.Value);
                if (def != null)
                {
                    def.Details.RemoveAll(p => p.Id == detail.Id);
                    def.Details.Add(detail);
                }
            }

            foreach (var item in items)
            {
                newState.Add(item, new List<InventoryItem>(item.Details.ToArray()));
            }

            //Compare
            List<ItemChangeRecord> changes = new List<ItemChangeRecord>();

            foreach (var kvp in currentState)
            {
                var item = kvp.Key;
                var before = kvp.Value;
                var after = newState[item];

                //Was in before is not in after
                var removed = before.Where(b => !after.Any(a => a.Id == b.Id));

                //Is in after was not in before
                var added = after.Where(a => !before.Any(b => b.Id == a.Id));

                //Is in both but count doesn't match
                var bChange = before.Where(b => after.Any(a => a.Id == b.Id) && after.FirstOrDefault(a => a.Id == b.Id).Quantity != b.Quantity);
                var aChange = after.Where(a => before.Any(b => b.Id == a.Id) && before.FirstOrDefault(b => b.Id == a.Id).Quantity != a.Quantity);

                if (removed.Count() > 0
                    || added.Count() > 0
                    || bChange.Count() > 0)
                {
                    //We have some change so record it
                    List<ItemInstanceChangeRecord> changeRecords = new List<ItemInstanceChangeRecord>();

                    foreach (var r in removed)
                    {
                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = false,
                            changed = false,
                            removed = true,
                            quantityBefore = r.Quantity,
                            quantityAfter = 0,
                            instance = r.Id
                        });
                    }

                    foreach (var r in added)
                    {
                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = true,
                            changed = false,
                            removed = false,
                            quantityBefore = 0,
                            quantityAfter = r.Quantity,
                            instance = r.Id
                        });
                    }

                    foreach (var r in bChange)
                    {
                        var match = aChange.FirstOrDefault(a => a.Id == r.Id);

                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = false,
                            changed = true,
                            removed = false,
                            quantityBefore = r.Quantity,
                            quantityAfter = match.Quantity,
                            instance = match.Id
                        });
                    }

                    var iChanged = new ItemChangeRecord
                    {
                        item = item,
                        changes = changeRecords.ToArray()
                    };

                    changes.Add(iChanged);

                    try
                    {
                        iChanged.item.EventChanged.Invoke(iChanged);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            if (changes.Count > 0)
            {
                EventChanged.Invoke(new InventoryChangeRecord
                {
                    changes = changes.ToArray()
                });
            }
        }
    }
#endif
}
#endif