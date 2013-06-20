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
    match (TrySubDirectories "_relfease") with
    | Success (maybeArray) ->
        match maybeArray with
        | None -> releaseDir <- sprintf "0001_%s" version
        | Some array -> releaseDir <- ""
    | Failure error -> failwith error
)

Target "GenerateRelease" (fun _ ->
    traceImportant "Generating database release..."
)

"LoadVersion" 
    ==> "CreateReleaseDir"
    ==> "GenerateRelease"

Run "GenerateRelease"