#include "Sensor.h"

#include "Sensor_DHT.h"
#include "Sensor_Switch.h"
#include "Log.h"
#include <ArduinoJson.h>


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
      s = new DHTSensor((JsonObject&)info["config"]);
      break;
    case 4:
      s = new SensorSwitch((JsonObject&)info["config"]);
      break;
  }

  if(s)
    s->id = info["id"];
  
  return s;
}

