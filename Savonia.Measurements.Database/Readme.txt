Spatial data types and this app in server environment!

Savonia Measurements V3 app uses spatial data types in the database.

Read http://msdn.microsoft.com/en-us/data/dn194325

The server propably needs Microsoft® System CLR Types for SQL Server® 2008 R2 installed for spatial data types to work.


The Savonia Measurements V3 app uses SavoniaMeasurementsV2 database!
---------------------------------------------------------------------

Database project is used to communicate with defined database.

Repository.cs delivers functionality to communicate with all the tables except META-table

MetaDataRepositry.cs is implemented to communicate with Meta-table