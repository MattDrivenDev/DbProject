@ECHO OFF

ECHO ENSURING MASTER BRANCH...
IF EXIST "_develop" (
	RMDIR /Q /S "_develop"
)
git clone "git@github.com:saxonmatt/DbProject.git" "_develop"
cd "_develop"
git checkout master
cd ..

ECHO ENSURING RELEASE BRANCH...
IF EXIST "_release" (
	RMDIR /Q /S "_release"
)
git clone "git@github.com:saxonmatt/DbProject.git" "_release"
cd "_release"
git checkout release
cd ..

ECHO GENERATING DATABASE RELEASE SCRIPTS...
"tools\FAKE\tools\FAKE.exe" "release.fsx"