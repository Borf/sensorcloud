
#include "Sensor_DHT.h"

#include <ArduinoJson.h>
#include "Log.h"
#include <SFE_MicroOLED.h>  // Include the SFE_MicroOLED library

DHTSensor::DHTSensor(JsonObject& config) : dht(config["pin"], DHT22)
{
  pin = config["pin"];
  dht.begin();
  lastSense = 0;  
}

void DHTSensor::update()
{
}

void DHTSensor::sense()
{
  if(millis() - lastSense > 4000)
  {
    lastSense = millis();
    humidity = dht.readHumidity();
    temperature = dht.readTemperature();
    if (isnan(humidity) || isnan(temperature)) {
      logger.printTime();
      logger.println("DHT\tFailed to read from DHT sensor!");
      return;
    }
  }
}


void DHTSensor::getData(JsonObject& o, JsonBuffer& buffer)
{
  sense();
  o["temperature"] = temperature;
  o["humidity"] = humidity;
}


void DHTSensor::settings(JsonObject &o)
{
  o["type"] = "DHT";
  o["pin"] = pin;
}

void DHTSensor::print(MicroOLED &oled)
{
  sense();
  oled.print("Temp ");
  oled.print(temperature);
  oled.print("Hum: ");
  oled.print(humidity);
}


