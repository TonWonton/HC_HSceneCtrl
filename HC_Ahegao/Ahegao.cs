using System;
using System.Collections;
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

namespace HC_Ahegao
{
    [BepInProcess("HoneyCome")]
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    public class Ahegao : BasePlugin
    {
        public const string PluginName = "HC_Ahegao";
        public const string GUID = "HC_Ahegao";
        public const string PluginVersion = "0.3.0";

        //Ahegao
        public static ConfigEntry<bool> ahegao;
        public static ConfigEntry<bool> ahegaoOnOrgasm;
        public static ConfigEntry<float> minSpeed;
        public static ConfigEntry<float> eyeMoveSpeed;
        //Ahegao orgasm
        public static ConfigEntry<int> eyePtnOrgasm;
        public static ConfigEntry<int> eyeBrowPtnOrgasm;
        public static ConfigEntry<bool> blinkOrgasm;
        public static ConfigEntry<float> openEyeOrgasm;
        public static ConfigEntry<float> eyeRollAmountOrgasm;
        public static ConfigEntry<float> eyeCrossAmountOrgasm;
        public static ConfigEntry<bool> eyeHighlightOrgasm;
        public static ConfigEntry<int> tearsLevelOrgasm;
        public static ConfigEntry<int> mouthPtnOrgasm;
        public static ConfigEntry<float> openMouthMinOrgasm;
        public static ConfigEntry<float> blushOrgasm;
        //Ahegao faintness
        public static ConfigEntry<int> eyePtnFaintness;
        public static ConfigEntry<int> eyeBrowPtnFaintness;
        public static ConfigEntry<bool> blinkFaintness;
        public static ConfigEntry<float> openEyeFaintness;
        public static ConfigEntry<float> eyeRollAmountFaintness;
        public static ConfigEntry<float> eyeCrossAmountFaintness;
        public static ConfigEntry<bool> eyeHighlightFaintness;
        public static ConfigEntry<int> tearsLevelFaintness;
        public static ConfigEntry<int> mouthPtnFaintness;
        public static ConfigEntry<float> openMouthMinFaintness;
        public static ConfigEntry<float> blushFaintness;
        //Ahegao faintness speed
        public static ConfigEntry<int> eyePtnFaintnessSpeed;
        public static ConfigEntry<int> eyeBrowPtnFaintnessSpeed;
        public static ConfigEntry<bool> blinkFaintnessSpeed;
        public static ConfigEntry<float> openEyeFaintnessSpeed;
        public static ConfigEntry<float> eyeRollAmountFaintnessSpeed;
        public static ConfigEntry<float> eyeCrossAmountFaintnessSpeed;
        public static ConfigEntry<bool> eyeHighlightFaintnessSpeed;
        public static ConfigEntry<int> tearsLevelFaintnessSpeed;
        public static ConfigEntry<int> mouthPtnFaintnessSpeed;
        public static ConfigEntry<float> openMouthMinFaintnessSpeed;
        public static ConfigEntry<float> blushFaintnessSpeed;

        public static ManualLogSource log = new ManualLogSource(PluginName);

        //Not implemented yet
        //public static ConfigEntry<float> eyeRollAmount;
        //public static ConfigEntry<float> eyeCrossAmount;

        public override void Load()
        {
            //Ahegao
            ahegao = Config.Bind("Ahegao", "Ahegao", true, "Enable ahegao");
            ahegaoOnOrgasm = Config.Bind("Ahegao", "Ahegao on normal orgasm", false);
            minSpeed = Config.Bind("Ahegao", "Minimum speed for eye roll during faintness", 0.75f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 2f)));
            eyeMoveSpeed = Config.Bind("Ahegao", "Eye move speed", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 10f)));
            //Ahegao orgasm
            eyePtnOrgasm = Config.Bind("Ahegao orgasm", "Eye pattern", 0, new ConfigDescription("Set eye pattern", new AcceptableValueRange<int>(0, 24)));
            eyeBrowPtnOrgasm = Config.Bind("Ahegao orgasm", "Eyebrow pattern", 0, new ConfigDescription("Set eyebrow pattern", new AcceptableValueRange<int>(0, 10)));
            blinkOrgasm = Config.Bind("Ahegao orgasm", "Blink during ahegao", true);
            openEyeOrgasm = Config.Bind("Ahegao orgasm", "Eye open amount", 0.79f, new ConfigDescription("Set eye open amount", new AcceptableValueRange<float>(0f, 1f)));
            eyeRollAmountOrgasm = Config.Bind("Ahegao orgasm", "Eye roll amount during ahegao and faintness", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeCrossAmountOrgasm = Config.Bind("Ahegao orgasm", "Eye cross offset amount during ahegao and faintness", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeHighlightOrgasm = Config.Bind("Ahegao orgasm", "Eye highlight during ahegao", true);
            tearsLevelOrgasm = Config.Bind("Ahegao orgasm", "Tears level during ahegao and faintness", 2, new ConfigDescription("Set tears level", new AcceptableValueList<int>(0, 1, 2, 3)));
            mouthPtnOrgasm = Config.Bind("Ahegao orgasm", "Mouth pattern", 0, new ConfigDescription("Set mouth pattern", new AcceptableValueRange<int>(0, 27)));
            openMouthMinOrgasm = Config.Bind("Ahegao orgasm", "Mouth open amount", 1f, new ConfigDescription("Set mouth open amount", new AcceptableValueRange<float>(0f, 1f)));
            blushOrgasm = Config.Bind("Ahegao orgasm", "Blush amount during ahegao and faintness", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 2.8f)));
            //Ahegao faintness
            eyePtnFaintness = Config.Bind("Ahegao faintness", "Eye pattern", 0, new ConfigDescription("Set eye pattern", new AcceptableValueRange<int>(0, 24)));
            eyeBrowPtnFaintness = Config.Bind("Ahegao faintness", "Eyebrow pattern", 0, new ConfigDescription("Set eyebrow pattern", new AcceptableValueRange<int>(0, 10)));
            blinkFaintness = Config.Bind("Ahegao faintness", "Blink during ahegao", true);
            openEyeFaintness = Config.Bind("Ahegao faintness", "Eye open amount", 0.79f, new ConfigDescription("Set eye open amount", new AcceptableValueRange<float>(0f, 1f)));
            eyeRollAmountFaintness = Config.Bind("Ahegao faintness", "Eye roll amount during ahegao and faintness", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeCrossAmountFaintness = Config.Bind("Ahegao faintness", "Eye cross offset amount during ahegao and faintness", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeHighlightFaintness = Config.Bind("Ahegao faintness", "Eye highlight during ahegao", true);
            tearsLevelFaintness = Config.Bind("Ahegao faintness", "Tears level during ahegao and faintness", 2, new ConfigDescription("Set tears level", new AcceptableValueList<int>(0, 1, 2, 3)));
            mouthPtnFaintness = Config.Bind("Ahegao faintness", "Mouth pattern", 0, new ConfigDescription("Set mouth pattern", new AcceptableValueRange<int>(0, 27)));
            openMouthMinFaintness = Config.Bind("Ahegao faintness", "Mouth open amount", 1f, new ConfigDescription("Set mouth open amount", new AcceptableValueRange<float>(0f, 1f)));
            blushFaintness = Config.Bind("Ahegao faintness", "Blush amount during ahegao and faintness", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 2.8f)));
            //Ahegao faintness speed
            eyePtnFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye pattern", 0, new ConfigDescription("Set eye pattern", new AcceptableValueRange<int>(0, 24)));
            eyeBrowPtnFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eyebrow pattern", 0, new ConfigDescription("Set eyebrow pattern", new AcceptableValueRange<int>(0, 10)));
            blinkFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Blink during ahegao", true);
            openEyeFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye open amount", 0.79f, new ConfigDescription("Set eye open amount", new AcceptableValueRange<float>(0f, 1f)));
            eyeRollAmountFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye roll amount during ahegao and faintness", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeCrossAmountFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye cross offset amount during ahegao and faintness", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeHighlightFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye highlight during ahegao", true);
            tearsLevelFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Tears level during ahegao and faintness", 2, new ConfigDescription("Set tears level", new AcceptableValueList<int>(0, 1, 2, 3)));
            mouthPtnFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Mouth pattern", 0, new ConfigDescription("Set mouth pattern", new AcceptableValueRange<int>(0, 27)));
            openMouthMinFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Mouth open amount", 1f, new ConfigDescription("Set mouth open amount", new AcceptableValueRange<float>(0f, 1f)));
            blushFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Blush amount during ahegao and faintness", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 2.8f)));
            //Make settings reflect in gameplay when config is changed
            //Ahegao
            ahegao.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            ahegaoOnOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            minSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            //Ahegao orgasm
            eyePtnOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeBrowPtnOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            blinkOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            openEyeOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeRollAmountOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeCrossAmountOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeHighlightOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            tearsLevelOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            mouthPtnOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            openMouthMinOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            blushOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            //Ahegao faintness
            eyePtnFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeBrowPtnFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            blinkFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            openEyeFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeRollAmountFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeCrossAmountFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeHighlightFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            tearsLevelFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            mouthPtnFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            openMouthMinFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            blushFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            //Ahegao faintness speed
            eyePtnFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeBrowPtnFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            blinkFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            openEyeFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeRollAmountFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeCrossAmountFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            eyeHighlightFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            tearsLevelFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            mouthPtnFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            openMouthMinFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            blushFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings();
            //Not implemented yet
            //eyeCrossAmount = Config.Bind("Ahegao", "Eye cross offset amount during ahegao and faintness", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            //eyeRollAmount = Config.Bind("Ahegao", "Eye roll amount during ahegao and faintness", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            //Patch hook methods and register monobehaviour component

            //Patch methods
            BepInEx.Logging.Logger.Sources.Add(log);
            Harmony.CreateAndPatchAll(typeof(AhegaoComponent.Hooks), GUID);
            this.AddComponent<AhegaoComponent>();
        }

        public class AhegaoComponent : MonoBehaviour
        {
            //Instances
            public static HScene hScene;
            //Variables
            public static bool applied;
            public static bool nowFemaleOrgasm;
            public static int orgasms;
            public static float[] originalEyeOpenMin;
            public static float[] originalEyeY;
            public static float[] originalEyeX;
            public static float[] originalBlush;
            public static HVoiceCtrl.FaceInfo[] ahegaoFace;
            public static Human[] hSceneFemales;
            public static int femalesCount;
            public static AhegaoState state;
            public static bool afterOrgasm;
            public static bool moveEyes;
            public static float offsetY;
            public static float offsetX;
            public static float targetY;
            public static float targetX;
            public static float moveMultiY;
            public static float moveMultiX;
            public static bool moveEyesPositive;


            public enum AhegaoState
            {
                none = 0,
                orgasm = 1,
                faintness = 2,
                faintnessSpeed = 3,
                afterOrgasm = 4
            }

            void Update()
            {
                if (moveEyes)
                {
                    for (int i = 0; i < femalesCount; i++)
                    {
                        DoMoveEyes(i);
                    }
                }
            }

            public static void DoMoveEyes(int i)
            {
                if (moveEyesPositive)
                {
                    
                    var move = hSceneFemales[i].fileCustom.Face.eyeY + moveMultiY;
                    hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(move));
                    if (hSceneFemales[i].fileCustom.Face.eyeY > 1f)
                    {
                        hSceneFemales[i].fileCustom.Face.eyeY = 1f;
                        moveEyes = false;
                    }
                    else if (hSceneFemales[i].fileCustom.Face.eyeY >= targetY)
                    {
                        moveEyes = false;
                    }
                }
                else
                {
                    var move = hSceneFemales[i].fileCustom.Face.eyeY + moveMultiY;
                    hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(move));
                    if (hSceneFemales[i].fileCustom.Face.eyeY < 0f)
                    {
                        hSceneFemales[i].fileCustom.Face.eyeY = 0f;
                        moveEyes = false;
                    }
                    else if (hSceneFemales[i].fileCustom.Face.eyeY <= targetY)
                    {
                        moveEyes = false;
                    }
                }
            }

            public static void MoveEyes(int i, float targetPositionY, float targetPositionX)
            {
                targetY = targetPositionY;
                targetX = targetPositionX;
                if (targetY > 1f)
                    targetY = 1f;
                else if (targetY < 0f)
                    targetY = 0f;
                if (targetX > 1f)
                    targetX = 1f;
                else if (targetX < 0f)
                    targetX = 0f;
                offsetY = targetY - hSceneFemales[i].fileCustom.Face.eyeY;
                offsetX = targetX - hSceneFemales[i].fileCustom.Face.eyeX;
                moveMultiY = offsetY * Time.deltaTime * eyeMoveSpeed.Value;
                moveMultiX = offsetX * Time.deltaTime * eyeMoveSpeed.Value;
                if (moveMultiY > 0)
                    moveEyesPositive = true;
                else
                    moveEyesPositive = false;
                moveEyes = true;
            }

            public static void GetFemales()
            {
                if (AhegaoComponent.hScene != null)
                {
                    //Get females from HScene refrence array and put into array for saving and updating data
                    Il2CppReferenceArray<Human> females = AhegaoComponent.hScene.GetFemales();
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
                    originalEyeOpenMin = new float[femalesCount];
                    originalEyeY = new float[femalesCount];
                    originalEyeX = new float[femalesCount];
                    originalBlush = new float[femalesCount];
                    hSceneFemales = femalesList.ToArray();
                    for (int i = 0; i < femalesCount; i++)
                        GetOriginalFace(i);
                }
            }

            public static void GetOriginalFace(int i)
            {
                originalEyeOpenMin[i] = hSceneFemales[i].face.eyesCtrl.OpenMin;
                originalEyeY[i] = hSceneFemales[i].fileCustom.Face.eyeY;
                originalEyeX[i] = hSceneFemales[i].fileCustom.Face.eyeX;
                originalBlush[i] = hSceneFemales[i].face.fileStatus.hohoAkaRate;
            }

            public static void UpdateAhegaoSettings()
            {
                if (hScene != null)
                {
                    switch (state)
                    {
                        case AhegaoState.none:
                            break;
                        case AhegaoState.faintness:
                            DoAhegaoFaintness();
                            break;
                        case AhegaoState.faintnessSpeed:
                            DoAhegaoFaintnessSpeed();
                            break;
                        case AhegaoState.orgasm:
                            DoAhegaoOrgasm();
                            break;
                    }
                }
            }

            public static void DoAhegaoOrgasm()
            {
                for (int i = 0; i < femalesCount; i++)
                {
                    MoveEyes(i, originalEyeY[i] + eyeRollAmountOrgasm.Value, originalEyeX[i] + eyeCrossAmountOrgasm.Value);
                    hSceneFemales[i].face.ChangeEyesPtn(eyePtnOrgasm.Value);
                    hSceneFemales[i].face.ChangeEyebrowPtn(eyeBrowPtnOrgasm.Value);
                    hSceneFemales[i].face.ChangeEyesBlinkFlag(blinkOrgasm.Value);
                    //hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(originalEyeY[i] + eyeRollAmountOrgasm.Value));
                    //hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(originalEyeX[i] + eyeCrossAmountOrgasm.Value));
                    hSceneFemales[i].face.HideEyeHighlight(!eyeHighlightOrgasm.Value);
                    hSceneFemales[i].face.fileStatus.tearsLv = (byte)tearsLevelOrgasm.Value;
                    hSceneFemales[i].face.ChangeMouthPtn(mouthPtnOrgasm.Value, true);
                    hSceneFemales[i].face.ChangeMouthOpenMin(openMouthMinOrgasm.Value);
                    hSceneFemales[i].face.ChangeMouthOpenMax(openMouthMinOrgasm.Value);
                    hSceneFemales[i].face.ChangeHohoAkaRate(new Il2CppSystem.Nullable<float>(originalBlush[i] + blushOrgasm.Value));
                    if (blinkOrgasm.Value)
                    {
                        hSceneFemales[i].face.ChangeEyesOpenMax(openEyeOrgasm.Value * 0.92f);
                        hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                    }
                    else
                    {
                        hSceneFemales[i].face.eyesCtrl.SetCorrectOpenMax(openEyeOrgasm.Value);
                        hSceneFemales[i].face.eyesCtrl.OpenMin = openEyeOrgasm.Value;
                    }
                }
            }

            public static void DoAhegaoFaintness()
            {
                for (int i = 0; i < femalesCount; i++)
                {
                    MoveEyes(i, originalEyeY[i] + eyeRollAmountFaintness.Value, originalEyeX[i] + eyeCrossAmountFaintness.Value);
                    hSceneFemales[i].face.ChangeEyesPtn(eyePtnFaintness.Value);
                    hSceneFemales[i].face.ChangeEyebrowPtn(eyeBrowPtnFaintness.Value);
                    hSceneFemales[i].face.ChangeEyesBlinkFlag(blinkFaintness.Value);
                    //hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(originalEyeY[i] + eyeRollAmountFaintness.Value));
                    //hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(originalEyeX[i] + eyeCrossAmountFaintness.Value));
                    hSceneFemales[i].face.HideEyeHighlight(!eyeHighlightFaintness.Value);
                    hSceneFemales[i].face.fileStatus.tearsLv = (byte)tearsLevelFaintness.Value;
                    hSceneFemales[i].face.ChangeMouthPtn(mouthPtnFaintness.Value, true);
                    hSceneFemales[i].face.ChangeMouthOpenMin(openMouthMinFaintness.Value);
                    hSceneFemales[i].face.ChangeMouthOpenMax(openMouthMinFaintness.Value);
                    hSceneFemales[i].face.ChangeHohoAkaRate(new Il2CppSystem.Nullable<float>(originalBlush[i] + blushFaintness.Value));
                    if (blinkFaintness.Value)
                    {
                        hSceneFemales[i].face.ChangeEyesOpenMax(openEyeFaintness.Value * 0.92f);
                        hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                    }
                    else
                    {
                        hSceneFemales[i].face.eyesCtrl.SetCorrectOpenMax(openEyeFaintness.Value);
                        hSceneFemales[i].face.eyesCtrl.OpenMin = openEyeFaintness.Value;
                    }
                }
            }

            public static void DoAhegaoFaintnessSpeed()
            {
                for (int i = 0; i < femalesCount; i++)
                {
                    MoveEyes(i, originalEyeY[i] + eyeRollAmountFaintnessSpeed.Value, originalEyeX[i] + eyeCrossAmountFaintnessSpeed.Value);
                    hSceneFemales[i].face.ChangeEyesPtn(eyePtnFaintnessSpeed.Value);
                    hSceneFemales[i].face.ChangeEyebrowPtn(eyeBrowPtnFaintnessSpeed.Value);
                    hSceneFemales[i].face.ChangeEyesBlinkFlag(blinkFaintnessSpeed.Value);
                    //hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(originalEyeY[i] + eyeRollAmountFaintnessSpeed.Value));
                    //hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(originalEyeX[i] + eyeCrossAmountFaintnessSpeed.Value));
                    hSceneFemales[i].face.HideEyeHighlight(!eyeHighlightFaintnessSpeed.Value);
                    hSceneFemales[i].face.fileStatus.tearsLv = (byte)tearsLevelFaintnessSpeed.Value;
                    hSceneFemales[i].face.ChangeMouthPtn(mouthPtnFaintnessSpeed.Value, true);
                    hSceneFemales[i].face.ChangeMouthOpenMin(openMouthMinFaintnessSpeed.Value);
                    hSceneFemales[i].face.ChangeMouthOpenMax(openMouthMinFaintnessSpeed.Value);
                    hSceneFemales[i].face.ChangeHohoAkaRate(new Il2CppSystem.Nullable<float>(originalBlush[i] + blushFaintnessSpeed.Value));
                    if (blinkFaintnessSpeed.Value)
                    {
                        hSceneFemales[i].face.ChangeEyesOpenMax(openEyeFaintnessSpeed.Value * 0.92f);
                        hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                    }
                    else
                    {
                        hSceneFemales[i].face.eyesCtrl.SetCorrectOpenMax(openEyeFaintnessSpeed.Value);
                        hSceneFemales[i].face.eyesCtrl.OpenMin = openEyeFaintnessSpeed.Value;
                    }
                }
            }
            public static void ResetAhegao()
            {
                for (int i = 0; i < femalesCount; i++)
                {
                    log.LogMessage($"Original face eye openMin = {originalEyeOpenMin[i]}");
                    hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                    //hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(originalEyeY[i]));
                    //hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(originalEyeX[i]));
                    MoveEyes(i, originalEyeY[i], originalEyeX[i]);

                }
            }

            public static void ResetAhegaoInstant()
            {
                for (int i = 0; i < femalesCount; i++)
                {
                    log.LogMessage($"Original face eye openMin = {originalEyeOpenMin[i]}");
                    hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                    hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(originalEyeY[i]));
                    hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(originalEyeX[i]));
                }
            }

            public static class Hooks
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(HScene), "Start")]
                public static void StartHook(HScene __instance)
                {
                    hScene = __instance;
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(HScene), "ChangeModeCtrl")]
                public static void ChangeModeCtrlHook()
                {
                    //Apply only once per HScene
                    if (!applied)
                    {
                        GetFemales();
                        applied = true;
                    }
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(HSceneFlagCtrl), "AddOrgasm")]
                public static void HSceneFlagCtrlAddOrgasm()
                {
                    nowFemaleOrgasm = true;
                    if (orgasms < 3)
                        orgasms++;
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HVoiceCtrl), "SetFace")]
                public static bool SetFaceHook(bool __runOriginal)
                {
                    if (state != AhegaoState.none)
                    {
                        return __runOriginal = false;
                    }
                    return __runOriginal = true;
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HScene), "Update")]
                public static void PreUpdateHook()
                {
                    if (ahegao.Value)
                    {
                        if (state != AhegaoState.orgasm && (ahegaoOnOrgasm.Value || orgasms > 2) && hScene.CtrlFlag.NowOrgasm && nowFemaleOrgasm)
                        {
                            log.LogMessage("Orgasm. State = orgasm");
                            state = AhegaoState.orgasm;
                            DoAhegaoOrgasm();
                        }
                        else if (state == AhegaoState.orgasm)
                        {
                            if (!hScene.CtrlFlag.NowOrgasm && nowFemaleOrgasm)
                            {
                                log.LogMessage("Orgasm over. Resetting nowFemaleOrgasm");
                                nowFemaleOrgasm = false;
                                state = AhegaoState.afterOrgasm;
                            }
                            else
                            {
                                //Do nothing
                            }
                        }
                        else if (hScene.CtrlFlag.IsFaintness && hScene._sprite.CategoryFinish._houshiPosKind == 0)
                        {
                            if (hScene.CtrlFlag.LoopType == 2)
                            {
                                if (state != AhegaoState.faintnessSpeed)
                                {
                                    log.LogMessage("LoopType 2. State = faintnessSpeed");
                                    state = AhegaoState.faintnessSpeed;
                                    DoAhegaoFaintnessSpeed();
                                }
                                else
                                {
                                    //Do nothing
                                }
                            }
                            else if (state != AhegaoState.faintnessSpeed && (hScene.CtrlFlag.Speed >= minSpeed.Value))
                            {
                                log.LogMessage("Enough speed. State = faintnessSpeed");
                                state = AhegaoState.faintnessSpeed;
                                DoAhegaoFaintnessSpeed();
                            }
                            else if (state != AhegaoState.faintness && (hScene.CtrlFlag.Speed < minSpeed.Value))
                            {
                                log.LogMessage("Not enough speed. State = faintness");
                                state = AhegaoState.faintness;
                                DoAhegaoFaintness();
                            }
                        }
                        else if (hScene.CtrlFlag.IsFaintness && hScene._sprite.CategoryFinish._houshiPosKind == 1)
                        {
                            if (state != AhegaoState.faintness)
                            {
                                log.LogMessage("HoushiPosKind 1. State = faintness");
                                state = AhegaoState.faintness;
                                DoAhegaoFaintness();
                            }
                            else
                            {
                                //Do nothing
                            }
                        }
                        else if (state != AhegaoState.none)
                        {
                            log.LogMessage("No ahegao, resetting state");
                            state = AhegaoState.none;
                            ResetAhegao();
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
                    state = AhegaoState.none;
                    ResetAhegaoInstant();
                    hScene = null;
                    applied = false;
                    femalesCount = 0;
                    hSceneFemales = null;
                }
            }
        }
    }
}
