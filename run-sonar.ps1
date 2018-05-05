$projectFile = "src\NLog\NLog.csproj"
$sonarQubeId = "nlog"
$github = "nlog/nlog"
$baseBranch = "master"

if ($env:APPVEYOR_REPO_NAME -eq $github) {

    if (-not $env:sonar_token) {
        Write-warning "Sonar: not running SonarQube, no sonar_token"
        return;
    }
 
    $preview = $false;
    $branchMode = $false;
     
    if ($env:APPVEYOR_PULL_REQUEST_NUMBER) { 
        $preview = $true;
        Write-Output "Sonar: on PR $env:APPVEYOR_PULL_REQUEST_NUMBER"
    }
    elseif ($env:APPVEYOR_REPO_BRANCH -eq $baseBranch) {
        Write-Output "Sonar: on base Branch"
    }
    else {
        $branchMode = $true;
        Write-Output "Sonar: on branch $env:APPVEYOR_REPO_BRANCH"
        
    }

    choco install "msbuild-sonarqube-runner" -y

    if ($preview) {
        Write-Output "Sonar: Running Sonar in preview mode for PR $env:APPVEYOR_PULL_REQUEST_NUMBER"
        SonarScanner.MSBuild.exe begin /k:"$sonarQubeId" /d:"sonar.analysis.mode=preview" /d:"sonar.github.pullRequest=$env:APPVEYOR_PULL_REQUEST_NUMBER" /d:"sonar.github.repository=$github" /d:"sonar.host.url=https://sonarcloud.io" /d:"sonar.login=$env:sonar_token" 
    }
    elseif ($branchMode) {
        $branch = $env:APPVEYOR_REPO_BRANCH;
        Write-Output "Sonar: Running Sonar in branch mode for branch $branch"
        SonarScanner.MSBuild.exe begin /k:"$sonarQubeId" /d:"sonar.branch.name=$branch" /d:"sonar.github.repository=$github" /d:"sonar.host.url=https://sonarcloud.io" /d:"sonar.login=$env:sonar_token" 
    }
    else {
        Write-Output "Sonar: Running Sonar in non-preview mode, on branch $env:APPVEYOR_REPO_BRANCH"
        SonarScanner.MSBuild.exe begin /k:"$sonarQubeId" /d:"sonar.host.url=https://sonarcloud.io" /d:"sonar.login=$env:sonar_token" 
    }

    msbuild /t:Rebuild $projectFile /p:targetFrameworks=net45 /verbosity:minimal

    SonarScanner.MSBuild.exe end /d:"sonar.login=$env:sonar_token"
}
else {
    Write-Output "not running SonarQube as we're on '$env:APPVEYOR_REPO_NAME'"
}
