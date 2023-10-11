open Fake.Core
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators


let serverPath = Path.getFullName "./src/Server"

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool

    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ ->
        let errorMsg =
            tool
            + " was not found in path. "
            + "Please install it and make sure it's available from your path. "
            + "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"

        failwith errorMsg

let nodeTool = platformTool "node" "node.exe"
let yarnTool = platformTool "yarn" "yarn.cmd"
let npmTool = platformTool "npm" "npm.cmd"

let runTool procStart cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs

    RawCommand(cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> procStart
    
let runDotNet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""

    if result.ExitCode <> 0 then
        failwithf "'dotnet %s' failed in %s" cmd workingDir

let initTargets () =
    Target.create "Clean" (fun _ -> [  ] |> Shell.cleanDirs)
    "Clean" |> ignore
    Target.create "RunServer" (fun _ ->
    runDotNet "watch run" serverPath 
 )



[<EntryPoint>]
let main argv =
    argv
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    initTargets () |> ignore
    Target.runOrDefaultWithArguments "RunServer"

    0