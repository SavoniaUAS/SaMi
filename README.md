# SaMi
Savonia measurements system V3 (SAMI)

Solution structure:

Savonia.Measurements.Database
- a class library containing all functionality with database communication

Savonia.Measurements.Manager
- Asp.Net MVC 5 project for measurements management

Savonia.Measurements.Models
- a class library for all model classes used in SAMI

Savonia.Measurements.Services
- MeasurementService.svc WCF service for production

(Savonia.Measurements.Providers.MeasurerService
- Service to import data automatically from measurement providers
	* More information about measurerService can be found from its project root at Readme.txt)

Production install instructions:
Create two (2) applications in IIS 
	- one for manager app (Savonia.Measurements.Manager) 
	- and one for the service (Savonia.Measurements.Services) 
and place these apps in different app pools.

Configure the app pools to use a windows account that has read/write access to measurements database.

It is recommended to use Microsoft SQL server (minimum MSSQL 2008).

Note! The app server needs CLR Types for SQL Server to use SQL spatial types. 
http://blogs.msdn.com/b/adonet/archive/2013/12/09/microsoft-sqlserver-types-nuget-package-spatial-on-azure.aspx
http://www.microsoft.com/en-us/download/confirmation.aspx?id=43339
---------------------------------------------------------
Configure Web.configs/App.configs atleast in Manager, Services, (Providers.MeasurerService projects and Providers.WCFRepository).
Change the values of following keys:
_________________________________________________________
In Manager, Services and Providers.MeasurerService:
------------------------------
KeyDefaultSalt			 = Salt which is used in hashing. (should be same in every project)
KeyEncryptionKey		 = Key that is used in encryption. (should be same in every project)
+
<ConnectionString>
<add name="Yourdatabasename" connectionString="Your-Database-Connection-String">
_________________________________________________________
In Manager:
------------------------------
ServiceV3BaseUrl		 = Url of the Services project (Project Url + MeasurementsService.svc [Example: http://MySami.Sami.fi/Service/MeasurementsService.svc])
CsvFilePath				 = Filepath of temporary csv-files that client generates (app pool user must run on a account that has modify permission to this folder)
JsonFilePath			 = Filepath of temporary JSON-files that client generates (app pool user must run on a account that has modify permission to this folder)
AdminUsers				 = Determines the admin users on Manager that has permission to manage section of the app. Users must be existing windows users on the domain! [Example: value="Domain\adminUser1,Domain\adminUser2"]
ExceptionMailsRecipients = Determines the mails where the exceptions will be send. [Example: value="Email@domain.com,Email2@Domain.com"]
_________________________________________________________
In Providers.MeasurerService
------------------------------
More information about Savonia.Measurements.Providers.MeasurerServices configurations you can find from projects root (Readme.txt)!
_________________________________________________________
In Providers.WCFRepository
------------------------------
Change the client endpoint address to correspond your domain address to MeasurementsService.svc
[Example: http://MySami.Sami.fi/Service/MeasurementsService.svc]
_________________________________________________________

NOTE:
OPCUAPlugin project requires 3th-party dlls to compile! [Check OPCUAPlugin Readme.txt]
PilotMeasurementPlugin requires 3th-party dlls to compile! [Check PilotMeasurementPlugin Readme.txt]

Note 2:
It is recommended to encrypt web.configs on deployment.

Change log:

31.8.2016
- GitHub publish


TODO:
- add more logging options, maybe use Enterprise Library https://msdn.microsoft.com/en-us/library/dn169621.aspx
- better path management for plugins (ie. if plugin tries to read a file with no path it should automatically read from the same folder where the plugin was loaded)
	* the plugin folder should be the default working directory for plugins
- GetMeasurements: some king of paging for large data retrieval???
- json date value parsing must be better! (this is propably done!)
- Solutions should start using Forms authentication (or mixed forms/windows)
	* login helper, (ajaxscript redirection on success) could be erased
	* authentication would be more centered and easier to manage
	* could also be used in WCF??
- (developer should be uptodate when ASP.NET implements good way of cancelling requests)
- add logic to only select sensors(tag)/datum(tag) that are related to measurements(object)

