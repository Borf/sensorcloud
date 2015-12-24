#include "IPComm.h"

#include <vector>
#include <sstream>
#include <algorithm>

#include <unistd.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <netdb.h>
#include <netinet/in.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>



extern void closesocket(SOCKET s);


IPComm::IPComm()
{
	printf("IPComm: Initializing\n");
	int port = 9090;

	struct sockaddr_in sin;
	if ((s = socket(AF_INET,SOCK_STREAM,0)) == 0)
	{
		printf("IPComm: error creating socket!\n");
		return;
	}
	memset(&sin, 0, sizeof(sin));
	sin.sin_family = AF_INET;
	inet_aton("0.0.0.0", &sin.sin_addr);
	sin.sin_port = htons(port);
	int t_reuse = 1;
	setsockopt(s, SOL_SOCKET, SO_REUSEADDR, (char*)&t_reuse, sizeof(t_reuse));

	if ((bind(s, (struct sockaddr*)&sin, sizeof(sin))))
	{
		printf("IPComm: Error binding socket, port %i\n", port);
		closesocket(s);
		return;
	}
	
	if ((listen(s, 10)))
	{
		printf("IPComm: Error listening to port %i\n", port);
		closesocket(s);
		return;
	}
	printf("IPComm: Listening on 0.0.0.0:%i\n", port);
}


void IPComm::update()
{
	fd_set socks;
	struct timeval timeout;
	SOCKET highsock = 0;

	for(std::list<Connection>::iterator it = connections.begin(); it != connections.end(); it++)
		if(it->s == 0)
			it = connections.erase(it);

	FD_ZERO(&socks);

	FD_SET(s, &socks);		highsock = highsock > s ? highsock : s;	
	for(Connection& connection : connections)
	{
		FD_SET(connection.s, &socks);		highsock = highsock > connection.s ? highsock : connection.s;
	}


	timeout.tv_sec = 0;
	timeout.tv_usec = 1;
	int readsocks = select(highsock+1, &socks, (fd_set *) 0, (fd_set *) 0, &timeout);
	if (readsocks < 0)
	{
		printf("IPComm: Error with select()\n");
		return;
	}
	if(readsocks == 0)
		return;
	if(FD_ISSET(s, &socks))
	{
		struct sockaddr_in client;
		SOCKET new_s;
		socklen_t size = sizeof(client);
		if((new_s = accept(s, (struct sockaddr*)&client, &size)))
		{
			printf("IPComm: New connection\n");
			char ip[32];
			inet_ntop(AF_INET, &client.sin_addr, ip, 32);
			connections.push_back(Connection(new_s, ip));			
		}
	}

	for(Connection& connection : connections)
	{
		if(FD_ISSET(connection.s, &socks))
		{
			char buf[1024];
			memset(buf,0,1024);
			int rc = recv(connection.s, buf, 1024, 0);
			if(rc <= 0)
			{
				printf("IPComm: Connection to node %i lost\n", connection.nodeId);
				closesocket(connection.s);
				connection.s = 0;
				continue;
			}
			connection.data += std::string(buf, rc);
			
			while(connection.data.size() >= 3)
			{
				int nodeId = connection.data[0];
				int packetId = connection.data[1];
				unsigned int len = connection.data[2];
				if(connection.nodeId == -1)
					printf("IPComm: NodeID for new connection is %i\n", nodeId);
				connection.nodeId = nodeId;
				
				for(Connection& c : connections)
					if(c.s != connection.s && c.s != 0 && c.nodeId == nodeId)
					{
						closesocket(c.s);
						c.s = 0;
					}
				
				
				if(connection.data.size() >= len+3)
				{
					if(handlers.find(packetId) != handlers.end())
						handlers[packetId](this, nodeId, (char*)connection.data.c_str()+3);
					connection.data = connection.data.substr(len+3);
				}
				else
					break;
			}
			
		}
	}

	connections.remove_if([](Connection& c) { return c.s == 0; });
	
}


const IPComm::Connection* IPComm::getConnectionForId(int nodeId) const
{
	for(const IPComm::Connection& c : connections)
		if(c.nodeId == nodeId)
			return &c;
	return NULL;
}


void IPComm::send(int nodeId, unsigned char packetId, char* data, int len) const
{
	const Connection* conn = getConnectionForId(nodeId);
	if(!conn)
		return;
	char buf[100];
	buf[0] = packetId;
	buf[1] = len;
	if(len > 0)
		memcpy(buf+2, data, len);
	::send(conn->s, buf, len+2, 0);
}


void IPComm::sendMulti(int nodeId, unsigned char packetId, char* data, int len) const
{
	const Connection* conn = getConnectionForId(nodeId);
	if(!conn)
		return;
		
	char buffer[33];
	buffer[0] = packetId;
	buffer[1] = 24;
	for(int i = 0; i < len; i+=23)
	{
		buffer[2] = i;
		memcpy(buffer+3, data+i, 23);
		::send(conn->s, buffer, 26, 0);
		usleep(1000);

	}
	
}



std::string IPComm::getConnectionInfo(int nodeId) const
{
	const Connection* con = getConnectionForId(nodeId);
	if(!con)
		return "";
		
	return "IP: " + con->ip;
}
