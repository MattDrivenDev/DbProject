#r @"tools/FAKE/tools/FakeLib.dll"
#load "helpers.fsx"

open System
open System.IO
open System.Text
open Fake
open Helpers

let dbName             = "DbProject"
let mutable version    = ""
let mutable releaseDir = ""
let mutable sqlFile    = ""

/// <summary>
/// Because ASCII art matters.
/// </summary>
Target "Start" (fun _ ->
    trace          ""
    traceImportant "          _                           _ _     "
    traceImportant "         | |                         | | |    "
    traceImportant " _ __ ___| | ___  __ _ ___  ___    __| | |__  "
    traceImportant "| '__/ _ \ |/ _ \/ _` / __|/ _ \  / _` | '_ \ "
    traceImportant "| | |  __/ |  __/ (_| \__ \  __/ | (_| | |_) |"
    traceImportant "|_|  \___|_|\___|\__,_|___/\___|  \__,_|_.__/ "
    trace          ""
)

/// <summary>
/// Load the version information from the develop branch, and just make
/// sure that it is 'basically' valid semantic version format.
/// </summary>
Target "LoadVersion" (fun _ ->
    
    let LoadVersion = 
        TryReadFile "_develop/version.txt"
        >>= TrySemanticVersion

    match LoadVersion with
    | Success (SemanticVersion semver) ->
        version <- semver
        traceImportant (sprintf "Generating release '%s'" version)  
          
    | Failure error -> failStepWithTrace "LoadVersion" error
)

/// <summary>
/// Works out what the name of the next release directory should be, based
/// on the version and previous releases - and then creates it.
/// </summary>
Target "CreateReleaseDir" (fun _ ->    

    let releaseSubdirectories = TrySubDirectoriesWithFilter (fun (d:DirectoryInfo) -> not (d.Name.StartsWith(".")))
   
    let getNextPrefix maybeArray =         
        match (maybeArray:'a array option) with
        | None -> Success 1
        | Some array -> Success (array.Length + 1)

    let getReleasePath prefix = 
        sprintf "_release/%s_%s" prefix version |> Success

    let CreateReleaseDir = 
        releaseSubdirectories "_release"
        >>= getNextPrefix
        >>= TryMakeNumberString 4
        >>= getReleasePath
        >>= TryMakeDirectory

    match CreateReleaseDir with
    | Success path -> 
        releaseDir <- path
        traceImportant (sprintf "Release dir '%s'" releaseDir)

    | Failure error -> failStepWithTrace "CreateReleaseDir" error
)

/// <summary>
/// Compiles the individual sql scripts in the develop branch into a single
/// transactional sql script in the release.
/// </summary>
Target "CompileSql" (fun _ ->

    let developedScripts = 
        match Files "*.sql" "_develop" with
        | Success maybeArray ->
            match maybeArray with
            | Some scripts ->
                traceImportant (sprintf "%i SQL scripts found to create deployment." (scripts |> Array.length))
                scripts 
                |> Array.map (fun sql ->                                          
                                  match TryReadFile (sprintf "_develop/%s" sql) with
                                  | Success content -> Some ({ ScriptName = sql; ScriptContent = content; })
                                  | Failure e -> None)
                |> Array.fold (fun xs x -> 
                                  match x with
                                  | Some script -> script :: xs
                                  | None -> xs) []
            | None -> failStepWithTrace "CompileSql" "Error, no SQL scripts to compile into a release"
        | Failure error -> failStepWithTrace "CompileSql" error

    let transform = {
        DbName      = dbName
        Version     = SemanticVersion version
        Timestamp   = DateTime.Now
        MachineName = Environment.MachineName
        UserName    = Environment.UserName
        Scripts     = developedScripts
    }

    let CompileSql = 
        TryReadFile "release-template.sql"
        >>= ValidateSqlTemplate
        >>= ParseSqlTemplate
        >>= ApplySqlTransform transform
        >>= TryWriteFile (sprintf "%s/deploy.sql" releaseDir)

    match CompileSql with
    | Success filename ->
        sqlFile <- filename
        traceImportant (sprintf "SQL script compiled '%s'" sqlFile)

    | Failure error -> failStepWithTrace "CompileSql" error
)

/// <summary>
/// Draws up a little report of the database release.
/// </summary>
Target "GenerateRelease" (fun _ ->

    trace          "" 
    traceImportant "---------------------------------------------------------------------"
    traceImportant "DATABASE RELEASE REPORT"
    traceImportant "---------------------------------------------------------------------"
    traceImportant "  ACTIVITY          RESULT"
    traceImportant "  --------          ------"
    traceImportant (sprintf "  Version:          %s" version)
    traceImportant (sprintf "  Release dir:      %s" releaseDir)
    traceImportant (sprintf "  SQL script:       %s" sqlFile)
    traceImportant "---------------------------------------------------------------------"
    trace          "" 
)

Target "CommitReleaseBranch" (fun _ ->

    let pathToRelease = "_release"

    Git.CommandHelper.gitCommand pathToRelease "add ."
    Git.CommandHelper.gitCommand pathToRelease (sprintf "commit -m \"'%s' version '%s' generated.\"" dbName version)
    Git.CommandHelper.gitCommand pathToRelease "push origin release"
)

Target "TagMasterBranch" (fun _ ->

    let pathToMaster = "_develop"

    Git.CommandHelper.gitCommand pathToMaster (sprintf "tag -a %s-%s -m \"Tagging off version '%s'.\"" dbName version version)
    Git.CommandHelper.gitCommand pathToMaster "push --tags"
)

Target "ResetMasterBranchForDevelopment" (fun _ -> 
    
    let pathToMaster = "_develop"
    match DeleteAllFiles pathToMaster with
    | Success msg -> traceImportant msg
    | Failure error -> failStepWithTrace "ResetMasterBranchForDevelopment" error

    let versionFilename = "_develop/version.txt"
    let versionTemplate = "x.x.x.x"
    match TryWriteFile versionFilename versionTemplate with
    | Success filename -> traceImportant (sprintf "New version template file '%s' created" filename)
    | Failure error -> failStepWithTrace "ResetMasterBranchForDevelopment" error

    Git.CommandHelper.gitCommand pathToMaster "add ."
    Git.CommandHelper.gitCommand pathToMaster "rm *.sql"
    Git.CommandHelper.gitCommand pathToMaster (sprintf "commit -m \"Cleaned 'master' branch after releasing version '%s'.\"" version)
    Git.CommandHelper.gitCommand pathToMaster "push origin master"
)

/// <summary>
/// Draws up a little report of the CI-build database release - which does all the operations
/// on the git repo.
/// </summary>
Target "CI-Build" (fun _ ->

    trace          "" 
    traceImportant "---------------------------------------------------------------------"
    traceImportant "DATABASE CI-BUILD RELEASE REPORT"
    traceImportant "---------------------------------------------------------------------"
    traceImportant "  ACTIVITY          RESULT"
    traceImportant "  --------          ------"
    traceImportant "---------------------------------------------------------------------"
    trace          "" 
)



"Start"
==> "LoadVersion" 
==> "CreateReleaseDir"
==> "CompileSql"
==> "GenerateRelease" // developer run ends here, CI (eg. Team City continues)
==> "CommitReleaseBranch"
==> "TagMasterBranch"
==> "ResetMasterBranchForDevelopment"
==> "CI-Build"

RunTargetOrDefault "GenerateRelease"