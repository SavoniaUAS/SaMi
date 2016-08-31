This is a sample plugin implementation. You can use this as a model for your plugin.

The sample has three files
	- DemoPlugin.cs, that contains the IMeasurer interface implementation
	- DemoPluginSettings.cs, that has xml serializable settings for the plugin
	- demomeasurersettings.xml that has serialized settings.


The plugin needs two dll references:
- Savonia.Measurements.Providers.Models.dll, this contains the IMeasurer interface and some helper methods.
- Savonia.Measurements.Models.dll, this contains MeasurementPackage and other needed classes for measurements.

Both of these files comes with the MeasurerService.