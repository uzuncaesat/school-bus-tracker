@echo off
chcp 65001 >nul
title Okyanus Servis - Tunelli Baslatici
cd /d "%~dp0"

echo.
echo ============================================
echo   OKYANUS SERVIS baslatiliyor (tunelli)...
echo   Telefon adresi birazdan ekranda cikacak.
echo ============================================
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0baslat-tunnel.ps1"
