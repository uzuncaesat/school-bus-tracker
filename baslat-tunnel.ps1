# Okyanus Servis - kalici tunelli baslatici (named Cloudflare tunnel)
# Sabit adres: https://servis.tantanoglu.com  (hic degismez)
# API + Web sunucularini ve tek bir cloudflare tunelini baslatir.

$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$api  = Join-Path $root 'OkyanusServis.Api'
$web  = Join-Path $root 'OkyanusServis.Web'
$appUrl = 'https://servis.tantanoglu.com'

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   OKYANUS SERVIS baslatiliyor..." -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# --- cloudflared'i bul ---
$cf = (Get-Command cloudflared -ErrorAction SilentlyContinue).Source
if (-not $cf) {
    foreach ($p in @(
        "C:\Program Files (x86)\cloudflared\cloudflared.exe",
        "C:\Program Files\cloudflared\cloudflared.exe",
        "$env:LOCALAPPDATA\Microsoft\WinGet\Links\cloudflared.exe")) {
        if (Test-Path $p) { $cf = $p; break }
    }
}
if (-not $cf) {
    Write-Host "HATA: cloudflared bulunamadi." -ForegroundColor Red
    Read-Host "Cikmak icin Enter"; exit 1
}

# --- LocalDB (veritabani) calisir durumda mi? Degilse baslat ---
Write-Host "Veritabani (LocalDB) kontrol ediliyor..." -ForegroundColor DarkGray
& sqllocaldb start MSSQLLocalDB 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Get-CimInstance Win32_Process -Filter "Name='sqlservr.exe'" -ErrorAction SilentlyContinue | ForEach-Object {
        try { if ((Get-Process -Id $_.ProcessId -ErrorAction SilentlyContinue).SessionId -ne 0) { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue } } catch {}
    }
    Start-Sleep -Seconds 2
    & sqllocaldb start MSSQLLocalDB 2>&1 | Out-Null
}

# --- onceki calisan surecleri temizle ---
Write-Host "Onceki surecler temizleniyor..." -ForegroundColor DarkGray
Get-Process cloudflared -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
foreach ($port in 5165, 5248) {
    $c = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($c) { $c.OwningProcess | Sort-Object -Unique | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue } }
}
Start-Sleep -Seconds 1

# --- sunucular ---
Write-Host "API sunucu baslatiliyor..." -ForegroundColor DarkGray
Start-Process cmd -ArgumentList '/k', "cd /d `"$api`" && dotnet run --launch-profile http" -WindowStyle Minimized
Write-Host "Web sunucu baslatiliyor..." -ForegroundColor DarkGray
Start-Process cmd -ArgumentList '/k', "cd /d `"$web`" && dotnet run --launch-profile http" -WindowStyle Minimized

# --- named tunnel (tek surec, sabit adresler) ---
Write-Host "Tunel baslatiliyor (servis.tantanoglu.com + api.tantanoglu.com)..." -ForegroundColor DarkGray
Start-Process cmd -ArgumentList '/k', "`"$cf`" tunnel run okyanus-servis" -WindowStyle Minimized

Start-Sleep -Seconds 3
try { Set-Clipboard -Value $appUrl } catch {}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Yellow
Write-Host "  UYGULAMA ADRESI (sabit - hic degismez):" -ForegroundColor Yellow
Write-Host ""
Write-Host "     $appUrl" -ForegroundColor White
Write-Host ""
Write-Host "  * Bu adres panoya kopyalandi." -ForegroundColor Gray
Write-Host "  * Sunucular hazir olmasi ~15-20 sn surebilir (ilk acilis)." -ForegroundColor Gray
Write-Host "  * Durdurmak icin: durdur.bat" -ForegroundColor Gray
Write-Host "============================================================" -ForegroundColor Yellow
Write-Host ""

Read-Host "Bu pencereyi acik birak. Kapatmak icin Enter (uygulama calismaya devam eder)"
