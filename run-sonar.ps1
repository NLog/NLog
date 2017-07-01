$projectFile = "src\NLog\NLog.netfx45.csproj"
$sonarQubeId = "nlog"
$github = "nlog/nlog"
$baseBranch = "master"


if ($env:APPVEYOR_REPO_NAME -eq $github) {

    if (-not $env:sonar_token) {
        Write-warning "Sonar: not running SonarQube, no sonar_token"
        return;
    }

  
    $preview = $true;
     
    if ($env:APPVEYOR_PULL_REQUEST_NUMBER) { 
        $preview = $true;
        Write-Output "Sonar: on PR $env:APPVEYOR_PULL_REQUEST_NUMBER"
    }
    elseif ($env:APPVEYOR_REPO_BRANCH -eq $baseBranch) {
        Write-Output "Sonar: on branch $env:APPVEYOR_REPO_BRANCH"
    }
    else {
        Write-Output "Sonar: not running SonarQube as this isn't a PR and we running on branch $env:APPVEYOR_REPO_BRANCH"
        return;
    }

    choco install "msbuild-sonarqube-runner" -y

    if ($preview) {
        Write-Output "Sonar: Running Sonar in preview mode for PR $env:APPVEYOR_PULL_REQUEST_NUMBER"
        MSBuild.SonarQube.Runner.exe begin /k:"$sonarQubeId" /d:"sonar.host.url=https://sonarqube.com" /d:"sonar.login=$env:sonar_token" /d:"sonar.analysis.mode=preview" /d:"sonar.github.pullRequest=$env:APPVEYOR_PULL_REQUEST_NUMBER" /d:"sonar.github.repository=$github" /d:"sonar.github.oauth=$env:github_auth_token" 
    }
    else {
        Write-Output "Sonar: Running Sonar in non-preview mode, on branch $env:APPVEYOR_REPO_BRANCH"
        MSBuild.SonarQube.Runner.exe begin /k:"$sonarQubeId" /d:"sonar.host.url=https://sonarqube.com" /d:"sonar.login=$env:sonar_token" 
    }

    msbuild $projectFile /verbosity:minimal
    MSBuild.SonarQube.Runner.exe end /d:"sonar.login=$env:sonar_token"
}
else {
    Write-Output "not running SonarQube as we're on '$env:APPVEYOR_REPO_NAME'"
}