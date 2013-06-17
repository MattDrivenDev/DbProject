@ECHO OFF

ECHO INSTALLING FAKE...
".nuget\nuget.exe" install "FAKE" -Version "1.74.196.0" -ExcludeVersion -OutputDirectory "tools"

ECHO INSTALLING NUGET PACKAGES...
".nuget\nuget.exe" install "packages.config" -ExcludeVersion -OutputDirectory "packages"