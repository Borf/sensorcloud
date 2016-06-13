extern ESP8266WebServer httpServer;
#include <FS.h>



bool callApi(String api, String method, String postData, std::function<void(JsonObject& data)> callback)
{
  HTTPClient http;
  http.begin(String("http://api.sensorcloud.borf.info/" + api)); //HTTP
  int httpCode = 0;
  if(method == "GET")         httpCode = http.GET();
  else if(method == "POST")   httpCode = http.POST(postData);
  else { logger.printTime(); logger.println("API\tWrong calling method!"); }
  

  if(httpCode > 0) {
      if(httpCode != 200)
        Serial.printf("[HTTP] GET... code: %d\n", httpCode);
      if(httpCode == HTTP_CODE_OK) {
          String payload = http.getString();
          StaticJsonBuffer<2000> jsonBuffer;
          JsonObject& root = jsonBuffer.parseObject(payload);
          if (!root.success()) 
          {
            Serial.println("parseObject() failed");
            Serial.println(payload);
            return false;
          }
          callback(root);
      }
      return true;
  } else {
      Serial.printf("[HTTP] GET... failed, error %i: %s\n", httpCode, http.errorToString(httpCode).c_str());
      return false;
  }
}


bool callApi(String api, String method, JsonObject& postData, std::function<void(JsonObject& data)> callback)
{
  char buf[200];
  postData.printTo(buf, 200);
  return callApi(api, method, buf, callback);
}
bool callApi(String api, String method, JsonArray& postData, std::function<void(JsonObject& data)> callback)
{
  char buf[200];
  postData.printTo(buf, 200);
  return callApi(api, method, buf, callback);
}

String getContentType(String filename){
  if(httpServer.hasArg("download")) return "application/octet-stream";
  else if(filename.endsWith(".htm")) return "text/html";
  else if(filename.endsWith(".html")) return "text/html";
  else if(filename.endsWith(".css")) return "text/css";
  else if(filename.endsWith(".js")) return "application/javascript";
  else if(filename.endsWith(".png")) return "image/png";
  else if(filename.endsWith(".gif")) return "image/gif";
  else if(filename.endsWith(".jpg")) return "image/jpeg";
  else if(filename.endsWith(".ico")) return "image/x-icon";
  else if(filename.endsWith(".xml")) return "text/xml";
  else if(filename.endsWith(".pdf")) return "application/x-pdf";
  else if(filename.endsWith(".zip")) return "application/x-zip";
  else if(filename.endsWith(".gz")) return "application/x-gzip";
  return "text/plain";
}


bool handleFileRead(String path){
  path = "/www" + path;
  if(path.endsWith("/")) path += "index.html";
  String contentType = getContentType(path);
  String pathWithGz = path + ".gz";
  if(SPIFFS.exists(pathWithGz) || SPIFFS.exists(path)){
    if(SPIFFS.exists(pathWithGz))
      path += ".gz";
    File file = SPIFFS.open(path, "r");
    size_t sent = httpServer.streamFile(file, contentType);
    file.close();
    return true;
  }
  else
  {
    logger.printTime();
    logger.print("File ");
    logger.print(path);
    logger.println(" not found");
    
  }
  return false;
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
  logger.printTime();
  logger.print("\tClient ");
  logger.print(httpServer.client().remoteIP());
  logger.print(" tried to access file ");
  logger.print(httpServer.uri());
  logger.println(", but this file was not found");
  httpServer.send(404, "text/plain", message);
}



void handleRoot() {
  
  for(int i = 0; i < 3; i++)
    pixels.setPixelColor(i, pixels.Color(0,0,255));
  pixels.show();
  httpServer.send(200, "text/plain", "Everything seems to be working");
 
}



void initApi()
{
  SPIFFS.begin();
  httpServer.onNotFound([](){
    if(!handleFileRead(httpServer.uri()))
      handleNotFound();
  });

  httpServer.on("/", handleRoot);


  httpServer.on("/api/wifi/scan", [](){
    logger.printTime();
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

  
  httpServer.on("/api/sensors", [](){
    StaticJsonBuffer<200> buffer;
    JsonArray& sensors = buffer.createArray();

    for(auto s : settings.sensors)
    {
      JsonObject& o = buffer.createObject();
      o["id"] = s->id;
      s->settings(o);
      sensors.add(o);
    }
    
    char buf[200];
    sensors.printTo(buf, 200);
    httpServer.send(200, "application/json",  buf);
  }); 

  httpServer.on("/api/version", []()
  {
    httpServer.send(200, "application/json", "{\"version\" : 0.5 }");
    
  });
  
  httpServer.on("/api/actuate", HTTP_POST, [](){
    StaticJsonBuffer<200> jsonBuffer;
    JsonObject& root = jsonBuffer.parseObject(httpServer.arg(0));

    bool activated = false;
    for(auto a : settings.actuators)
    {
      logger.print("Actuator ");
      logger.println(a->id);
      if(a->id == root["id"])
      {
        a->activate(root);
        activated = true;
      }
    }
    if(!activated)
    {
      logger.printTime();
      logger.print("Could not find activator ");
      logger.println((int)root["id"]);
    }
    httpServer.send(200, "application/json",  "{\"res\":\"ok\"}");
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
}
