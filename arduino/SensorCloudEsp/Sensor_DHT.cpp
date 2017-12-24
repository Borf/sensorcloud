
#include "Sensor_DHT.h"

#include <ArduinoJson.h>
#include "Log.h"
#include <OLEDDisplay.h>

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
    float newhumidity = dht.readHumidity();
    float newtemperature = dht.readTemperature();
    if (isnan(newhumidity) || isnan(newtemperature)) {
      logger.println("DHT\tFailed to read from DHT sensor!");
	  humidity = 0;
	  temperature = 0;
      return;
    }
	else
	{
		humidity = newhumidity;
		temperature = newtemperature;
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

void DHTSensor::print(OLEDDisplay* display, int x, int y)
{
  sense();
  display->drawString(x, y, "DHT Sensor");
  display->drawString(x, y+10, "Temp: " + String(temperature));
  display->drawString(x, y+20, "Hum: " + String(humidity));

}
