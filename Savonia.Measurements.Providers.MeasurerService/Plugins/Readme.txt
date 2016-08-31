The plugins folder contains dlls that implemements IMeasurer interface and
other dlls required by those implementations. These are the codes that reads measurements from sensors etc.

If your plugin dll uses external dlls other than 
 - Savonia.Measurements.Models.dll 
 - Savonia.Measurements.Providers.Models.dll 
copy those also in the plugins folder.

plugins.xml contains settings for your plugins. Note that all times are seconds!

Use Plugin element(s) to discribe your plugin(s).

  <Plugin>
    <Name>Your plugin name. The service writes this to your IMeasurer.Name property.</Name>
    <PluginAssembly>You plugin dll. Dll file name is sufficient if the file is in this folder.</PluginAssembly>
    <Enabled>true/false, only enabled plugins are loaded when the service starts.</Enabled>
    <Description>description of plugin, this is informational only.</Description>
    <ConfigFile>Path to a config file or the config string itself. This can be filename only if the file is in this folder or full path.</ConfigFile>
    <ExecuteInterval>20 (in seconds)</ExecuteInterval>
    <ExecuteOnServiceStart>false/true, if the plugin is executed when the service starts.</ExecuteOnServiceStart>
  </Plugin>