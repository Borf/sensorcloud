#include "Actuator.h"

#include "Actuator_Switch.h"
#include "Actuator_rgb.h"
#include "Log.h"


Actuator* Actuator::build(JsonObject& info)
{
  int type = info["type"];
  Actuator* a = nullptr;
  
  switch(type)
  {
    case 8:
      a = new ActuatorSwitch((JsonObject&)info["config"]);
      break;
    case 9:
      a = new ActuatorRgb((JsonObject&)info["config"]);
      break;
  }

  if(a)
    a->id = info["id"];
  
  return a;
}

