#pragma once

#include "Sensor.h"
#include <DHT.h>

class DHTSensor : public Sensor
{
  DHT dht;
  long lastSense;

  float humidity;
  float temperature;
  
public:
  DHTSensor(JsonObject& config);
  virtual ~DHTSensor() {};
 
  virtual void update();
  void sense();
  virtual void getData(JsonObject& o, JsonBuffer& buffer);
  virtual void settings(JsonObject &o);
  virtual void print(MicroOLED &oled);
};

