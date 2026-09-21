@echo off
chcp 65001 >nul
title Okyanus Servis - Tunel Baslat (takilmayi coz)
cd /d "%~dp0"

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo HATA: Yonetici olarak calistir.
    pause
    exit /b 1
)

echo Takilan cloudflared surecleri kapatiliyor ve servis yeniden baslatiliyor...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "Get-Process cloudflared -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue;" ^
  "Start-Sleep 2;" ^
  "try { Stop-Service Cloudflared -Force -ErrorAction SilentlyContinue } catch {};" ^
  "$n=0; while ((Get-Service Cloudflared).Status -ne 'Stopped' -and $n -lt 10) { Start-Sleep 2; $n++ };" ^
  "Start-Service Cloudflared;" ^
  "Start-Sleep 8;" ^
  "Write-Host ('Cloudflared durumu: ' + (Get-Service Cloudflared).Status)"

echo.
echo Bitti. https://servis.tantanoglu.com adresini dene.
pause
