[CmdletBinding()]
param([switch]$BuildAndroid)
$ErrorActionPreference = 'Stop'
$version = '6000.3.26f1'
$editor = Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Unity.exe"
if (-not (Test-Path -LiteralPath $editor)) {
    throw "Falta Unity $version. Termina su instalacion desde Unity Hub antes de ejecutar este archivo."
}
$repo = 'joseruyca/ONE-SHOT'
$headers = @{ 'User-Agent' = 'ONE-SHOT-Setup'; 'Accept' = 'application/vnd.github+json' }
$head = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/commits/main" -Headers $headers
$sha = [string]$head.sha
if ($sha -notmatch '^[0-9a-f]{40}$') { throw 'GitHub no ha devuelto una version valida.' }
$desktop = [Environment]::GetFolderPath('Desktop')
$destination = Join-Path $desktop 'ONE-SHOT'
if (Test-Path -LiteralPath $destination) {
    $destination = Join-Path $desktop ('ONE-SHOT-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
}
$tempFolder = Join-Path ([IO.Path]::GetTempPath()) ('oneshot-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempFolder | Out-Null
$archive = Join-Path $tempFolder 'project.zip'
Write-Host "Descargando ONE SHOT. Version: $sha"
Invoke-WebRequest -Uri "https://github.com/$repo/archive/$sha.zip" -OutFile $archive -UseBasicParsing
Expand-Archive -LiteralPath $archive -DestinationPath $tempFolder
$source = Join-Path $tempFolder "ONE-SHOT-$sha"
if (-not (Test-Path -LiteralPath (Join-Path $source 'client\ProjectSettings\ProjectVersion.txt'))) {
    throw 'La descarga no contiene el proyecto Unity esperado.'
}
Move-Item -LiteralPath $source -Destination $destination
Set-Content -LiteralPath (Join-Path $destination 'SOURCE_COMMIT.txt') -Value $sha
$project = Join-Path $destination 'client'
Write-Host "Proyecto: $project"
if ($BuildAndroid) {
    $android = Join-Path (Split-Path (Split-Path $editor -Parent) -Parent) 'Editor\Data\PlaybackEngines\AndroidPlayer'
    if (-not (Test-Path -LiteralPath $android)) { throw 'Falta Android Build Support en Unity Hub.' }
    $log = Join-Path $destination 'ANDROID_BUILD.log'
    Write-Host 'Compilando APK. Puede tardar varios minutos.'
    $arguments = '-batchmode -quit -buildTarget Android -projectPath "' + $project + '" -executeMethod OneShot.Editor.Build.Android -logFile "' + $log + '"'
    $process = Start-Process -FilePath $editor -ArgumentList $arguments -Wait -PassThru
    $apk = Join-Path $project 'Builds\ONE_SHOT.apk'
    if ($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $apk)) {
        Write-Host "La compilacion ha fallado. Log: $log" -ForegroundColor Red
        if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log -Tail 80 }
        throw 'No se ha generado un APK valido. Comparte este error con el agente; no edites codigo.'
    }
    Write-Host "APK generado: $apk" -ForegroundColor Green
    Start-Process explorer.exe -ArgumentList ('/select,"' + $apk + '"')
} else {
    Start-Process -FilePath $editor -ArgumentList ('-projectPath "' + $project + '"')
    Write-Host 'Unity se esta abriendo. Espera a que termine la importacion inicial.'
}
