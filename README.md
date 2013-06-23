# SQL Server Release Tools

The aim is to develop a command-line based set of tools and scripts that will help with developing, maintaining, releasing and deploying SQL Server database changes from scripts alone. The idea is that the database can be built up over time, and will be treated as a service, with an API - and should be verioned appropriately.

This project is inspired by several tools and products, and will likely be unstable for some time while I learn how it needs to be used and what works etc.

The idea is to make it as difficult as possible for developers to do something wrong. 

## 3 branches

It is my intention that there will be 3 branches running (with little in common):

1. `tools` branch - which contains all the scripts and binaries etc required for doing the SQL CI/D work.
2. `release` branch - which contains a full representation of the database, in script form. Organised into releases.
3. `master` branch - which contains only those scripts being developed for the next release.

There will also be a number of `tags` that will be created from the `master` branch during the process of creating each release.

### 1) master branch

As mentioned this is where the development work goes on. It is intended that developers checkout `master` (or rather, just `clone` the repo`) and begin contributing SQL scripts that will be delivered as part of the next release.

To avoid race-conditions etc. SQL scripts should be numerically named. E.g. here is the root of the `master` branch as intended.

```
	./
	  ./0001_create_table_Customer.sql
	  ./0002_create_view_v_Customer.sql
	  ./0003_create_procedure_up_Customer_Insert.sql
	  ./version.txt
```

For the time being, the naming of scripts is just something of a convention - you could not bother with the numerical prefixes; but the release process does rely on alphabetical ordering of the files. `version.txt` needs to exist, and should contain a valid [Semantic Version](http://semver.org/) number in it (nothing else). The tools rely on this for generating the release.

In time, `version.txt` might contain more information (release notes etc).

### 2) tools branch

This is the main branch of operation for a CI build server (or perhaps just another developer). This branch contains everything that is required, to turn the latest changes in the `master` branch - into a deployable release to be deployed to systems. Everything should be scripted again - there is use of [FAKE](http://fsharp.github.io/FAKE/) to do some of the lifting; and because anyone extending the processes might find the multitude of tools available as part of the `Fake` libraries useful.

The idea is that the `tools` branch will be used for the build/release process for database changes. When used, it will pull in the other branches to subdirectories, and then the main `release.cmd` can be run to turn the contents of the `master` branch into a single transactional SQL script (with TRY/CATCH logic) and tags it off as the version specified in the `version.txt` and then adds it to the `release` branch.


### 3) release branch

This contains all of the SQL scripts as generated, organised in a sensible way that they can be executed from beginning to end to deploy the database from scratch. Alternatively just a few single scripts can be cherry-picked for use.

### 4) release tags

E.g. `2.10.3` - will contain a folder with the source files, the generated (from the tools) single SQL script and perhaps a log file from the generation step. This is essentially a snap-shot of the `master` branch after the tools have done some of their work.