@echo off
chcp 65001 >nul
title Okyanus Servis - Guncelle (yeni surumu yayinla)
cd /d "%~dp0"

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo HATA: Bu dosyayi YONETICI olarak calistir.
    echo Sag tikla -^> "Yonetici olarak calistir".
    pause
    exit /b 1
)

echo ============================================
echo   OKYANUS SERVIS - Guncelleme
echo ============================================
echo.

echo [1/3] Servis durduruluyor...
sc stop OkyanusServisApi >nul 2>&1
powershell -NoProfile -Command "$n=0; while ((Get-Service OkyanusServisApi).Status -ne 'Stopped' -and $n -lt 20) { Start-Sleep 1; $n++ }"
echo     Durdu.
echo.

echo [2/3] Yeni surum yayinlaniyor (biraz surebilir)...
dotnet publish "%~dp0OkyanusServis.Api\OkyanusServis.Api.csproj" -c Release -o "C:\OkyanusServis\api"
if %errorLevel% neq 0 (
    echo.
    echo HATA: Yayinlama basarisiz. Servis yine de baslatiliyor...
)
echo.

echo [3/3] Servis baslatiliyor (veritabani otomatik guncellenir)...
sc start OkyanusServisApi
timeout /t 6 >nul
echo.

echo Durum:
sc query OkyanusServisApi | find "STATE"
echo.
echo Bitti. https://servis.tantanoglu.com
pause
