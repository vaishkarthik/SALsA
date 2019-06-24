@echo off

REM
REM Setup the developer environment
REM **This script is not run on build machines**
REM
REM Use this script to setup tools which make local development better
REM

REM if not found in PATH, add .config\InPath to PATH.
REM set PATH=%PATH%;%~dp0%.config\InPath
set newpath=%~dp0%.config\InPath
echo %path%| find /i "%newpath%">nul || set path=%path%;%newpath%
set newpath=

REM Setup common doskey shortcuts
doskey root=pushd %~dp0%
doskey src=pushd %~dp0%src
doskey out=pushd %~dp0%out
call restore.cmd