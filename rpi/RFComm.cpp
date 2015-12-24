#include "RFComm.h"



RFComm::RFComm()
{
	radio = new RF24(RPI_V2_GPIO_P1_15, BCM2835_SPI_CS0, BCM2835_SPI_SPEED_8MHZ);
	network = new RF24Network(*radio);
	mesh = new RF24Mesh(*radio,*network);

	mesh->setNodeID(0);
	mesh->begin();
	radio->printDetails();
	printf("RFComm: started\n");
}


void RFComm::update()
{
	// Call network.update as usual to keep the network updated
	mesh->update();
	// In addition, keep the 'DHCP service' running on the master node so addresses will
	// be assigned to the sensor nodes
	mesh->DHCP();

	while ( network->available() > 0)
	{
		// If so, grab it and print it out
		char buffer[32];
		RF24NetworkHeader header;
		network->read(header, buffer, 32);
		int nodeId = mesh->getNodeId(header.from_node);


		unsigned char id = header.type;
		//printf("RFComm: Received packet %i from node %i (address %i)\n", id, nodeId, header.from_node);

		if(handlers.find(id) != handlers.end())
			handlers[id](this, nodeId, buffer);
		else
		{
			printf("RFComm: received unknown packetId: %i from node %i ( address %i)\n", id, nodeId, header.from_node);    
			send(header.from_node, 5, NULL, 0);
		}
	}
}



void RFComm::send(int nodeId, unsigned char packetId, char* data, int len) const
{
	int address = mesh->getAddress(nodeId);
	RF24NetworkHeader header(address, packetId);
	bool ok = network->write(header,data,len);
	if(!ok)
		printf("RFComm: Error sending packet\n");
}

void RFComm::send(int address, unsigned char packetId, const std::string &) const
{
	
}

void RFComm::sendMulti(int nodeId, unsigned char packetId, char* data, int len) const
{
	int address = mesh->getAddress(nodeId);
	char buffer[32];
	RF24NetworkHeader header(address, packetId);
	for(int i = 0; i < len; i+=23)
	{
		buffer[0] = i;
		memcpy(buffer+1, data+i, 23);

		bool ok = network->write(header,buffer,24);
		if (!ok)
			printf("RFComm::sendMulti, sending data to address %i  failed.\n", address);
	}

	
}


std::string RFComm::getConnectionInfo(int nodeId) const
{
	int address = mesh->getAddress(nodeId);
	return "RF: " + std::to_string(address);
}