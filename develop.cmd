@ECHO OFF

IF NOT EXIST "_develop" (

git clone "git@github.com:saxonmatt/DbProject.git" "_develop"

) ELSE (

ECHO Cannot create a new 'develop' branch before releasing the current one.

)

PAUSE