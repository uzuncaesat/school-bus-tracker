@echo off
chcp 65001 >nul
title Okyanus Servis - Durdur
echo Okyanus Servis durduruluyor...

REM cloudflared tunelleri kapat
taskkill /IM cloudflared.exe /F >nul 2>&1

REM 5165 ve 5248 portlarindaki sunuculari kapat
powershell -NoProfile -ExecutionPolicy Bypass -Command "foreach($p in 5165,5248){$c=Get-NetTCPConnection -LocalPort $p -State Listen -ErrorAction SilentlyContinue; if($c){$c.OwningProcess | Sort-Object -Unique | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }}}"

echo Durduruldu. (Acik kalan siyah pencereler varsa kapatabilirsiniz.)
timeout /t 3 >nul
