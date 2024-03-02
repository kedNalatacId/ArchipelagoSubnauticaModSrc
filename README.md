# An errant Archipelago Subnautica Mod
A Subnautica mod client for Archipelago Randomizer. More info on Archipelago here: https://github.com/ArchipelagoMW/Archipelago

## Note:
This is NOT the regular archipelago subnautica mod. It's likely not the one you want!
Do not use this version unless you've found the accompanying archipelago library and are generating locally.

This mod is intended to be used with Ken's version of Archipelago here:
https://github.com/kedNalatacId/Archipelago/tree/subnautica

Noted differences:
- Swim depth is a range from 100-600
    - defaults to 200m (old "easy" mode)
- "Consider Items" is its own toggle
    - Otherwise the same logic RE: ultra glide fins and ultra high capacity tanks
- SeaglideDepth is a range from 100-400
    - defaults to 200m (old default)
- "Consider Exterior Growbed" adds (currently) 500m depth; iykyk
- "PreSeaglideDistance" is a range from 600m to edge of map (2500m)
    - Anything outside this range is considered out of logic until the seaglide is crafted
    - Shortening the range puts the seaglide very early  in single-player mode
- "Ignore Radiation" means logic doesn't care about the radiation suit (opens aurora as sphere one)
- Number of checks is always printed (even if none are available)
- Depth of closest check is also printed.

Hopefully coming soon:
- not showing fish not in logic
- showing depth of current "closest" fish
- flora?
