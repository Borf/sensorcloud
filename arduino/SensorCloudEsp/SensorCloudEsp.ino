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
static const uint8_t D10  = 1;

#include <DHT.h>

#define _NOMINMAX
#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <ESP8266WebServer.h>
#include <ESP8266mDNS.h>
#include <ESP8266HTTPUpdateServer.h>
#include <ESP8266HTTPClient.h>
#include <SPI.h>
#include <Wire.h>  // Include Wire if you're using I2C
#include <SFE_MicroOLED.h>  // Include the SFE_MicroOLED library

#include "Log.h"
#include "Timer.h"

#include <list>
#include "Sensor.h"
#include "Actuator.h"

#include <ArduinoJson.h>
#include <Adafruit_NeoPixel.h>

extern "C" {
#include "user_interface.h"
}

struct Settings
{
  int id = -1;
  String name;
  std::list<Sensor*> sensors;
  std::list<Actuator*> actuators;
} settings;
int getId() { return settings.id; };



Log logger;
Timer timer;
const char* ssid = "borf.info";
const char* password = "StroopWafel";
//#define USE_DISPLAY



#ifdef USE_DISPLAY
MicroOLED oled(255, 0); // Example I2C declaration
#endif

ESP8266WebServer httpServer(80);
ESP8266HTTPUpdateServer httpUpdater;

Adafruit_NeoPixel pixels(3, 4, NEO_RGB + NEO_KHZ800);
int pulse = 0;
long displayOffTime;

bool callApi(String api, String method, String postData, std::function<void(JsonObject& data)> callback);
bool callApi(String api, String method, JsonObject& postData, std::function<void(JsonObject& data)> callback);
bool callApi(String api, String method, JsonArray& postData, std::function<void(JsonObject& data)> callback);



void setup() {
  pixels.begin(); // This initializes the NeoPixel library.
  for(int i = 0; i < 3; i++)
    pixels.setPixelColor(i, pixels.Color(255,0,0));
  pixels.show();

#ifdef USE_DISPLAY
  oled.begin();
  oled.clear(PAGE);  // Clear the display's memory (gets rid of artifacts)
  oled.clear(ALL);  // Clear the display's memory (gets rid of artifacts)
  oled.display();   
  oled.clear(PAGE);  // Clear the display's memory (gets rid of artifacts)
  oled.println("Booting...");
  oled.display();
#endif

  Serial.begin(115200);
  logger.println("ESP\tBooting ESP...");
  logger.print("ESP\tMy ID is ");
  logger.println(ESP.getChipId(), HEX);
  logger.print("WIFI\tConnecting to WiFi...");
  WiFi.mode(WIFI_AP_STA);
  WiFi.begin(ssid, password);
  while(WiFi.waitForConnectResult() != WL_CONNECTED)
  {
    logger.print(".");
    WiFi.begin(ssid, password);
    delay(1000);
  }
  logger.print("\nWIFI\tConnected to ");
  logger.println(ssid);
  logger.print("WIFI\tIP address: ");
  logger.println(WiFi.localIP());
  logger.print("WIFI\tDNS address: ");
  logger.println(WiFi.dnsIP(0));
  logger.println("API\tConnecting to API for ID");
  if(!callApi(String("nodes/hwid:") + String(ESP.getChipId(), HEX), "GET", "", [](JsonObject& ret)
  {
    settings.name = ret["data"][0]["name"].as<const char*>();
    settings.id = ret["data"][0]["id"];
    logger.print("API\tMy ID is ");
    logger.println(settings.id);
    logger.print("API\tMy name is ");
    logger.println(settings.name);
    if(settings.id == 0)
    {
      logger.println("Oops, cannot have ID 0");
      delay(1000);
      ESP.restart();
    }
  })) ESP.restart();
#ifdef USE_DISPLAY
  oled.print("I am ");
  oled.println(settings.name);
  oled.display();
#endif
  String host = String("esp8266-sensor.") + settings.id;
  String wifiApName = String("borf.sensor.") + settings.id;
  IPAddress Ip(10, 0, settings.id, 1);
  IPAddress NMask(255, 255, 255, 0);
  WiFi.softAPConfig(Ip, Ip, NMask);
  WiFi.softAP(wifiApName.c_str(), password);
  wifi_station_set_hostname((char*)host.c_str());
#if 1
  logger.print("WIFI\treconnecting for proper name in dhcpd....");
  WiFi.reconnect();  
  WiFi.begin(ssid, password);
  while(WiFi.waitForConnectResult() != WL_CONNECTED)
  {
    WiFi.begin(ssid, password);
    logger.print(".");
    delay(100);
  }
  logger.println("\nWIFI\treconnected");

#endif
  //ok, all initialized
  
  MDNS.begin(host.c_str());
  timer.begin();

  callApi(String("sensors/nodeid:") + settings.id, "GET", "", [](JsonObject& ret)
  {
    logger.printTime();
    logger.print("API\tGot sensor info. Sensorcount: ");
    logger.println(ret["data"].size());
    for(int i = 0; i < ret["data"].size(); i++)
    {
      Sensor* sensor = Sensor::build(ret["data"][i]);
      if(sensor)
        settings.sensors.push_back(sensor);
      else
      {
        Actuator* actuator = Actuator::build(ret["data"][i]);
        if(actuator)
          settings.actuators.push_back(actuator);

        else
        {
          logger.printTime();
          logger.print("Unknown sensor / actuator type: ");
          logger.print((int)ret["data"]["type"]);
        }
      }
    }
  });


  timer.addCallback(60, []()
  {
    logger.printTime();
    logger.println("Reading out sensors...");
    StaticJsonBuffer<200> buffer;
    JsonArray& allData = buffer.createArray();
    for(auto s : settings.sensors)
    {
      JsonObject& o = buffer.createObject();
      o["id"] = s->id;
      s->getData(o, buffer);
      allData.add(o);
    }

    callApi(String("report/:") + settings.id, "POST", allData, [](JsonObject &ret)   {   });
    
  }, true);



  timer.addCallback(13, []()
  {
    StaticJsonBuffer<200> buffer;
    JsonObject& o = buffer.createObject();
    o["heapspace"] = ESP.getFreeHeap();
    callApi(String("ping/:") + settings.id, "POST", o, [](JsonObject &ret) {  });
  });



  httpUpdater.setup(&httpServer);
  httpServer.begin();
  MDNS.addService("http", "tcp", 80);
  logger.printTime();
  logger.printf("HTTP\tHTTPUpdateServer ready! Open http://%s.local/update in your browser\n", host.c_str());
  initApi();


#ifdef USE_DISPLAY
  oled.println("Booted :)");
  oled.display();
  delay(1000);
  oled.clear(PAGE);
  oled.display();
  displayOffTime = millis() + 10000;
#endif
 
  for(int i = 0; i < 3; i++)
    pixels.setPixelColor(i, pixels.Color(0,255,0));
  pixels.show();
}

long displayScroll;

void loop() {
  httpServer.handleClient();
  timer.update();

  for(auto s : settings.sensors)
    s->update();
  for(auto a : settings.actuators)
    a->update();

#ifdef USE_DISPLAY
  if(analogRead(A0) > 100)
  {
      if(millis() >= displayOffTime)
      {
        oled.command(DISPLAYON);
        displayScroll = millis();
      }
      displayOffTime = millis() + 10000;
  }
  
  if(millis() < displayOffTime)
  {
    oled.clear(PAGE);
    oled.setCursor(0,0);
    oled.println(settings.name);
    oled.print("IP: .");
    oled.print(WiFi.localIP()[2]);
    oled.print(".");
    oled.println(WiFi.localIP()[3]);


    int i = 0;
    long line = ((millis() - displayScroll) / 1500) % settings.sensors.size();
    for(auto s : settings.sensors)
    {
      if(i == line)
        s->print(oled);
      i++;
    }
    

  
    int bars = -1;
    int rssi = WiFi.RSSI();
    if(rssi > -60)
      bars = 5;
    else if(rssi > -70)
      bars = 4;
    else if(rssi > -80)
      bars = 3;
    else if(rssi > -90)
      bars = 2;
    else if(rssi > -100)
      bars = 1;
  
    if(bars >= 0)
    {
      for(int i = 0; i < bars; i++)
      {
        oled.lineV(58 + i,5-i,i+1);
      }
    }
      
    oled.lineH(0,6,64);
    oled.display();
  }
  else
    oled.command(DISPLAYOFF);
#endif

  delay(1);
}
