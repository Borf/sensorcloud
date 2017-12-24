#include <ESP8266WiFi.h>
#include "Sensor_Switch.h"
#include "Settings.h"

#include <functional>
#include <ArduinoJson.h>
#include "Log.h"
#include "Display.h"


bool callApi(const char* api, const char* method, JsonArray& postData, std::function<void(JsonObject& data)> callback);


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
    logger.print("Switch change: ");
	logger.println(value);

	DynamicJsonBuffer buffer(2048);
    JsonArray& allData = buffer.createArray();
    JsonObject& o = buffer.createObject();
    o["id"] = id;
    o["switch"] = value;
    allData.add(o);

	char apiCall[100];
	sprintf(apiCall, "report/:%i", ::settings.id);

	callApi(apiCall, "POST", allData, [](JsonObject &ret) {});
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

void SensorSwitch::print(OLEDDisplay* display)
{
}


