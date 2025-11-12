using Duckov.Economy;
using Duckov.Economy.UI;
using HarmonyLib;
using SodaCraft.Localizations;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace OneClickSell
{
    [HarmonyPatch(typeof(StockShop), "ShowUI")]
    internal static class StockShop_ShowUI_Patch
    {
        private static GameObject _sellAllButtonGO;

        static void Postfix(StockShop __instance)
        {
            try
            {
                var view = StockShopView.Instance;
                if (view == null) return;

                // 尝试取得 playerInventoryDisplay，并从中通过取到 sortButton
                Transform parent = null;
                GameObject sortGo = null;
                var playerInvField = typeof(StockShopView).GetField("playerInventoryDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
                if (playerInvField != null)
                {
                    var playerInvObj = playerInvField.GetValue(view) as Component;
                    if (playerInvObj != null)
                    {
                        var invType = playerInvObj.GetType();
                        var sortField = invType.GetField("sortButton", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (sortField != null)
                        {
                            try
                            {
                                var sortBtn = sortField.GetValue(playerInvObj) as Button;
                                if (sortBtn != null)
                                {
                                    sortGo = sortBtn.gameObject;
                                    var sortTransform = sortBtn.transform;
                                    parent = sortTransform.parent ?? playerInvObj.transform;
                                }
                            }
                            catch { /* ignore */ }
                        }

                        // 正向查找，但是不用Sort按钮的样式就有问题，Sort都找不到还是算了，懒得画按钮了
                        //if (parent == null)
                        //{
                        //    Transform titleBarTransform = playerInvObj.transform.Find("TitleBar (1)/TitleBar");
                        //    if (titleBarTransform == null)
                        //    {
                        //        foreach (Transform c in playerInvObj.transform)
                        //        {
                        //            if (c.name.IndexOf("TitleBar", StringComparison.OrdinalIgnoreCase) >= 0)
                        //            {
                        //                var t = c.Find("TitleBar");
                        //                titleBarTransform = t ?? c;
                        //                break;
                        //            }
                        //        }
                        //    }
                        //    parent = (titleBarTransform != null) ? titleBarTransform : playerInvObj.transform;
                        //}
                    }
                }

                // 若之前已创建并且父对象未变则重用
                if (_sellAllButtonGO != null)
                {
                    if (_sellAllButtonGO.transform.parent == parent)
                    {
                        _sellAllButtonGO.SetActive(true);
                        return;
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(_sellAllButtonGO);
                        _sellAllButtonGO = null;
                    }
                }

                if (sortGo == null)
                {
                    Debug.LogError("OneClickSell: can't find sort button to clone; aborting create sell-all button.");
                    return;
                }

                GameObject clone;
                try
                {
                    clone = UnityEngine.Object.Instantiate(sortGo, parent, false);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"OneClickSell: Instantiate(sortGo) failed: {ex.Message}");
                    return;
                }

                try
                {
                    clone.name = "OneClickSell_Button";

                    var cloneButton = clone.GetComponent<Button>();
                    if (cloneButton == null)
                    {
                        Debug.LogError("OneClickSell: cloned object doesn't have a Button component.");
                        UnityEngine.Object.Destroy(clone);
                        return;
                    }

                    UpdateButtonVisual(cloneButton, ModBehaviour.i18n_Key_SellAllItem, Color.green);

                    cloneButton.onClick.RemoveAllListeners();
                    cloneButton.onClick.AddListener(() => ModBehaviour.Instance.SellAll(__instance));

                    _sellAllButtonGO = clone;
                    _sellAllButtonGO.SetActive(true);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"OneClickSell: error configuring cloned button: {ex.Message}");
                    UnityEngine.Object.Destroy(clone);
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void UpdateButtonVisual(Button btn, string i18nKey, Color color)
        {
            try
            {
                var textComp = btn.GetComponentInChildren<TextLocalizor>();
                if (textComp != null)
                {
                    textComp.Key = i18nKey;
                }

                //color修改失败，放弃了

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
