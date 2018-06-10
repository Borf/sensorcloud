#include <ESP8266WiFi.h>
#include "Sensor_Switch.h"
#include "Settings.h"
#include <FunctionalInterrupt.h>
#include <functional>
#include <ArduinoJson.h>
#include "Log.h"
#include "Display.h"


bool callApi(const char* api, const char* method, JsonArray& postData, std::function<void(JsonObject& data)> callback);


SensorSwitch::SensorSwitch(JsonObject& config)
{
  pin = config["pin"];
  pinMode(pin, INPUT_PULLUP);
  lastValue = digitalRead(pin);
  sendTrigger = false;
  lastTurnOff = 0;
  lastTurnOn = millis();
  turnedOn = false;

  attachInterrupt(pin, std::bind(&SensorSwitch::change, this), CHANGE);
}

void SensorSwitch::change()
{
	int value = digitalRead(pin);
	long time = millis();
	if (value == LOW && lastValue == HIGH)
	{
		if (time - lastTurnOn > 1000)
		{
			sendTrigger = true;
			turnedOn = true;
		}
		lastTurnOn = time;
	}
	if (value == HIGH && lastValue == LOW && turnedOn) //turn off
	{
		lastTurnOff = time;
	}


	lastValue = value;
}


void SensorSwitch::update()
{
	long time = millis();

	if (sendTrigger)
	{
		logger.println("Send trigger on!");
		sendTrigger = false;
		publish("switch", 1);
	}
	if (lastTurnOff != 0 && time - lastTurnOff > 1000 && turnedOn)
	{
		lastTurnOff = 0;
		turnedOn = false;
		logger.println("Send trigger turn off");
		publish("switch", 0);
	}
}



void SensorSwitch::report()
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


