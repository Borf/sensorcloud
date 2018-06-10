#pragma once

#include "Sensor.h"

class SensorSwitch : public Sensor
{
  int lastValue;
  int turnOffDelay;
  long lastTurnOn;
  long lastTurnOff;

  bool sendTrigger;
  bool turnedOn;

public:
  SensorSwitch(JsonObject& config);
  virtual ~SensorSwitch() {};
 
  void change();

  virtual void update();
  virtual void report();
  virtual void settings(JsonObject &o);
  virtual void print(OLEDDisplay* display);
};

