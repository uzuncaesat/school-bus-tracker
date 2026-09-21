# Cloudflared servisini acik komutla duzeltir ve baslatir.
$svc = 'Cloudflared'
$exe = 'C:\Program Files (x86)\cloudflared\cloudflared.exe'
$cfg = 'C:\OkyanusServis\cloudflared\config.yml'

Write-Host "Takilan cloudflared surecleri kapatiliyor..."
Get-Process cloudflared -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep 2
try { Stop-Service $svc -Force -ErrorAction SilentlyContinue } catch {}
$n = 0
while ((Get-Service $svc -ErrorAction SilentlyContinue).Status -ne 'Stopped' -and $n -lt 15) { Start-Sleep 2; $n++ }

Write-Host "Servis komutu (tunnel run) yaziliyor..."
$img = '"{0}" --config "{1}" --no-autoupdate tunnel run' -f $exe, $cfg
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\Cloudflared' -Name ImagePath -Value $img
try { Set-Service $svc -StartupType Automatic } catch {}

# Acilista ag hazir olduktan sonra baslasin (gecikmeli otomatik)
& sc.exe config Cloudflared start= delayed-auto | Out-Null
# Coker/durursa kendini yeniden baslatsin
& sc.exe failure Cloudflared reset= 60 actions= restart/5000/restart/10000/restart/60000 | Out-Null

Write-Host "Servis baslatiliyor..."
Start-Service $svc
Start-Sleep 10
Write-Host ("Cloudflared servis durumu: " + (Get-Service $svc).Status)
Write-Host ""
Write-Host "Tunel baglanti kontrolu:"
& $exe tunnel info okyanus-servis 2>&1 | Select-Object -First 6
