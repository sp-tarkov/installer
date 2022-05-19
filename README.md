# SPT-AKI Installer made for EFT.

![Finished Installer](https://cdn.discordapp.com/attachments/976519592119762994/976845998930419732/unknown.png)

## New implementation of an Installer for SPT-AKI.

### Spectre Console for a cleaner look:
- added "SPT-AKI Installer" as Figlet title,

![Figlet Picture](https://cdn.discordapp.com/attachments/976519592119762994/976845245553717248/unknown.png)

- progress bars for extracting and copying of files.

![Progress Bars](https://cdn.discordapp.com/attachments/976519592119762994/976845443831070790/unknown.png)

### pre install checks:
- checks installer is not in OG game directory,
- checks install folder does not have game files already in it,
- checks if gameversion matches aki version, if so skip patcher process,
- checks both zips are there, other than when the above match, patcher isnt checked for.

### Installer Processes:
- copies files from registry logged GamePath to new location,
- extracts, runs and deletes patcher with no user input,
- extracts Aki,
- deletes both Patcher and AKI zips at the end.

### plans:
- maybe download right version for EFT patcher and server
- locales, language selection

----

# Setup

- Requires: .net 6.0
1. Visual Studio > File > Open > Project/Solution `\SPT_AKI Installer.sln`
2. Visual Studio > Build > Publish Selection > Publish
3. check your folder for the project `\bin\Debug\net6.0\publish\`
4. in here should be an .Exe and a .pdb
- only the .exe is needed

# Debug

- The project has PreProccessor Directives for the location to test installing too
- If you want to change the location for this under L18 make this the path you desire
- you have to be in Debug for this to be used

#### to run Debug:

- make sure Visual Studio is in Debug mode.

![Debug Mode](https://cdn.discordapp.com/attachments/976519592119762994/976850003521835058/unknown.png)

1. Visual Studio > Debug > Start Debugging or the F5 shortcut Key