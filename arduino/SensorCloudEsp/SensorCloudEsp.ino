#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <ESP8266WebServer.h>
#include <ESP8266mDNS.h>
#include <ESP8266HTTPUpdateServer.h>
#include "Log.h"

#include <Adafruit_NeoPixel.h>

extern "C" {
#include "user_interface.h"
}

Log logger;
char* host = "esp8266-sensor.1";
const char* ssid = "borf.info";
const char* password = "StroopWafel";
const char* wifiApName = "borf.sensor.1";
IPAddress Ip(10, 0, 1, 1);
IPAddress NMask(255, 255, 255, 0);

ESP8266WebServer httpServer(80);
ESP8266HTTPUpdateServer httpUpdater;

Adafruit_NeoPixel pixels(3, 4, NEO_RGB + NEO_KHZ800);
int pulse = 0;

void handleRoot() {
  
  for(int i = 0; i < 3; i++)
    pixels.setPixelColor(i, pixels.Color(0,0,255));
  pixels.show();
  httpServer.send(200, "text/plain", "Everything seems to be working");
 
}
void handleNotFound(){
  String message = "File Not Found\n\n";
  message += "URI: ";
  message += httpServer.uri();
  message += "\nMethod: ";
  message += (httpServer.method() == HTTP_GET)?"GET":"POST";
  message += "\nArguments: ";
  message += httpServer.args();
  message += "\n";
  for (uint8_t i=0; i<httpServer.args(); i++){
    message += " " + httpServer.argName(i) + ": " + httpServer.arg(i) + "\n";
  }
  logger.print("Client ");
  logger.print(httpServer.client().remoteIP());
  logger.print(" tried to access file ");
  logger.print(httpServer.uri());
  logger.println(", but this file was not found");
  httpServer.send(404, "text/plain", message);
}


void setup() {
  pixels.begin(); // This initializes the NeoPixel library.
  for(int i = 0; i < 3; i++)
    pixels.setPixelColor(i, pixels.Color(255,0,0));
  pixels.show();
  
  Serial.begin(115200);
  logger.println();
  logger.println("Booting ESP...");
  logger.println("");
  
  WiFi.mode(WIFI_AP_STA);
  WiFi.softAPConfig(Ip, Ip, NMask);
  WiFi.softAP(wifiApName, password);
  wifi_station_set_hostname(host);
  WiFi.begin(ssid, password);
  while(WiFi.waitForConnectResult() != WL_CONNECTED){
    WiFi.begin(ssid, password);
    logger.println("WiFi failed, retrying.");
  }
  logger.println("");
  logger.print("Connected to ");
  logger.println(ssid);
  logger.print("IP address: ");
  logger.println(WiFi.localIP());
  
  MDNS.begin(host);

  httpUpdater.setup(&httpServer);
  httpServer.begin();
  MDNS.addService("http", "tcp", 80);
  Serial.printf("HTTPUpdateServer ready! Open http://%s.local/update in your browser\n", host);

  httpServer.onNotFound(handleNotFound);
  httpServer.on("/", handleRoot);
  httpServer.on("/inline", [](){
    httpServer.send(200, "text/plain", "this works as well");
  });

  httpServer.on("/api/wifi/scan", [](){
    logger.println("Scanning for networks");
    int networksFound = WiFi.scanNetworks(false, true);
    String message = "[";
    for (int i = 0; i < networksFound; ++i)
    {
      if(i != 0)
        message += ",";
      message += "{ \"id\" : ";
      message += (i + 1);
      message += ", \"ssid\" : \"";
      message += WiFi.SSID(i);
      message += "\", \"rssi\" : ";
      message += WiFi.RSSI(i);
      message += ", \"encryption\" : ";
      message += WiFi.encryptionType(i);
      message += ", \"channel\" : ";
      message += WiFi.channel(i);
      message += ", \"BSSID\" : \"";
      message += WiFi.BSSIDstr(i);
      message += "\", \"hidden\" : ";
      message += WiFi.isHidden(i) ? "true" : "false";
      message += "} ";
      delay(5);
    }
    message += "]";
    httpServer.send(200, "application/json", message);
  });
   httpServer.on("/api/wifi/settings", [](){
    httpServer.send(200, "application/json", "{ \"ssid\" : \"borf.info\", \"password\" : \"StroopWafel\" }" );
  }); 

  httpServer.on("/api/log", [](){
    String message = "[";
    bool first = true;
    for(int i = (logger.index+1) % 100; i != logger.index; i = (i+1)%100)
    {
      if(logger.lines[i].length() != 0)
      {
        if(!first)
          message += ",";
        message += "\"";
        String s = logger.lines[i];
        s.replace(String("\""), String("\\\""));
        message += s;
        message += "\"";
        first = false;
      }
      
    }
    
    message += "]";

    httpServer.send(200, "application/json", message);
    
  });
  

  for(int i = 0; i < 3; i++)
    pixels.setPixelColor(i, pixels.Color(0,255,0));
  pixels.show();
}

void loop() {
  httpServer.handleClient();

  delay(20);
}
