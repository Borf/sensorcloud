
# Detect the Raspberry Pi from cpuinfo
#Count the matches for BCM2708 or BCM2709 in cpuinfo
RPI=$(shell cat /proc/cpuinfo | grep Hardware | grep -c BCM2708)
ifneq "${RPI}" "1"
RPI=$(shell cat /proc/cpuinfo | grep Hardware | grep -c BCM2709)
endif

ifeq "$(RPI)" "1"
# The recommended compiler flags for the Raspberry Pi
#CCFLAGS=-march=armv6zk -mtune=arm1176jzf-s -std=c++0x
CCFLAGS=-mfpu=vfp -mfloat-abi=hard -mtune=arm1176jzf-s --std=c++11
endif

LDFLAGS=
CCFLAGS+=-ggdb `mysql_config --cflags`
LDFLAGS+=`mysql_config --cflags --libs`
LDFLAGS+=`curl-config --libs`
CCFLAGS += -MMD

# define all programs
PROGRAMS = sensorcloud
SRC_FILES := ${wildcard *.cpp}
OBJ_FILES := $(SRC_FILES:.cpp=.o)


all: ${PROGRAMS}

${PROGRAMS}: ${OBJ_FILES}
	g++-4.8 ${LDFLAGS} -pthread -lrf24-bcm -lrf24network -lrf24mesh -o $@ $^ --std=c++11

%.o: %.cpp
	g++-4.8 ${CCFLAGS} -Wall -I../ -c -o $@ $<
	

clean:
	rm -rf $(PROGRAMS)
	rm -rf *.o
	rm -rf *.d

-include $(OBJ_FILES:.o=.d)
