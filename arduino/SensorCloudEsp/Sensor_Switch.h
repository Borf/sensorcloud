#pragma once

#include "Sensor.h"

class SensorSwitch : public Sensor
{
  int lastValue;
public:
  SensorSwitch(JsonObject& config);
  virtual ~SensorSwitch() {};
 
  virtual void update();
  virtual void getData(JsonObject& o, JsonBuffer& buffer);
  virtual void settings(JsonObject &o);
  virtual void print(MicroOLED &oled);
};

