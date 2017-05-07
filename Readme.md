[![RimWorld Alpha 16](https://img.shields.io/badge/RimWorld-Alpha 16-brightgreen.svg)](http://rimworldgame.com/)

Make your haulers understand that a tidy stockpile is an efficient stockpile.

# Important
This mod uses [Harmony](https://github.com/pardeike/harmony/), a fancy new toy for modders. You don't have to install harmony as it comes bundled with the mod, but if you encounter any errors, please let me know!

# Features
Adds a low-priority hauling job that makes your haulers merge stacks in your stockpiles. Works per stockpile, so if items are in different stockpiles they will not be merged. 

# Issues
As each storage building (e.g. vanilla racks, extended storage, deep storage, etc) count as a single stockpile (or technically, have their own SlotGroup - which for this mod is equivalent), stacks in different storage units will not be merged. 

# Notes
When I was playtesting this, [Stockpile Efficiency](http://steamcommunity.com/sharedfiles/filedetails/?id=857055488) was brought to my attention. These mods do pretty much exactly the same thing (in fact we both use the same vanilla methods for the actual hauling). The main difference is in how we keep track of what can be merged. Stockpile Efficiency searches for stacks that can potentially be merged whenever a pawn is looking for something to merge. Stack Merger keeps a permanent cache of stacks that can be merged. I haven't run any benchmarks, but I expect my mod to have a mostly constant, relatively low cpu load. Stockpile Efficiency will have no load as long as pawns don't have time to do the hauling jobs, but when they search for things to merge the cpu load will most likely be considerably larger. 



# Contributors
 - duduluu:	Chinese translations
 - Zhentar:	

# Think you found a bug? 
Please read [this guide](http://steamcommunity.com/sharedfiles/filedetails/?id=725234314) before creating a bug report,
 and then create a bug report [here](https://github.com/FluffierThanThou/StackMerger/issues)

# Older versions
All current and past versions of this mod can be downloaded from [GitHub](https://github.com/FluffierThanThou/StackMerger/releases).

# License
All original code in this mod is licensed under the [MIT license](https://opensource.org/licenses/MIT). Do what you want, but give me credit. 
All original content (e.g. text, imagery, sounds) in this mod is licensed under the [CC-BY-SA 4.0 license](http://creativecommons.org/licenses/by-sa/4.0/).

Parts of the code in this mod, and some content may be licensed by their original authors. If this is the case, the original author & license will either be given in the source code, or be in a LICENSE file next to the content. Please do not decompile my mods, but use the original source code available on [GitHub](https://github.com/FluffierThanThou/StackMerger/), so license information in the source code is preserved.

# Version
This is version v0.16.0.9