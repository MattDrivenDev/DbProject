#r @"tools/FAKE/tools/FakeLib.dll"
#load "helpers.fsx"

open System
open System.IO
open System.Text
open Fake
open Helpers

let mutable version    = ""
let mutable releaseDir = ""

Target "LoadVersion" (fun _ ->
    match (TryReadFile "_develop/version.txt" >>= TrySemanticVersion) with
    | Success (SemanticVersion semver) ->
        version <- semver
        traceImportant (sprintf "Generating release '%s'" version)    
    | Failure error -> failwith error
)

Target "CreateReleaseDir" (fun _ ->       
    let getNextPrefix maybeArray =         
        match (maybeArray:'a array option) with
        | None -> Success 1
        | Some array -> Success (array.Length + 1)

    let getReleasePath prefix = 
        sprintf "_release/%s_%s" prefix version |> Success
    
    let result = 
        TrySubDirectories "_release"
        >>= getNextPrefix
        >>= TryMakeNumberString 4
        >>= getReleasePath
        >>= TryMakeDirectory

    match result with
    | Success path -> 
        releaseDir <- path
        traceImportant (sprintf "Release dir '%s'" releaseDir)

    | Failure error -> traceError error
)

Target "GenerateRelease" (fun _ ->
    traceImportant "Generating database release..."
)

"LoadVersion" 
    ==> "CreateReleaseDir"
    ==> "GenerateRelease"

Run "GenerateRelease"