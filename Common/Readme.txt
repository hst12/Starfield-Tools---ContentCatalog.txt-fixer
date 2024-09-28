It seems like the Starfield vehicle update has fixed the Contentcatalog.txt bug, at least on my side so far.

The mod manager section of the tool will continue to be updated and will move to a stand-alone tool later.

For keyboard shortcuts see Help->Shortcut keys

Checks ContentCatalog.txt file automatically when launched.

This tool would not have been possible without the invaluable contributions of ZeeOgre who spent many hours testing, troubleshooting and offering advice.

Starfield currently has an issue that causes corruption of a file called ContentCatalog.txt when you load a save game.
This tool is intended to repair and then automatically perform a backup/restore of the catalog file.
You could simply delete the catalog file and have the game rebuild it, but that will force a download of all your Creations mods next time you use the Creations menu.

To install, extract the zip file and run the msi installer. Un-install any version before running the new installer.
The tool can be un-installed from the usual Windows settings menu or by re-running the installer.

Enable all the checkboxes under Auto Functions except for Force Clean to have the tool work automatically. This is the default.
Force clean is an experimental option to make the tool run a cleaning process even if it considers the catalog to be ok. Force clean should be off for normal use.

Usage Instructions:
First time usage: Run the tool once to have it repair the catalog file. You should now be able to enter the Creations menu without a lockup.
Important: Quit the game and run the tool again before loading a saved game if you've been in the Creations menu.
In other words, run it before going to the Creations menu and after exiting the Creations menu. Don't load a save or you'll be back to square one.

Main button functions - Skip to the next section if the auto functions are on:
Use the Backup function after you've been in the Creations menu to backup the catalog file - on by default.
Use the Restore function to restore a backup of the catalog if you've made a backup - on by default.
Press the Check button if necessary to check if the catalog is ok.
Press the Clean button if necessary to clean the catalog.

When the auto functions are enabled, the tool will look for a backup of the catalog first and try to restore that.
If the restore fails, it will run a cleaning process.

There is no need to use the tool if you're just playing the game normally and are not using the Creations menu.
Use the catalog backup and restore features instead of the repair features when possible.
These functions are on by default and the tool will automatically decide what action to take.

You can skip reading from here on or keep going for more detail.

What's new: Experimental Support for reading LOOT groups and load order sorting.
In the load order editor enable the Group column, then in the Tools menu set the path to the LOOT executable.

Overview of buttons:
Check button re-checks the file.

Clean button repairs the file. It may take a while with a large mod list.

Backup button copies ContentCatalog.txt to ContentCatalog.txt.bak. A previous backup if it exists will be overwritten without warning. Use the backup function before loading your saved game since this is when the corruption may occur. It's best to exit the game after making modifications to your installed mods, updating mods or changing the load order.

Restore button copies ContentCatalog.txt.bak to ContentCatalog.txt

Remove Unused button checks for mods that you've deleted and removes them from the catalog. This is optional but might be useful for troubleshooting.

Reset All Versions sets all the mod version numbers to a minimum value. This will force all your mods to re-download if you try to update in the Creation menu. Use it only for troubleshooting.

Edit buttons are for opening ContentCatalog.txt or Plugins.txt files for editing with your default text editor.

Explore button opens the folder with your plugin and catalog files. You could manually edit the Plugins.txt file to enable or disable mods if needed. A * character indicates that a mod is enabled.

Load Order button shows a list of mods and allows them to be turned on or off or moved up or down in the load order. Some stats about your mods will be shown if you've used the Set Starfield Path button on the main screen.
There is a somewhat experimental mod profile switching feature in the load order editor. Backup your Plugins.txt file before trying it.
A one-time only automatic backup of your Plugins.txt file is made the first time you open the load editor. Use the File->restore menu to restore this backup if necessary.

The tool can be used to launch Starfield. Click one of the radio buttons to choose either the Steam or MS versions of the game first.
The tool will close a few seconds after launching the game.
You can also launch the game from within the Load Order Editor. Set the game version in the Tools menu first - it defaults to Steam.

Command line options - not case sensitive:
-noauto Clears all the auto check boxes. This will stop any auto repair functions from running automatically when the tool starts.
-auto Sets recommended auto check boxes.
-runSteam or -runMS Starts the tool with whatever auto settings were used last then launches the game.

Example: "Starfield Tools.exe" -auto -runSteam will run auto checks and launch Starfield Steam version.

What files are affected by this tool?
The tool reads and writes to %localappdata%\Starfield
The following files are affected:
ContentCatalog.txt - Read and Write
ContentCatalog.txt.bak - Read and Write
Plugins.txt - Read

Applicable only when the Load Order Editor is used:
Plugins.txt - Read and Write
Plugins.txt.bak -  - Read and Write

Game Data folder Read-only if mod install and un-install features are not used. Used to display some mod stats.
Installing mods from the menu will copy .esm and .ba2 files to the game data folder.
Un-install will permanently DELETE the highlighted mod .esm and .ba2 files.

The Load Order Editor can install or un-install mods from a folder with the following limitations:
1. Zip files only
2. The contents of the Zip archive are limited to .esm and .ba2 files only. Loose file are not supported. FOMOD files are not supported.

Profile switching: An experimental feature allowing you to switch mod profiles. Backup your Plugins.txt file before trying this out.
Pick an empty folder or create a folder somewhere in My Documents to save your profiles.
Profile switching is always off by default until the checkbox next to Profiles is checked.

When launching a game from the Load Order Editor a load screen is displayed for a few seconds while the game loads.
A custom load screen picture can be set in the Tools menu.