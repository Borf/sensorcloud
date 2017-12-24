#include "Timer.h"
#include "Log.h"
#include <ESP8266WiFi.h>
#include <Time.h>

Timer timer;
Timer::Timer() {}

Timer::Callback::Callback(const std::function<void()>& callback, int delay, bool repeat)
{
	this->callback = callback;
	this->fireTime = time(nullptr) + delay;
	this->delay = delay;
	if (!repeat)
		delay = 0;
}


void Timer::begin()
{
	configTime(2 * 3600, 3600, "192.168.2.202", "93.94.224.67");
	logger.print("Timer\tWaiting for time");
	while (!time(nullptr)) {
		logger.print(". ");
		delay(1000);
	}
	logger.println(" got time :)");

}



void Timer::addCallback(int delay, const std::function<void()> &callback, bool repeat)
{
	callbacks.push_back(Callback(callback, delay, repeat));
}


void Timer::update()
{
	time_t now = time(nullptr);
	for (std::list<Callback>::iterator it = callbacks.begin(); it != callbacks.end(); it++)
	{
		if (it->fireTime < now)
		{
			it->callback();
			if (it->delay == 0)
			{
				it = callbacks.erase(it);
				it--;
				continue;
			}
			else
				it->fireTime = now + it->delay;
		}

	}




}
