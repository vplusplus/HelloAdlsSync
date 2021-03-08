
# ---------------------------------------------------------------------------------------
# Sample script to create/register self-signed-certificate.
# Do not use self-signed-certs for real-life-apps.
# ---------------------------------------------------------------------------------------

$dnsName = "adlssync-poc"
$certLocation = "Cert:\CurrentUser\My"
$outputPath = "D:\Junk"

# Output file names.
$pfxFileName = "$outputPath\$dnsName.pfx"
$cerFileName = "$outputPath\$dnsName.cer"
$thumbprintFileName = "$outputPath\$dnsName.thumb.txt"

# Cert validity, from yesterday, for two years...
$dtStart = (Get-Date).ToUniversalTime().AddDays(-1).Date
$dtEnd = $dtStart.AddYears(2)

# Collect password for the PFX file. Username is ignored.
$pfxCredentials = Get-Credential -Message "Enter a password for the PFX" -UserName "$dnsName"

# NOTE: "-KeySpec Signature" is required to use Cert.PrivateKey in .Net Framework 4.7.1
Write-Host "Creating (and registering) $certLocation\$dnsName"
$cert = New-SelfSignedCertificate `
	-DnsName $dnsName `
	-CertStoreLocation $certLocation `
	-KeyLength 2048 `
	-NotBefore $dtStart `
	-NotAfter $dtEnd `
    -KeySpec Signature

# Export to files...
Write-Host "Exporting PFX, CER and thumbprint"
Export-PfxCertificate -Cert $cert -FilePath $pfxFileName -Password $pfxCredentials.Password | Out-Null
Export-Certificate -Cert $cert -FilePath $cerFileName | Out-Null
Set-Content -Path $thumbprintFileName -Value $cert.Thumbprint | Out-Null

# Print the file names and the thumbprint
Write-Host "--------------------------------------------------------------------"
Write-Host "Generated..."
Write-Host "--------------------------------------------------------------------"
$pfxFileName
$cerFileName
$thumbprintFileName
$cert.Thumbprint