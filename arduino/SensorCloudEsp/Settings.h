#pragma once

#include <ESP8266WiFi.h>
#include <Arduino.h>
#include <list>
#include <ArduinoJson.h>
#include "Display.h"


class Sensor;
class Actuator;

class Settings
{
public:
  unsigned char crc;
  int id = -1;
  String name;
  String topicid;

  enum class Mode
  {
    Sensor,
    PowerSaveSensor,
    OnkyoRemote,
	OnkyoDisplay,
  } mode = Mode::Sensor;

  enum class DisplayType
  {
	  None = 0,
	  D1MiniShield = 1,
	  D1Mini = 2,
	  Seperate = 3,
	  WaveShare = 4,
  } type;


  DisplayType display = DisplayType::None;
  
  
  std::list<Sensor*> sensors;
  std::list<Actuator*> actuators;

  bool saveToEeprom();
  bool loadFromEeprom();
  void reset();

  unsigned char calcCrc();
  
};


extern Settings settings;
