#include "Server.h"
#include <fstream>


int main(int argc, char** argv) 
{
  json::Value config;
  std::ifstream configFile("config.json");
  if(configFile.is_open())
    config = json::readJson(configFile);
  if(config.isNull() || !config.isMember("mysql"))
  {
    config["mysql"]["host"] = "localhost";
    config["mysql"]["user"] = "sensorcloud";
    config["mysql"]["pass"] = "sensorcloud";
    config["mysql"]["daba"] = "sensorcloud";
    std::ofstream("config.json")<<config;
    printf("Error loading config... generated new configfile\n");
    exit(0);
  }


  Server server(config);
  printf("Done initializing...\n");

  while(true)
    server.update();
/*
	setup();
	while(1)
	{
		loop();
	}*/
	return 0;
}

      
      
      
