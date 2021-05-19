Beef's Shader Fixes<a name="TOP"></a>
===================

## Completed features ##

  - Dynamically creates interleaved gradient noise texture on start (~1 second) to replace vanilla noise texture
  - Optimizes existing SSAO effect to increase both visual fidelity and performance (have your cake and eat it too)
  
## Planned Features ##

#### Short term: ####
    - Add support for custom configuration file
    - Alter tonemapping effect to further improve visuals
  
#### Long term: ####
    - Attempt to optimize volumetric lighting and shadows
    - Implement PCSS to soften the jagged shadow edges
    - Implement HBAO to replace SSAO

## Compatibility ##

This mod is compatible with existing saves and will not corrupt them when you install or uninstall the mod

However, it could interfere with other plugin based mods that alter visuals

## Installation ##

    Download the latest release https://github.com/TheRealBeef/Stationeers-Shader-Fixes/releases/latest
    Install BepInEx to your Stationeers folder in SteamApps/Common/Stationeers/
    Run Stationeers once to complete the BepInEx installation
    Install ShaderFixes.dll to the Stationeers/BepInEx/plugins folder
    Enjoy

## Contributions ##

    Please, feel free to contribute either in issues or in pull requests, or to fork this repo for your own take on Stationeers visuals.
