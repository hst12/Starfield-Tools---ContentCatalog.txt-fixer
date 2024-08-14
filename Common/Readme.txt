Checks ContentCatalog.txt file automatically when launched.

What's new: Switched to .NET 8.0. Some UI changes mostly in load order editor.

This tool would not have been possible without the invaluable contributions of ZeeOgre who spent many hours testing, troubleshooting and offering advice.

Starfield currently has an issue that causes corruption of a file called ContentCatalog.txt when you load a save game.
This tool is intended to repair the catalog file. You could simply delete the catalog file and have the game rebuild it, but that will force a download of all your Creations mods next time you use the Creations menu.

To install, extract the zip file and run the setup.exe installer. Un-install any version before running the new installer.
The tool can be un-installed from the usual Windows settings menu or by re-running the installer.

Enable all the checkboxes under Auto Functions except for Force Clean to have the tool work automatically. This is the default.
Force clean is an experimental option to make the tool run a cleaning process even if it considers the catalog to be ok.

Usage Instructions:
Important: Quit the game and run the tool before loading a saved game if you've installed new mods or updated mods in the Creations menu.
Run it before going to the Creations menu and after exiting the Creations menu.
Use the Backup function after you've been in the Creations menu to backup the catalog file.
Use the Restore function to restore a backup up catalog if you've made a backup.
Press the Check button if necessary to check if the catalog is ok.
Press the Clean button if necessary to clean the catalog.
When the auto functions are enabled, the tool will look for a backup of the catalog file first and try to restore that. If the restore fails, it will run a cleaning process.

There is no need to use the tool if you're just playing the game normally and are not using the Creations menu.
Use the catalog backup and restore features instead of the repair features when possible.

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

You can alt-tab from the game to check the ContentCatalog file to see if corruption has occurred by pressing the Check button.

Two Starfield launch buttons are provided. One for the Steam version of the game and another for the MS Store version. The tool will close after using either of these launch buttons.

Quit the game if it's running before using the Clean or Edit buttons.

Command line options:
-noauto Clears all the auto check boxes. This will stop any auto repair functions from running automatically when the tool starts.
-auto Sets recommended auto check boxes
-runSteam or -runMS Starts the tool with whatever auto settings were used last then launches the game

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

Read only access is performed on the Data folder of Starfield to provide some stats on installed mods.