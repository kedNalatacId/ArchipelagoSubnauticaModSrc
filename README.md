# An errant Archipelago Subnautica Mod
A Subnautica mod client for Archipelago Randomizer. More info on Archipelago here: https://github.com/ArchipelagoMW/Archipelago

## Note:
This is NOT the regular archipelago subnautica mod. It's likely not the one you want!
Do not use this version unless you've found the accompanying archipelago library and are generating locally.

<details>
<summary>How to use this mod</summary>

### Use and intention
This mod is intended to be used with Ken's version of subnautica apworld here:
[https://github.com/kedNalatacId/alternate-subnautica-apworld](https://github.com/kedNalatacId/alternate-subnautica-apworld)

### How to use this mod
- Install BepInEx
  - There are other guides for this, for your platform
- Move the OG archipelago subnautica dll aside, if installed
  - Keep the Archipelago.MultiClient.dll, if installed
- Compile this dll
  - using dotnet, something like:
    - dotnet build --configuration release
- Move it into place into your BepInEx plugins folder
  - if needed, create an archipelago folder
- If you do not have an Archipelago.MultiClient.dll, download and put into the same folder
- Using the alternate apworld (see link above), run `python3 exports.py` to create the encyclopedia.json and logic.json
  - move the encyclopedia.json into the archipelago plugin folder, overwriting the OG archipelago version of such if it exists
  - these files are not compatible with OG subnautica; they will have to be reverted. You may want to keep copies of the original versions for easy uninstall later, if you're so inclined.
- Launch Subnautica and see if the archipelago login appears on screen. Contact Ken with a debug log if not.

### Potential breaking changes

Newer Archipelago Subnautica mods hide the non-AP saves by using an alternate directory. If you want to access both AP and non-AP saves, you'll need to move the saves out of the subfolder and into the main folder. The saves are not "lost". If you are in the middle of an archipelago game you may want to hold off on changing out mods, just in case.

In a later update this mod will hopefully scan the alternate folder to have all saves available (if that's possible).

Additionally, the encyclopedia and logic json files are not compatible between the OG archipelago subnautica mod and this one.

### Noted differences (Why to use this mod)

- Swim depth is a range from 100-600
    - defaults to 200m (old "easy" mode)
- "Consider Items" is its own toggle
    - Otherwise the same logic RE: ultra glide fins and ultra high capacity tanks
- SeaglideDepth is a range from 100-400
    - defaults to 200m (old default)
- "PreSeaglideDistance" is a range from 600m to edge of map (2500m)
    - Shortening the range puts the seaglide very early in single-player mode
    - Anything outside this range is considered out of logic until the seaglide, seamoth, or cyclops is crafted
        - TODO: Add Prawn with grapple?
- "Ignore Radiation" means logic doesn't care about the radiation suit (opens aurora as sphere one)
- Number of checks is always printed (even if none are available)
  - A special "no checks in logic" message shown when no checks are in logic, making that state clearer.
- Depth of closest check is printed when available.
- Allow ignoring all 3x vehicles logically, or removing them entirely
    - Use alternate logic if all 3x vehicles are gone
    - Allow the Cyclops Shield Generator to be built at the Moonpool if the Cyclops is not in game and the goal is "launch"
- Added a "Cancel" button to the login. If pressed, can play non-AP Subnautica.
- Removed Picture Frame and Desk scan possibility (accidental oversight from original mod)
- Added backwards compatibility for original archipelago mod
    - correctly parse and bypass: SwimRule, Propulsion Cannon, Laser Cutter, and Seaglide
- Added "Can Slip Through" option for bypassing Propulsion Cannon or Laser Cutter on certain checks
    - Fixed tracking for 2x weird spots in the Aurora when "Can Slip Through" is true
- not here (in python), interior growbed (as a separate item)
- flora (plant scanning; requires updating the encyclopedia.json)
- Allow naming games (makes it easier to play multiple games at once)
    - Archipelago games will also have "AP: " pre-pended to their style
    - e.g. `AP: Creative` or `AP: Survival`
</details>
