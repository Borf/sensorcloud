#include "RotaryKnob.h"

#include <ESP8266WiFi.h>


int RotaryKnob::rotaryCount = 0;
RotaryKnob* RotaryKnob::knobs[3];




void RotaryKnob::begin(int pinA, int pinB, int buttonPin)
{
  this->pinA = pinA;
  this->pinB = pinB;
  this->buttonPin = buttonPin;
  this->id = rotaryCount++;
  knobs[id] = this;

  if(buttonPin != D8)
    pinMode(buttonPin, INPUT_PULLUP);
  else
    pinMode(buttonPin, INPUT);
  pinMode(pinA, INPUT_PULLUP);
  pinMode(pinB, INPUT_PULLUP);

  //ugh, nasty
  switch(id)
  {
    case 0:     
      attachInterrupt(buttonPin, onButton<0>, buttonPin != D8 ? FALLING : RISING);
      attachInterrupt(pinA, onPinA<0>, FALLING);
      attachInterrupt(pinB, onPinB<0>, FALLING);
      break;
    case 1:     
      attachInterrupt(buttonPin, onButton<1>, buttonPin != D8 ? FALLING : RISING);   
      attachInterrupt(pinA, onPinA<1>, FALLING);
      attachInterrupt(pinB, onPinB<1>, FALLING);
      break;
    case 2:     
      attachInterrupt(buttonPin, onButton<2>, buttonPin != D8 ? FALLING : RISING);   
      attachInterrupt(pinA, onPinA<2>, FALLING);
      attachInterrupt(pinB, onPinB<2>, FALLING);
      break;
  }
  
}


void RotaryKnob::update()
{
  if(digitalRead(buttonPin) == (buttonPin != D8 ? LOW : HIGH))
    lastButtonTime = millis() + 50;
  if(digitalRead(pinA) == LOW || digitalRead(pinB) == LOW)
    lastRotaryTime = millis() + 15;


}


void RotaryKnob::handleButton()
{
  long currentTime = millis();
  if(lastButtonTime < currentTime && onButtonPress)
  {
    onButtonPress();
  }
  lastButtonTime = currentTime + 50;
}


void RotaryKnob::handlePinA()
{
  int currentTime = millis();
  if(lastRotaryTime < currentTime && digitalRead(pinB) == HIGH)
    onRotation(1);
  lastRotaryTime = currentTime + 15;
}


void RotaryKnob::handlePinB()
{
  int currentTime = millis();
  if(lastRotaryTime < currentTime && digitalRead(pinA) == HIGH)
    onRotation(-1);
  lastRotaryTime = currentTime + 15;
}

