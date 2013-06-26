@ECHO OFF

ECHO INSTALLING FAKE...
".nuget\nuget.exe" install "FAKE" -Version "1.74.196.0" -ExcludeVersion -OutputDirectory "tools"

ECHO INSTALLING NUGET PACKAGES...
".nuget\nuget.exe" install "packages.config" -ExcludeVersion -OutputDirectory "packages"

ECHO FETCHING ALL FROM REMOTE REPO...
git fetch --all

SET /p SOURCE_BRANCH=[specify a branch to pull]=

ECHO ENSURING MASTER BRANCH...
IF EXIST "_develop" (
	RMDIR /Q /S "_develop"
)
git clone "git@github.com:saxonmatt/DbProject.git" "_develop"
cd "_develop"
git checkout "%SOURCE_BRANCH%"
git pull "%SOURCE_BRANCH%"
cd ..

ECHO ENSURING RELEASE BRANCH...
IF EXIST "_release" (
	RMDIR /Q /S "_release"
)
git clone "git@github.com:saxonmatt/DbProject.git" "_release"
cd "_release"
git checkout release
git pull release
cd ..


ECHO GENERATING DATABASE RELEASE SCRIPTS...
"tools\FAKE\tools\FAKE.exe" "release.fsx"

EXIT /B %errorlevel%