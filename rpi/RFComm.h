#pragma once

#include <map>
#include <functional>
#include <string>

#include <RF24Mesh/RF24Mesh.h>
#include <RF24/RF24.h>
#include <RF24Network/RF24Network.h>


class RFComm
{

public:
	RF24* radio;  
	RF24Network* network;
	RF24Mesh* mesh;


	std::map<unsigned char, std::function<void(unsigned char nodeId, const RF24NetworkHeader& header, char* buffer)> > packetHandlers;

	RFComm();
	void update();
	void send(int address, unsigned char packetId, char* data, int len) const;
	void send(int address, unsigned char packetId, const std::string &) const;
	void sendMulti(int address, unsigned char packetId, char* data, int len) const;

	void registerHandler(unsigned char, const std::function<void(unsigned char nodeId, const RF24NetworkHeader& header, char* buffer)> &);

};
