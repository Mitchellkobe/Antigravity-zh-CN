@echo off
chcp 65001 >nul
title Antigravity 汉化安装工具

echo.
echo ==================================================
echo         Antigravity 简体中文汉化安装工具
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
        echo [错误] 未检测到系统 Node.js，亦未找到 Antigravity 客户端！
        echo 建议您直接双击运行本目录下的图形界面程序: "Antigravity汉化工具.exe"
        echo.
        pause
        exit /b 1
    )
    echo [提示] 未安装系统 Node.js，已自动启用 Antigravity 内置运行引擎。
    taskkill /f /im Antigravity.exe >nul 2>nul
    set "EXTRA_OPTS=--no-kill"
)

echo.
echo 请选择左上角品牌显示方式：
echo [1] 保持英文 Antigravity（推荐）
echo [2] 隐藏品牌名
echo [3] 汉化为品牌名（反重力）
set "CHOICE_VAL=1"
set /p "CHOICE_VAL=请选择 [1/2/3] (直接按 Enter 默认为 1): "
set "BRAND_ARG=--brand-title english"
if "%CHOICE_VAL%"=="2" set "BRAND_ARG=--brand-title hidden"
if "%CHOICE_VAL%"=="3" set "BRAND_ARG=--brand-title translated"

echo.
echo [1/2] 正在注入汉化...
"%NODE_CMD%" "%~dp0localization_engine.js" %BRAND_ARG% %EXTRA_OPTS% %*

if %errorlevel% neq 0 (
    echo.
    echo [×] 注入失败，请检查上方错误信息。
    pause
    exit /b 1
)

echo.
echo [2/2] 注入完成！
echo.
echo 提示：汉化已成功安装。
echo.
echo 窗口将在 5 秒后自动关闭...
timeout /t 5
