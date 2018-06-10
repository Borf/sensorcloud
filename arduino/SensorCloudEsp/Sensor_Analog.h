#pragma once

#include "Sensor.h"

class AnalogSensor : public Sensor
{
	float value;
public:
  AnalogSensor(JsonObject& config);
  virtual ~AnalogSensor() {};

  virtual void update();
  void sense();
  virtual void report();
  virtual void settings(JsonObject &o);
  virtual void print(OLEDDisplay* display);
};
