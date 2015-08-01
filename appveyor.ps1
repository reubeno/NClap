# Define build command.
$buildCmd = "C:\Program Files (x86)\MSBuild\12.0\bin\msbuild.exe"
$buildArgs = @(
  "NClap.sln",
  "/m",
  "/l:C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll",
  "/p:Configuration=$env:CONFIGURATION",
  "/p:Platform=$env:PLATFORM")

# If build is not a scheduled one, than simply build project with MSBuild.
if ($env:APPVEYOR_SCHEDULED_BUILD -ne "True") {
  & $buildCmd $buildArgs
  return  # exit script
}

# Else, build project with Coverity Scan.
& "cov-build.exe" --dir cov-int $buildCmd $buildArgs

# Compress and upload scan data.
PublishCoverity.exe compress -o coverity.zip -i cov-int
PublishCoverity.exe publish -z coverity.zip -t $env:COVERITY_SCAN_TOKEN -d "AppVeyor scheduled build ($env:APPVEYOR_BUILD_VERSION)" --codeVersion "$env:APPVEYOR_BUILD_VERSION"
