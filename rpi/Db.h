#pragma once

#include <mysql.h>
#include <thread>
#include <mutex>
#include <vector>
#include <functional>

namespace json { class Value; }

class QueryObject
{
public:
	MYSQL_RES* result;
	bool canBeDeleted;
	QueryObject() { result = NULL; canBeDeleted = false; onDone = []() {};  }

	std::string query;
	std::function<void(MYSQL_ROW)> onRow;
	std::function<void(MYSQL_RES*)> onQuery;
	std::function<void()> onDone;
	
	void then(const std::function<void()> &onDone) { this->onDone = onDone; };
	void callThen() { onDone(); };
};

class Db
{
private:
	MYSQL* mysql;
	std::thread thread;
	std::mutex mutex;
	bool running;

	std::vector<QueryObject*> queryObjects;

	void threadFunc();

public:
	Db(const json::Value &config);
	~Db();

	QueryObject& query(const std::string &query, 
		const std::function<void(MYSQL_ROW)> &onRow = [](MYSQL_ROW row){}, 
		const std::function<void(MYSQL_RES*)> &onQuery = [](MYSQL_RES* res){}); 

	void update();

};