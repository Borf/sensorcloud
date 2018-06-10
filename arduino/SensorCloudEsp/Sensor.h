#pragma once

#include <ArduinoJson.h>

class OLEDDisplay;

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
  virtual void report() = 0;
  virtual void print(OLEDDisplay *, int x, int y) {  };
  
  void publish(const String &topic, float value);
  void publish(const String &topic, int value);
  void publish(const String &topic, const char* value);

};

