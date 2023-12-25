using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Character;
using H;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace Plugin
{
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    public class HC_HSceneCtrl : BasePlugin
    {
        public const string PluginName = "HC_HSceneCtrl";

        public const string GUID = "HC_HSceneCtrl";

        public const string PluginVersion = "0.3.0";

        //Ahegao
        public static ConfigEntry<bool> ahegao;
        public static ConfigEntry<bool> ahegaoOnOrgasm;
        public static ConfigEntry<int> eyePtn;
        public static ConfigEntry<int> eyeBrowPtn;
        public static ConfigEntry<int> mouthPtn;
        public static ConfigEntry<float> eyeRollAmount;
        public static ConfigEntry<float> eyeCrossAmount;
        public static ConfigEntry<float> minSpeed;
        public static ConfigEntry<int> tearsLevel;
        public static ConfigEntry<float> blush;
        //Breast softness
        public static ConfigEntry<bool> EnableBreastChange;
        public static ConfigEntry<float> BaseSoftness;
        public static ConfigEntry<float> TipSoftness;
        public static ConfigEntry<float> BreastSizeScalingMultiplier;
        public static ConfigEntry<float> Softness;
        public static ConfigEntry<bool> ScaleSoftness;
        //Patching GameObject
        public GameObject HSceneCtrl;
        //Females array for applying breast softness
        public static Human[] hSceneFemales;
        public static int femalesCount;
        public static bool doAhegao;
        public static bool doingAhegao;
        public static bool resetAhegao;
        public static float[] originalEyeX = new float[2];
        public static float[] originalEyeY = new float[2];
        public static int[] originalMouth = new int[2];
        public static float[] originalBreastData = new float[3];
        public static H.HVoiceCtrl.Voice[] originalVoice;
        public static HVoiceCtrl.FaceInfo originalFace;
        public static int randomEye;

        public static ManualLogSource log = new ManualLogSource("HC_HSceneCtrl");

        public override void Load()
        {
            //Ahegao
            ahegao = Config.Bind("Ahegao", "Ahegao", true, "Enable ahegao");
            ahegaoOnOrgasm = Config.Bind("Ahegao", "Ahegao on normal orgasm", false);
            eyePtn = Config.Bind("Ahegao", "Eye pattern", 0, new ConfigDescription("Set eye pattern", new AcceptableValueRange<int>(0, 24)));
            eyeBrowPtn = Config.Bind("Ahegao", "Eyebrow pattern", 0, new ConfigDescription("Set eyebrow pattern", new AcceptableValueRange<int>(0, 10)));
            mouthPtn = Config.Bind("Ahegao", "Mouth pattern", 0, new ConfigDescription("Set mouth pattern", new AcceptableValueRange<int>(0, 27)));
            eyeCrossAmount = Config.Bind("Ahegao", "Eye cross offset amount during ahegao and faintness", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeRollAmount = Config.Bind("Ahegao", "Eye roll amount during ahegao and faintness", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            blush = Config.Bind("Ahegao", "Blush amount during ahegao and faintness", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 2.8f)));
            minSpeed = Config.Bind("Ahegao", "Minimum speed for eye roll during faintness", 0.75f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            tearsLevel = Config.Bind("Ahegao", "Tears level during ahegao and faintness", 2, new ConfigDescription("Set tears level", new AcceptableValueList<int>(0, 1, 2, 3)));
            //Breast softness
            EnableBreastChange = Config.Bind("Breast softness", "Enable custom breast softness values during HScene", false, "Enable custom values during HScene\nOnly affects HScene");
            BaseSoftness = Config.Bind("Breast softness", "Base softness", 0.5f, new ConfigDescription("Set base softness", new AcceptableValueRange<float>(0f, 1f)));
            TipSoftness = Config.Bind("Breast softness", "Tip softness", 0.5f, new ConfigDescription("Set tip softness", new AcceptableValueRange<float>(0f, 1f)));
            Softness = Config.Bind("Breast softness", "Weight", 0.5f, new ConfigDescription("Set weight", new AcceptableValueRange<float>(0f, 1f)));
            BreastSizeScalingMultiplier = Config.Bind("Breast softness", "How much breast softness scales exponentially down with breast size", 0.5f, new ConfigDescription
                ("Higher values = less bounce on larger breasts. Smaller breasts are affected exponentially less.\n100% = 0% bounce on max size, 0% = same as settings.", new AcceptableValueRange<float>(0f, 1f)));
            ScaleSoftness = Config.Bind("Breast softness", "Also scale weight down with size", true, "Also scale weight down with breast size");
            EnableBreastChange.SettingChanged += (sender, args) => UpdateBreastSoftness();
            BaseSoftness.SettingChanged += (sender, args) => UpdateBreastSoftness();
            TipSoftness.SettingChanged += (sender, args) => UpdateBreastSoftness();
            BreastSizeScalingMultiplier.SettingChanged += (sender, args) => UpdateBreastSoftness();
            Softness.SettingChanged += (sender, args) => UpdateBreastSoftness();
            ScaleSoftness.SettingChanged += (sender, args) => UpdateBreastSoftness();

            //Patch hook methods and register monobehaviour component
            Harmony.CreateAndPatchAll(typeof(HSceneCtrlComponent.Hooks), GUID);
            ClassInjector.RegisterTypeInIl2Cpp<HSceneCtrlComponent>();
            // Add the monobehavior component to your personal GameObject. Try to not duplicate.
            HSceneCtrl = GameObject.Find("HSceneCtrl");
            if (HSceneCtrl == null)
            {
                HSceneCtrl = new GameObject("HSceneCtrl");
                GameObject.DontDestroyOnLoad(HSceneCtrl);
                HSceneCtrl.hideFlags = HideFlags.HideAndDontSave;
                HSceneCtrl.AddComponent<HSceneCtrlComponent>();
            }
            else HSceneCtrl.AddComponent<HSceneCtrlComponent>();
        }


        public static void GetFemales()
        {
            if (HSceneCtrlComponent.hScene != null)
            {
                //Get females from HScene refrence array and put into array for saving and updating data
                Il2CppReferenceArray<Human> females = HSceneCtrlComponent.hScene.GetFemales();
                List<Human> femalesList = new List<Human>();
                foreach (Human female in females)
                {
                    if (female != null)
                        femalesList.Add(female);
                }
                femalesCount = femalesList.Count;
                hSceneFemales = new Human[femalesList.Count];
                hSceneFemales = femalesList.ToArray();

                List<HVoiceCtrl.Voice> voiceList = new List<HVoiceCtrl.Voice>();
                foreach (HVoiceCtrl.Voice voice in HSceneCtrlComponent.hScene.CtrlVoice.NowVoices)
                { 
                    if (voice != null)
                        voiceList.Add(voice);
                }

                HVoiceCtrl.Voice[] voiceArray = new HVoiceCtrl.Voice[voiceList.Count];
                voiceArray = voiceList.ToArray();
                HSceneCtrlComponent.dummy = new HVoiceCtrl.FaceInfo[voiceList.Count];
                for (int i = 0; i < voiceList.Count; i++)
                {
                    HSceneCtrlComponent.dummy[i] = voiceArray[i].Face;
                }
            }
        }

        public static void SaveData()
        {
            if (femalesCount >= 1)
            {
                for (int i = 0; i < femalesCount; i++)
                {
                    originalBreastData = new float[3] { hSceneFemales[i].fileCustom.Body.bustSoftness, hSceneFemales[i].fileCustom.Body.bustSoftness2, hSceneFemales[i].fileCustom.Body.bustWeight };
                    originalEyeX[i] = hSceneFemales[i].fileCustom.Face.eyeX;
                    originalEyeY[i] = hSceneFemales[i].fileCustom.Face.eyeY;
                }
            }
        }

        public static void ApplyAhegaoFull(int i)
        {
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeEyesPtn(HC_HSceneCtrl.eyePtn.Value);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeEyebrowPtn(HC_HSceneCtrl.eyeBrowPtn.Value);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeMouthPtn(HC_HSceneCtrl.mouthPtn.Value, true);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeEyesOpenMax(1);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(HC_HSceneCtrl.originalEyeX[i] + HC_HSceneCtrl.eyeCrossAmount.Value));
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(HC_HSceneCtrl.originalEyeY[i] + HC_HSceneCtrl.eyeRollAmount.Value));
            HC_HSceneCtrl.hSceneFemales[i].face.fileStatus.tearsLv = (byte)HC_HSceneCtrl.tearsLevel.Value;
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeMouthFixed(true);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeHohoAkaRate(new Il2CppSystem.Nullable<float>(HC_HSceneCtrl.blush.Value));
            doingAhegao = true;
        }

        public static void RevertAhegao(int i)
        {
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeEyesPtn(HC_HSceneCtrl.randomEye);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeEyebrowPtn(HC_HSceneCtrl.eyeBrowPtn.Value);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeMouthPtn(HC_HSceneCtrl.mouthPtn.Value, true);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeEyesOpenMax(1);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(HC_HSceneCtrl.originalEyeX[i] + HC_HSceneCtrl.eyeCrossAmount.Value));
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(HC_HSceneCtrl.originalEyeY[i] + HC_HSceneCtrl.eyeRollAmount.Value));
            HC_HSceneCtrl.hSceneFemales[i].face.fileStatus.tearsLv = (byte)HC_HSceneCtrl.tearsLevel.Value;
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeMouthFixed(true);
            HC_HSceneCtrl.hSceneFemales[i].face.ChangeHohoAkaRate(new Il2CppSystem.Nullable<float>(HC_HSceneCtrl.blush.Value));
            doingAhegao = false;
        }


        public static void UpdateBreastSoftness()
        {
            if (femalesCount >= 1)
            {
                if (EnableBreastChange.Value)
                {
                    //Apply and update breast softness then revert changes to avoid permanently changing character
                    for (int i = 0; i < femalesCount; i++)
                    {
                        float breastSizeMultiplier = ((hSceneFemales[i].body.breastFixed.bustSize * hSceneFemales[i].body.breastFixed.bustSize * hSceneFemales[i].body.breastFixed.bustSize
                                                       * HC_HSceneCtrl.BreastSizeScalingMultiplier.Value * hSceneFemales[i].body.breastFixed.bustSize * -1) + 1);
                        hSceneFemales[i].fileCustom.Body.bustSoftness = BaseSoftness.Value * breastSizeMultiplier;
                        hSceneFemales[i].fileCustom.Body.bustSoftness2 = HC_HSceneCtrl.TipSoftness.Value * breastSizeMultiplier;
                        if (HC_HSceneCtrl.ScaleSoftness.Value)
                            hSceneFemales[i].fileCustom.Body.bustWeight = HC_HSceneCtrl.Softness.Value * breastSizeMultiplier;
                        else
                            hSceneFemales[i].fileCustom.Body.bustWeight = HC_HSceneCtrl.Softness.Value;
                        hSceneFemales[i].body.UpdateBustShake();
                        RevertBreastData(i);
                    }
                }
                else
                {
                    //Revert breast data if setting disabled
                    for (int i = 0; i < femalesCount; i++)
                    {
                        RevertBreastData(i);
                        hSceneFemales[i].body.UpdateBustShake();
                    }
                }
            }
        }

        public static void RevertBreastData(int i)
        {
            hSceneFemales[i].fileCustom.Body.bustSoftness = originalBreastData[0];
            hSceneFemales[i].fileCustom.Body.bustSoftness2 = originalBreastData[1];
            hSceneFemales[i].fileCustom.Body.bustWeight = originalBreastData[2];
        }

        public class HSceneCtrlComponent : MonoBehaviour
        {
            //Instances
            public static HSceneSprite hSceneSprite;
            public static HScene hScene;
            //Variables
            public static bool applied;
            public static bool nowFemaleOrgasm;
            public static int orgasms;
            public static HVoiceCtrl.FaceInfo[] dummy;
            public static bool ahegaoReset;
            public static float randomTimer;

            public static class Hooks
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(HScene), "Start")]
                public static void StartHook(HScene __instance)
                {
                    hScene = __instance;
                    hSceneSprite = hScene._sprite;
                }


                [HarmonyPostfix]
                [HarmonyPatch(typeof(HSceneFlagCtrl), "AddOrgasm")]
                public static void HSceneFlagCtrlAddOrgasm()
                {
                    nowFemaleOrgasm = true;
                    if (orgasms < 3)
                        orgasms++;
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(HScene), "ChangeModeCtrl")]
                public static void ChangeModeCtrlHook()
                {
                    if (!applied)
                    {
                        HC_HSceneCtrl.GetFemales();
                        HC_HSceneCtrl.SaveData();
                        HC_HSceneCtrl.UpdateBreastSoftness();
                        applied = true;
                    }
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HVoiceCtrl), "SetFace")]
                public static bool SetFaceHook(ref bool __runOriginal, HVoiceCtrl.FaceInfo face)
                {
                    originalFace = face;
                    if (HC_HSceneCtrl.doAhegao)
                    {
                        for (int i = 0; i < HC_HSceneCtrl.femalesCount; i++)
                        {
                            ApplyAhegaoFull(i);
                        }
                        ahegaoReset = false;
                        return __runOriginal = false;
                    }
                    
                    doingAhegao = false;
                    ahegaoReset = true;
                    return __runOriginal = true;
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HScene), "Update")]
                public static void PreUpdateHook1()
                {
                    //Check if ahegao enabled and if orgasm
                    if (HC_HSceneCtrl.ahegao.Value)
                    {
                        if (!HC_HSceneCtrl.doingAhegao && (HC_HSceneCtrl.ahegaoOnOrgasm.Value || orgasms > 2) && (hScene.CtrlFlag.NowOrgasm && nowFemaleOrgasm))
                        {
                            HC_HSceneCtrl.log.LogMessage("Female orgasm ahegao");
                            doAhegao = true;
                            for (int i = 0; i < HC_HSceneCtrl.femalesCount; i++)
                                HC_HSceneCtrl.ApplyAhegaoFull(i);
                        }
                        else nowFemaleOrgasm = false;

                        //Check if faintness and settings for min speed
                        if (!HC_HSceneCtrl.doingAhegao && hScene.CtrlFlag.IsFaintness && !nowFemaleOrgasm)
                        {
                            HC_HSceneCtrl.log.LogMessage("Female feintness ahegao");
                            doAhegao = true;
                            for (int i = 0; i < HC_HSceneCtrl.femalesCount; i++)
                                HC_HSceneCtrl.ApplyAhegaoFull(i);
                        }

                        //Reset ahegao if nothing
                        else if (HC_HSceneCtrl.doingAhegao && !ahegaoReset && !hScene.CtrlFlag.IsFaintness && !hScene.CtrlFlag.NowOrgasm)
                        {
                            HC_HSceneCtrl.log.LogMessage("Resetting ahegao");
                            for (int i = 0; i > HC_HSceneCtrl.femalesCount; i++)
                            {
                                HC_HSceneCtrl.hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(HC_HSceneCtrl.originalEyeX[i]));
                                HC_HSceneCtrl.hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(HC_HSceneCtrl.originalEyeY[i]));
                            }
                            doAhegao = false;
                            ahegaoReset = true;
                        }
                    }
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(HSceneSprite), "OnClickRecover")]
                public static void OnClickRecoverHook()
                {
                    orgasms = 0;
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HScene), "OnDestroy")]
                public static void HScenePreOnDestroy()
                {
                    //Reset variables and destroy monobehaviour
                    hSceneSprite = null;
                    hScene = null;
                    applied = false;
                    for (int i = 0; i < HC_HSceneCtrl.femalesCount; i++)
                        HC_HSceneCtrl.hSceneFemales[i] = null;
                }
            }
        }
    }
}
