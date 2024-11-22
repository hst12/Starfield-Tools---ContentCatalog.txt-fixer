This tool provides a basic load order editor. and checks ContentCatalog.txt file automatically when launched.

New experimental feature:
Click the Backup button in the catalog checker screen before trying this feature.
Set all Creation mod achievement flags to enabled. Press Catalog, then press Achievements.
Use at your own risk. Only usable for Creations mods.
Won't reset the status of a game save that already has the [C] tag. The game will reset the achievements flags if you load a modded game.
Added a menu option Mods->Enable Achievement Safe only

The load order editor can be used outside of the game to manage Creations mods and manually installed mods that use .esm and .ba2 files.
If you're primarily a user of Creations mods and don't really use other mod managers then you may find this tool useful.

The tool works the same way with mods as the game does. A Plugins.txt file contains a list of your mods. The order of the lines in Plugins.txt determines the load order.
The tool doesn't require the use of any .ini edits, folder junctions or virtual folders.
The tool doesn't need to be running if you use it to launch the game. It will automatically quit a few seconds after launching the game.
Creations mods and mods that are packaged in a format compatible with Creations are automatically recognised and you can adjust load order or enable/disable mods.
The tool can be used to edit a Creations load order while your PC is offline.

Loose file mods are not supported for installation, however you can use Vortex to install such mods.
Altenatively if you have the Creation Kit installed, many mods can be converted into .esm/.ba2 files packages that are compatible with pretty much any mod manager.

What this tool does:
Re-order mods by using hot keys (WASD) or drag and drop.
Enable/Disable mods. Disabling a mod keeps the mod files on your system but the game won't load the mod.
Mod profiles - for example a no-mods profile, an Achievements only profile or a fully modded profile.
Pick or create an empty directory and use File->Save As to start creating profiles.
Install mods (loose files and FOMOD not supported)
Un-install mods (loose files not supported)
View the mod page on the Creations web site
Works with LOOT if you have it installed to do mod sorting. LOOT groups are read and can be displayed. Enable the display of LOOT groups via the View->Columns->Group menu
Acts as a game launcher once you've made your load order edits and you can select between the Steam, MS Store or SFSE flavours of the game.

What it doesn't do for Creations mods:
Like/unlike a mod - you'll have to go to the in-game load order tool for that.
Subscribe/unsubscribe/bookmark a mod - you have to go to the Creations web site or in-game Creations menu for that. Select a mod, right-click and choose View Web Site.
Updating mods - you'll have to do this in the Creations menu in game.
You can un-install a Creations mod from this app but you still need to visit the Creations web site and un-subscribe from the mod to prevent it downloading again.
Some Creations mods apparently can't be un-subscribed from. This is a bug with some mods that Bethesda seems to fix from time to time.

Its a good idea to exit the game when you are done in the Creations menu and run the tool to review and adjust your load order if necessary before loading a saved game.

If you have LOOT installed and configured with groups, press the Autosort button to sort your load order automatically after making changes in the in-game Creations menu.

You can install mods from sites like Nexus using the manual download option if they only include a .esm file and (optionally) one or more .ba2 archives.
Use the Preview file contents option on Nexus to check how the mod has been packaged.
To install a new mod right-click anywhere on the mod list and choose Install or click the Mods menu then Install mod. Most file archive types are recognised.

For keyboard shortcuts see Help->Shortcut keys.

This tool would not have been possible without the invaluable contributions of ZeeOgre who spent many hours testing, troubleshooting and offering advice.

Software used:
7-Zip - https://www.7-zip.org/
SevenZipExtractor - https://github.com/adoconnection/SevenZipExtractor

The rest of the readme pertains mostly to the catalog fixer function which isn't really needed any more unless you have an old save game you want to check.

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
1. The contents of the archive are limited to .esm and .ba2 files only.
2. Loose file are not supported.
3. FOMOD files are not supported.

Profile switching: Allows you to switch mod profiles. Backup your Plugins.txt file before trying this out.
Pick an empty folder or create a folder somewhere in My Documents to save your profiles.
Profile switching is always off by default until the checkbox next to Profiles is checked.

When launching a game from the Load Order Editor a load screen is displayed for a few seconds while the game loads.
A custom load screen picture can be set in the Tools menu.

<div align="left">
    <img src="/Screenshot.png" align="left"</img> 
</div>
<p></p>
<div align="left">
    <img src="/Screenshot2.png" align="left"</img> 
</div>
<div align="left">
    <img src="/Screenshot3.png" align="left"</img> 
</div>
