#include "Actuator_rgb.h"



ActuatorRgb::ActuatorRgb(const JsonObject& config)
{
    pins[0] = config["r"];
    pins[1] = config["g"];
    pins[2] = config["b"];

    for(int i = 0; i < 3; i++)
    {
      pinMode(pins[i], OUTPUT);
      analogWrite(pins[i], 0);
      currentValues[i] = 0;
      targetValues[i] = 0;
    }
}

void ActuatorRgb::activate(JsonObject& o)
{
  targetValues[0] = o["r"];
  targetValues[1] = o["g"];
  targetValues[2] = o["b"];
  nextUpdate = millis()+10;
}

void ActuatorRgb::update()
{
  unsigned long t = millis();
  if(nextUpdate < t)
  {
    for(int i = 0; i < 3; i++)
    {
      if(currentValues[i] != targetValues[i])
      {
        currentValues[i] += (targetValues[i] < currentValues[i] ? -1 : 1);
        analogWrite(pins[i], currentValues[i]);
      }
    }
    nextUpdate = t + 10;
  }
  
}

