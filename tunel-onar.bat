@echo off
chcp 65001 >nul
title Okyanus Servis - Tunel Onar
cd /d "%~dp0"

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo HATA: Yonetici olarak calistir.
    pause
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tunel-onar.ps1"

echo.
echo Bitti. https://servis.tantanoglu.com adresini dene.
pause
