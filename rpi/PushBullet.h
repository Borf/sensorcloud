#pragma once

#include <thread>
#include <mutex>
#include <functional>
#include <vector>

namespace json {
	class Value;
}

class PushBullet
{
	std::thread thread;
	std::mutex mutex;
	bool running;
	void threadFunc();
	std::string key;

public:
	class Job
	{
	public:
		std::string url;
		std::string method;
		std::string data;
		std::string result;
		std::function<void(const json::Value&)> callback;
		bool done;

		Job()
		{
			callback = [](const json::Value&) {};
			done = false;
		}
	};

	std::vector<Job*> jobs;

public:
	PushBullet(const json::Value& config);
	~PushBullet();

	void update();

	const Job& getDevices(std::function<void(const std::vector<std::string> &)>);
	const Job& sendMessage(const std::string &title, const std::string &message, const std::function<void(const json::Value&)> &callback = [](const json::Value& v){});
};