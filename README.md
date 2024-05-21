# SPT Installer made for EFT.

<img src="https://i.imgur.com/jtlwLsr.png" alt="spt installer 2.59" width="700"/>

### Pre install checks:
- Checks if .net 4.7.2 (or higher) is installed
- Checks if .net 8 runtime is installed
- Checks if EFT is installed
- Checks if there is enough space before install
- Checks installer is not in a problematic path
- Checks install folder does not have game files already in it
- Checks if gameversion matches SPT version, if so skip patcher process
- Checks both zips are there, other than when the above match, patcher isnt checked for
- downloads both Zips from the Repo's if needed

### Installer Processes:
- Copies files from registry logged GamePath to new location
- Extracts, runs and deletes patcher with no user input
- Extracts SPT
- Deletes both Patcher and SPT zips at the end