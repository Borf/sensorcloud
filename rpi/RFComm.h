#pragma once

#include <map>
#include <functional>
#include <string>

#include "Comm.h"

#include <RF24Mesh/RF24Mesh.h>
#include <RF24/RF24.h>
#include <RF24Network/RF24Network.h>


class RFComm : public Comm
{

public:
	RF24* radio;  
	RF24Network* network;
	RF24Mesh* mesh;

	RFComm();
	void update();
	void send(int address, unsigned char packetId, char* data, int len) const;
	void send(int address, unsigned char packetId, const std::string &) const;
	void sendMulti(int address, unsigned char packetId, char* data, int len) const;

	std::string getConnectionInfo(int nodeId) const;

};
