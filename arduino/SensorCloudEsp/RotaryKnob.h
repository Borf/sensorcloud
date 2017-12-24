#pragma once

#include <functional>

class RotaryKnob
{
  int pinA;
  int pinB;
  int buttonPin;
  int id;

  long lastButtonTime;
  long lastRotaryTime;

  static int rotaryCount;
  static RotaryKnob* knobs[3];
  template<int index> static void onButton(){  RotaryKnob::knobs[index]->handleButton(); }
  template<int index> static void onPinA(){  RotaryKnob::knobs[index]->handlePinA(); }
  template<int index> static void onPinB(){  RotaryKnob::knobs[index]->handlePinB(); }

  void handleButton();
  void handlePinA();
  void handlePinB();
  
public:
  std::function<void(int)> onRotation;    
  std::function<void()> onButtonPress;


  void begin(int pinA, int pinB, int pinButton);
  void update();
};


