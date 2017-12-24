@echo off
SET ft=
SET ftt=

:repeat

for /f "delims=" %%i in ('"forfiles /m *.bin /c "cmd /c echo @ftime" "') do set ftt=%%i
IF "%ftt%"=="%ft%" goto skip
echo "yay"

:skip
set ft=%ftt%
ping 127.0.0.1 -n 2 >null
goto repeat