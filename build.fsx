// include Fake lib
#r @"tools\FAKE\tools\FakeLib.dll"
open Fake
open Fake.FileUtils
open System

let buildDir = "./build/"
let testDir = buildDir + "tests/"

Target "RestoreBuildPackages" (fun _ ->
  !! "./src/**/packages.config"
    |> Seq.iter (RestorePackage (fun p -> { p with OutputPath = "./src/packages" }))
)

Target "RestoreTestPackages" (fun _ ->
  !! "./tests/**/packages.config"
    |> Seq.iter (RestorePackage (fun p -> { p with OutputPath = "./src/packages" }))
)

Target "Clean" (fun _ ->
  CleanDir buildDir
)

Target "BuildMono2" (fun _ ->
  MSBuildRelease (buildDir + "Mono 2.x") "Build" ["./src/NLog/NLog.mono2.csproj"]
    |> Log "AppBuild-Output: "
)

Target "BuildMono2Tests" (fun _ ->
  MSBuildDebug (testDir + "Mono 2.x") "Build" ["./tests/NLog.UnitTests/NLog.UnitTests.mono2.csproj"]
    |> Log "AppBuild-Output: "
)

Target "RunMono2Tests" (fun _ ->
  !! (testDir + "Mono 2.x/NLog.UnitTests.dll")
    |> xUnit (fun p -> { p with OutputDir = testDir + "Mono 2.x" })
)

Target "BuildNETFX35" (fun _ ->
  MSBuildRelease (buildDir + ".NET Framework 3.5") "Build" ["./src/NLog/NLog.netfx35.csproj"]
    |> Log "AppBuild-Output: "
)

Target "BuildNETFX35Tests" (fun _ ->
  MSBuildDebug (testDir + ".NET Framework 3.5") "Build" ["./tests/NLog.UnitTests/NLog.UnitTests.netfx35.csproj"]
    |> Log "AppBuild-Output: "
)

Target "RunNETFX35Tests" (fun _ ->
  !! (testDir + "/.NET Framework 3.5/NLog.UnitTests.dll")
    |> xUnit (fun p -> { p with OutputDir = testDir + "/.NET Framework 3.5" })
)

Target "BuildNETFX40" (fun _ ->
  MSBuildRelease (buildDir + ".NET Framework 4.0") "Build" ["./src/NLog/NLog.netfx40.csproj"]
    |> Log "AppBuild-Output: "
)

Target "BuildNETFX40Tests" (fun _ ->
  MSBuildDebug (testDir + ".NET Framework 4.0") "Build" ["./tests/NLog.UnitTests/NLog.UnitTests.netfx40.csproj"]
    |> Log "AppBuild-Output: "
)

Target "RunNETFX40Tests" (fun _ ->
  !! (testDir + "/.NET Framework 4.0/NLog.UnitTests.dll")
    |> xUnit (fun p -> { p with OutputDir = testDir + "/.NET Framework 4.0" })
)

Target "BuildNETFX45" (fun _ ->
  MSBuildRelease (buildDir + ".NET Framework 4.5") "Build" ["./src/NLog/NLog.netfx45.csproj"]
    |> Log "AppBuild-Output: "
)

Target "BuildNETFX45Tests" (fun _ ->
  MSBuildDebug (testDir + ".NET Framework 4.5") "Build" ["./tests/NLog.UnitTests/NLog.UnitTests.netfx45.csproj"]
    |> Log "AppBuild-Output: "
)

Target "RunNETFX45Tests" (fun _ ->
  !! (testDir + "/.NET Framework 4.5/NLog.UnitTests.dll")
    |> xUnit (fun p -> { p with OutputDir = testDir + "/.NET Framework 4.5" })
)

Target "BuildSL4" (fun _ ->
  MSBuildRelease (buildDir + "Silverlight 4.0") "Build" ["./src/NLog/NLog.sl4.csproj"]
    |> Log "AppBuild-Output: "
)

Target "BuildSL4Tests" (fun _ ->
  MSBuildDebug (testDir + "Silverlight 4.0") "Build" ["./tests/NLog.UnitTests/NLog.UnitTests.netsl4.csproj"]
    |> Log "AppBuild-Output: "
)

Target "RunSL4Tests" (fun _ ->
  !! (testDir + "/Silverlight 4.0/NLog.UnitTests.dll")
    |> xUnit (fun p -> { p with OutputDir = testDir + "/Silverlight 4.0" })
)

Target "BuildSL5" (fun _ ->
  MSBuildRelease (buildDir + "Silverlight 5.0") "Build" ["./src/NLog/NLog.sl5.csproj"]
    |> Log "AppBuild-Output: "
)

Target "BuildSL5Tests" (fun _ ->
  MSBuildDebug (testDir + "Silverlight 5.0") "Build" ["./tests/NLog.UnitTests/NLog.UnitTests.netsl5.csproj"]
    |> Log "AppBuild-Output: "
)

Target "RunSL5Tests" (fun _ ->
  !! (testDir + "/Silverlight 5.0/NLog.UnitTests.dll")
    |> xUnit (fun p -> { p with OutputDir = testDir + "/Silverlight 5.0" })
)

Target "BuildWP7" (fun _ ->
  MSBuildRelease (buildDir + "Silverlight for Windows Phone 7") "Build" ["./src/NLog/NLog.wp7.csproj"]
    |> Log "AppBuild-Output: "
)

Target "BuildWP7Tests" (fun _ ->
  MSBuildDebug (testDir + "Silverlight for Windows Phone 7") "Build" ["./tests/NLog.UnitTests/NLog.UnitTests.wp7.csproj"]
    |> Log "AppBuild-Output: "
)

Target "RunWP7Tests" (fun _ ->
  !! (testDir + "/Silverlight for Windows Phone 7/NLog.UnitTests.dll")
    |> xUnit (fun p -> { p with OutputDir = testDir + "/Silverlight for Windows Phone 7" })
)

Target "BuildWP71" (fun _ ->
  MSBuildRelease (buildDir + "Silverlight for Windows Phone 7.1") "Build" ["./src/NLog/NLog.wp71.csproj"]
    |> Log "AppBuild-Output: "
)

Target "BuildWP71Tests" (fun _ ->
  MSBuildDebug (testDir + "Silverlight for Windows Phone 7.1") "Build" ["./tests/NLog.UnitTests/NLog.UnitTests.wp71.csproj"]
    |> Log "AppBuild-Output: "
)

Target "RunWP71Tests" (fun _ ->
  !! (testDir + "/Silverlight for Windows Phone 7.1/NLog.UnitTests.dll")
    |> xUnit (fun p -> { p with OutputDir = testDir + "/Silverlight for Windows Phone 7.1" })
)

Target "Default" (fun _ ->
  Run "BuildMono2"
  Run "BuildNETFX35"
  Run "BuildNETFX40"
  Run "BuildNETFX45"
  Run "BuildSL4"
  Run "BuildSL5"
  Run "BuildWP71"
  Run "BuildWP7"
)

Target "RunAllTests" (fun _ ->
  Run "RunNETFX35Tests"
  Run "RunNETFX40Tests"
  Run "RunNETFX45Tests"
  Run "RunSL4Tests"
  Run "RunSL5Tests"
  Run "RunMono2Tests"
  Run "RunWP7Tests"
  Run "RunWP71Tests"
)

Target "BuildAllTests" (fun _ ->
  Run "BuildNETFX35Tests"
  Run "BuildNETFX40Tests"
  Run "BuildNETFX45Tests"
  Run "BuildSL4Tests"
  Run "BuildSL5Tests"
  Run "BuildMono2Tests"
  Run "BuildWP7Tests"
  Run "BuildWP71Tests"
)

"Clean"
  ==> "Default"

"Clean"
  ==> "RestoreBuildPackages"
  ==> "BuildMono2"

"Clean"
  ==> "RestoreBuildPackages"
  ==> "BuildMono2"

"Clean"
  ==> "RestoreBuildPackages"
  ==> "BuildNETFX35"

"Clean"
  ==> "RestoreBuildPackages"
  ==> "BuildNETFX40"

"Clean"
  ==> "RestoreBuildPackages"
  ==> "BuildNETFX45"

"Clean"
  ==> "RestoreBuildPackages"
  ==> "BuildSL4"

"Clean"
  ==> "RestoreBuildPackages"
  ==> "BuildSL5"

"Clean"
  ==> "RestoreTestPackages"
  ==> "BuildNETFX35Tests"
  ==> "RunNETFX35Tests"

"Clean"
  ==> "RestoreTestPackages"
  ==> "BuildNETFX40Tests"
  ==> "RunNETFX40Tests"

"Clean"
  ==> "RestoreTestPackages"
  ==> "BuildNETFX45Tests"
  ==> "RunNETFX45Tests"

"Clean"
  ==> "RestoreTestPackages"
  ==> "BuildSL4Tests"
  ==> "RunSL4Tests"

"Clean"
  ==> "RestoreTestPackages"
  ==> "BuildSL5Tests"
  ==> "RunSL5Tests"

"Clean"
  ==> "RestoreTestPackages"
  ==> "BuildMono2Tests"
  ==> "RunMono2Tests"

// start build
RunTargetOrDefault "Default"