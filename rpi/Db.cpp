#include "Db.h"
#include "json.h"
#include <cstddef>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>



Db::Db(const json::Value &config)
{
	mysql = mysql_init(NULL);

	if (mysql_real_connect(mysql, 
			config["host"].asString().c_str(), 
			config["user"].asString().c_str(), 
			config["pass"].asString().c_str(), 
			config["daba"].asString().c_str(), 0, NULL, 0) == NULL) 
	{
		fprintf(stderr, "Could not connect to mysql\n%s\n", mysql_error(mysql));
		mysql_close(mysql);
		exit(1);
	}
	running = true;
	thread = std::thread(std::bind(&Db::threadFunc, this));
}

Db::~Db()
{
	running = false;
	thread.join();
}

void Db::update()
{
	mutex.lock();
	for(int i = 0; i < (int)queryObjects.size(); i++)
	{
		if(queryObjects[i]->result)
		{
			mutex.unlock();
			queryObjects[i]->onQuery(queryObjects[i]->result);
			MYSQL_ROW row;
			while((row = mysql_fetch_row(queryObjects[i]->result)))
			{
				queryObjects[i]->onRow(row);
			}
			mutex.lock();
			mysql_free_result(queryObjects[i]->result);
		}


		if(queryObjects[i]->canBeDeleted)
		{
			mutex.unlock();
			queryObjects[i]->callThen();
			mutex.lock();
			delete queryObjects[i];
			queryObjects.erase(queryObjects.begin()+i);
			i--;
		}
	}

	mutex.unlock();
}

void Db::threadFunc()
{
	while(running)
	{
		mutex.lock();
		std::vector<QueryObject*> objects = queryObjects;
		mutex.unlock();

		for(auto q : objects)
		{
			if(!q->result && !q->canBeDeleted)
			{
				if(mysql_query(mysql, q->query.c_str()))
				{
					printf("Db: Error running query \"%s\", %s\n", q->query.c_str(), mysql_error(mysql));
					q->canBeDeleted = true;
				}
				q->result = mysql_store_result(mysql);
				q->canBeDeleted = true;
			}
		}




		usleep(100);
	}
}


QueryObject& Db::query(const std::string &query, 
	const std::function<void(MYSQL_ROW)> &onRow, 
	const std::function<void(MYSQL_RES*)> &onQuery)
{
	printf("Db: Running query %s\n", query.c_str());

	QueryObject* o = new QueryObject();
	o->query = query;
	o->onRow = onRow;
	o->onQuery = onQuery;

	mutex.lock();
	queryObjects.push_back(o);
	mutex.unlock();

	return *o;
}
