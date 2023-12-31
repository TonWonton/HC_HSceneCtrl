using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Character;
using H;
using HarmonyLib;
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
        public const string PluginVersion = "1.0.0";
        //Instances
        public static Ahegao ahegaoInstance;
        public static AhegaoComponent currentComponent;
        //Ahegao
        public static ConfigEntry<bool> ahegao;
        public static ConfigEntry<bool> ahegaoOnOrgasm;
        public static ConfigEntry<int> orgasmAmount;
        public static ConfigEntry<float> minSpeed;
        public static ConfigEntry<float> eyeMoveSpeed;
        public static ConfigEntry<float> maxBlush;
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

        public override void Load()
        {
            //Ahegao
            ahegao = Config.Bind("Ahegao", "Ahegao", false, "Enable ahegao");
            ahegaoOnOrgasm = Config.Bind("Ahegao", "Ahegao on normal orgasm", false);
            orgasmAmount = Config.Bind("Ahegao", "Orgasm amount for faintness", 3, new ConfigDescription("Set amount of orgasms needed for faintness", new AcceptableValueRange<int>(2, 10)));
            minSpeed = Config.Bind("Ahegao", "Minimum speed for eye roll during faintness", 0.75f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 2f)));
            eyeMoveSpeed = Config.Bind("Ahegao", "Eye move speed", 15f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 40f)));
            maxBlush = Config.Bind("Ahegao", "Maximum blush amount", 2.2f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 3.4f)));
            //Ahegao orgasm
            eyePtnOrgasm = Config.Bind("Ahegao orgasm", "Eye pattern", 7, new ConfigDescription("Set eye pattern", new AcceptableValueRange<int>(0, 24)));
            eyeBrowPtnOrgasm = Config.Bind("Ahegao orgasm", "Eyebrow pattern", 8, new ConfigDescription("Set eyebrow pattern", new AcceptableValueRange<int>(0, 10)));
            blinkOrgasm = Config.Bind("Ahegao orgasm", "Blink during ahegao", false);
            openEyeOrgasm = Config.Bind("Ahegao orgasm", "Eye open amount", 0.72f, new ConfigDescription("Set eye open amount", new AcceptableValueRange<float>(0f, 1f)));
            eyeRollAmountOrgasm = Config.Bind("Ahegao orgasm", "Eye roll amount during ahegao and faintness", 0.25f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeCrossAmountOrgasm = Config.Bind("Ahegao orgasm", "Eye cross offset amount during ahegao and faintness", 0.175f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeHighlightOrgasm = Config.Bind("Ahegao orgasm", "Eye highlight during ahegao", true);
            tearsLevelOrgasm = Config.Bind("Ahegao orgasm", "Tears level during ahegao and faintness", 3, new ConfigDescription("Set tears level", new AcceptableValueList<int>(0, 1, 2, 3)));
            mouthPtnOrgasm = Config.Bind("Ahegao orgasm", "Mouth pattern", 18, new ConfigDescription("Set mouth pattern", new AcceptableValueRange<int>(0, 27)));
            openMouthMinOrgasm = Config.Bind("Ahegao orgasm", "Mouth open amount", 0.33f, new ConfigDescription("Set mouth open amount", new AcceptableValueRange<float>(0f, 1f)));
            blushOrgasm = Config.Bind("Ahegao orgasm", "Blush amount during ahegao and faintness", 2f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 3.4f)));
            //Ahegao faintness
            eyePtnFaintness = Config.Bind("Ahegao faintness", "Eye pattern", 2, new ConfigDescription("Set eye pattern", new AcceptableValueRange<int>(0, 24)));
            eyeBrowPtnFaintness = Config.Bind("Ahegao faintness", "Eyebrow pattern", 1, new ConfigDescription("Set eyebrow pattern", new AcceptableValueRange<int>(0, 10)));
            blinkFaintness = Config.Bind("Ahegao faintness", "Blink during ahegao", true);
            openEyeFaintness = Config.Bind("Ahegao faintness", "Eye open amount", 0.82f, new ConfigDescription("Set eye open amount", new AcceptableValueRange<float>(0f, 1f)));
            eyeRollAmountFaintness = Config.Bind("Ahegao faintness", "Eye roll amount during ahegao and faintness", 0.075f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeCrossAmountFaintness = Config.Bind("Ahegao faintness", "Eye cross offset amount during ahegao and faintness", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeHighlightFaintness = Config.Bind("Ahegao faintness", "Eye highlight during ahegao", true);
            tearsLevelFaintness = Config.Bind("Ahegao faintness", "Tears level during ahegao and faintness", 1, new ConfigDescription("Set tears level", new AcceptableValueList<int>(0, 1, 2, 3)));
            mouthPtnFaintness = Config.Bind("Ahegao faintness", "Mouth pattern", 3, new ConfigDescription("Set mouth pattern", new AcceptableValueRange<int>(0, 27)));
            openMouthMinFaintness = Config.Bind("Ahegao faintness", "Mouth open amount", 0.33f, new ConfigDescription("Set mouth open amount", new AcceptableValueRange<float>(0f, 1f)));
            blushFaintness = Config.Bind("Ahegao faintness", "Blush amount during ahegao and faintness", 1.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 3.4f)));
            //Ahegao faintness speed
            eyePtnFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye pattern", 9, new ConfigDescription("Set eye pattern", new AcceptableValueRange<int>(0, 24)));
            eyeBrowPtnFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eyebrow pattern", 8, new ConfigDescription("Set eyebrow pattern", new AcceptableValueRange<int>(0, 10)));
            blinkFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Blink during ahegao", false);
            openEyeFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye open amount", 0.82f, new ConfigDescription("Set eye open amount", new AcceptableValueRange<float>(0f, 1f)));
            eyeRollAmountFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye roll amount during ahegao and faintness", 0.15f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeCrossAmountFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye cross offset amount during ahegao and faintness", 0.1f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            eyeHighlightFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Eye highlight during ahegao", true);
            tearsLevelFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Tears level during ahegao and faintness", 2, new ConfigDescription("Set tears level", new AcceptableValueList<int>(0, 1, 2, 3)));
            mouthPtnFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Mouth pattern", 10, new ConfigDescription("Set mouth pattern", new AcceptableValueRange<int>(0, 27)));
            openMouthMinFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Mouth open amount", 0.63f, new ConfigDescription("Set mouth open amount", new AcceptableValueRange<float>(0f, 1f)));
            blushFaintnessSpeed = Config.Bind("Ahegao faintness speed", "Blush amount during ahegao and faintness", 1.75f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 3.4f)));
            //Make settings reflect in gameplay when config is changed if settings changed are current ahegao state
            //Ahegao
            ahegao.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.state);
            ahegaoOnOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.state);
            orgasmAmount.SettingChanged += (sender, args) => AhegaoComponent.SetOrgasmAmount();
            minSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.state);
            maxBlush.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.state);
            //Ahegao orgasm
            eyePtnOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            eyeBrowPtnOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            blinkOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            openEyeOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            eyeRollAmountOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            eyeCrossAmountOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            eyeHighlightOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            tearsLevelOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            mouthPtnOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            openMouthMinOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            blushOrgasm.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.orgasm);
            //Ahegao faintness
            eyePtnFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            eyeBrowPtnFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            blinkFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            openEyeFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            eyeRollAmountFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            eyeCrossAmountFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            eyeHighlightFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            tearsLevelFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            mouthPtnFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            openMouthMinFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            blushFaintness.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintness);
            //Ahegao faintness speed
            eyePtnFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            eyeBrowPtnFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            blinkFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            openEyeFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            eyeRollAmountFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            eyeCrossAmountFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            eyeHighlightFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            tearsLevelFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            mouthPtnFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            openMouthMinFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            blushFaintnessSpeed.SettingChanged += (sender, args) => AhegaoComponent.UpdateAhegaoSettings(AhegaoComponent.AhegaoState.faintnessSpeed);
            //Patch methods
            Harmony.CreateAndPatchAll(typeof(AhegaoComponent.Hooks), GUID);
            ahegaoInstance = this;
        }

        private void AddAhegaoComponent()
        {
            currentComponent = this.AddComponent<AhegaoComponent>();
        }

        public class AhegaoComponent : MonoBehaviour
        {
            //Instances
            private static HScene hScene;
            //Female data
            public static AhegaoState state;
            private static int femalesCount;
            private static Human[] hSceneFemales;
            private static float[] originalEyeOpenMin;
            private static float[] originalEyeY;
            private static float[] originalEyeX;
            private static float[] originalBlush;
            private static bool nowFemaleOrgasm;
            private static bool ahegaoOrgasmProc;
            private static bool ahegaoMainFemaleProc;
            private static bool ahegaoBothFemaleProc;
            //MoveEyes() variables
            private static float[] moveArray;
            private static int target;
            private static float startY;
            private static float startX;
            private static float targetY;
            private static float targetX;
            private static float posPercent;
            private static bool doBlend;
            private static bool moveEyes;
            private static bool moveEyesBoth;
            //Variables
            private static bool applied;
            //Bool flag functions
            private static bool ahegaoOrgasmShouldProc() {
                return nowFemaleOrgasm && (ahegaoOnOrgasm.Value || hScene.CtrlFlag.IsFaintness);
            }
            
            //States for ahegao in order to not apply ahegao when not needed and to control logic
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
                //MoveEyes() function will calculate movement and set moveEyes to true
                //which will be seen by Update() and will call DoMoveEyes()
                //until eyes reach target position, which will set moveEyes to false
                if (moveEyes && doBlend)
                {
                    DoMoveEyes(target);
                }
                else if (moveEyesBoth && doBlend)
                {
                    DoMoveEyesBoth(moveArray);
                }
                else if ((moveEyes || moveEyesBoth) && !doBlend)
                {
                    for (int i = 0; i < femalesCount; i++)
                    {
                        hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(targetY));
                        hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(targetX));
                    }
                    moveEyes = false;
                }
            }

            public static void DoMoveEyes(int i)
            {
                //Move eyes and if target reached stop moving
                posPercent += Time.deltaTime * eyeMoveSpeed.Value;
                hSceneFemales[i].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(Mathf.Lerp(startY, targetY, posPercent)));
                hSceneFemales[i].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(Mathf.Lerp(startX, targetX, posPercent)));
                if (posPercent >= 1)
                {
                    moveEyes = false;
                }
            }

            public static void DoMoveEyesBoth(float[] moveEyesData)
            {
                //Move eyes and if target reached stop moving
                posPercent += Time.deltaTime * eyeMoveSpeed.Value;
                hSceneFemales[0].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(Mathf.Lerp(moveArray[0], moveArray[1], posPercent)));
                hSceneFemales[0].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(Mathf.Lerp(moveArray[2], moveArray[3], posPercent)));
                hSceneFemales[1].face.ChangeSettingEyePosY(new Il2CppSystem.Nullable<float>(Mathf.Lerp(moveArray[4], moveArray[5], posPercent)));
                hSceneFemales[1].face.ChangeSettingEyePosX(new Il2CppSystem.Nullable<float>(Mathf.Lerp(moveArray[6], moveArray[7], posPercent)));
                if (posPercent >= 1)
                {
                    moveEyesBoth = false;
                }
            }

            public static void MoveEyes(int i, float targetPositionY, float targetPositionX, bool blend)
            {
                //Set start and targets for lerp
                posPercent = 0;
                startY = hSceneFemales[i].fileCustom.Face.eyeY;
                startX = hSceneFemales[i].fileCustom.Face.eyeX;
                targetY = targetPositionY;
                targetX = targetPositionX;
                doBlend = blend;
                if (targetY > 1f)
                    targetY = 1f;
                else if (targetY < 0f)
                    targetY = 0f;
                if (targetX > 1f)
                    targetX = 1f;
                else if (targetX < 0f)
                    targetX = 0f;
                //If both female ahegao
                if (ahegaoBothFemaleProc)
                {
                    if (i == 0)
                    {
                        moveArray[0] = hSceneFemales[0].fileCustom.Face.eyeY;
                        moveArray[1] = targetY;
                        moveArray[2] = hSceneFemales[0].fileCustom.Face.eyeX;
                        moveArray[3] = targetX;
                    }
                    else if (i == 1)
                    {
                        moveArray[4] = hSceneFemales[1].fileCustom.Face.eyeY;
                        moveArray[5] = targetY;
                        moveArray[6] = hSceneFemales[1].fileCustom.Face.eyeX;
                        moveArray[7] = targetX;
                        moveEyesBoth = true;
                    }
                }
                //If only one female ahegao
                else
                {
                    target = i;
                    moveEyes = true;
                }
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
                            femalesList.Add(female);
                    }
                    //Get femalesCount and initialize arrays
                    femalesCount = femalesList.Count;
                    hSceneFemales = new Human[femalesCount];
                    originalEyeOpenMin = new float[femalesCount];
                    originalEyeY = new float[femalesCount];
                    originalEyeX = new float[femalesCount];
                    originalBlush = new float[femalesCount];
                    if (femalesCount > 1)
                        moveArray = new float[8];
                    //Put females into array and get original data
                    hSceneFemales = femalesList.ToArray();
                    for (int i = 0; i < femalesCount; i++)
                        GetOriginalFace(i);
                }
            }

            public static void GetOriginalFace(int i)
            {
                //Get original face data for calculations and resetting
                originalEyeOpenMin[i] = hSceneFemales[i].face.eyesCtrl.OpenMin;
                originalEyeY[i] = hSceneFemales[i].fileCustom.Face.eyeY;
                originalEyeX[i] = hSceneFemales[i].fileCustom.Face.eyeX;
                originalBlush[i] = hSceneFemales[i].face.fileStatus.hohoAkaRate;
            }

            public static void SetOrgasmAmount()
            {
                if (hScene != null)
                {
                    hScene.CtrlFlag.GotoFaintnessCount = orgasmAmount.Value;
                }
            }

            public static void UpdateAhegaoSettings(AhegaoState updateThis)
            {
                //If setting changed are the same as state, update current state ahegao
                if (hScene != null && state == updateThis)
                    switch (state)
                    {
                        case AhegaoState.faintness:
                            DoAhegao();
                            break;
                        case AhegaoState.faintnessSpeed:
                            DoAhegao();
                            break;
                        case AhegaoState.orgasm:
                            DoAhegao();
                            break;
                        case AhegaoState.none:
                            break;
                    }
            }

            public static void DoAhegao()
            {
                //Checks for which female to do ahegao on, and which state of ahegao to apply
                //Main female ahegao
                if (ahegaoMainFemaleProc)
                    switch (state)
                    {
                        case AhegaoState.orgasm:
                            DoAhegaoOrgasm(0);
                            break;
                        case AhegaoState.faintness:
                            DoAhegaoFaintness(0);
                            break;
                        case AhegaoState.faintnessSpeed:
                            DoAhegaoFaintnessSpeed(0);
                            break;
                    }
                //Both female ahegao
                else if (ahegaoBothFemaleProc)
                    switch (state)
                    {
                        case AhegaoState.orgasm:
                            for (int i = 0; i < femalesCount; i++)
                            DoAhegaoOrgasm(i);
                            break;
                        case AhegaoState.faintness:
                            for (int i = 0; i < femalesCount; i++)
                            DoAhegaoFaintness(i);
                            break;
                        case AhegaoState.faintnessSpeed:
                            for (int i = 0; i < femalesCount; i++)
                            DoAhegaoFaintnessSpeed(i);
                            break;
                    }
                //Les mode single female ahegao (can be either, modeCtrl - 1 is correct female)
                else if (hScene._mode == 6)
                    switch (state)
                    {
                        case AhegaoState.orgasm:
                            DoAhegaoOrgasm(hScene._modeCtrl - 1);
                            break;
                        case AhegaoState.faintness:
                            DoAhegaoFaintness(hScene._modeCtrl - 1);
                            break;
                        case AhegaoState.faintnessSpeed:
                            DoAhegaoFaintnessSpeed(hScene._modeCtrl - 1);
                            break;
                    }
            }

            public static void DoAhegaoOrgasm(int i)
            {
                //Call MoveEyes() to move eyes and change rest of settings
                MoveEyes(i, originalEyeY[i] + eyeRollAmountOrgasm.Value, originalEyeX[i] + eyeCrossAmountOrgasm.Value, true);
                hSceneFemales[i].face.ChangeEyesPtn(eyePtnOrgasm.Value);
                hSceneFemales[i].face.ChangeEyebrowPtn(eyeBrowPtnOrgasm.Value);
                hSceneFemales[i].face.ChangeEyesBlinkFlag(blinkOrgasm.Value);
                hSceneFemales[i].face.HideEyeHighlight(!eyeHighlightOrgasm.Value);
                hSceneFemales[i].face.fileStatus.tearsLv = (byte)tearsLevelOrgasm.Value;
                hSceneFemales[i].face.ChangeMouthPtn(mouthPtnOrgasm.Value, true);
                hSceneFemales[i].face.ChangeMouthOpenMin(openMouthMinOrgasm.Value);
                hSceneFemales[i].face.ChangeMouthOpenMax(openMouthMinOrgasm.Value);
                //Check if blush is over max limit since ahegao settings is offset
                float newBlush = originalBlush[i] + blushOrgasm.Value;
                if (newBlush > maxBlush.Value)
                    newBlush = maxBlush.Value;
                hSceneFemales[i].face.ChangeHohoAkaRate(new Il2CppSystem.Nullable<float>(newBlush));
                if (blinkOrgasm.Value) //If blinking enabled set EyesOpenMax/Min
                {
                    hSceneFemales[i].face.ChangeEyesOpenMax(openEyeOrgasm.Value * 0.92f);
                    hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                }
                else //If blinking disabled set static EyeOpen
                {
                    hSceneFemales[i].face.eyesCtrl.SetCorrectOpenMax(openEyeOrgasm.Value);
                    hSceneFemales[i].face.eyesCtrl.OpenMin = openEyeOrgasm.Value;
                }
            }

            public static void DoAhegaoFaintness(int i)
            {
                //Call MoveEyes() to move eyes and change rest of settings
                MoveEyes(i, originalEyeY[i] + eyeRollAmountFaintness.Value, originalEyeX[i] + eyeCrossAmountFaintness.Value, true);
                hSceneFemales[i].face.ChangeEyesPtn(eyePtnFaintness.Value);
                hSceneFemales[i].face.ChangeEyebrowPtn(eyeBrowPtnFaintness.Value);
                hSceneFemales[i].face.ChangeEyesBlinkFlag(blinkFaintness.Value);
                hSceneFemales[i].face.HideEyeHighlight(!eyeHighlightFaintness.Value);
                hSceneFemales[i].face.fileStatus.tearsLv = (byte)tearsLevelFaintness.Value;
                hSceneFemales[i].face.ChangeMouthPtn(mouthPtnFaintness.Value, true);
                hSceneFemales[i].face.ChangeMouthOpenMin(openMouthMinFaintness.Value);
                hSceneFemales[i].face.ChangeMouthOpenMax(openMouthMinFaintness.Value);
                //Check if blush is over max limit since ahegao settings is offset
                float newBlush = originalBlush[i] + blushFaintness.Value;
                if (newBlush > maxBlush.Value)
                    newBlush = maxBlush.Value;
                hSceneFemales[i].face.ChangeHohoAkaRate(new Il2CppSystem.Nullable<float>(newBlush));
                if (blinkFaintness.Value) //If blinking enabled set EyesOpenMax/Min
                {
                    hSceneFemales[i].face.ChangeEyesOpenMax(openEyeFaintness.Value * 0.92f);
                    hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                }
                else //If blinking disabled set static EyeOpen
                {
                    hSceneFemales[i].face.eyesCtrl.SetCorrectOpenMax(openEyeFaintness.Value);
                    hSceneFemales[i].face.eyesCtrl.OpenMin = openEyeFaintness.Value;
                }
            }

            public static void DoAhegaoFaintnessSpeed(int i)
            {
                //Call MoveEyes() to move eyes and change rest of settings
                MoveEyes(i, originalEyeY[i] + eyeRollAmountFaintnessSpeed.Value, originalEyeX[i] + eyeCrossAmountFaintnessSpeed.Value, true);
                hSceneFemales[i].face.ChangeEyesPtn(eyePtnFaintnessSpeed.Value);
                hSceneFemales[i].face.ChangeEyebrowPtn(eyeBrowPtnFaintnessSpeed.Value);
                hSceneFemales[i].face.ChangeEyesBlinkFlag(blinkFaintnessSpeed.Value);
                hSceneFemales[i].face.HideEyeHighlight(!eyeHighlightFaintnessSpeed.Value);
                hSceneFemales[i].face.fileStatus.tearsLv = (byte)tearsLevelFaintnessSpeed.Value;
                hSceneFemales[i].face.ChangeMouthPtn(mouthPtnFaintnessSpeed.Value, true);
                hSceneFemales[i].face.ChangeMouthOpenMin(openMouthMinFaintnessSpeed.Value);
                //Check if blush is over max limit since ahegao settings is offset
                float newBlush = originalBlush[i] + blushFaintnessSpeed.Value;
                if (newBlush > maxBlush.Value)
                    newBlush = maxBlush.Value;
                hSceneFemales[i].face.ChangeHohoAkaRate(new Il2CppSystem.Nullable<float>(newBlush));
                if (blinkFaintnessSpeed.Value) //If blinking enabled set EyesOpenMax/Min
                {
                    hSceneFemales[i].face.ChangeEyesOpenMax(openEyeFaintnessSpeed.Value * 0.92f);
                    hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                }
                else //If blinking disabled set static EyeOpen
                {
                    hSceneFemales[i].face.eyesCtrl.SetCorrectOpenMax(openEyeFaintnessSpeed.Value);
                    hSceneFemales[i].face.eyesCtrl.OpenMin = openEyeFaintnessSpeed.Value;
                }
            }
            public static void ResetAhegao()
            {
                //Reset ahegao
                for (int i = 0; i < femalesCount; i++)
                {
                    hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                    MoveEyes(i, originalEyeY[i], originalEyeX[i], true);
                }
            }

            public static void ResetAhegaoInstant()
            {
                //Instantly reset ahegao
                for (int i = 0; i < femalesCount; i++)
                {
                    hSceneFemales[i].face.eyesCtrl.OpenMin = originalEyeOpenMin[i];
                    hSceneFemales[i].fileCustom.Face.eyeY = originalEyeY[i];
                    hSceneFemales[i].fileCustom.Face.eyeX = originalEyeX[i];
                }
            }

            public static class Hooks
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(HScene), "Start")]
                public static void StartHook(HScene __instance)
                {
                    //Get instance and create component
                    hScene = __instance;
                    ahegaoInstance.AddAhegaoComponent();
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(HScene), "ChangeModeCtrl")]
                public static void ChangeModeCtrlHook()
                {
                    
                    //Apply only once per HScene
                    if (!applied)
                    {
                        GetFemales();
                        SetOrgasmAmount();
                        applied = true;
                    }
                    //Check which female can ahegao
                    if (hScene._mode == 6 && hScene._modeCtrl == 0 || hScene._mode == 7)
                    {
                        ahegaoMainFemaleProc = false;
                        ahegaoBothFemaleProc = true;
                    }
                    else if (!(hScene._mode == 6 && hScene._modeCtrl == 2))
                    {
                        ahegaoMainFemaleProc = true;
                        ahegaoBothFemaleProc = false;
                    }
                    else
                    {
                        ahegaoMainFemaleProc = false;
                        ahegaoBothFemaleProc = false;
                    }
                    //If faintness, set ahegao to normal faintness on mode change for all females
                    //then set correct ahegao for female that can ahegao
                    if (hScene.CtrlFlag.IsFaintness && ahegao.Value)
                    {
                        for (int i = 0; i < femalesCount; i++)
                        {
                            DoAhegaoFaintness(i);
                            state = AhegaoState.faintness;
                        }
                        DoAhegao();
                    }
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(HSceneFlagCtrl), "AddOrgasm")]
                public static void HSceneFlagCtrlAddOrgasm()
                {
                    //Set flag for nowFemaleOrgasm
                    nowFemaleOrgasm = true;
                    ahegaoOrgasmProc = ahegaoOrgasmShouldProc();
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HVoiceCtrl), "SetFace")]
                public static bool SetFaceHook(bool __runOriginal)
                {
                    //If ahegao skip SetFace, else let SetFace run to reset ahegao
                    if (state != AhegaoState.none)
                        return __runOriginal = false;
                    return __runOriginal = true;
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HScene), "Update")]
                public static void PreUpdateHook()                                      //State system to control ahegao and only apply face once //
                {                                                                       //in order to not tank performance. Will set state and not//
                    if (ahegao.Value)                                                   //check for itself. Ahegao state disables SetFace so only //
                    {                                                                   //need to apply face once since ahegao doesn't get reset. //
                        //Check for orgasm. ahegaoOrgasmProc is set when femaleOrgasm
                        if (state != AhegaoState.orgasm && ahegaoOrgasmProc && hScene.CtrlFlag.NowOrgasm)
                        {
                            state = AhegaoState.orgasm;
                            DoAhegao();
                        }
                        else if (state == AhegaoState.orgasm)
                        {
                            //If orgasm is over reset state and ahegaoOrgasmProc
                            if (!hScene.CtrlFlag.NowOrgasm && nowFemaleOrgasm)
                            {
                                nowFemaleOrgasm = false;
                                ahegaoOrgasmProc = ahegaoOrgasmShouldProc();
                                state = AhegaoState.afterOrgasm;
                            }
                            else { /*Do nothing if still orgasming*/ }
                        }
                        //Reset ahegao if not in orgasm or faintness
                        else if (!hScene.CtrlFlag.IsFaintness)
                        {
                            if (state != AhegaoState.none)
                            {
                                state = AhegaoState.none;
                                ResetAhegao();
                            }
                            else { /*Do nothing if state already set to none*/ }
                        }
                        //If not houshi(male foreplay) and faintness
                        else if (hScene.CtrlFlag.IsFaintness && hScene._sprite.CategoryFinish._houshiPosKind == 0)
                        {
                            //If loopType 2 always set state to faintnessSpeed
                            if (hScene.CtrlFlag.LoopType == 2)
                            {
                                if (state != AhegaoState.faintnessSpeed)
                                {
                                    state = AhegaoState.faintnessSpeed;
                                    DoAhegao();
                                }
                                else { /*Do nothing if state already set to faintness speed*/ }
                            }
                            //If enough speed set state to faintnessSpeed
                            else if (state != AhegaoState.faintnessSpeed && (hScene.CtrlFlag.Speed >= minSpeed.Value))
                            {
                                state = AhegaoState.faintnessSpeed;
                                DoAhegao();
                            }
                            //If not enough speed set state to faintness
                            else if (state != AhegaoState.faintness && !(hScene.CtrlFlag.Speed >= minSpeed.Value))
                            {
                                state = AhegaoState.faintness;
                                DoAhegao();
                            }
                        }
                        //If houshi(male foreplay) always do faintness instead of faintnessSpeed
                        else if (hScene.CtrlFlag.IsFaintness && hScene._sprite.CategoryFinish._houshiPosKind == 1)
                        {
                            if (state != AhegaoState.faintness)
                            {
                                state = AhegaoState.faintness;
                                DoAhegao();
                            }
                            else { /*Do nothing if state already set to faintness*/ }
                        }
                    }
                    else if (!ahegao.Value)
                    {
                        if (state != AhegaoState.none)
                        {
                            state = AhegaoState.none;
                            ResetAhegao();
                        }
                        else { /*Do nothing if state already set to none*/ }
                    }
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(HScene), "OnDestroy")]
                public static void HScenePreOnDestroy()
                {
                    //Reset ahegao to prevent permanent changes and destroy component
                    ResetAhegaoInstant();
                    state = AhegaoState.none;
                    applied = false;
                    femalesCount = 0;
                    hSceneFemales = null;
                    nowFemaleOrgasm = false;
                    moveArray = null;
                    moveEyes = false;
                    moveEyesBoth = false;
                    Destroy(currentComponent);
                    hScene = null;
                }
            }
        }
    }
}
