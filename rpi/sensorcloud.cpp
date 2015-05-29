/*
#include "RF24Mesh/RF24Mesh.h"  
#include <RF24/RF24.h>
#include <RF24Network/RF24Network.h>
#include <mysql.h>
#include <stdint.h>


#define MYSQL_HOST "192.168.2.204"
#define MYSQL_USER "sensorcloud"
#define MYSQL_PASS "L98JjXFveaBv8BBs"
#define MYSQL_DABA "sensorcloud"

RF24 radio(RPI_V2_GPIO_P1_15, BCM2835_SPI_CS0, BCM2835_SPI_SPEED_8MHZ);  
RF24Network network(radio);
RF24Mesh mesh(radio,network);

MYSQL* mysql;

#define MAX_LEAFS 32
#define MAX_SENSORCOUNT 8

#pragma pack(push)  // push current alignment to stack 
#pragma pack(1)     // set alignment to 1 byte boundary 
class EepromData
{public:
  uint16_t id;
  uint8_t sensorCount;
  struct Sensor
  {
    uint8_t type;
    uint8_t pin;
    uint16_t values[3];
  } sensors[MAX_SENSORCOUNT];
  char name[10];
};
#pragma pack(pop)   // restore original alignment from stack 

class LeafInfo
{
public:
  int id;
  int address;
  long milli;
  bool connected;
} leafs[MAX_LEAFS];


 enum SensorType
{
  SENSOR_DHT11_TEMP =     1,
  SENSOR_DHT11_HUM =      2,
  SENSOR_DHT22_TEMP =     3,
  SENSOR_DHT22_HUM =      4,
  SENSOR_YL38_HUM =       5,
  SENSOR_SWITCH =         6,
  SENSOR_ANALOG =         7,
  SENSOR_SHT10_TEMP =     8,
  SENSOR_SHT10_HUM =      9,

};

#define PKT_HELLO 1
#define PKT_REQINFO 2
#define PKT_INFO 3
#define PKT_SENSORINFO 4

#define PKT_MAX 32



void send(int address, int packet, char* data, size_t size)
{
  printf("Sending packet %i to address %i...", packet, address);
 
  RF24NetworkHeader header(address, packet);
  bool ok = network.write(header,data,size);
  if (ok)
    printf("...ok.\n");
  else
    printf("...failed :(.\n");
}

void (*packetHandlers[PKT_MAX])(const RF24NetworkHeader& header, char* buffer);

void handleHello(const RF24NetworkHeader& header, char* buffer)
{
	printf("Hello\n");
  bool found = false;
  for(int i = 0; i < MAX_LEAFS && !found; i++)
    if(leafs[i].connected && leafs[i].address == header.from_node)
    {
      leafs[i].milli = millis();
      found = true;
    }
  if(!found)
  {
    for(int i = 0; i < MAX_LEAFS; i++)
    {
      if(!leafs[i].connected) 
      {
        leafs[i].connected = true;
        leafs[i].address = header.from_node;
        leafs[i].milli = millis();
        break; 
      }
    }
  }
}

void handleReqInfo(const RF24NetworkHeader& header, char* buffer)
{
	EepromData data;
	memset(&data, 0, sizeof(data));
	printf("Data is %i bytes\n", sizeof(data));
	
	//requestSerial.print("I");
	//requestSerial.print(header.from_node, DEC);
	//requestSerial.print("\n");
	int id = mesh.getNodeId(header.from_node);
	printf("Got a request for whoami for sensor ID %i\n", id);
	char query[256];
	sprintf(query, "SELECT * FROM `nodes` WHERE `address` = %i", id);
	mysql_query(mysql, query);
	MYSQL_RES* result = mysql_store_result(mysql);
	if(!result)
	{
	  printf("Could not find this node in the database! no result\n");
	  return;
	}
	MYSQL_ROW row = mysql_fetch_row(result);
	if(!row)
	{
	  printf("Could not find this node in the database! no row\n");
	  return;
	}
	data.id = id;
	strcpy(data.name, row[1]);
	printf("Name: %s\n", data.name);
	
	 mysql_free_result(result);
	
	
	sprintf(query, "SELECT * FROM `sensors` WHERE `node` = %i", id);
	mysql_query(mysql, query);
	result = mysql_store_result(mysql);
	if(!result)
	{
	  printf("Could not find sensors in the database! no result\n");
	  return;
	}

	data.sensorCount = mysql_num_rows(result);
	while((row = mysql_fetch_row(result)))
	{
		int sensorId = atoi(row[1]);
		data.sensors[sensorId].type = atoi(row[2]);
		data.sensors[sensorId].pin = atoi(row[3]);
		data.sensors[sensorId].values[0] = atoi(row[4]);
		data.sensors[sensorId].values[1] = atoi(row[5]);
		data.sensors[sensorId].values[2] = atoi(row[6]);
	}
	mysql_free_result(result);
	
	for(int i = 0; i < sizeof(data); i+=23)
	{
		char buffer[24];
		buffer[0] = i;
		memcpy(buffer+1, ((char*)&data)+i, 23);

		RF24NetworkHeader header2(header.from_node, PKT_INFO);
		bool ok = network.write(header2,buffer,24);
		if (ok)
			printf("ok.");
		else
			printf("failed.");
	}
	printf("...Done sending info\n");

  
}

void handleSensorInfo(const RF24NetworkHeader& header, char* buffer)
{
  printf("Got sensorinfo from %i: %i -> ", header.from_node, buffer[1]);

  if(buffer[1] == SENSOR_DHT11_TEMP || 
     buffer[1] == SENSOR_DHT11_HUM || 
     buffer[1] == SENSOR_DHT22_TEMP || 
     buffer[1] == SENSOR_DHT22_HUM || 
     buffer[1] == SENSOR_YL38_HUM ||
     0)
  {
//    requestSerial.print((float)*((float*)(buffer+2)));
    printf("%f ", (float)*((float*)(buffer+2)));
  }
  if(buffer[1] == SENSOR_SWITCH ||
  
  0)
  {

  }
  
  printf("\n");

}

/ *
bool done;
void handleInfo(const RF24NetworkHeader& header, char* buffer)
{
  int offset = buffer[0];
  int len = 23;
  if(offset + len >= (int)sizeof(eepromData))
  {
    len = sizeof(eepromData) - offset;
    done = true;
  }
  memcpy(((char*)&eepromData) + offset, buffer+1, len);
}* /

void setup(void)
{
	mysql = mysql_init(NULL);
	 if (mysql_real_connect(mysql, MYSQL_HOST, MYSQL_USER, MYSQL_PASS, MYSQL_DABA, 0, NULL, 0) == NULL) 
	{
		fprintf(stderr, "%s\n", mysql_error(mysql));
		mysql_close(mysql);
		exit(1);
	}  
	
	// Set the nodeID to 0 for the master node
	mesh.setNodeID(0);
	// Connect to the mesh
	printf("start\n");
	mesh.begin();
	radio.printDetails();

	for(int i = 0; i < MAX_LEAFS; i++)
		leafs[i].connected = false;
	for(int i = 0; i < 32; i++)
		packetHandlers[i] = NULL;
    
	packetHandlers[PKT_HELLO] = &handleHello;
	packetHandlers[PKT_SENSORINFO] = &handleSensorInfo;
	packetHandlers[PKT_REQINFO] = &handleReqInfo;

}



void handleNetwork()
{
  while ( network.available() > 0)
  {
    // If so, grab it and print it out
    char buffer[32];
    RF24NetworkHeader header;
    network.read(header, buffer, 32);
    unsigned char id = header.type;
    printf("Received packet 0x%02x from %i\n", id, header.from_node);
    
    if(packetHandlers[id])
      packetHandlers[id](header, buffer);
    else
      printf("Unknown id\n");    
  }
}

void loop(void)
{
	// Call network.update as usual to keep the network updated
	mesh.update();
	// In addition, keep the 'DHCP service' running on the master node so addresses will
	// be assigned to the sensor nodes
	mesh.DHCP();


/ *

      for(int i = 0; i < sizeof(EepromData); i+=23)
      {
        char buffer[24];
        buffer[0] = i;
        memcpy(buffer+1, ((char*)&eepromData)+i, 31);
        
        RF24NetworkHeader header(eepromData.address, PKT_INFO);
        bool ok = network.write(header,buffer,24);
        if (ok)
          Serial.println("ok.");
        else
          Serial.println("failed.");
      }
   * /   

  handleNetwork();
}



*/

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

      
      
      
