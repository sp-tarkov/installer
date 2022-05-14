# SPTinstaller.
## EFT - SPT-AKI. - 2.3.1

### New implementation of an Installer for SPT-AKI.
- uses Spectre Console for a cleaner look
- copies files from registry logged GamePath to new location,
- extracts, runs and deletes patcher with minor user input,
- extracts Aki
- static: FileHelper, ZipHelper, LogHelper 
- nonStatic: ProcessHelper, PreCheckHelper, StringHelper

### plans:
- maybe download right version for EFT patcher and server
- maybe make a cool UI :OWO:
- delete patcher zip and aki zip
- progressBar for CopyDirectory
- add figlet for SPT-AKI INSTALLER
- locales, language selection
- fix PreCheckHelper.AkiCheck currently being hardcoded for 2.3.1
- get waffle to add exit code on patcher to remove the need for user input bar errors