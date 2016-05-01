#pragma once

#include <ArduinoJson.h>

class Actuator
{
protected:
  int pin;
public:
  int id;
  static Actuator* build(JsonObject& o);

  virtual void activate(JsonObject& o) = 0;
  
};

