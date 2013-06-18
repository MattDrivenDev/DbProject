## SQL Server Continuous Integration & Delivery

The aim is to develop a command-line based set of tools and scripts that will help with developing, maintaining, releasing and deploying SQL Server database changes from scripts alone.

This project is inspired by several tools and products, and will likely be unstable for some time while I learn how it needs to be used and what works etc.

Ideally the default branch of the repository will be `develop` - and developers will `clone` this repository and begin working on that branch. All changes in the repository will be ear-marked against a version, as specified in the `version.txt` file.

The idea is to make it as difficult as possible for developers to do something wrong. Once a development (be that a feature, bug fix or sprint) is complete and testing has proven to be a success, it is intended that the CI build server run a `release` script that turns the contents of the `develop` branch into a `SQL Server` deployment script, with the associated version information and adds to the `release` branch, and creates a `git tag` of the repo, before emptying the `develop` branch once again for the next set of development work.

To avoid race-conditions etc. SQL scripts should be numerically named. E.g. here is the root of the `develop` branch as intended.

```
	./
	  ./0001_create_table_Customer.sql
	  ./0002_create_view_v_Customer.sql
	  ./0003_create_procedure_up_Customer_Insert.sql
	  ./version.txt
```