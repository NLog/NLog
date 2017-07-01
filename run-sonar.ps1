$projectFile = "src\NLog\NLog.netfx45.csproj"
$sonarQubeId = "nlog"
$github = "nlog/nlog"


if($env:APPVEYOR_REPO_NAME -eq $github){

    if(-not $env:sonar_token){
        Write-warning "not running SonarQube, no sonar_token"
        return;
    }

    choco install "msbuild-sonarqube-runner" -y
     
    if ($env:APPVEYOR_PULL_REQUEST_NUMBER) { 
        Write-Output "Running Sonar in preview mode for PR $env:APPVEYOR_PULL_REQUEST_NUMBER"
        MSBuild.SonarQube.Runner.exe begin /k:"$sonarQubeId" /d:"sonar.host.url=https://sonarqube.com" /d:"sonar.login=$env:sonar_token" /d:"sonar.analysis.mode=preview" /d:"sonar.github.pullRequest=$env:APPVEYOR_PULL_REQUEST_NUMBER" /d:"sonar.github.repository=$github" /d:"sonar.github.oauth=$env:github_auth_token" 
    }
    else {
        Write-Output "Running Sonar in non-preview mode"
        MSBuild.SonarQube.Runner.exe begin /k:"$sonarQubeId" /d:"sonar.host.url=https://sonarqube.com" /d:"sonar.login=$env:sonar_token" 
    }
    msbuild $projectFile /verbosity:minimal
    MSBuild.SonarQube.Runner.exe end /d:"sonar.login=$env:sonar_token"
}else {
    Write-Output "not running SonarQube as we're on '$env:APPVEYOR_REPO_NAME'"
}