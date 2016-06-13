#pragma once
#include <time.h>
#include <HardwareSerial.h>

class Log : public Print
{
  int currentType;
  
public:
  int index = 0;
  String lines[100];

  
  void begin()
  {
    
  }

  
  size_t write(uint8_t data)
  {
   /* if(data == '\n')
      index = (index + 1)%100;
    else
      lines[index] += (char)data;*/
    Serial.write(data);
  }

  size_t write(const uint8_t *buffer, size_t size)
  {
   /* String data((char*)buffer);
    while(data.indexOf("\n") >= 0)
    {
      lines[index] += data.substring(0, data.indexOf("\n")-1);
      index = (index + 1)%100;
      data = data.substring(data.indexOf("\n")+1);
    }
    lines[index] += data;*/
        
    Serial.write(buffer, size);    
  }

  void printTime()
  {
    time_t now = time(nullptr);
    /*tm* lt = localtime(&now);
    char buf[64];
    
    print(strftime(buf, 64, "%c", lt));*/

    char* timestr = asctime(localtime(&now));
    timestr[strlen(timestr)-1] = 0;
    print(timestr);    
    print("\t");
  }

  void setType(int type)
  {
    currentType = type;  
  }
  
  
};


extern Log logger;
