This is indubitably Beef's Shader Fixes<a name="TOP"></a>
===================

## Completed features ##

  - Dynamically creates interleaved gradient noise texture on start (~1 second) to replace vanilla noise texture
  - Optimizes existing SSAO effect to increase both visual fidelity and performance (have your cake and eat it too)
  - Optimize Volumetric Lighting - now much faster and can be run at medium or low quality with only a small loss in quality (vs big loss in vanilla) and huge increase in performance
  - Expose some options for bloom
  
## Planned Features ##

#### Short term: ####
    - Add support for custom configuration file to adjust high/low performance and high/low bloom effects
    - Alter the tonemapping effect to further improve visuals
  
#### Long term: ####
    - Attempt to optimize shadows
    - Implement PCSS to soften the jagged shadow edges
    - Implement HBAO to replace SSAO
    - Implement some kind of 

## Compatibility ##

This mod is compatible with existing saves and will not corrupt them when you install or uninstall the mod

However, it could interfere with other plugin based mods that alter visuals

## Installation ##

    Download the latest release https://github.com/TheRealBeef/Stationeers-Shader-Fixes/releases/latest
    Drop BepInEx to your Stationeers folder in SteamApps/Common/Stationeers/
    Run Stationeers once to complete the BepInEx installation
    Drop ShaderFixes.dll to the Stationeers/BepInEx/plugins folder
    Enjoy

## Contributions ##

    Please, feel free to contribute either in issues or in pull requests, or to fork 
    this repo for your own take on Stationeers visuals.
