SAMI Measurer Service


Service folders:
	
	plugins\
	Add plugins to plugins folder and use plugins.xml file in plugins folder to configure your plugins.
	If your plugin dll uses external dlls other than Savonia.Measurements.Models.dll and Savonia.Measurements.Providers.Models.dll copy those also in the plugins folder.

	repository\
	Add repository dll to repository folder. The repository dll is the one that saves measurements to SAMI backend.

	store\
	The default measurement package local store.

	log\
	By default the MeasurerService uses log\measurerservice.log file and application log to log events. Log file logging can be disabled from measurer service config.

Access rights:

	Make sure that the account that runs the service has read access to plugins and repository folders and all those files that you configure as your plugin's settings.
	Then make sure that the account has modify right to the folder that is specified as local store (store folder by default) for measurement package files and log folder.


SAMI Measurer Service Install instructions:

	To install the SAMI Measurer Windows Service use installservice.cmd
	To uninstall the SAMI Measurer Windows Service use uninstallservice.cmd

	After installation:
		- the service is shown in Windows Services list as SAMI Measurer Service. 
		- set the service startup type as desired (this is Manual by default).
		- set log on as to desired value (this is Local System as default). This is the "user" who runs the service.

Both commands might need elevated access (admin rights).

Or check detailed instructions at the end of this file.


SAMI Measurer Service configuration:

	Savonia.Measurements.Providers.MeasurerService.exe.config contains some config for the service.
	If your plugin needs or uses app.config (connectionstring, WCF client config, app settings etc.) those must also be in the service config file.


To install the SAMI Measurer Windows Service use installservice.cmd or follow directions below.

1.	In Windows 7 and Windows Server, open the Developer Command Prompt under Visual Studio Tools in the Start menu. 
	In Windows 8 or Windows 8.1, choose the Visual Studio Tools tile on the Start screen, and then run Developer Command Prompt with administrative credentials. 
	(If you’re using a mouse, right-click on Developer Command Prompt, and then choose Run as Administrator.)

2.	In the Command Prompt window, navigate to the folder that contains your project's output. 
	For example, under your My Documents folder, navigate to Visual Studio 2013\Projects\MyNewService\bin\Debug.

3.	Enter the following command: installutil.exe Savonia.Measurements.Providers.MeasurerService.exe

If the service installs successfully, installutil.exe will report success. 
If the system could not find InstallUtil.exe, make sure that it exists on your computer. 
This tool is installed with the .NET Framework to the folder %WINDIR%\Microsoft.NET\Framework[64]\framework_version. 
For example, the default path for the 32-bit version of the .NET Framework 4, 4.5, 4.5.1, and 4.5.2 is C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe.


To uninstall the SAMI Measurer Windows Service use uninstallservice.cmd or follow directions below.

1.	Open a developer command prompt with administrative credentials.

2.	In the Command Prompt window, navigate to the folder that contains your project's output. 
	For example, under your My Documents folder, navigate to Visual Studio 2013\Projects\MyNewService\bin\Debug.

3.	Enter the following command: installutil.exe /u Savonia.Measurements.Providers.MeasurerService.exe

For details see https://msdn.microsoft.com/en-us/library/zt39148a(v=vs.110).aspx