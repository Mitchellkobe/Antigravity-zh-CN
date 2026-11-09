@echo off
chcp 65001 >nul
title Antigravity - Install Traditional Chinese

echo.
echo ==================================================
echo         Antigravity 繁體中文漢化安裝工具
echo ==================================================
echo.

:: 智能环境检测
set "NODE_CMD=node"
set "EXTRA_OPTS="
where node >nul 2>nul
if %errorlevel% neq 0 (
    set "ELECTRON_RUN_AS_NODE=1"
    if exist "%LOCALAPPDATA%\Programs\antigravity\Antigravity.exe" (
        set "NODE_CMD=%LOCALAPPDATA%\Programs\antigravity\Antigravity.exe"
    ) else if exist "C:\Program Files\Antigravity\Antigravity.exe" (
        set "NODE_CMD=C:\Program Files\Antigravity\Antigravity.exe"
    ) else (
        echo [錯誤] 未檢測到系統 Node.js，亦未找到 Antigravity 客戶端！
        echo 建議您直接雙擊運行本目錄下的圖形界面程序: "Antigravity汉化工具.exe"
        echo.
        pause
        exit /b 1
    )
    echo [提示] 未安裝系統 Node.js，已自動啟用 Antigravity 內置運行引擎。
    taskkill /f /im Antigravity.exe >nul 2>nul
    set "EXTRA_OPTS=--no-kill"
)

echo.
echo 請選擇左上角品牌顯示方式:
echo [1] 保持英文 Antigravity（推薦）
echo [2] 不顯示品牌名稱
echo [3] 顯示繁體中文品牌名
set "CHOICE_VAL=1"
set /p "CHOICE_VAL=請輸入選項 [1/2/3] (直接按 Enter 默認為 1): "
set "BRAND_ARG=--brand-title english"
if "%CHOICE_VAL%"=="2" set "BRAND_ARG=--brand-title hidden"
if "%CHOICE_VAL%"=="3" set "BRAND_ARG=--brand-title translated"

echo.
echo [1/2] 正在注入繁體中文漢化...
cd /d "%~dp0"
"%NODE_CMD%" localization_engine.js --tw %BRAND_ARG% %EXTRA_OPTS% %*

if %errorlevel% neq 0 (
    echo.
    echo 錯誤: 漢化注入失敗，請檢查上方錯誤信息。
    pause
    exit /b 1
)

echo.
echo [2/2] 完成!
echo.
echo 提示: 繁體中文漢化已成功部署。
echo.
echo 視窗將在 5 秒後自動關閉...
timeout /t 5
