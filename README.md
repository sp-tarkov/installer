# SPT-AKI Installer made for EFT.

![Finished Installer](https://cdn.discordapp.com/attachments/976519592119762994/976845998930419732/unknown.png)

## New implementation of an Installer for SPT-AKI.

### Spectre Console for a cleaner look:
- Added "SPT-AKI Installer" as Figlet title,

![Figlet Picture](https://cdn.discordapp.com/attachments/976519592119762994/976845245553717248/unknown.png)

- progress bars for extracting and copying of files.

![Progress Bars](https://cdn.discordapp.com/attachments/976519592119762994/976845443831070790/unknown.png)

### pre install checks:
- Checks installer is not in OG game directory,
- Checks install folder does not have game files already in it,
- Checks if gameversion matches aki version, if so skip patcher process,
- Checks both zips are there, other than when the above match, patcher isnt checked for.

### Installer Processes:
- Copies files from registry logged GamePath to new location,
- Extracts, runs and deletes patcher with no user input,
- Extracts Aki,
- Deletes both Patcher and AKI zips at the end.

### plans:
- Maybe download right version for EFT patcher and server
- Locales, Language selection

----

# Setup:

1. Visual Studio > File > Open > Project/Solution `\SPT_AKI Installer.sln`
2. Visual Studio > Build > Publish Selection > Publish
3. Check your folder for the project `\bin\Debug\net6.0\publish\`
4. In here should be an .Exe and a .pdb
-  Only the .exe is needed

# Debug:

- The project has PreProccessor Directives for the location to test installing too
- If you want to change the location for this under L18 make this the path you desire
- You have to be in Debug for this to be used

#### to run Debug:

- Make sure Visual Studio is in Debug mode.

![Debug Mode](https://cdn.discordapp.com/attachments/976519592119762994/976850003521835058/unknown.png)

1. Visual Studio > Debug > Start Debugging or the F5 shortcut Key

# Dependencies:

- .net 6.0
- SharpCompress
- SpectreConsole
