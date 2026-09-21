@echo off
chcp 65001 >nul
title Okyanus Servis - Windows Servisi Kurulumu
cd /d "%~dp0"

REM --- Yonetici kontrolu ---
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo.
    echo HATA: Bu dosyayi YONETICI olarak calistirmalisin.
    echo Sag tikla -> "Yonetici olarak calistir".
    echo.
    pause
    exit /b 1
)

echo ============================================
echo   OKYANUS SERVIS - 7/24 Windows Servisi
echo ============================================
echo.

REM --- 1) SQL Express: SYSTEM hesabina veritabani izni ---
echo [1/3] SQL Express izinleri veriliyor...
sqlcmd -S .\SQLEXPRESS -E -I -b -Q "IF SUSER_ID('NT AUTHORITY\SYSTEM') IS NULL CREATE LOGIN [NT AUTHORITY\SYSTEM] FROM WINDOWS;"
sqlcmd -S .\SQLEXPRESS -E -I -b -d OkyanusServisDb -Q "IF USER_ID('NT AUTHORITY\SYSTEM') IS NULL CREATE USER [NT AUTHORITY\SYSTEM] FOR LOGIN [NT AUTHORITY\SYSTEM]; ALTER ROLE db_owner ADD MEMBER [NT AUTHORITY\SYSTEM];"
echo     SQL izinleri tamam.
echo.

REM --- 2) API + Web servisi ---
echo [2/3] API servisi kuruluyor...
sc stop OkyanusServisApi >nul 2>&1
sc delete OkyanusServisApi >nul 2>&1
sc create OkyanusServisApi binPath= "C:\OkyanusServis\api\OkyanusServis.Api.exe" start= auto DisplayName= "Okyanus Servis (API + Web)"
sc description OkyanusServisApi "Okyanus Servis backend ve arayuz - 7/24 calisir"
sc failure OkyanusServisApi reset= 60 actions= restart/5000/restart/5000/restart/5000
sc start OkyanusServisApi
echo.

REM --- 3) Cloudflare tuneli servisi ---
echo [3/3] Cloudflare tuneli servisi kuruluyor...
REM Servis, config'i SYSTEM'in varsayilan klasorunden okur - oraya kopyala
set SYSCF=C:\Windows\System32\config\systemprofile\.cloudflared
if not exist "%SYSCF%" mkdir "%SYSCF%"
copy /Y "C:\OkyanusServis\cloudflared\config.yml" "%SYSCF%\config.yml"
copy /Y "C:\OkyanusServis\cloudflared\e0aea844-8fa6-4703-a964-48482cc86bda.json" "%SYSCF%\"
"C:\Program Files (x86)\cloudflared\cloudflared.exe" service install >nul 2>&1
sc stop Cloudflared >nul 2>&1
timeout /t 2 >nul
sc start Cloudflared >nul 2>&1
timeout /t 6 >nul
echo.

echo ============================================
echo   KURULUM TAMAM.
echo   Adres: https://servis.tantanoglu.com
echo   Servisler bilgisayar acilinca otomatik baslar.
echo ============================================
echo.
echo Durum kontrolu:
sc query OkyanusServisApi | find "STATE"
sc query Cloudflared | find "STATE"
echo.
pause
