# SPTinstaller.
## EFT - SPT-AKI. - 2.3.1

### New implementation of an Installer for SPT-AKI.
- uses Spectre Console for a cleaner look:
added "SPT-AKI Installer" as Figlet title,
progress bars for extracting and copying of files.

- pre install checks:
checks installer is not in OG game directory, \n
checks install folder does not have game files already in it,
checks if gameversion matches aki version, if so skip patcher process,
checks both zips are there, other than when the above match, patcher isnt checked for.

- copies files from registry logged GamePath to new location,
- extracts, runs and deletes patcher with no user input,
- extracts Aki
- deletes both Patcher and AKI zips at the end

### plans:
- maybe download right version for EFT patcher and server
- locales, language selection