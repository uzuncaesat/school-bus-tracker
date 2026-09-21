@echo off
chcp 65001 >nul
title Okyanus Servis - Servisleri Kaldir
cd /d "%~dp0"

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo HATA: Yonetici olarak calistir.
    pause
    exit /b 1
)

echo Servisler kaldiriliyor...
sc stop OkyanusServisApi >nul 2>&1
sc delete OkyanusServisApi >nul 2>&1
"C:\Program Files (x86)\cloudflared\cloudflared.exe" service uninstall >nul 2>&1
sc stop Cloudflared >nul 2>&1
sc delete Cloudflared >nul 2>&1
echo Kaldirildi.
pause
