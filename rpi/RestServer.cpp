#include "RestServer.h"
#include "json.h"

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


// trim from start
static inline std::string &ltrim(std::string &s) {
	s.erase(s.begin(), std::find_if(s.begin(), s.end(), std::not1(std::ptr_fun<int, int>(std::isspace))));
	return s;
}

// trim from end
static inline std::string &rtrim(std::string &s) {
	s.erase(std::find_if(s.rbegin(), s.rend(), std::not1(std::ptr_fun<int, int>(std::isspace))).base(), s.end());
	return s;
}

// trim from both ends
static inline std::string &trim(std::string &s) {
	return ltrim(rtrim(s));
}

void closesocket(SOCKET s)
{
    close(s);
}

static std::vector<std::string> split( std::string value, std::string seperator )
{
	std::vector<std::string> ret;
	while(value.find(seperator) != std::string::npos)
	{
		int index = value.find(seperator);
		if(index != 0)
			ret.push_back(value.substr(0, index));
		value = value.substr(index+seperator.length());
	}
	ret.push_back(value);
	return ret;
}


RestServer::RestServer()
{
	printf("RestServer: Initializing\n");
	int port = 8080;

	struct sockaddr_in sin;
	if ((s = socket(AF_INET,SOCK_STREAM,0)) == 0)
	{
		printf("RestServer: error creating socket!\n");
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
		printf("RestServer: Error binding socket, port %i\n", port);
		closesocket(s);
		return;
	}
	
	if ((listen(s, 10)))
	{
		printf("RestServer: Error listening to port %i\n", port);
		closesocket(s);
		return;
	}
	printf("RestServer: Listening on 0.0.0.0:%i\n", port);
}


void RestServer::update()
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
		printf("RestServer: Error with select()\n");
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
			int rc = recv(connection.s, buf, 1024, 0);
			if(rc <= 0)
			{
				closesocket(connection.s);
				connection.s = 0;
				continue;
			}
			connection.data += std::string(buf, rc);
			if(connection.data.find("\r\n\r\n") != std::string::npos)
			{
				std::string req = connection.data;
				req = req.substr(0, req.find("\r\n"));
				std::string method = req.substr(0, req.find(" "));
				std::string url = req.substr(req.find(" ")+1);
				url = url.substr(0, url.find(" "));
				std::string data = connection.data.substr(connection.data.find("\r\n\r\n")+4);
				std::string rawheaders = connection.data.substr(0, connection.data.find("\r\n\r\n"));
				rawheaders = rawheaders.substr(rawheaders.find("\r\n") + 2);

				std::vector<std::string> headers = split(rawheaders, "\r\n");


				HttpRequest request;
				request.method = method;
				request.url = url;
				request.data = data;
				for (std::string &h : headers)
				{
					std::string headerName = h.substr(0, h.find(":"));
					std::transform(headerName.begin(), headerName.end(), headerName.begin(), ::tolower);
					std::string headerData = h.substr(h.find(":") + 1);
					request.headers[headerName] = trim(headerData);
				}
				
				if (request.headers.find("content-length") != request.headers.end())
				{
					int len = atoi(request.headers["content-length"].c_str());
					if (request.data.length() != len)
					{
						//printf("RestServer: Got a request for %s, but not enough data yet. Expected %i, got %i\n", url.c_str(), len, request.data.length());
						//printf("---\n%s\n---\n", connection.data.c_str());
						continue;
					}
					//else
					//	printf("RestServer: Got enough data: %s\n", request.data.c_str());
				}


				printf("RestServer: Got %s request for %s from %s, data '%s'\n", method.c_str(), url.c_str(), connection.ip.c_str(), request.data.c_str());


				bool handled = false;
				HttpResponse response;
				response.addHeader("Access-Control-Allow-Origin", "http://sensorcloud.borf.info");


				if(method == "OPTIONS")
				{
					response.setCode(200);
					response.addHeader("Access-Control-Allow-Methods", "GET,PUT,POST,OPTIONS");
					response.addHeader("Access-Control-Allow-Headers", "Content-Type");
					handled = true;
				}

				for(Handler& h : handlers)
				{
					if(h.path ==  url.substr(0, h.path.size()) && h.method == method)
					{
						h.callback(request, response);
						handled = true;
						break;
					}
				}
				if(!handled)
				{
					response.setCode(404);
					response.setBody("Sorry, file not found");
				}

				
				response.send(connection.s);

				closesocket(connection.s);
				connection.s = 0;
			}
		}
	}

	
}



std::vector<std::string> HttpRequest::splitUrl() const
{
	return split(url, "/");

}

json::Value HttpRequest::getPostData() const
{
	return json::readJson(data);
}



void RestServer::addHandler(const std::string &path, const std::string &method, const std::function<void(const HttpRequest&, HttpResponse&)> &callback)
{
	handlers.push_back(Handler(path, method, callback));
}


void HttpResponse::addHeader(const std::string &header, const std::string &value)
{
	headers[header] = value;
}
void HttpResponse::setBody(const std::string& value)
{
	this->body = value;
}
void HttpResponse::setCode(int code)
{
	this->code = code;
}


void HttpResponse::send(SOCKET s)
{
	std::string data = "HTTP/1.1 " + std::to_string(code) + "\r\n";
	for(auto h : headers)
		data += h.first + ": " + h.second + "\r\n";
	data += "\r\n" + body;

	::send(s, data.c_str(), data.size(), 0);

}

void HttpResponse::setJson(const json::Value& value)
{
	setCode(200);
	addHeader("Content-Type", "application/json");
	setBody(((std::stringstream&)(std::stringstream() << value)).str());
}

