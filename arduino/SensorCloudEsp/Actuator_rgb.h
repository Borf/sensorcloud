#pragma once

#include "Actuator.h"
#include "Log.h"
#include "Arduino.h"

class ActuatorRgb : public Actuator
{
  int pins[3];
  int currentValues[3];
  int targetValues[3];
  unsigned long nextUpdate;
public:
  ActuatorRgb(const JsonObject& config);
  void activate(JsonObject& o);
  void update();

};
