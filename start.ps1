#$Mode clrcert - Clear certificates
Param
(
	[parameter(Mandatory=$false)][string]$Mode="up",
	[parameter(Mandatory=$false)][bool]$ELKenable=$false,
	[parameter(Mandatory=$false)][bool]$IsSimpleCertificate=$true
)
$ExitCode = 0
Write-Host "Batch mode="$Mode
Write-Host "ELK enable mode="$ELKenable
Write-Host "Simple Certificate enable mode="$IsSimpleCertificate
$Projectpath = Convert-Path .

try{
	$res = Get-Process 'com.docker.proxy' -ErrorAction SilentlyContinue
	if([string]::IsNullOrEmpty($res)){
		Write-Host "DOCKER is not running. Visit and download https://docs.docker.com/docker-for-windows/install/ " -fore red
		Set-Location -Path $Projectpath
		exit -1
	}
	Set-Location -Path ./src
	$ResultSearch = docker ps -a | Select-String -Pattern "catalogapi"
	if(-Not [string]::IsNullOrEmpty($ResultSearch)){
		Write-Host "Stoped container: docker-compose  down"
		docker-compose down
		docker-compose -f docker-compose.elk.yml down

	}
    if($Mode -eq "down"){
		docker-compose -f docker-compose.misc.yml down
		docker-compose -f docker-compose.elk.yml down
		Set-Location -Path $Projectpath
		exit 0
	}
	


	#Check docker folder sharing
	$PostgresFolder = $Projectpath +"\src"
	$Opts = "-v ${PostgresFolder}:/prj "
	Write-Host "Check shared project folder: "$PostgresFolder
	Write-Host "cmd: docker run --rm ${Opts} alpine ls /prj"

	$IsFolderSharing = $false
	Invoke-Expression -Command "docker run --rm ${Opts} alpine ls /prj" | ForEach-Object {
		Write-Host $_
		IF ($_.Contains("ShopManager.sln")){
			$IsFolderSharing = $true
		}
	}
	Write-Host "IsFolderSharing="$IsFolderSharing
	if(-Not $IsFolderSharing){
		Write-Host "Alpine container haven't access to the solution file Shop Manager.sln. Please check docker folder sharing setup."
		Set-Location -Path $Projectpath
		exit
	}
	

	if($Mode -eq "clrcert"){
		if (Test-Path -Path $Projectpath\src\srv\buildcertificate\rootca\rootCA.crt -PathType Leaf)
		{
			Write-Host "Delete rootCA.crt"
			Remove-Item -Recurse -Path $Projectpath\src\srv\buildcertificate\rootca\rootCA.crt -Force
		}
		if ((Test-Path -Path $Projectpath\src\srv\nginx\certs\host.docker.internal.crt -PathType Leaf))
		{
			Write-Host "Delete host.docker.internal.crt"
			Remove-Item -Recurse -Path $Projectpath\src\srv\nginx\certs\host.docker.internal.crt -Force
			Remove-Item -Recurse -Path $Projectpath\src\srv\buildcertificate\aspcertificat.pfx -Force
		}		
		exit
	}
	#Create self signet certificate
	Set-Location -Path $Projectpath\src\srv\buildcertificate
	./buildcrt.ps1 $IsSimpleCertificate
	if (-Not $LASTEXITCODE -eq 0)
    {
		Write-Host "Cant generate certificate host.docker.internal.crt"
    	exit -1
    }

	#Build main docker compose
	Set-Location -Path $Projectpath
	Set-Location -Path ./src
	if($Mode -eq "build"){
		docker-compose --env-file ./.env.win build  	
	}
	
	#You can check port 80
	#$check=Test-NetConnection $localhost -Port 80 -WarningAction SilentlyContinue
	#If ($check.tcpTestSucceeded -eq $true){
	#	Write-Host "Port 80 in use"
	#	netstat -ano -p tcp | Select-String "0.0.0.0:80"
	#	Set-Location -Path $Projectpath
	#	exit
	#}



	#Clean log folder
	$LogPath=$Projectpath+"/src/logs"
	if (Test-Path -Path $LogPath) {
		Write-Host "Remove old log"
		Remove-Item -Recurse -Path $LogPath -Force
	}

	New-Item -ItemType Directory -Force -Path $LogPath
	$LogPath=$Projectpath+"/src/logs/nginx"
	New-Item -ItemType Directory -Force -Path $LogPath
	Write-Host "docker-compose  start"
	docker-compose -f docker-compose.misc.yml up -d
	
	#Check database starting
	Write-Host "Check database starting"
	$res = 0
	Do {
		Start-Sleep -s 5
		$ResultSearch =""
		$ResultSearch = docker ps -a | Select-String -Pattern "aspnet7-postgres15" | Select-String -Pattern  "(healthy)"
		Write-Host $ResultSearch
		if(-Not [string]::IsNullOrEmpty($ResultSearch)){
			break
		}
		$res = $res + 1
		Write-Host "Wait database startting. ${res}"
	}while (($res -le 5))
	

	# Export kibana index patern
	if($ELKenable -eq $true){
		docker-compose -f docker-compose.elk.yml up -d
		$res = 0
		$StatusCode = 0
		Do {
			#curl -X GET api/task_manager/_health
			$url="http://127.0.01:5601/api/task_manager/_health"
			Write-Host "Ineration="$res
			try{
				$invres = Invoke-RestMethod $url -ErrorAction SilentlyContinue
				
			}catch {
				Write-Host $_ -fore green
				Write-Host "StatusCode:" $_.Exception.Response.StatusCode.value__ 
				Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription
				$StatusCode = $_.Exception.Response.StatusCode.value__ 
			}
			if($StatusCode -eq 500){
				break
			}

			$res = $res + 1
			# Sleep 7 seconds
			Start-Sleep -s 7
		}
		while (($res -le 20) -and ($StatusCode -ne 500))
		.\srv\kibanaview\CreateIndex.ps1 $Projectpath\src\srv\kibanaview\export.ndjson
	}
	
	Write-Host "Check database on Port 54321"
	$check=Test-NetConnection localhost -Port 54321 -WarningAction SilentlyContinue
	Write-Host $check
	If ($check.tcpTestSucceeded -ne $true){
		Write-Host "Database not found on Port 54321"
		netstat -ano -p tcp | Select-String "0.0.0.0:54321"
		Set-Location -Path $Projectpath
		exit
	}
	docker-compose --env-file ./.env.win up 
	$ExitCode = 0
}
catch {
  
  Write-Host "An error occurred:" -fore red
  Write-Host $_ -fore red
  Write-Host "Stack:"
  Write-Host $_.ScriptStackTrace
  $ExitCode = -1
}
finally {
	Set-Location -Path $Projectpath
	exit $ExitCode
}

