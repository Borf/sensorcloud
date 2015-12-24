#pragma once

#include <functional>
#include <map>
#include <string>

class Comm
{
public:

	virtual void update() = 0;
	virtual void send(int address, unsigned char packetId, char* data, int len) const = 0;
//	virtual void send(int address, unsigned char packetId, const std::string &) const = 0;
	virtual void sendMulti(int address, unsigned char packetId, char* data, int len) const = 0;

	virtual std::string getConnectionInfo(int nodeId) const = 0;


	std::map<unsigned char, std::function<void(Comm* comm, int nodeId, char* data)> > handlers;
	void registerHandler(unsigned char packetId, const std::function<void(Comm* comm, unsigned char nodeId, char* data)> &callback)
	{
		handlers[packetId] = callback;
	}


};