#include "Onkyo.h"
#include <ESP8266WiFi.h>

Onkyo onkyo;

//#if BYTE_ORDER == LITTLE_ENDIAN
//#define NTOHL(n) (n)
//#else
#define NTOHL(n) ((((n & 0xFF))       << 24) | \
              (((n & 0xFF00))     << 8)  | \
              (((n & 0xFF0000))   >> 8)  | \
              (((n & 0xFF000000)) >> 24))
//#endif



void Onkyo::begin(IPAddress ip, int port)
{
  this->ip = ip;
  this->port = port;
  connect();
}


void Onkyo::connect()
{
  connected = false;
  if(WiFi.status() != WL_CONNECTED)
  {
    lastConnectionAttempt = millis() - 9000;
    return;
  }
  if(millis() - lastConnectionAttempt < 10000)
    return;
  lastConnectionAttempt = millis();

  
  if(!client.connect(ip, port))
  {
    Serial.println("Connection to onkyo receiver failed");  
    Serial.print("Connecting to ");
    Serial.print(ip);
    Serial.print(":");
    Serial.println(port);
    return;
  }
  Serial.println("Connected to onkyo receiver");

  //write("SPAQSTN");
  //write("SPBQSTN");
  write("PWRQSTN");
  write("ATMQSTN");
  write("MVLQSTN");
  write("SLIQSTN");
  volumeChangeTime = millis()-10000;
  connected = true;
}


struct Header
{
  char magic[4];
  int32_t headerSize;
  int32_t packetSize;
  uint8_t flags[4];  
};

void Onkyo::update()
{
  if(!connected)
  {
    connect();
    return;
  }
  if(!client.connected())
  {
    connected = false;
    return;
  }
  if(client.available() > 16)
  {
    Header header;
    int rc = client.read((uint8_t*)&header, sizeof(Header));
    header.packetSize = NTOHL(header.packetSize);
    header.headerSize = NTOHL(header.headerSize);
    if(header.magic[0] != 'I' || header.magic[1] != 'S' || header.magic[2] != 'C' || header.magic[3] != 'P')
    {
      Serial.println("Error reading header magic");
      while(client.available())
        client.read();
      return;
    }

    if(header.packetSize > 1000)
    {
      Serial.print("Large packet size: ");
      Serial.println(header.packetSize);
    }
    if(header.headerSize != 16)
      Serial.println("Invalid header size");

    client.read(); //!
    client.read(); //1

    header.packetSize-=2;

    uint8_t* data = new uint8_t[header.packetSize+1];
    data[header.packetSize] = 0;
    int offset = 0;
    while(offset < header.packetSize)
    {
      int rc = client.read(data + offset, header.packetSize-offset);
      offset += rc;
      if(offset < header.packetSize)
      {
        Serial.print("Read ");
        Serial.print(rc);
        Serial.print(", total ");
        Serial.print(header.packetSize);
        Serial.print(", togo: ");
        Serial.println(header.packetSize-offset);
        delay(1);
      }
    }
    String line((char*)data);
    delete[] data;
    
    String command = line.substring(0, 3);
    String parameter = line.substring(3);
    parameter.trim();
    Serial.println(command);
    if(command == "NTM")
    {
      playTime = parameter.substring(0, parameter.indexOf('/'));
      totalTime = parameter.substring(parameter.indexOf('/')+1);
    }
    else if(command == "NTI") //Net Usb Title Name
    {
      title = parameter;      
      if(title[title.length()-1] < 32)
        title = title.substring(0, title.length()-1);
      Serial.print("Title: ");
      Serial.println(title);
    }
    else if(command == "MVL") // master volume
    { //volume is a hex
      if(millis() - volumeChangeTime > 5000) //only update if not changed in the last 5 seconds
      {
        volume =  (parameter[0] <= '9' ? (parameter[0] - '0') : (parameter[0] - '7')) * 16 +
                  (parameter[1] <= '9' ? (parameter[1] - '0') : (parameter[1] - '7'));
      }
    }
    else if(command == "PWR") // power status
    {
      Serial.print("Power: ");
      Serial.println(parameter);
      if(parameter[0] == '0' && parameter[1] == '0')
        poweredOn = false;
      else
        poweredOn = true; // dunno what the other value is, can't test it in China
    }
    else if(command == "NJA") {} // album art
    else if(command == "NMS") {}// unknown
    else if(command == "NTR") {}// track number
    else if(command == "NAL") {}// album
    else if(command == "NAT") 
	{
		artist = parameter;
		artist.trim();
	}// artist
    else if(command == "NST") 
    {// USB Status
    }
    else if(command == "DIM") {}// unknown, parameter is 02
    else if(command == "RAS") {}// Cinema filter. 00 is off, 01 is on
    else if(command == "AMT") {}// Audio Mute (00 = off, 01 = on, muted)
    else if(command == "SLI") {}// unknown
    else if(command == "NLT") 
    {// NET/USB title info: 
// {xx}uycccciiiillrraabbssnnn...nnn
//  xx : Service Type\n 00 : DLNA, 01 : Favorite, 02 : vTuner, 03 : SiriusXM, 04 : Pandora, 05 : Rhapsody, 06 : Last.fm,\n 07 : Napster, 08 : Slacker, 09 : Mediafly, 0A : Spotify, 0B : AUPEO!, 0C : radiko, 0D : e-onkyo,\n 0E : TuneIn Radio, 0F : MP3tunes, 10 : Simfy, 11:Home Media, 12:Deezer, 13:iHeartRadio,\n F0 : USB Front, F1 : USB Rear, F2 : Internet Radio, F3 : NET, FF : None\n
//  u : UI Type\n 0 : List, 1 : Menu, 2 : Playback, 3 : Popup, 4 : Keyboard, "5" : Menu List
//  y : Layer Info\n 0 : NET TOP, 1 : Service Top,DLNA/USB/iPod Top, 2 : under 2nd Layer
//  cccc : Current Cursor Position (HEX 4 letters)
//  iiii : Number of List Items (HEX 4 letters)
//  ll : Number of Layer(HEX 2 letters)
//  rr : Reserved (2 leters)
//  aa : Icon on Left of Title Bar\n 00 : Internet Radio, 01 : Server, 02 : USB, 03 : iPod, 04 : DLNA, 05 : WiFi, 06 : Favorite\n 10 : Account(Spotify), 11 : Album(Spotify), 12 : Playlist(Spotify), 13 : Playlist-C(Spotify)\n 14 : Starred(Spotify), 15 : What\'s New(Spotify), 16 : Track(Spotify), 17 : Artist(Spotify)\n 18 : Play(Spotify), 19 : Search(Spotify), 1A : Folder(Spotify)\n FF : None
//  bb : Icon on Right of Title Bar\n 00 : DLNA, 01 : Favorite, 02 : vTuner, 03 : SiriusXM, 04 : Pandora, 05 : Rhapsody, 06 : Last.fm,\n 07 : Napster, 08 : Slacker, 09 : Mediafly, 0A : Spotify, 0B : AUPEO!, 0C : radiko, 0D : e-onkyo,\n 0E : TuneIn Radio, 0F : MP3tunes, 10 : Simfy, 11:Home Media, 12:Deezer, 13:iHeartRadio,\n FF : None
//  ss : Status Info\n 00 : None, 01 : Connecting, 02 : Acquiring License, 03 : Buffering\n 04 : Cannot Play, 05 : Searching, 06 : Profile update, 07 : Operation disabled\n 08 : Server Start-up, 09 : Song rated as Favorite, 0A : Song banned from station,\n 0B : Authentication Failed, 0C : Spotify Paused(max 1 device), 0D : Track Not Available, 0E : Cannot Skip
//  nnn...nnn : Character of Title Bar (variable-length, 64 Unicode letters [UTF-8 encoded] max)'}),
      Serial.print("NLT: ");
      Serial.println(parameter);   
      menuTitle = parameter.substring(22);
      menuTitle.trim(); 
    }
    else if(command == "NLS") 
    {// NET / USB list info
// tlpnnnnnnnnnn  
// t ->Information Type (A : ASCII letter, C : Cursor Info, U : Unicode letter)
//  when t = A
//   l ->Line Info (0-9 : 1st to 10th Line)
//   nnnnnnnnn:Listed data (variable-length, 64 ASCII letters max)
// when AVR is not displayed NET/USB List(Keyboard,Menu,Popup), 
//  "nnnnnnnnn" is "See TV".
//   p ->Property\n         - : no\n         0 : Playing, A : Artist, B : Album, F : Folder, M : Music, P : Playlist, S : Search\n         a : Account, b : Playlist-C, c : Starred, d : Unstarred, e : What\'s New\n
// when t = C,
//   l ->Cursor Position (0-9 : 1st to 10th Line, - : No Cursor)
// p ->Update Type (P : Page Information Update ( Page Clear or Disable List Info) , C : Cursor Position Update)
// when t = U, (for Network Control Only)\n  
//   l ->Line Info (0-9 : 1st to 10th Line)\n  nnnnnnnnn:Listed data (variable-length, 64 Unicode letters [UTF-8 encoded] max)
// when AVR is not displayed NET/USB List(Keyboard,Menu,Popup\u2026), "nnnnnnnnn" is "See TV".\n  
// p ->Property\n         - : no\n         0 : Playing, A : Artist, B : Album, F : Folder, M : Music, P : Playlist, S : Search\n         a : Account, b : Playlist-C, c : Starred, d : Unstarred, e : What\'s New'}),
      if(parameter[0] == 'C')
      {
        menuCursor = (parameter[1] - '0');
        Serial.print("Cursor: ");
        Serial.println(menuCursor);
      }
      else if(parameter[0] == 'U')
      {
        menuItems[parameter[1] - '0'] = parameter.substring(2);
      }
      else
      {
        Serial.print("NLS: ");
        Serial.println(parameter);
      }
    }
    else if(command == "NLU") {}// unknown
    else
    {
      Serial.print("Unknown command: ");
      Serial.println(command);
      Serial.print("Line: ");
      Serial.println(line);
      for(int i = 0; i < line.length(); i++)
      {
        Serial.print(line[i], HEX);
        Serial.print(" ");
      }

      for(int i = 0; i < line.length(); i++)
      {
        if(line[i] < 32)
          Serial.print(".");
        else
          Serial.print(line[i]);
      }
      
      Serial.println("");
    }

  }
}



void Onkyo::setVolume(int newVolume)
{
  volume = newVolume;
  String hexValue = String((volume>>4) & 0xF, HEX) + String(volume & 0xF, HEX);
  hexValue.toUpperCase();  
  write(String("MVL") + hexValue);
  volumeChangeTime = millis();  
}

void Onkyo::setPower(bool power)
{
  if(power)
    write("PWR01");
  else
    write("PWR00");
}


int Onkyo::playTimeSeconds()
{
  int h = playTime.substring(0,2).toInt();
  int m = playTime.substring(3,5).toInt();
  int s = playTime.substring(6,8).toInt();
  return s + m*60 + h*60*60;
}

int Onkyo::totalTimeSeconds()
{
  int h = totalTime.substring(0,2).toInt();
  int m = totalTime.substring(3,5).toInt();
  int s = totalTime.substring(6,8).toInt();
  return s + m*60 + h*60*60;
}


void Onkyo::write(String data)
{
  // Header format is:
  // - magic: 4 bytes of ASCII characters 'ISCP';
  // - header size: unsigned integer 16;
  // - message size: unsigned integer;
  // - version: byte 1;
  // - reserved: 3 bytes 0.
  // Integers are 32 bit big-endian. There's no padding.

  client.write("ISCP");
  client.write((uint8_t)0);
  client.write((uint8_t)0);
  client.write((uint8_t)0);
  client.write((uint8_t)16);

  uint32_t len = data.length()+3;
  len = NTOHL(len);
  client.write((uint8_t*)&len, 4);
  client.write((uint8_t)1);
  client.write((uint8_t)0);
  client.write((uint8_t)0);
  client.write((uint8_t)0);

  client.write('!');
  client.write('1');

  client.write(data.c_str(), data.length());
  client.write((uint8_t)'\x0d');
  Serial.print("Sent ");
  Serial.println(data);
}

