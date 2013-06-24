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
   
    let getNextPrefix maybeArray =         
        match (maybeArray:'a array option) with
        | None -> Success 1
        | Some array -> Success (array.Length + 1)

    let getReleasePath prefix = 
        sprintf "_release/%s_%s" prefix version |> Success
    
    let CreateReleaseDir = 
        TrySubDirectoriesWithFilter (fun (d:DirectoryInfo) -> not (d.Name.StartsWith("."))) "_release"
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


"Start"
==> "LoadVersion" 
==> "CreateReleaseDir"
==> "CompileSql"
==> "GenerateRelease"

Run "GenerateRelease"