#include <Arduino.h>
/*
static const uint8_t D0   = 16;
static const uint8_t D1   = 5;
static const uint8_t D2   = 4;
static const uint8_t D3   = 0;
static const uint8_t D4   = 2;
static const uint8_t D5   = 14;
static const uint8_t D6   = 12;
static const uint8_t D7   = 13;
static const uint8_t D8   = 15;
static const uint8_t D9   = 3;
static const uint8_t D10  = 1;*/

//#include <GDBStub.h>

#define _NOMINMAX
#include <ESP8266WiFi.h>
#include <ArduinoOTA.h>
#include <WiFiClient.h>
#include <ESP8266WebServer.h>
#include <ESP8266mDNS.h>
#include <ESP8266HTTPUpdateServer.h>
#include <ESP8266HTTPClient.h>
#include <EEPROM.h>
#include <OLEDDisplayUi.h>
#include <OLEDDisplay.h>


#include "Log.h"
#include "Timer.h"
#include "Onkyo.h"

#include <list>
#include "Sensor.h"
#include "Actuator.h"
#include "Settings.h"
#include "Display.h"
#include <ArduinoJson.h>

extern "C" {
#include "user_interface.h"
	void dns_setserver(char numdns, ip_addr_t *dnsserver);
}

WiFiServer server(23);
WiFiClient serverClient;
const char* ssid = "borf.info";
const char* password = "StroopWafel";
const char* progressor = "|/-\\";


ESP8266WebServer httpServer(80);
ESP8266HTTPUpdateServer httpUpdater;

bool callApi(const char* api, const char* method, const char* postData, std::function<void(JsonObject& data)> callback);
bool callApi(const char* api, const char* method, JsonObject& postData, std::function<void(JsonObject& data)> callback);
bool callApi(const char* api, const char* method, JsonArray& postData, std::function<void(JsonObject& data)> callback);

void checkTelnet()
{
	if (server.hasClient())
	{
		serverClient.stop();
		serverClient = server.available();
		logger.println("Telnet client connected");
	}
	if (serverClient.connected() && serverClient.available() > 0)
		serverClient.read();
}



void setup() {
	EEPROM.begin(512);
	Serial.begin(115200);

	char host[48];
	if (settings.loadFromEeprom())
	{
		logger.println("ESP\tSetting WiFi settings");
		sprintf(host, "borf.sensor.%i.%s", settings.id, settings.name.c_str());
		String wifiApName = String("borf.sensor.") + settings.id;
		IPAddress Ip(10, 0, settings.id, 1);
		IPAddress NMask(255, 255, 255, 0);
		WiFi.softAPConfig(Ip, Ip, NMask);
		WiFi.softAP(wifiApName.c_str(), password);
		wifi_station_set_hostname(host);
		display.begin(settings.display);
	}

	server.begin();
	server.setNoDelay(true);
	checkTelnet();

	display.bootStart();
	logger.println("ESP\tBooting ESP...");
	logger.print("ESP\tMy ID is ");
	logger.println(ESP.getChipId(), HEX);
	logger.print("WIFI\tConnecting to WiFi...");
	WiFi.mode(WIFI_AP_STA);
	WiFi.begin(ssid, password);
	display.bootConnecting();
	while (WiFi.waitForConnectResult() != WL_CONNECTED)
	{
		logger.print(".");
		WiFi.begin(ssid, password);
		delay(1000);
	}
	display.bootConnected();
	checkTelnet();
	logger.println();
	logger.print("WIFI\tConnected to ");
	logger.println(ssid);
	logger.print("WIFI\tIP address: ");
	logger.println(WiFi.localIP());
	logger.print("WIFI\tDNS address: ");
	logger.println(WiFi.dnsIP(0));

	IPAddress dns(192, 168, 2, 202);
	ip_addr_t d;
	d.addr = static_cast<uint32_t>(dns);
	dns_setserver(0, &d);
	logger.print("WIFI\tDNS address now: ");
	logger.println(WiFi.dnsIP(0));


	logger.println("API\tConnecting to API for ID");
	char apiCall[100];
	sprintf(apiCall, "nodes/hwid:%x", ESP.getChipId());

	if (!callApi(apiCall, "GET", "", [&host](JsonObject& ret)
	{
		JsonObject &el = ret["data"][0];
		settings.name = ret["data"][0]["name"].as<const char*>();
		settings.id = ret["data"][0]["id"];
		if (el.containsKey("config"))
		{
			JsonObject &config = el["config"];
			if (config.containsKey("display"))
				settings.display = (Display::Type)config["display"].as<int>();
			if (config.containsKey("mode"))
				settings.mode = (Settings::Mode)config["mode"].as<int>();

		}
		sprintf(host, "borf.sensor.%i.%s", settings.id, settings.name.c_str());
		if (settings.saveToEeprom())
			ESP.reset();

		logger.print("API\tMy ID is ");
		logger.println(settings.id);
		logger.print("API\tMy name is ");
		logger.println(settings.name);
		if (settings.id == 0)
		{
			logger.println("Oops, cannot have ID 0");
			delay(1000);
			ESP.restart();
		}
	})) ESP.restart();

	display.bootBooted();
	checkTelnet();
	//ok, all wifi initialized

	MDNS.begin(host);
	timer.begin();

	sprintf(apiCall, "sensors/nodeid:%i", settings.id);
	callApi(apiCall, "GET", "", [](JsonObject& ret)
	{
		logger.print("API\tGot sensor info. Sensorcount: ");
		logger.println(ret["data"].size());
		for (size_t i = 0; i < ret["data"].size(); i++)
		{
			Sensor* sensor = Sensor::build(ret["data"][i]);
			if (sensor)
				settings.sensors.push_back(sensor);
			else
			{
				Actuator* actuator = Actuator::build(ret["data"][i]);
				if (actuator)
					settings.actuators.push_back(actuator);

				else
				{
					logger.print("Unknown sensor / actuator type: ");
					logger.print((int)ret["data"]["type"]);
				}
			}
		}
	});


	if (settings.mode == Settings::Mode::Sensor)
	{
		FrameCallback* frames = new FrameCallback[settings.sensors.size()];
		int i = 0;
		for (auto s : settings.sensors)
			frames[i++] = [](OLEDDisplay* d, OLEDDisplayUiState* state, int16_t x, int16_t y)
		{
			auto it = settings.sensors.begin();
			std::advance(it, state->currentFrame);
			(*it)->print(d, x + display.offX, y + display.offY);
		};
		// for(auto a : settings.actuators)
		//   frames[i++] = nullptr;
		if (display.ui)
			display.ui->setFrames(frames, i);

	}

	if (settings.mode == Settings::Mode::Sensor)
		timer.addCallback(60 * 5, []()
	{
		display.resetTimeout();
		logger.println("Reading out sensors...");
		DynamicJsonBuffer buffer(512);
		JsonArray& allData = buffer.createArray();
		for (auto s : settings.sensors)
		{
			JsonObject& o = buffer.createObject();
			o["id"] = s->id;
			s->getData(o, buffer);
			allData.add(o);
		}
		char apiCall[100];
		sprintf(apiCall, "report/:%i", settings.id);
		callApi(apiCall, "POST", allData, [](JsonObject &) {});

	}, true);

	if (settings.mode == Settings::Mode::OnkyoDisplay)
	{
		onkyo.begin(IPAddress(192, 168, 2, 200), 60128);
	}

	timer.addCallback(1, []()
	{
		DynamicJsonBuffer buffer;
		JsonObject& o = buffer.createObject();
		o["heapspace"] = ESP.getFreeHeap();
		o["ip"] = (String)(String(WiFi.localIP()[0]) + "." + String(WiFi.localIP()[1]) + "." + String(WiFi.localIP()[2]) + "." + String(WiFi.localIP()[3]));
		o["rssi"] = WiFi.RSSI();
		char apiCall[100];
		sprintf(apiCall, "ping/:%i", settings.id);
		callApi(apiCall, "POST", o, [](JsonObject &) {});
	});

	httpServer.begin();
	MDNS.addService("http", "tcp", 80);
	logger.printf("HTTP\tHTTPUpdateServer ready! Open http://");
	logger.print(host);
	logger.println(".local/update in your browser");
	initApi();
	httpUpdater.setup(&httpServer);
	ArduinoOTA.setPort(8266);
	ArduinoOTA.onStart([]() {
		Serial.println("Start");
	});
	ArduinoOTA.onEnd([]() {
		Serial.println("\nEnd");
	});
	ArduinoOTA.onProgress([](unsigned int progress, unsigned int total) {
		Serial.printf("Progress: %u%%\r", (progress / (total / 100)));
	});
	ArduinoOTA.onError([](ota_error_t error) {
		Serial.printf("Error[%u]: ", error);
		if (error == OTA_AUTH_ERROR) Serial.println("Auth Failed");
		else if (error == OTA_BEGIN_ERROR) Serial.println("Begin Failed");
		else if (error == OTA_RECEIVE_ERROR) Serial.println("Receive Failed");
		else if (error == OTA_END_ERROR) Serial.println("End Failed");
	});
	ArduinoOTA.begin();
	logger.println("Done setup...going to main loop");
}


void loop() {
	checkTelnet();
	ArduinoOTA.handle();
	httpServer.handleClient();
	timer.update();
	for (auto s : settings.sensors)
		s->update();
	for (auto a : settings.actuators)
		a->update();

	display.update();

	if (settings.mode == Settings::Mode::OnkyoDisplay)
	{
		onkyo.update();

		if (!onkyo.connected)
		{
			display.clear();
			display.setTextAlignment(TEXT_ALIGN_LEFT);
			display.drawString(0, 0, "Connecting to receiver:");
			display.drawString(0, 30, onkyo.getIp().toString());
			display.setTextAlignment(TEXT_ALIGN_RIGHT);
			display.drawString(64, 50, String(progressor[(millis() / 250) % 4]));
			display.swap();
		}
		else if (!onkyo.isPoweredOn())
		{
			display.clear();
			display.setTextAlignment(TEXT_ALIGN_LEFT);
			display.drawString(0, 0, "Receiver powered down");
			display.swap();
		}
		else
		{
			display.clear();
			display.setTextAlignment(TEXT_ALIGN_LEFT);
			display.drawStringMaxWidth(0, 0, 64, onkyo.artist);
			display.drawStringMaxWidth(0, 32, 64, onkyo.title);
			display.swap();

		}


		display.resetTimeout();

	}


	yield();
	delay(100);
}
