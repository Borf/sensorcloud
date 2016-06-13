#include "Server.h"
#include "json.h"
#include <RF24Network/RF24Network.h>
#include <unistd.h>
#include <netdb.h>
#include <netinet/in.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>


#define PKT_HELLO 1
#define PKT_REQINFO 2
#define PKT_INFO 3
#define PKT_SENSORINFO 4
#define PKT_RESET 5
#define PKT_ACTIVATE 6
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
  ACT_RF_PROJECTORSCREEN =10,
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

static std::vector<std::string> split(std::string value, std::string seperator)
{
	std::vector<std::string> ret;
	while (value.find(seperator) != std::string::npos)
	{
		int index = value.find(seperator);
		if (index != 0)
			ret.push_back(value.substr(0, index));
		value = value.substr(index + seperator.length());
	}
	ret.push_back(value);
	return ret;
}

Server::Server(const json::Value &config) : config(config), db(config["mysql"]), pb(config)
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
			n["lastHello"] = (int)((getTickCount() - node.second->lastHello) / 1000);
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
	restServer.addHandler("/activate", "POST", [this](const HttpRequest& r, HttpResponse& h)
	{
		std::vector<std::string> path = r.splitUrl();
		json::Value v = "error";
		if(path.size() == 2 && path[1][0] == ':')
		{
		 	int nodeId = atoi(path[1].substr(1).c_str());
		 	if(nodeId != 0)
		 	{
		 		json::Value postData = r.getPostData();
				if (!postData.isMember("sensor") && !postData.isMember("value"))
					return;
		 		char data[2];
		 		data[0] = postData["sensor"].asInt();
		 		data[1] = postData["value"].asInt();

				if (nodes.find(nodeId) != nodes.end() && nodes[nodeId])
				{
					rfcomm.send(nodes[nodeId]->address, PKT_ACTIVATE, data, 2);
					v = "ok";
				}
				else
				{
					printf("Server: Could not activate node %i\n", nodeId);
				}
		 	}
			if (nodeId == 0)
			{
				json::Value postData = r.getPostData();
				

			}
		}


		h.setJson(v);

	});


	restServer.addHandler("/command", "POST", [this](const HttpRequest& r, HttpResponse &h)
	{
		json::Value postData = r.getPostData();
		if (!postData.isMember("command"))
			return;
		std::string command = postData["command"];
		printf("Server: Received command %s\n", command.c_str());
		if (command == "start kodi")
			system("ssh root@192.168.2.17 \"systemctl stop service.multimedia.mpd && systemctl start kodi\"");
		else if (command == "stop kodi")
			system("ssh root@192.168.2.17 \"systemctl start service.multimedia.mpd && systemctl stop kodi\"");
		else if (command == "start projector" || command == "stop projector")
		{
			struct sockaddr_in sin = { 0 };
			sin.sin_family = AF_INET;
			sin.sin_port = htons(20554);
			sin.sin_addr.s_addr = inet_addr("192.168.2.19");

			SOCKET s = socket(AF_INET, SOCK_STREAM, 0);
			int rc = connect(s, (struct sockaddr*) &sin, sizeof(sockaddr_in));
			char buf[1025];
			rc = recv(s, buf, 1024, 0);
			if (rc < 0)
				return;
			buf[rc] = 0;
			printf("Server: received %i bytes: %s\n", rc, buf);
			send(s, "PJREQ", 5, 0);
			rc = recv(s, buf, 1024, 0);
			if (rc < 0)
				return;
			buf[rc] = 0;
			printf("Server: received %i bytes: %s\n", rc, buf);
			char binData[7] = { 0x21, 0x89, 0x01, 0x50, 0x57, 0x31, 0x0A }; // turn on
			if (command == "stop projector")
				binData[5] = 0x30;

			send(s, binData, 7, 0);
			rc = recv(s, buf, 1024, 0);
			if (rc < 0)
				return;
			buf[rc] = 0;
			printf("Server: received %i bytes: %s\n", rc, buf);
			close(s);

		}

	});

	db.query("INSERT INTO `events` (`date`, `type`, `text`, `notify`, `nodeid`) VALUEs (NOW(), 'status', 'SensorCloud server started', false, NULL)");
}


void Server::update()
{
	db.update();
	restServer.update();
	rfcomm.update();
	pb.update();


	for (auto n : nodes)
	{
		if (n.second && !n.second->timedOut && n.second->lastHello + 60000 < getTickCount())
		{
			pb.sendMessage("Sensorcloud: Error!", "Error: node "  + std::to_string(n.second->id) + " timed out");
			db.query("INSERT INTO `events` (`date`, `type`, `text`, `notify`, `nodeid`) VALUEs (NOW(), 'status', 'Node timed out', false, "+std::to_string(n.second->id)+")");
			n.second->timedOut = true;
		}

	}


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
						nodes[nodeId]->timedOut = false;
						nodes[nodeId]->address = header.from_node;
						nodes[nodeId]->lastHello = getTickCount();
						db.query("INSERT INTO `events` (`date`, `type`, `text`, `notify`, `nodeid`) VALUEs (NOW(), 'status', 'Node started', false, "+std::to_string(nodeId)+")");

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
	nodes[nodeId]->timedOut = false;
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