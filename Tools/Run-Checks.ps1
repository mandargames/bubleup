param(
    [string]$UnityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not (Test-Path -LiteralPath $UnityEditor)) { throw "Unity editor not found: $UnityEditor" }
$logDirectory = Join-Path $projectRoot 'Logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
foreach ($suite in @('EditMode', 'PlayMode')) {
    $resultPath = Join-Path $logDirectory "$suite-results.xml"
    $logPath = Join-Path $logDirectory "$suite.log"
    if (Test-Path -LiteralPath $resultPath) { Remove-Item -LiteralPath $resultPath }
    $arguments = "-batchmode -projectPath `"$projectRoot`" -runTests -testPlatform $suite -testResults `"$resultPath`" -logFile `"$logPath`""
    $process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -PassThru -Wait -WindowStyle Hidden
    if ($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $resultPath)) { throw "$suite failed; see $logPath" }
    [xml]$results = Get-Content -LiteralPath $resultPath -Raw
    if ($results.'test-run'.result -ne 'Passed') { throw "$suite did not pass; see $resultPath" }
    Write-Output "$suite passed: $($results.'test-run'.passed) tests."
}
