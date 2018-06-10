
#include "Sensor_DHT.h"

#include <ArduinoJson.h>
#include "Log.h"
#include <OLEDDisplay.h>

DHTSensor::DHTSensor(JsonObject& config) : dht(config["pin"], DHT22)
{
  pin = config["pin"];
  dht.begin();
  lastSense = 0;
  error = false;
}

void DHTSensor::update()
{
}

void DHTSensor::sense()
{
  if(millis() - lastSense > 30000)
  {
    lastSense = millis();
    float newhumidity = dht.readHumidity();
    float newtemperature = dht.readTemperature();
    if (isnan(newhumidity) || isnan(newtemperature)) {
      logger.print("DHT\tFailed to read from DHT sensor on pin ");
	  logger.println(pin);
	  humidity = 0;
	  temperature = 0;
	  error = true;
      return;
    }
	else
	{
		error = false;
		humidity = newhumidity;
		temperature = newtemperature;
	}
  }
}


void DHTSensor::report()
{
  sense();
  if (!error)
  {
	  publish("temperature", temperature);
	  publish("humidity", humidity);
  }
  else
	  publish("log", "Error reading out sensor");
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
