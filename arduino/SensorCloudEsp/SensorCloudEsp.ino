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
#include <PubSubClient.h>
//display
#include <Transition.h>
#include <Graphics.h>
#include <SSD1306.h>
#include <SH1106.h>
#include <WaveShare42B.h>

#include "Log.h"
#include "Timer.h"
#include "Onkyo.h"

#include <list>
#include "Sensor.h"
#include "Actuator.h"
#include "Settings.h"
#include <ArduinoJson.h>

extern "C" {
#include "user_interface.h"
	void dns_setserver(char numdns, ip_addr_t *dnsserver);
}

WiFiServer server(23);
WiFiClient serverClient;
const char* ssid = "borf.info";
const char* password = "StroopWafel";
const char* mqtt_server = "192.168.2.201";
const char* progressor = "|/-\\";
enum class State
{
	INIT,
	CONFIGURING,
	LOOP
} state = State::INIT;

#ifndef MQTT_MAX_PACKET_SIZE
#error need to define max packet size
#endif
#if MQTT_MAX_PACKET_SIZE < 512
#error packet size too small
#endif


ESP8266WebServer httpServer(80);
ESP8266HTTPUpdateServer httpUpdater;
WiFiClient mqttWiFiClient;
PubSubClient mqttClient(mqttWiFiClient);

DisplayDriver* display = nullptr;
Graphics* graphics = nullptr;
Transition* transition = nullptr;


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
	Serial.println();
	Serial.println();
	Serial.println();
	logger.println("ESP\tBooting ESP...");
	logger.print("ESP\tMy ID is ");
	logger.println(ESP.getChipId(), HEX);

	char host[48];
	if (settings.loadFromEeprom()) //if config has been loaded properly before
	{
		logger.println("ESP\tSetting WiFi settings");
		sprintf(host, "borf.sensor.%i.%s", settings.id, settings.name.c_str());
		String wifiApName = String("borf.sensor.") + settings.id;
		IPAddress Ip(10, 0, settings.id, 1);
		IPAddress NMask(255, 255, 255, 0);
		WiFi.softAPConfig(Ip, Ip, NMask);
		WiFi.softAP(wifiApName.c_str(), password);
		wifi_station_set_hostname(host);
		initDisplay();
	}

	server.begin();
	server.setNoDelay(true);
	checkTelnet();

	httpServer.begin();
	MDNS.addService("http", "tcp", 80);
	initApi();
	logger.println("INIT\tAPI initialized");
	initOTA();
	logger.println("INIT\tOTA initialized");
	initMQTT();
	logger.println("INIT\tMQTT initialized");
	httpUpdater.setup(&httpServer);
	logger.println("INIT\thttp updater initialized");




	showMessage("Booting Esp");
	WiFi.mode(WIFI_AP_STA);
	WiFi.begin(ssid, password);
	showMessage("Connecting to wifi");
	while (WiFi.waitForConnectResult() != WL_CONNECTED)
	{
		logger.print(".");
		WiFi.begin(ssid, password);
		delay(1000);
	}
	showMessage("Connected to wifi");
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

	showMessage("Booted up");
	checkTelnet();
	MDNS.begin(host);
	timer.begin();

	if (settings.mode == Settings::Mode::Sensor)
		timer.addCallback(60 * 5, []()
		//timer.addCallback(5, []()
	{
		//display.resetTimeout();
		logger.println("Reading out sensors...");
		for (auto s : settings.sensors)
		{
			s->report();
		}
	}, true);



	timer.addCallback(60, []()
	{
		if (!WiFi.isConnected())
			return;
		DynamicJsonBuffer buffer;
		JsonObject& o = buffer.createObject();
		o["heapspace"] = ESP.getFreeHeap();
		o["ip"] = (String)(String(WiFi.localIP()[0]) + "." + String(WiFi.localIP()[1]) + "." + String(WiFi.localIP()[2]) + "." + String(WiFi.localIP()[3]));
		o["rssi"] = WiFi.RSSI();

		char topic[100];
		sprintf(topic, "%s/ping", settings.topicid.c_str());

		char message[512];
		o.printTo(message);
		mqttClient.publish(topic, message);

		logger.print("PING\tPing ");
		logger.println(topic);
	});

	timer.addCallback(60, []()
	{
		if (!WiFi.isConnected())
		{
			logger.println("WIFI\tReconnecting to WiFi");
			WiFi.reconnect();
		}
	});


	logger.println("Done setup...going to main loop");
}


void loop() {
	checkTelnet();
	ArduinoOTA.handle();
	httpServer.handleClient();
	timer.update();

	if (!WiFi.isConnected())
		return;

	if (!mqttClient.connected())
	{
		logger.println("MQTT\tReconnecting to broker");
		char clientId[100];
		sprintf(clientId, "sensorcloud-%x", ESP.getChipId());
		if (!mqttClient.connect(clientId))
		{
			logger.print("MQTT\tCould not connect to broker, state=");
			logger.println(mqttClient.state());
			return;
		}
		logger.print("MQTT\tConnected to broker: ");
		logger.println(mqttClient.connected() ? "Connected" : "NOT connected");

		//(re)subscribe
	}
	else
		if (!mqttClient.loop())
			logger.println("MQTT\tError looping");

	if (state == State::INIT && mqttClient.connected())
	{
		mqttClient.subscribe("boot/server");
		char topic[100];
		sprintf(topic, "boot/whoami/%x", ESP.getChipId());
		if (!mqttClient.subscribe(topic))
			logger.println("MQTT\tError subscribing");
		logger.print("MQTT\tSubscribing to ");
		logger.println(topic);


		char hwid[10];
		sprintf(hwid, "%x", ESP.getChipId());
		DynamicJsonBuffer buffer;
		JsonObject& o = buffer.createObject();
		o["hwid"] = hwid;


		char buf[200];
		o.printTo(buf, 200);
		mqttClient.publish("boot/whoami", buf);
		logger.print("MQTT\tPublishing to boot/whoami: ");
		logger.println(buf);
		state = State::CONFIGURING;
	}


	if (state == State::LOOP)
	{
		for (auto s : settings.sensors)
			s->update();
		for (auto a : settings.actuators)
			a->update();
	}
	//display.update();





	if (settings.mode == Settings::Mode::OnkyoDisplay)
	{
		onkyo.update();

		if (!onkyo.connected)
		{
		/*	display.clear();
			display.setTextAlignment(TEXT_ALIGN_LEFT);
			display.drawString(0, 0, "Connecting to receiver:");
			display.drawString(0, 30, onkyo.getIp().toString());
			display.setTextAlignment(TEXT_ALIGN_RIGHT);
			display.drawString(64, 50, String(progressor[(millis() / 250) % 4]));
			display.swap();*/
		}
		else if (!onkyo.isPoweredOn())
		{
		/*	display.clear();
			display.setTextAlignment(TEXT_ALIGN_LEFT);
			display.drawString(0, 0, "Receiver powered down");
			display.swap();*/
		}
		else
		{
			/*display.clear();
			display.setTextAlignment(TEXT_ALIGN_LEFT);
			display.drawStringMaxWidth(0, 0, 64, onkyo.artist);
			display.drawStringMaxWidth(0, 32, 64, onkyo.title);
			display.swap();*/

		}


	//	display.resetTimeout();

	}


	yield();
	delay(1);
}


void initMQTT()
{
	mqttClient.setServer(mqtt_server, 1883);
	mqttClient.setCallback([](char* topic, byte* payload, unsigned int length)
	{
		logger.print("MQTT\tGot data: ");
		logger.print(topic);
		logger.print("] ");
		for (int i = 0; i < length; i++) {
			logger.print((char)payload[i]);
		}
		logger.println();
		if (strlen(topic) > 12 && memcmp(topic, "boot/whoami/", 12) == 0)
		{
			String data((char*)payload);
			DynamicJsonBuffer jsonBuffer;
			JsonObject& root = jsonBuffer.parseObject(data);
			if (!root.success())
			{
				logger.println("parseObject() failed");
				logger.println(data);
			}

			settings.name = root["name"].as<const char*>();
			settings.id = root["id"];
			settings.topicid = root["roomtopic"].as<String>() + "/" + root["topic"].as<String>();

			if (root.containsKey("config"))
			{
				JsonObject &config = root["config"];
				if (config.containsKey("display"))
					settings.display = (Settings::DisplayType)config["display"].as<int>();
				if (config.containsKey("mode"))
					settings.mode = (Settings::Mode)config["mode"].as<int>();

			}
			if (settings.saveToEeprom())
			{
				logger.println("API\tSettings changed, rebooting");
				ESP.reset();
			}

			logger.print("API\tMy ID is ");
			logger.println(settings.id);
			logger.print("API\tMy name is ");
			logger.println(settings.name);
			logger.print("API\tTopic: ");
			logger.println(settings.topicid);
			if (settings.id == 0)
			{
				logger.println("Oops, cannot have ID 0");
				delay(1000);
				ESP.restart();
			}

			mqttClient.subscribe(settings.topicid.c_str());

			logger.print("API\tGot sensor info. Sensorcount: ");
			logger.println(root["sensors"].size());
			for (size_t i = 0; i < root["sensors"].size(); i++)
			{
				Sensor* sensor = Sensor::build(root["sensors"][i]);
				if (sensor)
					settings.sensors.push_back(sensor);
				else
				{
					Actuator* actuator = Actuator::build(root["sensors"][i]);
					if (actuator)
						settings.actuators.push_back(actuator);

					else
					{
						logger.print("Unknown sensor / actuator type: ");
						logger.print((int)root["sensors"]["type"]);
					}
				}
			}



			if (settings.mode == Settings::Mode::Sensor)
			{
				/*FrameCallback* frames = new FrameCallback[settings.sensors.size()];
				int i = 0;
				for (auto s : settings.sensors)
					frames[i++] = [](OLEDDisplay* d, OLEDDisplayUiState* state, int16_t x, int16_t y)
				{
					auto it = settings.sensors.begin();
					std::advance(it, state->currentFrame);
					(*it)->print(d, x + display.offX, y + display.offY);
				};
				for (auto a : settings.actuators)
					frames[i++] = nullptr;

				if (display.ui && i > 0)
					display.ui->setFrames(frames, i);*/
			}

			if (settings.mode == Settings::Mode::OnkyoDisplay)
			{
				onkyo.begin(IPAddress(192, 168, 2, 200), 60128);
			}

			state = State::LOOP;
		}
		else if (strcmp(topic, "boot/server") == 0)
		{
			logger.println("MQTT\tServer message....");
			logger.print("MQTT\tLength: ");
			logger.println(length);
			logger.println(memcmp(payload, "alive", 5));
			if (length == 5 && memcmp(payload, "alive", 5) == 0) //dead, alive
			{
				if (state == State::LOOP)
				{
					logger.println("Server got back up....restarting.");
					ESP.reset();
				}
			}
		}
	});
}



void initOTA()
{
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
	ArduinoOTA.setHostname(settings.name.c_str());
	ArduinoOTA.begin();
}


void initDisplay()
{
	if (settings.display == Settings::DisplayType::None)
		return;

	if (settings.display == Settings::DisplayType::D1Mini)
		display = new SH1106(0x3c, D1, D2);
	else if (settings.display == Settings::DisplayType::D1MiniShield)
		display = new SSD1306(0x3c, SDA, SCL);
	else if (settings.display == Settings::DisplayType::WaveShare)
	{/* 
		BUSY     D1
		RST      D2
		DC       D8
		CS       D3
		CLK      D5
		DIN      D7
		GND      GND
		3.3V     3V3 */
		#define CS		D3
		#define RST		D2
		#define DC		D8
		#define BUSY	D1
		display = new WaveShare42B(CS, RST, DC, BUSY);
	}
	graphics = new Graphics(display);
	transition = new Transition(*graphics);

}




void showMessage(String msg)
{

//	graphics->clear(Graphics::Color::Black);
//	graphics->println(msg);
//	graphics->display();
}