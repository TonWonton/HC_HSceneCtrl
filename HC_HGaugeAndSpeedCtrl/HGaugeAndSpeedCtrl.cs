using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using H;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace HC_HGaugeAndSpeedCtrl
{
    [BepInProcess("HoneyCome")]
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    public class HGaugeAndSpeedCtrl : BasePlugin
    {
        public const string PluginName = "HC_HGaugeAndSpeedCtrl";
        public const string GUID = "HC_HGaugeAndSpeedCtrl";
        public const string PluginVersion = "1.0.1";

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

        public static ManualLogSource log = new ManualLogSource(PluginName);

        public override void Load()
        {
            //Climax together
            ClimaxTFemale = Config.Bind("Climax", "Female climax together", true, "Climax Together have priority when girl cums");
            ClimaxTMale = Config.Bind("Climax", "Male auto climax", true, "Priority:\nBoth(together, inside, outside)\nMale solo(swallow, spit, outside)");
            //Gauge speeds and scaling
            Speed = Config.Bind("Gauge", "Toggle speed scaling", false, "Gauge increase will scale with speed if enabled\nThis is independant from animation speed\nSlowest loop speed = 50% on current gauge speed\nFastest loop speed = 150% of current gauge speed");
            speedScale = Config.Bind("Gauge", "Speed scaling multiplier", 1f, new ConfigDescription("How much speed affects the gauge", new AcceptableValueRange<float>(0.1f, 4f)));
            gaugeSpeedMultiplierF = Config.Bind("Gauge", "Female base gauge speed", 0.7f, new ConfigDescription("How much the female gauge increases", new AcceptableValueRange<float>(0f, 4f)));
            gaugeHitSpeedMultiplierF = Config.Bind("Gauge", "Female pleasure gauge speed", 1.5f, new ConfigDescription("How much the female gauge increases when pleasure", new AcceptableValueRange<float>(0f, 6f)));
            gaugeSpeedMultiplierM = Config.Bind("Gauge", "Male base gauge speed", 1f, new ConfigDescription("How much the male gauge increases", new AcceptableValueRange<float>(0f, 4f)));
            gaugeHitSpeedMultiplierM = Config.Bind("Gauge", "Male pleasure gauge speed", 1.1f, new ConfigDescription("How much the male gauge increases when pleasure", new AcceptableValueRange<float>(0f, 6f)));
            Speed.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            speedScale.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            gaugeSpeedMultiplierF.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            gaugeHitSpeedMultiplierF.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            gaugeSpeedMultiplierM.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            gaugeHitSpeedMultiplierM.SettingChanged += (sender, args) => SetSpeedGaugeIncreaseRate();
            //Loop speeds
            minLoopSpeedW = Config.Bind("Loop speed", "Minimum speed weak loop", 1f, new ConfigDescription("If minimum is higher than max speed\nmax speed becomes minimum vice versa", new AcceptableValueRange<float>(0.2f, 5.8f)));
            maxLoopSpeedW = Config.Bind("Loop speed", "Maximum speed weak loop", 1.6f, new ConfigDescription("If minimum is higher than max speed\nmax speed becomes minimum vice versa", new AcceptableValueRange<float>(1f, 7.8f)));
            minLoopSpeedS = Config.Bind("Loop speed", "Minimum speed strong loop", 1.4f, new ConfigDescription("If minimum is higher than max speed\nmax speed becomes minimum vice versa", new AcceptableValueRange<float>(0.1f, 4.6f)));
            maxLoopSpeedS = Config.Bind("Loop speed", "Maximum speed strong loop", 2f, new ConfigDescription("If minimum is higher than max speed\nmax speed becomes minimum vice versa", new AcceptableValueRange<float>(1f, 6.2f)));
            minLoopSpeedO = Config.Bind("Loop speed", "Minimum speed orgasm loop", 1.4f, new ConfigDescription("If minimum is higher than max speed\nmax speed becomes minimum vice versa", new AcceptableValueRange<float>(0.1f, 3.2f)));
            maxLoopSpeedO = Config.Bind("Loop speed", "Maximum speed orgasm loop", 2f, new ConfigDescription("If minimum is higher than max speed\nmax speed becomes minimum vice versa", new AcceptableValueRange<float>(1f, 4.4f)));
            KeyO = Config.Bind("Keybind", "Enable keybinds", true, "LeftCtrl key + Finish button : female only\n" +
                               "LeftShift key + Finish button : male only\n" +
                               "Right mouse double click : Change the strength of the motion(WLoop <=> SLoop)");
            minLoopSpeedW.SettingChanged += (sender, args) => ApplyLoopSpeeds();
            maxLoopSpeedW.SettingChanged += (sender, args) => ApplyLoopSpeeds();
            minLoopSpeedS.SettingChanged += (sender, args) => ApplyLoopSpeeds();
            maxLoopSpeedS.SettingChanged += (sender, args) => ApplyLoopSpeeds();
            minLoopSpeedO.SettingChanged += (sender, args) => ApplyLoopSpeeds();
            maxLoopSpeedO.SettingChanged += (sender, args) => ApplyLoopSpeeds();
            //Patch and register type
            BepInEx.Logging.Logger.Sources.Add(log);
            Harmony.CreateAndPatchAll(typeof(HGaugeCtrlNewComponent.Hooks), GUID);
            ClassInjector.RegisterTypeInIl2Cpp<HGaugeCtrlNewComponent>();
        }

        public static void SetSpeedGaugeIncreaseRate()
        {
            if (HGaugeCtrlNewComponent.HGaugeCtrlNewObject != null)
            {
                if (Speed.Value)
                {
                    //If speed scaling enabled
                    HGaugeCtrlNewComponent.gaugeIncreaseF = Time.fixedDeltaTime * 0.03f * speedScale.Value * gaugeSpeedMultiplierF.Value;
                    HGaugeCtrlNewComponent.gaugeHitIncreaseF = Time.fixedDeltaTime * 0.03f * speedScale.Value * gaugeHitSpeedMultiplierF.Value;
                    HGaugeCtrlNewComponent.gaugeIncreaseM = Time.fixedDeltaTime * 0.03f * speedScale.Value * gaugeSpeedMultiplierM.Value;
                    HGaugeCtrlNewComponent.gaugeHitIncreaseM = Time.fixedDeltaTime * 0.03f * speedScale.Value * gaugeHitSpeedMultiplierM.Value;
                }
                else
                {
                    //If speed scaling disabled
                    HGaugeCtrlNewComponent.gaugeIncreaseF = Time.fixedDeltaTime * 0.03f * gaugeSpeedMultiplierF.Value;
                    HGaugeCtrlNewComponent.gaugeHitIncreaseF = Time.fixedDeltaTime * 0.03f * gaugeHitSpeedMultiplierF.Value;
                    HGaugeCtrlNewComponent.gaugeIncreaseM = Time.fixedDeltaTime * 0.03f * gaugeSpeedMultiplierM.Value;
                    HGaugeCtrlNewComponent.gaugeHitIncreaseM = Time.fixedDeltaTime * 0.03f * gaugeHitSpeedMultiplierM.Value;
                }
            }
        }

        public static void ApplyLoopSpeeds()
        {
            //Apply custom loop speeds
            if (HGaugeCtrlNewComponent.hScene != null)
                HGaugeCtrlNewComponent.hScene.CtrlFlag.LoopSpeeds = new HSceneFlagCtrl.LoopSpeed()
                {
                    MinLoopSpeedW = HGaugeAndSpeedCtrl.minLoopSpeedW.Value,
                    MaxLoopSpeedW = HGaugeAndSpeedCtrl.maxLoopSpeedW.Value,
                    MinLoopSpeedS = HGaugeAndSpeedCtrl.minLoopSpeedS.Value,
                    MaxLoopSpeedS = HGaugeAndSpeedCtrl.maxLoopSpeedS.Value,
                    MinLoopSpeedO = HGaugeAndSpeedCtrl.minLoopSpeedO.Value,
                    MaxLoopSpeedO = HGaugeAndSpeedCtrl.maxLoopSpeedO.Value
                };
        }

        public class HGaugeCtrlNewComponent : MonoBehaviour
        {
            public static HScene hScene;
            public static HSceneSprite hSceneSprite;
            public static GameObject HGaugeCtrlNewObject;

            public static float gaugeIncreaseF;
            public static float gaugeHitIncreaseF;
            public static float gaugeIncreaseM;
            public static float gaugeHitIncreaseM;

            public static bool fFeelAnimation;
            public static bool fFeelAnimationProc;
            public static bool mFeelAnimation;
            public static bool mFeelAnimationProc;
            public static bool flag;
            private static string _playAnimation;
            private static bool maleFinishing;
            private static bool[] buttonList;
            
            private static float paused;
            private static float _isDoubleClick;
            private static bool clickChangeSpeed;
            private static bool fFeelAnimationShouldProc() {
                return fFeelAnimation && flag && !hScene.CtrlFlag.StopFeelFemale && hSceneSprite.CategoryFinish._houshiPosKind == 0;
            }
            private static bool mFeelAnimationShouldProc() {
                return mFeelAnimation && flag && !hScene.CtrlFlag.StopFeelMale && !(hScene.CtrlFlag.IsFaintness && (hScene.CtrlFlag.NowAnimationInfo.ID == 29 || hScene.CtrlFlag.NowAnimationInfo.ID == 22 || hScene.CtrlFlag.NowAnimationInfo.ID == 9));
            }

            void Update()
            {
                if (Input.GetMouseButtonDown(1) && _isDoubleClick <= 0f) _isDoubleClick = 0.4f; //Set double click timer
                else if (Input.GetMouseButtonDown(1) && _isDoubleClick > 0f) //Check for new click while timer active
                {
                    clickChangeSpeed = true;
                    _isDoubleClick = 0f;
                }
                if (_isDoubleClick > 0f) _isDoubleClick -= Time.deltaTime; //If no click decrease timer
            }

            private void LateUpdate()
            {
                if (clickChangeSpeed == true)
                    clickChangeSpeed = false;
            }

            void FixedUpdate()
            {
                //If speed scaling enabled and game not paused
                if (paused == 0 && HGaugeAndSpeedCtrl.Speed.Value)
                {
                    //If female gauge can increase
                    if (fFeelAnimationProc)
                    {
                        //If normal female gauge
                        if (!HGaugeCtrlNewComponent.hScene.CtrlFlag.IsGaugeHit)
                        {
                            if (HGaugeCtrlNewComponent.hScene.CtrlFlag.LoopType == 1)
                                HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_f += gaugeIncreaseF * (HGaugeCtrlNewComponent.hScene.CtrlFlag.Speed - 0.5f);
                            else HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_f += gaugeIncreaseF * (HGaugeCtrlNewComponent.hScene.CtrlFlag.Speed + 0.5f);
                        }
                        else
                        {  //If female gauge hit
                            if (HGaugeCtrlNewComponent.hScene.CtrlFlag.LoopType == 1)
                                HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_f += gaugeHitIncreaseF * (HGaugeCtrlNewComponent.hScene.CtrlFlag.Speed - 0.5f);
                            else HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_f += gaugeHitIncreaseF * (HGaugeCtrlNewComponent.hScene.CtrlFlag.Speed + 0.5f);
                        }
                    }
                    //If male gauge can increase
                    if (mFeelAnimationProc)
                    {
                        //If normal male gauge
                        if (!HGaugeCtrlNewComponent.hScene.CtrlFlag.IsGaugeHit_M)
                        {
                            if (HGaugeCtrlNewComponent.hScene.CtrlFlag.LoopType == 1)
                                HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_m += gaugeIncreaseM * (HGaugeCtrlNewComponent.hScene.CtrlFlag.Speed - 0.5f);
                            else HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_m += gaugeIncreaseM * (HGaugeCtrlNewComponent.hScene.CtrlFlag.Speed + 0.5f);
                        }
                        else
                        {  //If male gauge hit
                            if (HGaugeCtrlNewComponent.hScene.CtrlFlag.LoopType == 1)
                                HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_m += gaugeHitIncreaseM * (HGaugeCtrlNewComponent.hScene.CtrlFlag.Speed - 0.5f);
                            else HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_m += gaugeHitIncreaseM * (HGaugeCtrlNewComponent.hScene.CtrlFlag.Speed + 0.5f);
                        }
                    }
                }

                //If speed scaling disabled
                else if (paused == 0 && !HGaugeAndSpeedCtrl.Speed.Value)
                {
                    //If female gauge can increase
                    if (fFeelAnimationProc)
                    {   //If normal female gauge
                        if (!HGaugeCtrlNewComponent.hScene.CtrlFlag.IsGaugeHit)
                        {
                            if (HGaugeCtrlNewComponent.hScene.CtrlFlag.LoopType == 1)
                                HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_f += gaugeIncreaseF;
                            else HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_f += gaugeIncreaseF;
                        }
                        else
                        {  //If female gauge hit
                            if (HGaugeCtrlNewComponent.hScene.CtrlFlag.LoopType == 1)
                                HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_f += gaugeHitIncreaseF;
                            else HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_f += gaugeHitIncreaseF;
                        }
                    }
                    //If male gauge can increase
                    if (mFeelAnimationProc)
                    {   //If normal male gauge
                        if (!HGaugeCtrlNewComponent.hScene.CtrlFlag.IsGaugeHit_M)
                        {
                            if (HGaugeCtrlNewComponent.hScene.CtrlFlag.LoopType == 1)
                                HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_m += gaugeIncreaseM;
                            else HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_m += gaugeIncreaseM;
                        }
                        else
                        { //If male gauge hit
                            if (HGaugeCtrlNewComponent.hScene.CtrlFlag.LoopType == 1)
                                HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_m += gaugeHitIncreaseM;
                            else HGaugeCtrlNewComponent.hScene.CtrlFlag.Feel_m += gaugeHitIncreaseM;
                        }
                    }
                }
            }

            public static class Hooks
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(HScene), "Start")]
                public static void StartHook(HScene __instance)
                {
                    //Get instances
                    hScene = __instance;
                    hSceneSprite = hScene._sprite;
                    //Create new GameObject for update functions so they only run during HScene
                    HGaugeCtrlNewObject = new GameObject("HGaugeCtrlNewObject");
                    GameObject.DontDestroyOnLoad(HGaugeCtrlNewObject);
                    HGaugeCtrlNewObject.hideFlags = HideFlags.HideAndDontSave;
                    HGaugeCtrlNewObject.AddComponent<HGaugeCtrlNewComponent>();
                    //Calculate and set gauge increase rates
                    hScene.CtrlFlag.SpeedGuageRate = 0f;
                    HGaugeAndSpeedCtrl.SetSpeedGaugeIncreaseRate();
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(HSceneFlagCtrl), "Start")]
                public static void HSceneFlagCtrlStartHook()
                {
                    HGaugeAndSpeedCtrl.ApplyLoopSpeeds();
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(HScene), "ChangeModeCtrl")]
                public static void ChangeModeCtrlHook()
                {
                    if (hScene.CtrlFlag.IsFaintness && (hScene.CtrlFlag.NowAnimationInfo.ID == 29 || hScene.CtrlFlag.NowAnimationInfo.ID == 22 || hScene.CtrlFlag.NowAnimationInfo.ID == 9))
                    {
                        mFeelAnimation = false;
                    }
                }

                    [HarmonyPostfix]
                [HarmonyPatch(typeof(Sonyu), "SetPlay")]
                [HarmonyPatch(typeof(Les), "SetPlay")]
                [HarmonyPatch(typeof(Aibu), "setPlay")]
                [HarmonyPatch(typeof(MultiPlay_F2M1), "setPlay")]
                [HarmonyPatch(typeof(MultiPlay_F1M2), "SetPlay")]
                [HarmonyPatch(typeof(Masturbation), "SetPlay")]
                [HarmonyPatch(typeof(Houshi), "SetPlay")]
                public static void HSceneGetAnimationFlagHook(string playAnimation)
                {
                    _playAnimation = playAnimation;
                    flag = (_playAnimation == "WLoop" || _playAnimation == "D_WLoop" || _playAnimation == "MLoop" || _playAnimation == "D_MLoop" ||
                            _playAnimation == "OLoop" || _playAnimation == "D_OLoop" || _playAnimation == "SLoop" || _playAnimation == "D_SLoop");
                    fFeelAnimationProc = fFeelAnimationShouldProc();
                    mFeelAnimationProc = mFeelAnimationShouldProc();
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
                public static void PreUpdateHook()
                {
                    //If female can climax together
                    if (HGaugeAndSpeedCtrl.ClimaxTFemale.Value && hScene.CtrlFlag.Feel_f >= 0.99f && hSceneSprite.CategoryFinish.GetActiveButton()[5] == true)
                    {
                        hSceneSprite.OnClickFinishSame();
                        maleFinishing = true;
                    }
                    //If male can climax
                    else if (HGaugeAndSpeedCtrl.ClimaxTMale.Value == true && hScene.CtrlFlag.Feel_m >= 0.99f)
                    {
                        //Together
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
                        //Alone
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
                    //If male is finishing stop gauge hit and reset gauge
                    if (hScene.CtrlFlag.NowOrgasm == true && maleFinishing == true)
                    {
                        hScene.CtrlFlag.IsGaugeHit = false;
                        hScene.CtrlFlag.IsGaugeHit_M = false;
                        hScene.CtrlFlag.Feel_m = 0f;
                    }
                    //Reset bool when male orgasm is over
                    else if (hScene.CtrlFlag.NowOrgasm == false && maleFinishing == true)
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
                        if (HGaugeAndSpeedCtrl.KeyO.Value)
                        {
                            switch (hScene.CtrlFlag.LoopType)
                            {
                                case 0:
                                    hScene.CtrlFlag.Speed += 1.001f;
                                    break;
                                case 1:
                                    hScene.CtrlFlag.Speed -= 1.001f;
                                    break;
                                case 2:
                                    if (hScene.CtrlFlag.Speed > 0.5f)
                                        hScene.CtrlFlag.Speed = 0f;
                                    else
                                        hScene.CtrlFlag.Speed = 1f;
                                    break;
                            }
                        }
                    }
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(HSceneSprite), "OnClickStopFeel")]
                public static void OnClickStopFeelHook(int sex)
                {
                    fFeelAnimationProc = fFeelAnimationShouldProc();
                    mFeelAnimationProc = mFeelAnimationShouldProc();
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HSceneSprite), "OnClickFinishBefore")]
                public static bool OnClickFinishBeforeHook()
                {
                    if (HGaugeAndSpeedCtrl.KeyO.Value)
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
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

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HScene), "OnDestroy")]
                public static void HScenePreOnDestroy()
                {
                    //Reset variables and destroy object
                    _playAnimation = null;
                    hSceneSprite = null;
                    hScene = null;
                    GameObject.Destroy(HGaugeCtrlNewObject);
                }
            }
        }
    }
}