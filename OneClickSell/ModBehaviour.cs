using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Economy;
using Duckov.Modding;
using Duckov.UI;
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


            LevelManager.OnAfterLevelInitialized += OnAfterLevelChanged;

            Init();
        }

        private void OnDisable()
        {
            Debug.Log("Mod OneClickSell OnDisable!");

            LevelManager.OnAfterLevelInitialized -= OnAfterLevelChanged;

            harmony.UnpatchAll("OneClickSell");

            Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                CharacterMainControl.Main.PopText("OneClickSell Mod Active!");
            }

        }

        private void Init()
        {
            i18nDataInit();
        }

        ////////////////////////////////////////////
        /// 本地化
        ////////////////////////////////////////////

        public const string i18n_Key_StartCollect = "StartCollect";
        public const string i18n_Key_StopCollect = "StopCollect";
        public const string i18n_Key_OpenInv = "OpenInv";
        public const string i18n_Key_SellAllItem = "SellAllItem";

        public const string i18n_Key_RobotBagFull = "RobotBagFull";
        public const string i18n_Key_RobotBagCreateFaild = "RobotBagCreateFaild";
        public const string i18n_Key_RobotStartInspect = "RobotStartInspect";
        public const string i18n_Key_RobotStopInspect = "RobotStopInspect";

        private void i18nDataInit()
        {

            if (LocalizationManager.CurrentLanguage == SystemLanguage.ChineseSimplified ||
                LocalizationManager.CurrentLanguage == SystemLanguage.ChineseTraditional)
            {
                LocalizationManager.SetOverrideText(i18n_Key_StartCollect, "开始收集");
                LocalizationManager.SetOverrideText(i18n_Key_StopCollect, "停止收集");
                LocalizationManager.SetOverrideText(i18n_Key_OpenInv, "打开背包");
                LocalizationManager.SetOverrideText(i18n_Key_SellAllItem, "一键出售");

                LocalizationManager.SetOverrideText(i18n_Key_RobotBagFull, "机器人背包满了");
                LocalizationManager.SetOverrideText(i18n_Key_RobotBagCreateFaild, "机器人背包创建失败");
                LocalizationManager.SetOverrideText(i18n_Key_RobotStartInspect, "机器人开始收集...");
                LocalizationManager.SetOverrideText(i18n_Key_RobotStopInspect, "机器人停止收集!");
            }
            else
            {
                LocalizationManager.SetOverrideText(i18n_Key_StartCollect, "Start Collecting");
                LocalizationManager.SetOverrideText(i18n_Key_StopCollect, "Stop Collecting");
                LocalizationManager.SetOverrideText(i18n_Key_OpenInv, "Open Inventory");
                LocalizationManager.SetOverrideText(i18n_Key_SellAllItem, "Sell All Items");

                LocalizationManager.SetOverrideText(i18n_Key_RobotBagFull, "Robot Inventory Full");
                LocalizationManager.SetOverrideText(i18n_Key_RobotBagCreateFaild, "Robot Inventory Creation Failed");
                LocalizationManager.SetOverrideText(i18n_Key_RobotStartInspect, "Robot Started Collecting...");
                LocalizationManager.SetOverrideText(i18n_Key_RobotStopInspect, "Robot Stopped Collecting!");
            }
        }

        ////////////////////////////////////////////
        /// 一键拾取
        ////////////////////////////////////////////
        public void OnAfterLevelChanged()
        {

        }
    }
}
