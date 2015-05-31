#include "Server.h"
#include "json.h"
#include <RF24Network/RF24Network.h>
#include <unistd.h>

#define PKT_HELLO 1
#define PKT_REQINFO 2
#define PKT_INFO 3
#define PKT_SENSORINFO 4
#define PKT_RESET 5
#define PKT_MAX 32

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

#define MAX_SENSORCOUNT 8
#pragma pack(push)  /* push current alignment to stack */
#pragma pack(1)     /* set alignment to 1 byte boundary */
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
#pragma pack(pop)   /* restore original alignment from stack */



Server::Server(const json::Value &config) : config(config), db(config["mysql"])
{

	rfcomm.registerHandler(PKT_REQINFO, std::bind(&Server::handleReqInfo, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3));
	rfcomm.registerHandler(PKT_HELLO, std::bind(&Server::handleHello, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3));
	rfcomm.registerHandler(PKT_SENSORINFO, std::bind(&Server::handleSensorInfo, this, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3));


	restServer.addHandler("/list", "GET", [this](const HttpRequest& r, HttpResponse& h) 
	{ 
		json::Value v;
		for(auto node : nodes)
		{
			json::Value n;
			n["id"] = node.second->id;
			n["lastHello"] = (int)(getTickCount() - node.second->lastHello);
			n["address"] = node.second->address;
			v.push_back(n);
		}
		h.setJson(v);
	});
	restServer.addHandler("/reset", "POST", [this](const HttpRequest& r, HttpResponse& h) 
	{
		std::vector<std::string> path = r.splitUrl();
		json::Value v;
		if(path.size() != 2 || path[1][0] != ':')
			v = "error";
		else
		{
		 	int nodeId = atoi(path[1].substr(1).c_str());
		 	if(nodeId != 0)
		 	{
			 	v = "ok";
			 	rfcomm.send(nodes[nodeId]->address, PKT_RESET, NULL, 0);
			 }
			 else
			 	v = "error";
		}
		h.setJson(v);
	});


}


void Server::update()
{
	db.update();
	restServer.update();
	rfcomm.update();


	usleep(1000);
}


bool Server::isAlive(int nodeId)
{
	if(nodes.find(nodeId) == nodes.end())
		return false;
	if(nodes[nodeId]->lastHello + 60000 < getTickCount())
		return false;
	return true;
}


void Server::handleReqInfo(unsigned char nodeId, const RF24NetworkHeader &header, char* data)
{
	printf("Server: Got request for whoami info\n");


	EepromData* clientInfo = new EepromData();
	memset(clientInfo, 0, sizeof(EepromData));

	clientInfo->id = nodeId;


	db.query("SELECT * FROM `nodes` WHERE `address` = " + std::to_string(nodeId), 
		[clientInfo](MYSQL_ROW row)
		{
			strcpy(clientInfo->name, row[1]);
		},
		[nodeId, clientInfo](MYSQL_RES* result)
		{
			if(mysql_num_rows(result) == 0)
			{
				printf("Server: Error, could not find whoami info in database for node %i\n", nodeId);
				clientInfo->id = 0;
			}
		}).then([this, clientInfo, nodeId, header]()
		{
			if(clientInfo->id == 0)
				return;

				db.query("SELECT * FROM `sensors` WHERE `node` = " + std::to_string(nodeId), 
					[clientInfo](MYSQL_ROW row)
					{
						int sensorId = atoi(row[1]);
						clientInfo->sensors[sensorId].type = atoi(row[2]);
						clientInfo->sensors[sensorId].pin = atoi(row[3]);
						clientInfo->sensors[sensorId].values[0] = atoi(row[4]);
						clientInfo->sensors[sensorId].values[1] = atoi(row[5]);
						clientInfo->sensors[sensorId].values[2] = atoi(row[6]);
					}, [clientInfo](MYSQL_RES* result)
					{
						clientInfo->sensorCount = mysql_num_rows(result);
					}).then([this, clientInfo, nodeId, header]()
					{
						rfcomm.sendMulti(header.from_node, PKT_INFO, (char*)clientInfo, sizeof(EepromData));
						nodes[nodeId] = new Node();
						nodes[nodeId]->id = nodeId;
						nodes[nodeId]->address = header.from_node;
						nodes[nodeId]->lastHello = getTickCount();

					});
		});
}


void Server::handleHello(unsigned char nodeId, const RF24NetworkHeader &header, char* data)
{
	if(!isAlive(nodeId))
	{
		rfcomm.send(header.from_node, PKT_RESET, NULL, 0);
		return;
	}
	nodes[nodeId]->lastHello = getTickCount();
}

void Server::handleSensorInfo(unsigned char nodeId, const RF24NetworkHeader &header, char* data)
{
	if(!isAlive(nodeId))
	{
		printf("Server: Got a sensorinfo packet from a timed out node. Resetting\n");
		rfcomm.send(header.from_node, PKT_RESET, NULL, 0);
		return;
	}
	printf("Server: Got sensorinfo from %i: %i -> ", nodeId, data[1]);

	if(	data[1] == SENSOR_DHT11_TEMP || 
		 data[1] == SENSOR_DHT11_HUM || 
		 data[1] == SENSOR_DHT22_TEMP || 
		 data[1] == SENSOR_DHT22_HUM || 
		 data[1] == SENSOR_YL38_HUM ||
	 0)
	{
		float value = (float)*((float*)(data+2));
	    printf("%f\n", value);
		db.query("INSERT INTO `data` (`date`, `nodeid`, `sensorid`, `data`) VALUES (NOW(), "+std::to_string(nodeId)+", "+std::to_string(data[1])+", " + std::to_string(value) + ")");
	}
	else if(data[1] == SENSOR_SWITCH ||
	0)
	{
		printf("Switch data\n");
	}
	else
		printf("UNKNOWN DATATYPE\n");
}



unsigned long Server::getTickCount()
{
	struct timespec ts;
	if(clock_gettime(CLOCK_MONOTONIC,&ts) != 0) {
	 //error
	}
	unsigned long result = 0U;
	result  = ts.tv_nsec / 1000000;
    result += ts.tv_sec * 1000;
    return result;
}