#pragma once

#include "Sensor.h"
#include <DHT.h>

class DHTSensor : public Sensor
{
  DHT dht;
  long lastSense;
  bool error;

  float humidity;
  float temperature;
  
public:
  DHTSensor(JsonObject& config);
  virtual ~DHTSensor() {};
 
  virtual void update();
  void sense();
  virtual void report();
  virtual void settings(JsonObject &o);
  virtual void print(OLEDDisplay * oled, int x, int y);
};

