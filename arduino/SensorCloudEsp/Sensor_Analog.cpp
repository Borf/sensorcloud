
#include "Sensor_Analog.h"

#include <ArduinoJson.h>
#include "Log.h"
#include <OLEDDisplay.h>

AnalogSensor::AnalogSensor(JsonObject& config)
{
}

void AnalogSensor::update()
{
}

void AnalogSensor::sense()
{

}


void AnalogSensor::getData(JsonObject& o, JsonBuffer& buffer)
{
  o["value"] = analogRead(A0);
}


void AnalogSensor::settings(JsonObject &o)
{
  o["type"] = "Analog";
  o["pin"] = pin;
}

void AnalogSensor::print(OLEDDisplay* display)
{
  display->print("Value: ");
  display->print(analogRead(A0));
}
