[CmdletBinding()]
param([string]$ProjectRoot = (Join-Path ([Environment]::GetFolderPath('Desktop')) 'ONE-SHOT'))
$ErrorActionPreference='Stop'
if(Get-Process -Name Unity -ErrorAction SilentlyContinue){throw 'Cierra el editor Unity antes de actualizar. Unity Hub puede permanecer abierto.'}
$client=Join-Path $ProjectRoot 'client'
if(-not(Test-Path (Join-Path $client 'ProjectSettings\ProjectVersion.txt'))){throw "No encuentro ONE SHOT en $ProjectRoot. Comparte este mensaje; no muevas carpetas."}
$headers=@{'User-Agent'='ONE-SHOT-Update'}
$head=Invoke-RestMethod 'https://api.github.com/repos/joseruyca/ONE-SHOT/commits/main' -Headers $headers
$sha=[string]$head.sha
if($sha -notmatch '^[a-f0-9]{40}$'){throw 'Version GitHub invalida.'}
$temp=Join-Path ([IO.Path]::GetTempPath()) ('oneshot-update-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
$zip=Join-Path $temp 'source.zip'
Invoke-WebRequest "https://github.com/joseruyca/ONE-SHOT/archive/$sha.zip" -OutFile $zip -UseBasicParsing
Expand-Archive $zip -DestinationPath $temp
$source=Join-Path $temp "ONE-SHOT-$sha\client"
if(-not(Test-Path (Join-Path $source 'Assets\OneShot\Runtime\OneShotApp.cs'))){throw 'Descarga incompleta.'}
$backup=Join-Path $ProjectRoot ('Backups\'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
foreach($file in Get-ChildItem $source -File -Recurse){
 $relative=$file.FullName.Substring($source.Length).TrimStart([char[]]'\/')
 $destination=Join-Path $client $relative
 if(Test-Path $destination){$saved=Join-Path $backup $relative;New-Item -ItemType Directory -Force (Split-Path $saved) | Out-Null;Copy-Item -LiteralPath $destination -Destination $saved}
 New-Item -ItemType Directory -Force (Split-Path $destination) | Out-Null
 Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
}
Set-Content (Join-Path $ProjectRoot 'SOURCE_COMMIT.txt') $sha
$editor=Join-Path $env:ProgramFiles 'Unity\Hub\Editor\6000.3.26f1\Editor\Unity.exe'
if(-not(Test-Path $editor)){throw 'Codigo actualizado; no encuentro el editor Unity 6000.3.26f1.'}
$qaLog=Join-Path $ProjectRoot 'UNITY_IMPORT_QA.log'
$qaReport=Join-Path $client 'Builds\IMPORT_QA.txt'
if(Test-Path $qaReport){Remove-Item -LiteralPath $qaReport}
Write-Host 'Comprobando compilacion e importacion en Unity. Puede tardar unos minutos.'
$check=Start-Process $editor -ArgumentList ('-batchmode -quit -projectPath "'+$client+'" -executeMethod OneShot.Editor.Validation.Check -logFile "'+$qaLog+'"') -Wait -PassThru
if($check.ExitCode -ne 0 -or -not(Test-Path $qaReport)){
 if(Test-Path $qaLog){Get-Content -LiteralPath $qaLog -Tail 100}
 throw "Unity no ha validado esta revision. Log: $qaLog. Archivos anteriores: $backup"
}
Write-Host "ONE SHOT actualizado e importado: $sha" -ForegroundColor Green
Write-Host "Copia de archivos anteriores: $backup"
Start-Process $editor -ArgumentList ('-projectPath "'+$client+'"')
