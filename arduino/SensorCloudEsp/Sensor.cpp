#include "Sensor.h"

#include "Sensor_DHT.h"
#include "Sensor_Switch.h"
#include "Log.h"


Sensor* Sensor::build(JsonObject& info)
{
  int type = info["type"];
  Sensor* s = nullptr;
  
  logger.printTime();
  switch(type)
  {
    case 1:
      logger.println("SENSOR\tUnsupported DHT11, use DHT21");
    case 2:
      logger.println("SENSOR\tMaking DHT");
      s = new DHTSensor(info["config"]);
      break;
    case 4:
      s = new SensorSwitch(info["config"]);
      break;
  }

  if(s)
    s->id = info["id"];
  
  return s;
}

