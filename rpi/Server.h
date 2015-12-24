#pragma once

#include "RFComm.h"
#include "IPComm.h"
#include "RestServer.h"
#include "Db.h"
#include "PushBullet.h"
#include "json.h"

class RF24NetworkHeader;

class Node
{
public:
	int id;
	unsigned long lastHello;
	bool timedOut;
	std::string name;

	Comm* comm;
	std::string connectionInfo;
	void send(unsigned char packetId, char* data, int len);
	void sendMulti(unsigned char packetId, char* data, int len);
};

class Server
{
public:
	json::Value config;
	RFComm rfcomm;
	IPComm ipcomm;
	RestServer restServer;
	Db db;
	PushBullet pb;

	std::map<int, Node*> nodes;


	Server(const json::Value &config);

	void update();

	void handleReqInfo(Comm* comm, unsigned char nodeId, char* data);
	void handleSensorInfo(Comm* comm, unsigned char nodeId, char* data);
	void handleHello(Comm* comm, unsigned char nodeId, char* data);


	bool isAlive(int nodeId);
	unsigned long getTickCount();

};