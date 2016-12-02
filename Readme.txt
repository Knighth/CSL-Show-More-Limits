CSL Show More Limits 
v1.6.0_f4 Build002

Purpose
-------
Basically an enhanced version of emf's Show Limits mod.
http://steamcommunity.com/sharedfiles/filedetails/?id=494094728
This one shows a few more items and corrects a few short comings I found in the original
As well as providing an optional ability to log or record the information collected.


Usage
------
 - CTRL + L to activate the panel in game.  
 - Hit the X button in the top right to close the panel.
 - By default the screen well refresh every 3 seconds, this is configurable, as are some more options.
 - Once a minute it will compare current numbers to the limiter figures and if with in 10% of the max\limit it will turn the text from green to yellow'ish.
 - Mouse over an object name and it will give you a general idea of the item.
 - 'Copy' button will copy stats to the clipboard.
 - 'Manual Refresh' is if you don't have auto-update on and want to pull fresh numbers.
 - 'Log Data' will dump the current data (as well as other helpful information) to either your game's log file, or a custom one of your choice set in the config.
 - Primary options settings - mouse over them, they have tool tips explaining.

Target Audiance
---------------
Anyone who wants to watch if they are approaching a particular object limit or is curious about these stats. If you are trying to troubleshoot the "I've hit the object limit but I don't think I should have yet" issue then this mod should be helpful in finding specifically where leaks are. Additionally the 'Log Data' button will also dump a list of enabled mods, so it can also help with tracking down what was in use with what map while you were testing.

It is not, I repeat NOT going to help you increase any limits or neccessarly solve an issue of any leaks (assuming they even exist), it just helps you spot them - pause game, note numbers, create item, note increases, delete items, note numbers, etc etc. 
If you want to discuss a certain issue relating to this issue do so via the discussions tab, not the general comments. Keep that clean for reports about the mod itself. 



Installation\Configuration Information
--------------
Configuation File Location: %SkylinesInstallFolder%\CSLShowMoreLimites_Config.xml

Where %SkylinesInstallFolder% is the root of your Cities Skylines installation folder, for most that would be something like c:\Games\Steam\steamapps\common\Cities_Skylines .



Configuration File Options
--------------------------

  <DebugLogging>false</DebugLogging>
This enables or disables debug logging. You probably don't need this unless you are having a problem. If you are having a problem you can turn this on via the Options setting in the game, or set it to 'true' here in the config file and reload. 


  <DebugLoggingLevel>0</DebugLoggingLevel>
This controls the level of detail. Debugging set to true and this to '0' is the first level of detail. Setting this past '0' or '1' for most of you will not be needed. Setting it to level '2' will record almost everything it does and start filling up your log fast. Level 3 is developer level only and is not meant to be on for more then a couple minutes. Valid values are integers between 0 and 3 - values are basically ignored if DebugLogging is set to 'false'. 


  <IncludePackageInfoInDump>false</IncludePackageInfoInDump>
This will include a ridiculous level of detail about what packages (maps\assetts\etc) are installed on your system for the game and what's inside each of those packages. Don't enable this it's really just for debugging literal system comparison purposes. Default is false.


  <IncludeModsInAfterMapDump>true</IncludeModsInAfterMapDump>
This will include the list of mods that were enabled in the data dumps (assuming dumps are enabled in the first place) when you exit a map. When using the gui button these always get logged though. Valid values are 'true|false' Default is true.

  <IncludeDisabledModsInDumps>false</IncludeDisabledModsInDumps>
This will include the list of mods that are installed and loaded but not enabled in the data dumps (assuming dumps are enabled in the first place). If this is enabled this data will be included in both in-game 'log data' dumps and dumps upon map unloading if you have enbled such. Valid values are 'true|false' Default is false.


  <EnableGui>true</EnableGui>
This option control if the CTRL + L GUI is available during a map\game. It can be controlled by the options setting panel in the game. You can not change this option while a map is loaded. Valid values are 'true|false' Default is true. It's extremely recommended but technically not required if you just want to log data after each map unload.
 
  <UseAutoRefresh>true</UseAutoRefresh>
Stores your last setting in-game in the gui for if you had Auto Refresh of the statistics enabled. Valid values are 'true|false' default is true.

  <AutoShowOnMapLoad>true</AutoShowOnMapLoad>
Tells the mod to automatically show the gui panel when the map loads instead of the default of hiding itself till the use presses the trigger key-binding. Valid values are 'true|false' default is false.


  <AutoRefreshSeconds>3</AutoRefreshSeconds>
This is how fast you want the statistics counters to refresh their data in the GUI. Valid values are floating point numbers between 1.0 and 120.0, the default is 3 seconds.


  <GuiOpacity>0.9</GuiOpacity>
This controls the opacity\transparency of the GUI panel itself. The default is 1.0 or 100%.  Value values are floating point numbers between 0.10 (10%) and 1.0 (100%). Higher equals less transparent.  


  <DumpStatsOnMapEnd>false</DumpStatsOnMapEnd>
This controls if you want to have the mod log statistic (and other info if enabled) to either your normal game output_log or a custom log at the end of every map. Valid values are 'true|false'. Default is false and can be set in the options GUI.


  <CheckStatsForLimitsEnabled>true</CheckStatsForLimitsEnabled>
This controls if you want the GUI stats window in the game to check current values against the limit and change the color to yellow if within 10% of the max. Valid values are 'true|false'  Default is true.

  <StatsCheckEverySeconds>60</StatsCheckEverySeconds>
This is how often in seconds you want the stat checker to check if you're with in 10% of the max. The default is 60 seconds. Valid values are floating point numbers between 3.0 and 180.0.


  <UseCustomDumpFile>true</UseCustomDumpFile>
This is a 'true|false' setting to enable\disable the use of a custom file to store the end-of-map object data dumps to, that is if you have also enabled that feature. Setting for false mean it will use the default game output_log.txt file if you enable dumps. I recommend using this. Default is false. 


  <DumpStatsFilePath>CSLShowMoreLimits_InfoDump.txt</DumpStatsFilePath>
This is the custom full path to the file you want to use with the afore mentioned UseCustomDumpFile option. You either have to use a full path including file name, or if you just want the file created in your Cities Skylines installation folder root you can just type a file name.  That said, the full path MUST EXIST for this to work, it will not create a folder for you, but it will create the file. So if you set it too 'c:\mydatafolder\mysubfolder\Somefilename.txt then make sure c:\mydatafolder\mysubfolder exists first, though the file itself will be created if need be.  The file is appended to over time, it is never overwritten. Each dump is about 4k of data, larger if you enable mod-list dumping or have huge list of mods running. If you enable the package dumper, watch out, you're looking at ~200k or more then.


  <UseCustomLogFile>false</UseCustomLogFile>
This option allows you to tell the mod instead of printing it's normal log or error data to the standard CSL output_log.txt log file, to dump it's own logging information to a custom file. This really is only useful for debugging purposes or, you don't want logs overwritten on every game start. If debug logging is disabled it's probably pointless to use this. Default is disabled.

  <CustomLogFilePath>CSLShowMoreLimits_Log.txt</CustomLogFilePath>
Works exactly like DumpStatFilePath setting only this one is for the CustomLogFile.  Default when in use is the filename above, created in the root of the CSL install folder.


Q: What happens if you delete your config?
Not to worry, if you lose it the mod will just create you a new one with default settings, so long as it can write to the path.  Though you'll need to check your setting again after that. Even if it can't it will probably still function you'll just be forced to use default settings every time.

Q: Can I move the in-game GUI panel?
Yes you can click-n-drag it around like most game panels.

Q: Can I resize the in-game GUI panel?
Sorry not at this time, I use most of the dialog's real-estate anyway. I know, that probably sucks if your playing on Tablet or something.

Q: What's the in-game GUI panel 'Log Data' button do?
It logs the information you see on your screen to the log file (or the custom dump one if enabled). Unlike the end-of-map dump though pressing this one logs a bunch of other data you might be interested in or if you're having problems might help in the debugging process.
Examples are information about the game itself like version, commandlines uses, paths in use, what mods you have enabled at the molment and a count of how many you have installed. It also dumps debug information about the internal values of parts of the mod, press it and go have a look and find out.


Q: What mod is this mod incompatiable with?
None that I know of, and it shouldn't be, it does not touch or redirect function in your game in anyway.
The only thing that might be an issue is if you have CTRL+L bound to something else.

Q: Does this mod effect my save\saved games.
No. This mod does not interact with your saved games, nor does it touch anything that gets saved. If you turn off the mod nothing will happen other then gui will be unavailable in game.

Q: Will this impact the performance of my game?
With the default settings it should not have any effect what so ever.
Each time the data is refreshed however takes about 1/3rd of a millisecond(3300ticks), which while minor,is not nothing, however running even once a second should be a total non-issue.
Additionally - When you close the window the update timers stop thier work until you open it again.
If you are a crappy system and already getting well less than 20fps and you set <AutoRefreshSeconds> to 1, and <StatsCheckEverySeconds> to 3... maybe, just maybe you might get a little jitter every few seconds. 