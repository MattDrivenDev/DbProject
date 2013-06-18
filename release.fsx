#I @"tools/FAKE/tools/"
#r @"FakeLib.dll"

open System
open System.IO
open System.Text
open Fake

let SemVer fileinfo = 
    use reader = (fileinfo:FileInfo).OpenText()
    let semVer = reader.ReadToEnd()
    semVer

let SqlScript fileinfo sql = 
    use writer = (fileinfo:FileInfo).CreateText()
    writer.Write((sql:string))

Target "GenerateReleaseScripts" (fun _ ->    
    let version = FileInfo("_develop/version.txt") |> SemVer    
    "some sql" |> SqlScript (FileInfo((sprintf "_release/deploy.%s.sql" version)))
)

Run "GenerateReleaseScripts"