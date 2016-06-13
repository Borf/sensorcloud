#pragma once

#include "RFComm.h"
#include "RestServer.h"
#include "Db.h"
#include "PushBullet.h"
#include "json.h"

class RF24NetworkHeader;

class Node
{
public:
	int id;
	int address;
	unsigned long lastHello;
	bool timedOut;
};

class Server
{
public:
	json::Value config;
	RFComm rfcomm;
	RestServer restServer;
	Db db;
	PushBullet pb;

	std::map<int, Node*> nodes;


	Server(const json::Value &config);

	void update();

	void handleReqInfo(unsigned char nodeId, const RF24NetworkHeader &header, char* data);
	void handleSensorInfo(unsigned char nodeId, const RF24NetworkHeader &header, char* data);
	void handleHello(unsigned char nodeId, const RF24NetworkHeader &header, char* data);


	bool isAlive(int nodeId);
	unsigned long getTickCount();

};