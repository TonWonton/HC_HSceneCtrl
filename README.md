## Prerequisites
BepInEx Unity Il2Cpp (6.0.0-be-680 or newer)

https://github.com/BepInEx/BepInEx


BepisPlugins for HC

https://github.com/IllusionMods/BepisPlugins


BepInEx.ConfigurationManager Il2Cpp

https://github.com/BepInEx/BepInEx.ConfigurationManager


All prerequisites already included in HF-patch

https://github.com/ManlyMarco/HC-HF_Patch

## Installation
1. Install correct versions of prerequisites
2. Download from releases
3. Extract into game folder

## Plugins
- **HC_Ahegao v1.0.0**

Set different faces for faintness, faintness when speed is high enough, and orgasm

- **HC_HGaugeAndSpeedCtrl v1.0.1**

Control HGauge speeds, auto climax, and animation speeds.

- **HC_HSceneBreastJiggle v1.0.1**

Set custom values for breast softness during HScenes

## Description
Thanks to Sabakan for sharing their HC_HGaugeCtrl code and letting me use it.

## Known issues
**HC_HGaugeAndSpeedCtrl v1.0.1**
- Male gauge will still increase when it's not supposed to after female faintness, if female faintness alters the position to where male gauge shouldn't increase
- Gauge will also increase if in multi female houshi(male foreplay)
Fix currently being made and will be released when ready

**HC_Ahegao v1.0.0**
- In scenes where eyes are supposed to be closed (eg. bedroom scene), eyes position and type will be set to plugin settings instead

## HC_Ahegao features
- Set custom faces for 3 different states (faintness, faintness when min speed is reached, orgasm)
- Change eye type, blinking, open/close amount, eye roll/cross, eyebrows, etc.
- Change mouth type and settings
- Set blush and tear level
- Animated eye movement
- Enable ahegao on normal orgasms too
- Change minimum speed required for the different states
- Change amount of orgasms needed for faintness state

## HC_HGaugeAndSpeedCtrl features
- Female can trigger climax together
- Male can trigger climax together, and can also auto climax when their pleasure gauge is full. Priority: Both(together, inside, outside). Male solo(swallow, spit, outside)
- Adjust the speed of the pleasure gauge for male and female separately
- Pleasure gauge increase can now scale of speed if enabled in plugin settings
- Keybind to switch state between WLoop and SLoop while remembering speed. When in OLoop toggles between min and max speed
- Change animation speed

## HC_HSceneBreastJiggle features
- Set custom values for breast base softness, breast tip softness, breast weight, temporarily during HScene
- Scale down values for larger sizes

## Notes
- HC_HSceneBreastJiggle changes are not permanent and only lasts for the duration of the HScene
- HC_HSceneBreastJiggle and HC_Ahegao are disabled by default, enable in plugin settings
