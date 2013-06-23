@ECHO OFF

ECHO ENSURING MASTER BRANCH...
IF NOT EXIST "_develop" (
	git clone "git@github.com:saxonmatt/DbProject.git" "_develop"
	cd "_develop"
	git checkout master
	cd ..
)

ECHO ENSURING RELEASE BRANCH...
IF NOT EXIST "_release" (
	git clone "git@github.com:saxonmatt/DbProject.git" "_release"
	cd "_release"
	git checkout release
	cd ..
)

ECHO GENERATING DATABASE RELEASE SCRIPTS...
"tools\FAKE\tools\FAKE.exe" "release.fsx"
