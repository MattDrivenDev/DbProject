@ECHO OFF

IF NOT EXIST "_develop" (

ECHO Creating a new 'develop' branch...
MKDIR "_develop"
	
) ELSE (

ECHO Cannot create a new 'develop' branch before releasing the current one.

)

PAUSE