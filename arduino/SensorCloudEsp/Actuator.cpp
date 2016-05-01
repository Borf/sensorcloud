#include "Actuator.h"

#include "Actuator_Switch.h"
#include "Log.h"


Actuator* Actuator::build(JsonObject& info)
{
  int type = info["type"];
  Actuator* a = nullptr;
  
  logger.printTime();
  switch(type)
  {
    case 8:
      a = new ActuatorSwitch(info["config"]);
      break;
  }

  if(a)
    a->id = info["id"];
  
  return a;
}

