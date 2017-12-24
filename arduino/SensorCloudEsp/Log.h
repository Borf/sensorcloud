#pragma once
#include <time.h>
#include <HardwareSerial.h>
#include <WiFiClient.h>

extern WiFiClient serverClient;

class Log : public Print
{
  int currentType;
  bool doPrintTime = true;
  void printTime();
public:
  //int index = 0;
  //String lines[100];


  void begin()
  {
  }


  size_t write(uint8_t data);
  size_t write(const uint8_t *buffer, size_t size);
  void setType(int type);
};


extern Log logger;
