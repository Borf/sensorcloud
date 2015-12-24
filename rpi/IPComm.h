#pragma once
#include "Comm.h"

#include <string>
#include <functional>
#include <list>
#include <map>
#include <vector>

typedef int SOCKET;

class IPComm : public Comm
{
	class Connection
	{
	public:
		SOCKET s;
		std::string data;
		std::string ip;
		Connection(SOCKET s, const std::string &ip) { this->s = s; this->ip = ip; this->nodeId = -1; }
		
		int nodeId;
	};

	std::list<Connection> connections;
	SOCKET s;

	const Connection* getConnectionForId(int nodeId) const;

public:
	IPComm();
	void update();
	virtual void send(int address, unsigned char packetId, char* data, int len) const;
	virtual void sendMulti(int address, unsigned char packetId, char* data, int len) const;
	virtual std::string getConnectionInfo(int nodeId) const;
};
