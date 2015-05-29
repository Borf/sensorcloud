#pragma once

#include <string>
#include <functional>
#include <list>
#include <map>


typedef int SOCKET;
namespace json { class Value; }
class HttpResponse
{
	std::map<std::string, std::string> headers;
	std::string body;
	int code;
public:
	HttpResponse()
	{
		code = 200;
	}
	void addHeader(const std::string &header, const std::string &value);
	void setBody(const std::string& value);
	void setCode(int code);
	void setJson(const json::Value& value);

	void send(SOCKET s);
};


class RestServer
{
	class Connection
	{
	public:
		SOCKET s;
		std::string data;
		std::string ip;
		Connection(SOCKET s, const std::string &ip) { this->s = s; this->ip = ip; }
	};


	class Handler
	{
	public:
		std::string path;
		std::string method;
		std::function<void(HttpResponse& response)> callback;
		Handler(const std::string &path, const std::string &method, const std::function<void(HttpResponse&)> &callback)
		{
			this->path = path;
			this->method = method;
			this->callback = callback;
		}
	};

	std::list<Connection> connections;
	SOCKET s;

	std::list<Handler> handlers;

public:
	RestServer();
	void update();

	void addHandler(const std::string &path, const std::string &method, const std::function<void(HttpResponse&)> &callback);

};
