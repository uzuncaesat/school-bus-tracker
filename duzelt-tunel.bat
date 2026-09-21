@echo off
chcp 65001 >nul
title Okyanus Servis - Tunel Duzeltme
cd /d "%~dp0"

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo HATA: Yonetici olarak calistir.
    pause
    exit /b 1
)

echo Cloudflared servisi config'i sistem konumuna kopyalaniyor...
set SYSCF=C:\Windows\System32\config\systemprofile\.cloudflared
if not exist "%SYSCF%" mkdir "%SYSCF%"
copy /Y "C:\OkyanusServis\cloudflared\config.yml" "%SYSCF%\config.yml"
copy /Y "C:\OkyanusServis\cloudflared\e0aea844-8fa6-4703-a964-48482cc86bda.json" "%SYSCF%\"

echo Cloudflared servisi yeniden baslatiliyor...
sc stop Cloudflared >nul 2>&1
timeout /t 3 >nul
sc start Cloudflared

echo.
echo Bekleniyor (tunel baglaniyor)...
timeout /t 8 >nul
echo.
echo Durum:
sc query Cloudflared | find "STATE"
echo.
echo Bitti. Simdi https://servis.tantanoglu.com adresini dene.
pause
