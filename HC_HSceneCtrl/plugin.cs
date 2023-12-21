using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Character;
using Gee.External.Capstone.PowerPc;
using H;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ILLGames.ADV.Commands.Base;
using UnityEngine;

namespace Plugin
{
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    public class HC_HSceneCtrl : BasePlugin
    {
        public const string PluginName = "HC_HSceneCtrl";

        public const string GUID = "HC_HSceneCtrl";

        public const string PluginVersion = "0.3.0";

        //Climax together
        public static ConfigEntry<bool> ClimaxTFemale;
        public static ConfigEntry<bool> ClimaxTMale;
        //Gauge speeds and scaling
        public static ConfigEntry<bool> Speed;
        public static ConfigEntry<float> speedScale;
        public static ConfigEntry<float> gaugeSpeedMultiplierF;
        public static ConfigEntry<float> gaugeHitSpeedMultiplierF;
        public static ConfigEntry<float> gaugeSpeedMultiplierM;
        public static ConfigEntry<float> gaugeHitSpeedMultiplierM;
        public static ConfigEntry<bool> KeyO;
        //Loop speeds
        public static ConfigEntry<float> minLoopSpeedW;
        public static ConfigEntry<float> maxLoopSpeedW;
        public static ConfigEntry<float> minLoopSpeedS;
        public static ConfigEntry<float> maxLoopSpeedS;
        public static ConfigEntry<float> minLoopSpeedO;
        public static ConfigEntry<float> maxLoopSpeedO;
        //Breast softness
        public static ConfigEntry<bool> Enable;
        public static ConfigEntry<float> BaseSoftness;
        public static ConfigEntry<float> TipSoftness;
        public static ConfigEntry<float> BreastSizeScalingMultiplier;
        public static ConfigEntry<float> Softness;
        public static ConfigEntry<bool> ScaleSoftness;
        //Patching GameObject
        public GameObject HSceneCtrl;

        public override void Load()
        {
            //Climax together
            ClimaxTFemale = Config.Bind("Options", "Together", false, "Climax Together have priority when girl cums");
            ClimaxTMale = Config.Bind("Options", "Male auto climax", false, "Priority:\nBoth(together, inside, outside)\nMale solo(swallow, spit, outside)");
            //Gauge speeds and scaling
            Speed = Config.Bind("Options", "Toggle speed scaling", false, "Pleasure gauge increase will scale with speed if enabled");
            speedScale = Config.Bind("Options", "Speed", 0.5f, new ConfigDescription("How much speed affects the gauge", new AcceptableValueRange<float>(0.1f, 2f)));
            gaugeSpeedMultiplierF = Config.Bind("Options", "femGaugeSpeedMultiplier", 1f, new ConfigDescription("femGaugeSpeedMultiplier", new AcceptableValueRange<float>(0f, 2f)));
            gaugeHitSpeedMultiplierF = Config.Bind("Options", "femGaugeHitSpeedMultiplier", 1f, new ConfigDescription("femGaugeHitSpeedMultiplier", new AcceptableValueRange<float>(0f, 2f)));
            gaugeSpeedMultiplierM = Config.Bind("Options", "maleGaugeSpeedMultiplier", 1f, new ConfigDescription("maleGaugeSpeedMultiplier", new AcceptableValueRange<float>(0f, 2f)));
            gaugeHitSpeedMultiplierM = Config.Bind("Options", "maleGaugeHitSpeedMultiplier", 1f, new ConfigDescription("maleGaugeHitSpeedMultiplier", new AcceptableValueRange<float>(0f, 2f)));
            speedScale.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            gaugeSpeedMultiplierF.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            gaugeHitSpeedMultiplierF.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            gaugeSpeedMultiplierM.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            gaugeHitSpeedMultiplierM.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            KeyO = Config.Bind("Options", "KeyOption", true, "LeftCtrl key + Finish button : female only\n" +
                               "RightCtrl key + Finish button : male only\n" +
                               "Right mouse double click : Change the strength of the motion(WLoop <=> SLoop)");
            //Loop speeds
            minLoopSpeedW = Config.Bind("Options", "minLoopSpeedW", 1f, new ConfigDescription("minLoopSpeedW", new AcceptableValueRange<float>(0.2f, 1f)));
            maxLoopSpeedW = Config.Bind("Options", "maxLoopSpeedW", 1.6f, new ConfigDescription("maxLoopSpeedW", new AcceptableValueRange<float>(1.01f, 7.6f)));
            minLoopSpeedS = Config.Bind("Options", "minLoopSpeedS", 1.4f, new ConfigDescription("minLoopSpeedS", new AcceptableValueRange<float>(0.1f, 1.4f)));
            maxLoopSpeedS = Config.Bind("Options", "maxLoopSpeedS", 2f, new ConfigDescription("maxLoopSpeedS", new AcceptableValueRange<float>(1.41f, 6.4f)));
            minLoopSpeedO = Config.Bind("Options", "minLoopSpeedO", 1.4f, new ConfigDescription("minLoopSpeedO", new AcceptableValueRange<float>(0.1f, 1.4f)));
            maxLoopSpeedO = Config.Bind("Options", "maxLoopSpeedO", 2f, new ConfigDescription("maxLoopSpeedO", new AcceptableValueRange<float>(1.41f, 4.6f)));
            minLoopSpeedW.SettingChanged += (sender, args) => ApplyLoopSpeed();
            maxLoopSpeedW.SettingChanged += (sender, args) => ApplyLoopSpeed();
            minLoopSpeedS.SettingChanged += (sender, args) => ApplyLoopSpeed();
            maxLoopSpeedS.SettingChanged += (sender, args) => ApplyLoopSpeed();
            minLoopSpeedO.SettingChanged += (sender, args) => ApplyLoopSpeed();
            maxLoopSpeedO.SettingChanged += (sender, args) => ApplyLoopSpeed();
            //Breast softness
            Enable = Config.Bind("JiggleControl", "Enable Jiggle Control", false, "Enable custom values during HScene");
            BaseSoftness = Config.Bind("JiggleControl", "Base softness", 0.5f, new ConfigDescription("Set base softness", new AcceptableValueRange<float>(0f, 1f)));
            TipSoftness = Config.Bind("JiggleControl", "Tip softness", 0.5f, new ConfigDescription("Set tip softness", new AcceptableValueRange<float>(0f, 1f)));
            BreastSizeScalingMultiplier = Config.Bind("JiggleControl", "Scale breast softness exponentially down with size", 0.5f,new ConfigDescription
                ("Higher values = less bounce on larger breasts. Smaller breasts are affected exponentially less. 100% = 0% bounce on max size",new AcceptableValueRange<float>(0f, 1f)));
            Softness = Config.Bind("JiggleControl", "Weight", 0.5f, new ConfigDescription("Set weight", new AcceptableValueRange<float>(0f, 1f)));
            ScaleSoftness = Config.Bind("JiggleControl", "Scale weight down with size", true, "Also scale weight down with breast size");
            //Patch hook methods and register monobehaviour component
            Harmony.CreateAndPatchAll(typeof(HSceneCtrlComponent.Hooks), GUID);
            ClassInjector.RegisterTypeInIl2Cpp<HSceneCtrlComponent>();
            ClassInjector.RegisterTypeInIl2Cpp<HSceneFixedUpdateComponent>();
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

        public static void ApplyLoopSpeed()
        {
            if (HSceneCtrlComponent.hScene != null)
            {
                HSceneCtrlComponent.hScene.CtrlFlag.LoopSpeeds = new HSceneFlagCtrl.LoopSpeed()
                {
                    MinLoopSpeedW = HC_HSceneCtrl.minLoopSpeedW.Value,
                    MaxLoopSpeedW = HC_HSceneCtrl.maxLoopSpeedW.Value,
                    MinLoopSpeedS = HC_HSceneCtrl.minLoopSpeedS.Value,
                    MaxLoopSpeedS = HC_HSceneCtrl.maxLoopSpeedS.Value,
                    MinLoopSpeedO = HC_HSceneCtrl.minLoopSpeedO.Value,
                    MaxLoopSpeedO = HC_HSceneCtrl.maxLoopSpeedO.Value
                };
            }
        }

        public static void SetSpeedGaugeIncreaseRate()
        {
            if (HSceneCtrlComponent.HSceneFixedUpdate != null)
            {
                if (Speed.Value)
                {
                    HSceneFixedUpdateComponent.gaugeIncreaseF = Time.fixedDeltaTime * 0.06f * HC_HSceneCtrl.speedScale.Value * HC_HSceneCtrl.gaugeSpeedMultiplierF.Value;
                    HSceneFixedUpdateComponent.gaugeHitIncreaseF = Time.fixedDeltaTime * 0.06f * HC_HSceneCtrl.speedScale.Value * HC_HSceneCtrl.gaugeHitSpeedMultiplierF.Value;
                    HSceneFixedUpdateComponent.gaugeIncreaseM = Time.fixedDeltaTime * 0.06f * HC_HSceneCtrl.speedScale.Value * HC_HSceneCtrl.gaugeSpeedMultiplierM.Value;
                    HSceneFixedUpdateComponent.gaugeHitIncreaseM = Time.fixedDeltaTime * 0.06f * HC_HSceneCtrl.speedScale.Value * HC_HSceneCtrl.gaugeHitSpeedMultiplierM.Value;
                }
                else
                {
                    HSceneFixedUpdateComponent.gaugeIncreaseF = Time.fixedDeltaTime * 0.06f * HC_HSceneCtrl.gaugeSpeedMultiplierF.Value;
                    HSceneFixedUpdateComponent.gaugeHitIncreaseF = Time.fixedDeltaTime * 0.06f * HC_HSceneCtrl.gaugeHitSpeedMultiplierF.Value;
                    HSceneFixedUpdateComponent.gaugeIncreaseM = Time.fixedDeltaTime * 0.06f * HC_HSceneCtrl.gaugeSpeedMultiplierM.Value;
                    HSceneFixedUpdateComponent.gaugeHitIncreaseM = Time.fixedDeltaTime * 0.06f * HC_HSceneCtrl.gaugeHitSpeedMultiplierM.Value;
                }
            }
        }

        public static Human[] GetFemales()
        {
            Human[] hSceneFemales;
            if (HSceneCtrlComponent.hScene != null)
            {
                Il2CppReferenceArray<Human> females = HSceneCtrlComponent.hScene.GetFemales();
                List<Human> femalesList = new List<Human>();

                foreach (Human female in females)
                {
                    if (female != null)
                    {
                        femalesList.Add(female);

                    }
                }
                hSceneFemales = femalesList.ToArray();
            }
            return null;
        }
    }

    public class HSceneFixedUpdateComponent : MonoBehaviour
    {
        public static float gaugeIncreaseF;
        public static float gaugeHitIncreaseF;
        public static float gaugeIncreaseM;
        public static float gaugeHitIncreaseM;

        void FixedUpdate()
        {
            if (HSceneCtrlComponent.paused == 0)
            {
                //If speed scaling enabled and game not paused
                if (HC_HSceneCtrl.Speed.Value)
                {   //If female gauge can increase
                    if (HSceneCtrlComponent.fFeelAnimation && HSceneCtrlComponent.flag && !HSceneCtrlComponent.hScene.CtrlFlag.StopFeelFemale){
                        //If normal female gauge
                        if (!HSceneCtrlComponent.hScene.CtrlFlag.IsGaugeHit){
                            if (HSceneCtrlComponent.hScene.CtrlFlag.LoopType == 1)
                                HSceneCtrlComponent.hScene.CtrlFlag.Feel_f += gaugeIncreaseF * (HSceneCtrlComponent.hScene.CtrlFlag.Speed - 0.9f);
                            else HSceneCtrlComponent.hScene.CtrlFlag.Feel_f += gaugeIncreaseF * (HSceneCtrlComponent.hScene.CtrlFlag.Speed + 0.1f);}
                        else{  //If female gauge hit
                            if (HSceneCtrlComponent.hScene.CtrlFlag.LoopType == 1)
                                HSceneCtrlComponent.hScene.CtrlFlag.Feel_f += gaugeHitIncreaseF * (HSceneCtrlComponent.hScene.CtrlFlag.Speed - 0.9f);
                            else HSceneCtrlComponent.hScene.CtrlFlag.Feel_f += gaugeHitIncreaseF * (HSceneCtrlComponent.hScene.CtrlFlag.Speed + 0.1f);}}
                    //If male gauge can increase
                    if (HSceneCtrlComponent.mFeelAnimation && HSceneCtrlComponent.flag && !HSceneCtrlComponent.hScene.CtrlFlag.StopFeelMale){
                        //If normal male gauge
                        if (!HSceneCtrlComponent.hScene.CtrlFlag.IsGaugeHit_M){
                            if (HSceneCtrlComponent.hScene.CtrlFlag.LoopType == 1)
                                HSceneCtrlComponent.hScene.CtrlFlag.Feel_m += gaugeIncreaseM * (HSceneCtrlComponent.hScene.CtrlFlag.Speed - 0.9f);
                            else HSceneCtrlComponent.hScene.CtrlFlag.Feel_m += gaugeIncreaseM * (HSceneCtrlComponent.hScene.CtrlFlag.Speed + 0.1f);}
                        else{  //If male gauge hit
                            if (HSceneCtrlComponent.hScene.CtrlFlag.LoopType == 1)
                                HSceneCtrlComponent.hScene.CtrlFlag.Feel_m += gaugeHitIncreaseM * (HSceneCtrlComponent.hScene.CtrlFlag.Speed - 0.9f);
                            else HSceneCtrlComponent.hScene.CtrlFlag.Feel_m += gaugeHitIncreaseM * (HSceneCtrlComponent.hScene.CtrlFlag.Speed + 0.1f);}}
                }
                else if (!HC_HSceneCtrl.Speed.Value)
                {   //If female gauge can increase
                    if (HSceneCtrlComponent.fFeelAnimation && HSceneCtrlComponent.flag && !HSceneCtrlComponent.hScene.CtrlFlag.StopFeelFemale)
                    {   //If normal female gauge
                        if (!HSceneCtrlComponent.hScene.CtrlFlag.IsGaugeHit){
                            if (HSceneCtrlComponent.hScene.CtrlFlag.LoopType == 1)
                                HSceneCtrlComponent.hScene.CtrlFlag.Feel_f += gaugeIncreaseF;
                            else HSceneCtrlComponent.hScene.CtrlFlag.Feel_f += gaugeIncreaseF;}
                        else{  //If female gauge hit
                            if (HSceneCtrlComponent.hScene.CtrlFlag.LoopType == 1)
                                HSceneCtrlComponent.hScene.CtrlFlag.Feel_f += gaugeHitIncreaseF;
                            else HSceneCtrlComponent.hScene.CtrlFlag.Feel_f += gaugeHitIncreaseF;}}
                    //If male gauge can increase
                    if (HSceneCtrlComponent.mFeelAnimation && HSceneCtrlComponent.flag && !HSceneCtrlComponent.hScene.CtrlFlag.StopFeelMale)
                    {   //If normal male gauge
                        if (!HSceneCtrlComponent.hScene.CtrlFlag.IsGaugeHit_M){
                            if (HSceneCtrlComponent.hScene.CtrlFlag.LoopType == 1)
                                HSceneCtrlComponent.hScene.CtrlFlag.Feel_m += gaugeIncreaseM;
                            else HSceneCtrlComponent.hScene.CtrlFlag.Feel_m += gaugeIncreaseM;}
                        else{ //If male gauge hit
                            if (HSceneCtrlComponent.hScene.CtrlFlag.LoopType == 1)
                                HSceneCtrlComponent.hScene.CtrlFlag.Feel_m += gaugeHitIncreaseM;
                            else HSceneCtrlComponent.hScene.CtrlFlag.Feel_m += gaugeHitIncreaseM;}}
                }
            }
        }
    }

    public class HSceneCtrlComponent : MonoBehaviour
    {
        //Instances
        static HSceneSprite hSceneSprite;
        public static HScene hScene;
        public static GameObject HSceneFixedUpdate;
        //Variables
        static string _playAnimation;
        public static bool fFeelAnimation;
        public static bool mFeelAnimation;
        public static bool flag;
        public static bool maleFinishing;
        public static bool[] buttonList = new bool[6];
        public static bool clickChangeSpeed = false;
        public static float _isDoubleClick = 0f;
        public static float speedGaugeIncrease;
        public static float paused;

        private void Update()
        {
            if (Input.GetMouseButtonDown(1) && _isDoubleClick <= 0f)
                _isDoubleClick = 0.4f;
            else if (Input.GetMouseButtonDown(1) && _isDoubleClick > 0f){
                clickChangeSpeed = true; _isDoubleClick = 0f;}
            if (_isDoubleClick > 0f)
                _isDoubleClick -= Time.deltaTime;
        }

        private void LateUpdate()
        {
            if (clickChangeSpeed == true)
                clickChangeSpeed = false;
        }

        public static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HScene), "Start")]
            public static void StartHook(HScene __instance)
            {
                hScene = __instance;
                hSceneSprite = hScene._sprite;
                hScene.CtrlFlag.SpeedGuageRate = 0f;
                HSceneFixedUpdate = new GameObject("HSceneFixedUpdate");
                GameObject.DontDestroyOnLoad(HSceneFixedUpdate);
                HSceneFixedUpdate.hideFlags = HideFlags.HideAndDontSave;
                HSceneFixedUpdate.AddComponent<HSceneFixedUpdateComponent>();
                HC_HSceneCtrl.SetSpeedGaugeIncreaseRate();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneFlagCtrl), "Start")]
            public static void HSceneFlagCtrlStartHook()
            {
                HC_HSceneCtrl.ApplyLoopSpeed();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Sonyu), "SetPlay")]
            [HarmonyPatch(typeof(Les), "SetPlay")]
            [HarmonyPatch(typeof(Aibu), "setPlay")]
            [HarmonyPatch(typeof(MultiPlay_F2M1), "setPlay")]
            [HarmonyPatch(typeof(MultiPlay_F1M2), "SetPlay")]
            [HarmonyPatch(typeof(Masturbation), "SetPlay")]
            [HarmonyPatch(typeof(Houshi), "SetPlay")]
            public static void HSceneGetAnimationFlag(string playAnimation)
            {
                _playAnimation = playAnimation;
                flag = (_playAnimation == "WLoop" || _playAnimation == "D_WLoop" || _playAnimation == "MLoop" || _playAnimation == "D_MLoop" || _playAnimation == "OLoop" ||
                        _playAnimation == "D_OLoop" || _playAnimation == "SLoop" || _playAnimation == "D_SLoop");
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Sonyu), "SetPlay")]
            [HarmonyPatch(typeof(Les), "SetPlay")]
            [HarmonyPatch(typeof(Aibu), "setPlay")]
            [HarmonyPatch(typeof(MultiPlay_F2M1), "setPlay")]
            [HarmonyPatch(typeof(MultiPlay_F1M2), "SetPlay")]
            [HarmonyPatch(typeof(Masturbation), "SetPlay")]
            public static void FHSceneFeelTrueSetPlayHook()
            {
                fFeelAnimation = true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Houshi), "SetPlay")]
            public static void FHSceneFeelFalseSetPlayHook()
            {
                fFeelAnimation = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Sonyu), "SetPlay")]
            [HarmonyPatch(typeof(MultiPlay_F2M1), "setPlay")]
            [HarmonyPatch(typeof(MultiPlay_F1M2), "SetPlay")]
            [HarmonyPatch(typeof(Houshi), "SetPlay")]
            public static void MHSceneFeelTrueSetPlayHook()
            {
                mFeelAnimation = true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Les), "SetPlay")]
            [HarmonyPatch(typeof(Aibu), "setPlay")]
            [HarmonyPatch(typeof(Masturbation), "SetPlay")]
            public static void MHSceneFeelFalseSetPlayHook()
            {
                mFeelAnimation = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HScene), "Update")]
            public static void UpdateHook()
            {
                if (HC_HSceneCtrl.ClimaxTFemale.Value && hScene.CtrlFlag.Feel_f >= 0.99f && hSceneSprite.CategoryFinish.GetActiveButton()[5] == true)
                {
                    hSceneSprite.OnClickFinishSame();
                    maleFinishing = true;
                }
                else if (HC_HSceneCtrl.ClimaxTMale.Value == true && hScene.CtrlFlag.Feel_m >= 0.99f)
                {
                    buttonList = hSceneSprite.CategoryFinish.GetActiveButton();
                    if (hSceneSprite.CategoryFinish._houshiPosKind == 0)
                    {
                        if (buttonList[5])
                            hSceneSprite.OnClickFinishSame();
                        else if (buttonList[2])
                            hSceneSprite.OnClickFinishInSide();
                        else if (buttonList[1])
                            hSceneSprite.OnClickFinishOutSide();
                    }
                    else if (hSceneSprite.CategoryFinish._houshiPosKind == 1)
                    {
                        if (buttonList[3])
                            hSceneSprite.OnClickFinishDrink();
                        else if (buttonList[4])
                            hSceneSprite.OnClickFinishVomit();
                        else if (buttonList[1])
                            hSceneSprite.OnClickFinishOutSide();
                    }
                    maleFinishing = true;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HScene), "LateUpdate")]
            public static void LateUpdateHook()
            {
                if (hScene.CtrlFlag.NowOrgasm == true && maleFinishing == true)
                {
                    hScene.CtrlFlag.IsGaugeHit = false;
                    hScene.CtrlFlag.IsGaugeHit_M = false;
                    hScene.CtrlFlag.Feel_m = 0f;
                }
                if (hScene.CtrlFlag.NowOrgasm == false && maleFinishing == true)
                    maleFinishing = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Sonyu), "SetAnimationParamater")]
            [HarmonyPatch(typeof(Aibu), "SetAnimationParamater")]
            [HarmonyPatch(typeof(Houshi), "SetAnimationParamater")]
            [HarmonyPatch(typeof(MultiPlay_F1M2), "SetAnimationParamater")]
            [HarmonyPatch(typeof(MultiPlay_F2M1), "SetAnimationParamater")]
            [HarmonyPatch(typeof(Les), "SetAnimationParamater")]
            [HarmonyPatch(typeof(Masturbation), "SetAnimationParamater")]
            public static void SetAnimationParamaterHook()
            {
                if (clickChangeSpeed == true)
                {
                    if (HC_HSceneCtrl.KeyO.Value)
                    {
                        if (hScene.CtrlFlag.LoopType != 2)
                            if (hScene.CtrlFlag.Speed <= 1f)
                                hScene.CtrlFlag.Speed = 1.5f;
                            else
                                hScene.CtrlFlag.Speed = 0.5f;
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HSceneSprite), "OnClickFinishBefore")]
            public static bool OnClickFinishBeforeHook()
            {
                if (HC_HSceneCtrl.KeyO.Value)
                {
                    if (Input.GetKey(KeyCode.RightControl))
                    {
                        if (hScene.CtrlFlag.Feel_m < 0.75 && mFeelAnimation)
                            hScene.CtrlFlag.Feel_m = 0.75f;
                        return false;
                    }
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        if (hScene.CtrlFlag.Feel_f < 0.75 && fFeelAnimation)
                            hScene.CtrlFlag.Feel_f = 0.75f;
                        return false;
                    }
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HScene), "OnDestroy")]
            public static void HScenePreOnDestroy()
            {
                _playAnimation = null; hSceneSprite = null; hScene = null; HSceneFixedUpdate = null;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HC.Dialog.ShortcutViewDialog), "Load")]
            [HarmonyPatch(typeof(HC.Dialog.HelpWindow), "Load")]
            [HarmonyPatch(typeof(HC.Dialog.ExitDialog), "Manager_Scene_IOverlap_AddEvent")]
            [HarmonyPatch(typeof(HC.Config.ConfigWindow), "Load")]
            public static void ConfigSHook()
            {
                paused++;
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HC.Dialog.ShortcutViewDialog), "OnBack")]
            [HarmonyPatch(typeof(HC.Dialog.HelpWindow), "SceneEnd")]
            [HarmonyPatch(typeof(HC.Dialog.ExitDialog), "Manager_Scene_IOverlap_RemoveEvent")]
            [HarmonyPatch(typeof(HC.Config.ConfigWindow), "Unload")]
            public static void ConfigEHook()
            {
                paused--;
            }
        }
    }
}
