Param
(
    [parameter(Mandatory=$false)][bool]$IsSimpleCertificate=$true
)
try{
# This worked only if $IsSimpleCertificate=false
if (-not(Test-Path -Path rootca/rootCA.crt -PathType Leaf) -and $IsSimpleCertificate -eq $false)
{
    Write-Host "Generate root CA"
    New-Item -Path "." -Name "rootca" -ItemType "directory" -Force
    #1)Created key
    openssl genpkey -algorithm RSA -out rootca/rootCA.key -aes-128-cbc      #Input password: blablabla
    #2) Created root 
    openssl req -x509 -new -key rootca/rootCA.key -sha256 -days 3650 -out rootca/rootCA.crt -config rootca.conf
    #3) For windows
    openssl pkcs12 -export -out rootca/rootCA.pfx -inkey rootca/rootCA.key -in rootca/rootCA.crt
    #https://stackoverflow.com/questions/39270992/creating-self-signed-certificates-with-open-ssl-on-windows
    "01" | Out-File -encoding ascii -NoNewline rootca/rootCA.srl
    
    Import-PfxCertificate -FilePath rootca/rootCA.pfx -CertStoreLocation 'Cert:\LocalMachine\Root'
}

if (-not(Test-Path -Path aspcertificat.pfx -PathType Leaf) -Or -not(Test-Path -Path ..\nginx\certs\host.docker.internal.crt -PathType Leaf))
{
    
    Write-Host "Certificate host.docker.internal.crt not exists"
    if($IsSimpleCertificate -eq $true){
        
        openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -nodes -keyout aspcertificat.key -out aspcertificat.crt -subj "/CN=host.docker.internal" -extensions v3_ca -extensions v3_req -config host.docker.internal.conf
        openssl x509 -noout -text -in aspcertificat.crt
        openssl pkcs12 -export -out aspcertificat.pfx -inkey aspcertificat.key -in aspcertificat.crt -passout pass:
        
      
    } else {
        openssl genpkey -algorithm RSA -out aspcertificat.key
        openssl req -new -key aspcertificat.key -config host.docker.internal.conf -reqexts v3_req -out aspcertificat.csr
        openssl x509 -req -days 730 -CA rootca/rootCA.crt -CAkey rootca/rootCA.key -extfile host.docker.internal.conf -extensions v3_req -in aspcertificat.csr -out aspcertificat.crt
        openssl pkcs12 -export -out aspcertificat.pfx -inkey aspcertificat.key -in aspcertificat.crt -passout pass:
    }
    Write-Host "Certificate host.docker.internal.crt generated"

    New-Item -Path ".." -Name "https" -ItemType "directory" -Force
    New-Item -Path "..\nginx\" -Name "certs" -ItemType "directory" -Force

    Copy-Item  -Path ./aspcertificat.pfx -Destination ..\https\host.docker.internal.pfx  -Recurse -force -errorAction stop

    Copy-Item  -Path ./aspcertificat.crt -Destination ..\nginx\certs\host.docker.internal.crt  -Recurse -force -errorAction stop
    Copy-Item  -Path ./aspcertificat.key -Destination ..\nginx\certs\host.docker.internal.key  -Recurse -force -errorAction stop
    if($IsSimpleCertificate -eq $true){
        Import-Certificate -FilePath ..\nginx\certs\host.docker.internal.crt -CertStoreLocation 'Cert:\LocalMachine\Root'
    }
} else {
    Write-Host "Certificate host.docker.internal.pfx exists"
}

$ExitCode = 0
}
catch {
  
  Write-Host "An error occurred:" -fore red
  Write-Host $_ -fore red
  Write-Host "Stack:"
  Write-Host $_.ScriptStackTrace
  $ExitCode = -1
}
finally{
    exit $ExitCode
}
