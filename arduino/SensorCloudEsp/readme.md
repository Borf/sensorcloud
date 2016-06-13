For some reason the standard ESP build does not link against libstdc++.

You'll need to edit platforms.txt in $ARDUINO_IDE/hardware/esp8266com/esp8266, and add -lstdc++ to the following line:

compiler.c.elf.libs=-lm -lgcc -lhal -lphy -lnet80211 -llwi