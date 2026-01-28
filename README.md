# An errant Archipelago Subnautica Mod
A Subnautica mod client for Archipelago Randomizer. More info on Archipelago here: https://github.com/ArchipelagoMW/Archipelago

## Note:
This is NOT the regular archipelago subnautica mod. It's likely not the one you want!
Do not use this version unless you've found the accompanying archipelago library and are generating locally.

This mod is intended to be used with Ken's version of subnautica apworld here:
[https://github.com/kedNalatacId/alternate-subnautica-apworld](https://github.com/kedNalatacId/alternate-subnautica-apworld)

Noted differences:
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
- Depth of closest check is also printed.
- Allow ignoring all 3x vehicles logically, or removing them entirely
    - Use alternate logic if all 3x vehicles are gone
    - Allow the Cyclops Shield Generator to be built at the Moonpool if the Cyclops is not in game and the goal is "launch"
- Added a "Cancel" button to the login. If pressed, can play non-AP Subnautica.
- Removed Picture Frame scan possibility (accidental oversight)
- Added backwards compatibility for original archipelago mod
    - correctly parse and bypass: SwimRule, Propulsion Cannon, Laser Cutter, and Seaglide
- Added "Can Slip Through" option for bypassing Propulsion Cannon or Laser Cutter on certain checks
    - Fixed tracking for 2x weird spots in the Aurora when "Can Slip Through" is true
- not here (in python), interior growbed (as a separate item)
- flora (plant scanning; requires updating the encyclopedia.json)
- Allow naming games (makes it easier to play multiple games at once)
    - Archipelago games will also have "AP: " pre-pended to their style
    - e.g. `AP: Creative` or `AP: Survival`
- Don't learn the Ion Battery or Ion Power Cell from the Inactive Lava Zone (thanks to Berserker)
