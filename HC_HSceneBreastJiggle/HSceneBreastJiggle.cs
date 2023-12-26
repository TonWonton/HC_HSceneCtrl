﻿using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using BepInEx.Configuration;
using Character;
using H;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using HarmonyLib;
using ILLGames.ADV.Commands.Base;
using BepInEx.Logging;
using Il2CppInterop.Runtime.Injection;


namespace HC_HSceneBreastJiggle
{
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    public class HSceneBreastJiggle : BasePlugin
    {
        public const string PluginName = "HC_HSceneBreastJiggle";
        public const string GUID = "HC_HSceneBreastJiggle";
        public const string PluginVersion = "0.0.1";

        //Breast softness
        public static ConfigEntry<bool> EnableBreastChange;
        public static ConfigEntry<float> BaseSoftness;
        public static ConfigEntry<float> TipSoftness;
        public static ConfigEntry<float> BreastSizeScalingMultiplier;
        public static ConfigEntry<float> Softness;
        public static ConfigEntry<bool> ScaleSoftness;
        //Instance
        public static HScene hScene;
        //Variables
        public static Human[] hSceneFemales;
        public static float[] originalBreastData = new float[3];
        public static int femalesCount;
        public static bool applied;

        public static ManualLogSource log = new ManualLogSource("HC_HSceneBreastJiggle");

        public override void Load()
        {
            //Breast softness
            EnableBreastChange = Config.Bind("Breast softness", "Enable custom breast softness values during HScene", false, "Enable custom values during HScene\nOnly affects HScene");
            BaseSoftness = Config.Bind("Breast softness", "Base softness", 0.5f, new ConfigDescription("Set base softness", new AcceptableValueRange<float>(0f, 1f)));
            TipSoftness = Config.Bind("Breast softness", "Tip softness", 0.5f, new ConfigDescription("Set tip softness", new AcceptableValueRange<float>(0f, 1f)));
            Softness = Config.Bind("Breast softness", "Weight", 0.5f, new ConfigDescription("Set weight", new AcceptableValueRange<float>(0f, 1f)));
            BreastSizeScalingMultiplier = Config.Bind("Breast softness", "How much breast softness scales exponentially down with breast size", 0.5f, new ConfigDescription
                ("Higher values = less bounce on larger breasts. Smaller breasts are affected exponentially less.\n100% = 0% bounce on max size, 0% = same as settings.", new AcceptableValueRange<float>(0f, 1f)));
            ScaleSoftness = Config.Bind("Breast softness", "Also scale weight down with size", true, "Also scale weight down with breast size");
            EnableBreastChange.SettingChanged += (sender, args) => SaveAndApplyData();
            BaseSoftness.SettingChanged += (sender, args) => SaveAndApplyData();
            TipSoftness.SettingChanged += (sender, args) => SaveAndApplyData();
            BreastSizeScalingMultiplier.SettingChanged += (sender, args) => SaveAndApplyData();
            Softness.SettingChanged += (sender, args) => SaveAndApplyData();
            ScaleSoftness.SettingChanged += (sender, args) => SaveAndApplyData();

            BepInEx.Logging.Logger.Sources.Add(log);
            Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
        }

        public static void GetFemales()
        {
            if (hScene != null)
            {
                //Get females from HScene refrence array and put into array for saving and updating data
                Il2CppReferenceArray<Human> females = hScene.GetFemales();
                List<Human> femalesList = new List<Human>();
                foreach (Human female in females)
                {
                    if (female != null)
                    {
                        femalesList.Add(female);
                    }
                }
                femalesCount = femalesList.Count;
                hSceneFemales = new Human[femalesCount];
                hSceneFemales = femalesList.ToArray();
            }
        }

        public static void SaveAndApplyData()
        {
            //If enabled, apply values
            if (EnableBreastChange.Value)
            {
                for (int i = 0; i < femalesCount; i++)
                {
                    SaveBreastData(i);
                    //Calculates new breast softness multiplier based on breast size
                    float breastSizeMultiplier = ((hSceneFemales[i].body.breastFixed.bustSize * hSceneFemales[i].body.breastFixed.bustSize * hSceneFemales[i].body.breastFixed.bustSize
                                                   * BreastSizeScalingMultiplier.Value * hSceneFemales[i].body.breastFixed.bustSize * -1) + 1);
                    //Apply breast softness values
                    hSceneFemales[i].fileCustom.Body.bustSoftness = BaseSoftness.Value * breastSizeMultiplier;
                    hSceneFemales[i].fileCustom.Body.bustSoftness2 = TipSoftness.Value * breastSizeMultiplier;
                    //If else "also scale weight" option is enabled
                    if (ScaleSoftness.Value)
                        hSceneFemales[i].fileCustom.Body.bustWeight = Softness.Value * breastSizeMultiplier;
                    else
                        hSceneFemales[i].fileCustom.Body.bustWeight = Softness.Value;
                    //Update and revert so changes are not permanent
                    hSceneFemales[i].body.UpdateBustShake();
                    RevertBreastData(i);
                }
            }
            //If disabled revert to original values
            else for (int i = 0; i < femalesCount; i++)
            {
                hSceneFemales[i].body.UpdateBustShake();
            }
        }

        public static void SaveBreastData(int i)
        {
            //Save original breast data in array
            originalBreastData = new float[3] { hSceneFemales[i].fileCustom.Body.bustSoftness,
                                                        hSceneFemales[i].fileCustom.Body.bustSoftness2,
                                                        hSceneFemales[i].fileCustom.Body.bustWeight};
        }

        public static void RevertBreastData(int i)
        {
            //Apply original values
            hSceneFemales[i].fileCustom.Body.bustSoftness = originalBreastData[0];
            hSceneFemales[i].fileCustom.Body.bustSoftness2 = originalBreastData[1];
            hSceneFemales[i].fileCustom.Body.bustWeight = originalBreastData[2];
        }

        public static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HScene), "Start")]
            public static void StartHook(HScene __instance)
            {
                //Get instances
                hScene = __instance;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HScene), "ChangeModeCtrl")]
            public static void ChangeModeCtrlHook()
            {
                //Apply only once per HScene
                if (!applied)
                {
                    HSceneBreastJiggle.GetFemales();
                    HSceneBreastJiggle.SaveAndApplyData();
                    applied = true;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HScene), "OnDestroy")]
            public static void HScenePreOnDestroy()
            {
                //Reset variables so settings don't proc outside of HScene
                hScene = null;
                applied = false;
                femalesCount = 0;
            }
        }
    }
}
