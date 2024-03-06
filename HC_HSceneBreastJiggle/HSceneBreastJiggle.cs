using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using BepInEx.Configuration;
using Character;
using H;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using HarmonyLib;
using MagicaCloth;

namespace HC_HSceneBreastJiggle
{
    [BepInProcess("HoneyCome")]
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    public class HSceneBreastJiggle : BasePlugin
    {
        public const string PluginName = "HC_HSceneBreastJiggle";
        public const string GUID = "HC_HSceneBreastJiggle";
        public const string PluginVersion = "1.1.0";
        //Breast softness
        public static ConfigEntry<bool> EnableBreastChange;
        public static ConfigEntry<float> BaseSoftness;
        public static ConfigEntry<float> TipSoftness;
        public static ConfigEntry<float> BreastSizeScalingMultiplier;
        public static ConfigEntry<float> Softness;
        public static ConfigEntry<bool> ScaleSoftness;
        //Magica settings
        public static ConfigEntry<int> updateRate;
        //Instance
        public static HScene hScene;
        public static MagicaPhysicsManager magicaPhysicsManager;
        //Variables
        public static Human[] hSceneFemales;
        public static float[] originalBreastData = new float[3];
        public static int femalesCount;
        public static bool applied;

        public override void Load()
        {
            //Breast softness
            EnableBreastChange = Config.Bind("Breast softness", "Enable custom breast softness values during HScene", false, "Enable custom values during HScene\nOnly affects HScene");
            BaseSoftness = Config.Bind("Breast softness", "Softness base", 0.66f, new ConfigDescription("Set base softness", new AcceptableValueRange<float>(0f, 1f)));
            TipSoftness = Config.Bind("Breast softness", "Softness tip", 0.66f, new ConfigDescription("Set tip softness", new AcceptableValueRange<float>(0f, 1f)));
            Softness = Config.Bind("Breast softness", "Softness weight", 0.66f, new ConfigDescription("Set weight", new AcceptableValueRange<float>(0f, 1f)));
            BreastSizeScalingMultiplier = Config.Bind("Breast softness", "Scale down softness with size", 0.25f, new ConfigDescription
                                                                        ("Higher values = less bounce on larger breasts.\nSmaller breasts are affected exponentially less." +
                                                                        "\n100% = 0% bounce on max size, 0% = same as settings.", new AcceptableValueRange<float>(0f, 1f)));
            ScaleSoftness = Config.Bind("Breast softness", "Scale weight up with size", true, "Scale weight up to weight value set\n" +
                                                                                                   "If enabled, smaller breasts will have less weight\n" +
                                                                                                   "Larger breasts will have more weight, scaling up to weight value");
            //Magica settings
            updateRate = Config.Bind("Magica settings", "Update rate", 90, new ConfigDescription("Change update rate of physics calculation\n" +
                                                                                                 "Higher values increases accuracy of physics but uses more CPU\n" +
                                                                                                 "Might drastically change how breasts jiggle\n" +
                                                                                                 "Also affects other physics object, such as clothes and hair", new AcceptableValueList<int>(60, 90, 120, 150, 180)));
            //Update settings
            EnableBreastChange.SettingChanged += (sender, args) => SaveAndApplyData();
            BaseSoftness.SettingChanged += (sender, args) => SaveAndApplyData();
            TipSoftness.SettingChanged += (sender, args) => SaveAndApplyData();
            Softness.SettingChanged += (sender, args) => SaveAndApplyData();
            BreastSizeScalingMultiplier.SettingChanged += (sender, args) => SaveAndApplyData();
            ScaleSoftness.SettingChanged += (sender, args) => SaveAndApplyData();
            updateRate.SettingChanged += (sender, args) => SaveAndApplyData();
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
                //Change update rate
                if (magicaPhysicsManager != null)
                    magicaPhysicsManager.UpdatePerSeccond = (MagicaCloth.UpdateTimeManager.UpdateCount)updateRate.Value;
                for (int i = 0; i < femalesCount; i++)
                {
                    SaveBreastData(i);
                    //Calculates new breast softness multiplier based on breast size
                    float breastSizeMultiplier = ((hSceneFemales[i].body.breastFixed.bustSize * hSceneFemales[i].body.breastFixed.bustSize * hSceneFemales[i].body.breastFixed.bustSize
                                                   * BreastSizeScalingMultiplier.Value * hSceneFemales[i].body.breastFixed.bustSize * -1f) + 1f);
                    //Apply breast softness values
                    hSceneFemales[i].fileCustom.Body.bustSoftness = BaseSoftness.Value * breastSizeMultiplier;
                    hSceneFemales[i].fileCustom.Body.bustSoftness2 = TipSoftness.Value * breastSizeMultiplier;
                    //If else "also scale weight" option is enabled
                    if (ScaleSoftness.Value)
                        hSceneFemales[i].fileCustom.Body.bustWeight = Softness.Value * (1f - breastSizeMultiplier);
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
            [HarmonyPatch(typeof(MagicaPhysicsManager), "Awake")]
            public static void MagicaStartHook(MagicaPhysicsManager __instance)
            {
                //Get instance
                magicaPhysicsManager = __instance;
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
                hSceneFemales = null;
                femalesCount = 0;
            }
        }
    }
}
