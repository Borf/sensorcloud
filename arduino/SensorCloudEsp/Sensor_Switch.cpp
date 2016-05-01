
#include "Sensor_Switch.h"

#include <functional>
#include <ArduinoJson.h>
#include "Log.h"
#include <SFE_MicroOLED.h>  // Include the SFE_MicroOLED library


bool callApi(String api, String method, String postData, std::function<void(JsonObject& data)> callback);
bool callApi(String api, String method, JsonObject& postData, std::function<void(JsonObject& data)> callback);
bool callApi(String api, String method, JsonArray& postData, std::function<void(JsonObject& data)> callback);
int getId();


SensorSwitch::SensorSwitch(JsonObject& config)
{
  pin = config["pin"];
  pinMode(pin, INPUT);
  lastValue = digitalRead(pin);
}

void SensorSwitch::update()
{
  int value = digitalRead(pin);
  if(value != lastValue)
  {
    logger.printTime();
    logger.println("Switch change!");

    StaticJsonBuffer<200> buffer;
    JsonArray& allData = buffer.createArray();
    JsonObject& o = buffer.createObject();
    o["id"] = id;
    o["switch"] = value;
    allData.add(o);
    callApi(String("report/:") + getId(), "POST", allData, [](JsonObject &ret)   {   });
  }
  lastValue = value;
}

void SensorSwitch::getData(JsonObject& o, JsonBuffer& buffer)
{
//  o["switch"] = digitalRead(pin);
}


void SensorSwitch::settings(JsonObject &o)
{
  o["type"] = "Switch";
  o["pin"] = pin;
}

void SensorSwitch::print(MicroOLED &oled)
{
  oled.print("Switch ");
  oled.print(digitalRead(pin));
}


