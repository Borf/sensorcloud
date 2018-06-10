#include "Sensor.h"

#include "Sensor_DHT.h"
#include "Sensor_Switch.h"
#include "Sensor_Analog.h"
#include "Log.h"
#include "Settings.h"
#include <ArduinoJson.h>
#include <PubSubClient.h>


extern PubSubClient mqttClient;

Sensor* Sensor::build(JsonObject& info)
{
	int type = info["type"];
	Sensor* s = nullptr;

	switch (type)
	{
	case 1:
		logger.println("SENSOR\tUnsupported DHT11, use DHT21");
	case 2:
		s = new DHTSensor((JsonObject&)info["config"]);
		break;
	case 4:
		s = new SensorSwitch((JsonObject&)info["config"]);
		break;
	case 5:
		s = new AnalogSensor((JsonObject&)info["config"]);
		break;
	}

	if (s)
		s->id = info["id"];

	return s;
}

void Sensor::publish(const String &topic, float value)
{
	char buf[32];
	dtostrf(value, 4, 2, buf);
	mqttClient.publish((::settings.topicid + "/" + topic).c_str(), buf, true);
}

void Sensor::publish(const String &topic, int value)
{
	char buf[32];
	sprintf(buf, "%i", value);
	mqttClient.publish((::settings.topicid + "/" + topic).c_str(), buf, true);
}

void Sensor::publish(const String &topic, const char* value)
{
	mqttClient.publish((::settings.topicid + "/" + topic).c_str(), value);
}