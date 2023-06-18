# SPT-AKI Installer made for EFT.

![Installer Image](https://media.discordapp.net/attachments/875707258074447904/1107352250705268807/image.png?width=1148&height=671)

### Pre install checks:
- Checks if .net 4.7.2 (or higher) is installed
- Checks if .net 6 desktop runtime is installed
- Checks if EFT is installed,
- Checks if there is enough space before install,
- Checks installer is not in OG game directory,
- Checks install folder does not have game files already in it,
- Checks if gameversion matches aki version, if so skip patcher process,
- Checks both zips are there, other than when the above match, patcher isnt checked for
- downloads both Zips from the Repo's if needed

### Installer Processes:
- Copies files from registry logged GamePath to new location,
- Extracts, runs and deletes patcher with no user input,
- Extracts Aki,
- Deletes both Patcher and AKI zips at the end.