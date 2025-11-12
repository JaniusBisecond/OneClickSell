using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Economy;
using Duckov.Modding;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Data;
using SodaCraft.Localizations;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OneClickSell
{
    public class SellItems
    {

        private const string merchantID_Fo = "Merchant_Fo";                 //佛哥
        private const string merchantID_Ming = "Merchant_Ming";             //小明
        private const string merchantID_Weapon = "Merchant_Weapon";         //老吴
        private const string merchantID_Equipment = "Merchant_Equipment";   //橘子
        private const string merchantID_Mud = "Merchant_Mud";               //泥巴
        private const string merchantID_Normal = "Merchant_Normal";         //售货机

        public static void SellItemsToShop(StockShop shop)
        {
            if (shop == null)
            {
                Debug.LogError("OneClickSell: SellItemsToShop: shop is null.");
                return;
            }

            try
            {
                // 收集要出售的物品快照，避免在循环中修改集合
                List<Item> toSell = new List<Item>();
                try
                {
                    var playerInv = LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory;
                    if (playerInv == null)
                    {
                        Debug.LogWarning("OneClickSell: SellItemsToShop: player inventory is null.");
                        return;
                    }
                    var count = playerInv.Content.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Item it = playerInv.Content[i];
                        if (it != null)
                        {
                            try
                            {
                                if (!it.CanBeSold) continue;
                                if (ItemWishlist.GetWishlistInfo(it.TypeID).isManuallyWishlisted) continue; //跳过愿望单物品
                                if (playerInv.IsIndexLocked(i)) continue; //跳过锁住的格子
                                if (it.TypeID == 451) continue; //跳过现金
                            }
                            catch
                            {
                                continue;
                            }
                            toSell.Add(it);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                if (toSell.Count == 0)
                {
                    Debug.Log("OneClickSell: SellItemsToShop: no eligible items to sell.");
                    return;
                }

                MethodInfo sellFunc = typeof(StockShop).GetMethod("Sell", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (sellFunc == null)
                {
                    Debug.LogError("OneClickSell: SellItemsToShop: can't find StockShop.Sell method via reflection.");
                    return;
                }

                Debug.Log($"OneClickSell: Selling {toSell.Count} items to shop '{shop.MerchantID}'");

                foreach (Item item in toSell)
                {
                    if (item == null) continue;
                    try
                    {
                        object task = sellFunc.Invoke(shop, new object[] { item });
                        if (task == null)
                        {
                            Debug.LogWarning($"OneClickSell: Sell invocation returned null for item {item.DisplayName} (type {item.TypeID}).");
                            continue;
                        }
                        if (task is UniTask ut)
                        {
                            ut.Forget();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                AudioManager.Post("UI/sell");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
