// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open System
open System.IO

// --------------------------------------------------------------------------------------
// Build variables
// --------------------------------------------------------------------------------------

let buildDir  = "./build/"
let outputDir = "./shared/app"
let appReferences = !! "/**/*.fsproj"
let dotnetcliVersion = "2.0.2"
let mutable dotnetExePath = "dotnet"

// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------

let CleanDirExcept pattern dir =
    !! (dir @@ "**/*.*")
        -- (dir @@ pattern)
        |> DeleteFiles
    // deletes all subdirectories
    let rec deleteDirs actDir =
        Directory.GetDirectories(actDir) |> Seq.iter deleteDirs
        Directory.Delete(actDir, true)
    Directory.GetDirectories dir |> Seq.iter deleteDirs

let run' timeout cmd args dir =
    if execProcess (fun info ->
        info.FileName <- cmd
        if not (String.IsNullOrWhiteSpace dir) then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) timeout |> not then
        failwithf "Error while running '%s' with args: %s" cmd args

let run = run' System.TimeSpan.MaxValue

let runDotnet workingDir args =
    let result =
        ExecProcess (fun info ->
            info.FileName <- dotnetExePath
            info.WorkingDirectory <- workingDir
            info.Arguments <- args) TimeSpan.MaxValue
    if result <> 0 then failwithf "dotnet %s failed" args

// --------------------------------------------------------------------------------------
// Targets
// --------------------------------------------------------------------------------------

Target "Clean" (fun _ ->
    [buildDir; outputDir]
    |> Seq.iter (CleanDirExcept "*.gitignore")
)

Target "InstallDotNetCLI" (fun _ ->
    dotnetExePath <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

Target "Restore" (fun _ ->
    appReferences
    |> Seq.iter (fun p ->
        let dir = System.IO.Path.GetDirectoryName p
        runDotnet dir "restore"
    )
)

Target "Build" (fun _ ->
    appReferences
    |> Seq.iter (fun p ->
        let dir = System.IO.Path.GetDirectoryName p
        runDotnet dir "build"
    )
)

Target "Publish" (fun _ -> 
    appReferences
    |> Seq.iter (fun p -> 
        let dir = System.IO.Path.GetDirectoryName p
        sprintf "publish -o %s" ("../.." @@ outputDir)
        |> runDotnet dir
    )
)

// --------------------------------------------------------------------------------------
// Build order
// --------------------------------------------------------------------------------------

"Clean"
  ==> "InstallDotNetCLI"
  ==> "Restore"
  ==> "Build"

"Clean"
  ==> "InstallDotNetCLI"
  ==> "Restore"
  ==> "Publish"

RunTargetOrDefault "Build"
