The repository folder contains a dll that implemements IMeasurementRepository interface and
other dlls required by that implementation. This is the code that saves measurements to the SAMI
backend.

If your repository dll uses external dlls other than 
 - Savonia.Measurements.Models.dll 
 - Savonia.Measurements.Providers.Models.dll 
copy those also in the repository folder.