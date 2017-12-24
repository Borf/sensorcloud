#pragma once

#include <functional>
#include <algorithm>
#include <list>
#include <time.h>

class Timer
{
  class Callback
  {
  public:
    std::function<void()> callback;
    int delay;
    time_t fireTime;

	Callback(const std::function<void()>& callback, int delay, bool repeat);
  };
  
  std::list<Callback> callbacks;

public:
  Timer();

  void begin();
  void update();

  void addCallback(int delay, const std::function<void()> &callback, bool repeat = false);
 
};


extern Timer timer;
