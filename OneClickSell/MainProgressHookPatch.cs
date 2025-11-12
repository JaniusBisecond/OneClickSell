using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.Economy.UI;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System;
using System.Reflection;
using TMPro;
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

                // 取 interactionButton 用作回退父容器参考
                var interactionButtonField = typeof(StockShopView).GetField("interactionButton", BindingFlags.Instance | BindingFlags.NonPublic);
                if (interactionButtonField == null) return;
                var interactionButton = interactionButtonField.GetValue(view) as Button;
                if (interactionButton == null) return;

                // 尝试取得 playerInventoryDisplay，并从中通过反射取到 sortButton（用于 clone）
                Transform parent = null;
                RectTransform srcRectForPlacement = null;
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
                                    srcRectForPlacement = sortTransform.GetComponent<RectTransform>();
                                }
                            }
                            catch { /* ignore */ }
                        }

                        // 回退到 TitleBar（若找不到 sort）
                        if (parent == null)
                        {
                            Transform titleBarTransform = playerInvObj.transform.Find("TitleBar (1)/TitleBar");
                            if (titleBarTransform == null)
                            {
                                foreach (Transform c in playerInvObj.transform)
                                {
                                    if (c.name.IndexOf("TitleBar", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        var t = c.Find("TitleBar");
                                        titleBarTransform = t ?? c;
                                        break;
                                    }
                                }
                            }
                            parent = (titleBarTransform != null) ? titleBarTransform : playerInvObj.transform;
                        }
                    }
                }

                // 最终回退：interactionButton 的父或 view 根
                if (parent == null)
                {
                    parent = interactionButton.transform.parent ?? (view as Component)?.transform;
                }
                if (parent == null)
                {
                    parent = (view as Component)?.transform;
                }
                if (parent == null) return;

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

                // 仅尝试克隆 sort 按钮复用样式；失败则记录错误并直接返回（不创建备用按钮）
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

                    // 绑定并替换点击逻辑
                    var cloneButton = clone.GetComponent<Button>();
                    if (cloneButton == null)
                    {
                        Debug.LogError("OneClickSell: cloned object doesn't have a Button component.");
                        UnityEngine.Object.Destroy(clone);
                        return;
                    }

                    cloneButton.onClick.RemoveAllListeners();
                    cloneButton.onClick.AddListener(() => DoSellAll(__instance).Forget());

                    // 使用提供的工具函数更新样式：本地化键 + 绿色背景
                    UpdateButtonVisual(cloneButton, ModBehaviour.i18n_Key_SellAllItem, new Color32(102, 204, 102, 255));

                    // 调整位置：将克隆放在原 sort 左侧（若有 srcRectForPlacement）
                    var cloneRt = clone.GetComponent<RectTransform>();
                    if (cloneRt != null && srcRectForPlacement != null)
                    {
                        // 保持与 sort 相同 anchors/pivot/size，然后向左移动并根据文本调整宽度
                        cloneRt.anchorMin = srcRectForPlacement.anchorMin;
                        cloneRt.anchorMax = srcRectForPlacement.anchorMax;
                        cloneRt.pivot = srcRectForPlacement.pivot;

                        var tmp = clone.GetComponentInChildren<TextMeshProUGUI>();
                        Vector2 pref = tmp != null ? tmp.GetPreferredValues(tmp.text, 1000f, 1000f) : new Vector2(80f, 20f);
                        float paddingH = 28f;
                        float desiredW = pref.x + paddingH;
                        float desiredH = Math.Max(pref.y + 12f, srcRectForPlacement.sizeDelta.y);
                        cloneRt.sizeDelta = new Vector2(desiredW, desiredH);

                        float spacing = 8f;
                        Vector2 srcAnch = srcRectForPlacement.anchoredPosition;
                        float srcHalfW = srcRectForPlacement.sizeDelta.x * 0.5f;
                        float myHalfW = cloneRt.sizeDelta.x * 0.5f;
                        float anchoredX = srcAnch.x - (srcHalfW + myHalfW + spacing);
                        float anchoredY = srcAnch.y + (srcRectForPlacement.sizeDelta.y * (srcRectForPlacement.pivot.y - cloneRt.pivot.y));
                        cloneRt.anchoredPosition = new Vector2(anchoredX, anchoredY);
                    }

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
                var textComp = btn.GetComponentInChildren(typeof(TextLocalizor), true) as MonoBehaviour;
                if (textComp != null)
                {
                    // TextLocalizor has a Key property; use reflection to set it to avoid compile error if type missing
                    var prop = textComp.GetType().GetProperty("Key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(textComp, i18nKey);
                    }
                }

                // 尝试查找名为 "BG" 的子对象并设置其 Image 颜色
                var bgTransform = btn.transform.Find("BG");
                if (bgTransform != null)
                {
                    var img = bgTransform.GetComponent<Image>();
                    if (img != null) img.color = color;
                }
                else
                {
                    // 若没有 BG 节点，直接设置按钮根 Image（常见情况）
                    var rootImg = btn.GetComponent<Image>();
                    if (rootImg != null) rootImg.color = color;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        // DoSellAll 保持空，用户后续自行实现
        private static async UniTaskVoid DoSellAll(StockShop shop)
        {
            await UniTask.Yield();
        }
    }
}
