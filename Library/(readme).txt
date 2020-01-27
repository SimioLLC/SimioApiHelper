This folder contains the files necessary to run a headless version of Simio.

A short description:

1. SimioDLL.dll - this is the bulk of the engine. For example, it contains the Simio Factory class.
2. IconLib.dll - contains the icons that can be referenced by other DLLs.
3. SimioAPI.dll - the Simio extensions and steps (API) interfaces.
4. SimioEnums.dll - internal enumerations used by Simio.
5. SimioAPI.Extensions.dll - 

Licensing Files. Simio uses two different licensing systems (RLM and QLM)
1. rlm944.dll
2. rlm944_x64.dll
3. qlmControls.dll
4. qlmLicenseLib.dll
5. SimioRoam.lic - needed if you have a floating license and you decide to roam one for the headless. Does no harm if you don't need it.

Common Extensions:
1. SimioSelectionRules.dll
2. TextFileReadWrite.dll


