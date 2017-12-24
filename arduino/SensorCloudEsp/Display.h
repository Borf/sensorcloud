#pragma once


class OLEDDisplayUi;
class OLEDDisplay;

class Display
{
  long displayOffTime;
  long displayScroll;

  int width = 0, height = 0;
public:
  int offX = 0, offY = 0;
  enum class Type
  {
    None = 0,
    D1MiniShield = 1,
    D1Mini = 2,
    Seperate = 3,    
  } type;

  OLEDDisplayUi* ui;
  OLEDDisplay* display;

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







extern Display display;

