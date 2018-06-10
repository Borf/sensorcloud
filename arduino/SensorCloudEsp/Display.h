#pragma once

class DisplayDriver;
class Graphics;
class Transition;


class Display
{
  long displayOffTime;
  long displayScroll;
public:
  enum class Type
  {
    None = 0,
    D1MiniShield = 1,
    D1Mini = 2,
    Seperate = 3,    
  } type;

  DisplayDriver* display;
  Graphics* graphics;
  Transition* transition;

  void begin(Type type);
  void update();


  void resetTimeout();

  void bootStart();
  void bootConnecting();
  void bootConnected();
  void bootBooted(); 

  void setTextAlignment(int i);
  void setFont(const char* i);
  void drawString(int x, int y, const String &text);
  void drawStringMaxWidth(int x, int y, int width, const String &text);
  void clear();
  void swap();
};






