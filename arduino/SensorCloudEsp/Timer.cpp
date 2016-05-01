#include "Timer.h"
#include <ESP8266WiFi.h>
#include <Time.h>

Timer::Timer() {}
void Timer::begin()
{
  configTime(2 * 3600, 3600, "192.168.2.204", "pool.ntp.org");
  Serial.print("Timer\tWaiting for time");
  while (!time(nullptr)) {
    Serial.print(". ");
    delay(1000);
  }
  Serial.println(" got time :)");

}



void Timer::addCallback(int delay, const std::function<void()> &callback, bool repeat)
{
  callbacks.push_back(Callback(callback, delay, repeat));
}


void Timer::update()
{
    time_t now = time(nullptr);

    for(std::list<Callback>::iterator it = callbacks.begin(); it != callbacks.end(); it++)
    {
      if(it->fireTime < now)
      {
        it->callback();
        if(it->delay == 0)
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

