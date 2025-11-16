using Duckov.Economy;
using HarmonyLib;
using SodaCraft.Localizations;
using UnityEngine;

namespace OneClickSell
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public Harmony harmony;

        public static ModBehaviour Instance { get; private set; }

        private void OnEnable()
        {
            Debug.Log("Mod OneClickSell OnEnable!");

            Instance = this;

            harmony = new Harmony("OneClickSell");
            harmony.PatchAll();

            Init();
        }

        private void OnDisable()
        {
            Debug.Log("Mod OneClickSell OnDisable!");

            harmony.UnpatchAll("OneClickSell");

            Instance = null;
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Z))
            //{
            //    CharacterMainControl.Main.PopText("OneClickSell Mod Active!");
            //}

        }

        private void Init()
        {
            i18nDataInit();
        }

        ////////////////////////////////////////////
        /// 本地化
        ////////////////////////////////////////////

        public const string i18n_Key_SellAllItem = "SellAllItem";


        private void i18nDataInit()
        {

            if (LocalizationManager.CurrentLanguage == SystemLanguage.ChineseSimplified ||
                LocalizationManager.CurrentLanguage == SystemLanguage.ChineseTraditional)
            {
                LocalizationManager.SetOverrideText(i18n_Key_SellAllItem, "一键出售");
            }
            else
            {
                LocalizationManager.SetOverrideText(i18n_Key_SellAllItem, "Sell All");
            }
        }

        ////////////////////////////////////////////
        /// 一键拾取
        ////////////////////////////////////////////
        public void SellAll(StockShop shop)
        {
            SellItems.SellItemsToShop(shop);
        }
    }
}
