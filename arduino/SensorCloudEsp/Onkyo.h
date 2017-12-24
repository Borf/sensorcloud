#pragma once

#include <vector>
#include <ESP8266WiFi.h>
#include "IPAddress.h"


class Onkyo
{
  WiFiClient client;

  void write(String data);
  IPAddress ip;
  int port;

  void connect();
  long lastConnectionAttempt;
  bool poweredOn = false;

public:
  void begin(IPAddress ip, int port);
  void update();

  bool connected;


  String playTime;
  String totalTime;
  String title;
  String artist;
  int volume = 0;

  bool shuffle;
  enum class PlayStatus
  {
    PLAYING,
    PAUSED,
    STOPPED    
  } playStatus;

  long volumeChangeTime;
  long volumeDelay;

  void setVolume(int newVolume);

  void setPower(bool power);
  bool isPoweredOn() { return poweredOn; }

  int playTimeSeconds();
  int totalTimeSeconds();

  String menuTitle;
  String menuItems[10];
  int menuCursor;
  int menuItemCount = 5;


  void play()     {    write("NTCPLAY");  }
  void pause()    {    write("NTCPAUSE"); }
  void stop()     {    write("NTCPAUSE"); }
  void nextTrack(){    write("NTCTRUP"); }
  void prevTrack(){    write("NTCTRDN"); }


  inline IPAddress &getIp() { return ip; }
};

extern Onkyo onkyo;
