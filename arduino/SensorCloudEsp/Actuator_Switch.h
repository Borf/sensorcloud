#pragma once

#include "Actuator.h"
#include "Log.h"
#include "Arduino.h"

class ActuatorSwitch : public Actuator
{
public:
  ActuatorSwitch(const JsonObject& config)
  {
    pin = config["pin"];
    pinMode(pin, OUTPUT);
  }

  void activate(JsonObject& o)
  {
    if(o["state"] == 1)
      digitalWrite(pin, HIGH);
    else
      digitalWrite(pin, LOW);
    
  }
  
};

