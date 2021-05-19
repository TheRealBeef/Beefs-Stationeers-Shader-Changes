This is a mod for Stationeers that provides players with a "walk" key - Left Control by default. Users can choose the key that slows them down and what speed to slow down to in Stationeers\BepInEx\config\SlowDown.cfg following installation of the mod (and running the game once with the mod installed).

To choose a different key than the default LeftControl, go to the config file at Stationeers\BepInEx\config\SlowDown.cfg" and change SlowKeyCode to a KeyCode from here: https://docs.unity3d.com/ScriptReference/KeyCode.html
Features

Completed features

  - Dynamically creates interleaved gradient noise texture on start (~1 second) to replace vanilla noise texture
  - Optimizes existing SSAO effect to increase both visual fidelity and performance (have your cake and eat it too)
  
Planned Features
  Short term:
    - Add support for custom configuration file
    - Alter tonemapping effect to further improve visuals
  
  Long term:
    - Attempt to optimize volumetric lighting and shadows
    - Implement PCSS to soften the jagged shadow edges
    - Implement HBAO to replace SSAO

Compatibility
With existing saves

    This mod is compatible with existing saves and will not corrupt them when you install or uninstall the mod

With other mods

    Could interfere with other mods that alter visuals

Installation

    Download the latest release https://github.com/TheRealBeef/Stationeers-Shader-Fixes/releases/latest
    Install BepInEx to your Stationeers folder in SteamApps/Common/Stationeers/
    Run Stationeers once to complete the BepInEx installation
    Install ShaderFixes.dll to the Stationeers/BepInEx/plugins folder
    Enjoy

Contributions

    Please, feel free to contribute either in issues or in pull requests, or to fork this repo for your own take on Stationeers visuals.
