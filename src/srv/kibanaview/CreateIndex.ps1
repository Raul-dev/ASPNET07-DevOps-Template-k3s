# .\src\srv\kibanaview\CreateIndex.ps1 E:\Work\GitLab\gitlab.neva.loc\nevashop\src\srv\kibanaview\export.ndjson
Param
(
	[parameter(Mandatory=$false)][string]$FileNdjson=".\src\srv\kibanaview\export.ndjson",
      [parameter(Mandatory=$false)][string]$Mode="import"
)
# Worked: curl -X POST localhost:5601/api/saved_objects/_import?createNewCopies=true -H "kbn-xsrf: true" --form file=@export.ndjson
$localhost = "localhost:5601"
$Headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
#$Headers.Add("Content-Type", "application/json")
#$Headers.Add("Content-Type", "application/x-ndjson")
#$Headers.Add("content-type", "multipart/form-data; boundary=WebBoundary1234")
#$Headers.Add("kbn-version", "8.5.2")
#?overwrite=true
$Headers.Add("kbn-xsrf", "true")

#$Body=Get-Content "export.ndjson"
#$url="http://"+$localhost+"/api/saved_objects/_import?createNewCopies=true"
#curl -XPOST "http://localhost:5601/api/saved_objects/_import?overwrite=true" -H "kbn-xsrf: true" --form file=@elastiflow.kibana.7.8.x.ndjson
#,      "excludeExportDetails": true
#$Body=Get-Content -Encoding UTF8 "export.ndjson"    

if( $Mode -eq "import" ) {
      $url="http://"+$localhost+"/api/saved_objects/_import?overwrite=true"
      $boundary = [System.Guid]::NewGuid().ToString(); 
      $FilePath = $FileNdjson;

      $fileBytes = [System.IO.File]::ReadAllBytes($FilePath);
      $fileEnc = [System.Text.Encoding]::GetEncoding('UTF-8').GetString($fileBytes);
      $boundary = [System.Guid]::NewGuid().ToString(); 
      $LF = "`r`n";

      $bodyLines = ( 
      "--$boundary",
      "Content-Disposition: form-data; name=`"file`"; filename=`"export.ndjson`"",
      "Content-Type: application/octet-stream$LF",
      $fileEnc,
      "--$boundary--$LF" 
      ) -join $LF

      Write-Host "Body for Invoke-RestMethod"
      Write-Host $bodyLines

      Invoke-RestMethod -Uri $url -Method Post -Headers $Headers -ContentType "multipart/form-data; boundary=`"$boundary`"" -Body $bodyLines
}
if( $Mode -eq "export" ) {
      $url="http://127.0.01:5601/api/saved_objects/_export"
      $Headers.Add("Content-Type", "application/json")
      $Body ='{
            "type": "index-pattern"
      }
      '
      $Body ='{
            "type": ["dashboard", "visualization", "index-pattern"],
            "includeReferencesDeep": "true"
      }
      '
      
      
      Write-Host "Body for Invoke-RestMethod"
      Write-Host $Body

      Invoke-RestMethod $url `
            -Method Post `
            -Headers $Headers `
            -Body $Body 
      #need convert output to utf-8

}
exit
