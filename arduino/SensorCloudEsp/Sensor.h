#pragma once

#include <ArduinoJson.h>

class MicroOLED;

class Sensor
{
protected:
  int pin;
public:
  int id;
  static Sensor* build(JsonObject& o);

  Sensor() {};
  virtual ~Sensor() {};
  virtual void update() = 0;
  virtual void settings(JsonObject &o) = 0;
  virtual void getData(JsonObject& o, JsonBuffer& buffer) = 0;
  virtual void print(MicroOLED &oled) {  };
  
};

