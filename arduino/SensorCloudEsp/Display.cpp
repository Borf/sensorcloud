#include <ESP8266WiFi.h>
#include "Settings.h"
#include "Display.h"
#include "Log.h"

#include <Transition.h>
#include <Graphics.h>
#include <SSD1306.h>
#include <SH1106.h>


void Display::begin(Type type)
{
	display = nullptr;
	graphics = nullptr;
	transition = nullptr;

	logger.print("DISPLAY\tDisplay: ");
	logger.println((int)type);

	if (type == Type::D1MiniShield)
		display = new SSD1306(0x3c, SDA, SCL);
	else if (type == Type::D1Mini)
		display = new SH1106(0x3c, D1, D2);

	if (display)
	{
		logger.print("DISPLAY\tdisplay");
		logger.println((int)type);
		graphics = new Graphics(display);

		if (settings.mode == Settings::Mode::Sensor)
		{
			logger.println("DISPLAY\tSensor type");
			transition = new Transition(*graphics);

		/*	ui->setTargetFPS(30);
			ui->setFrames(frames, 1);
			ui->disableAutoTransition();
			ui->init();*/
		}
		else if (settings.mode == Settings::Mode::OnkyoRemote)
		{
			logger.println("DISPLAY\tOnkyoRemote type");
		}
		else if (settings.mode == Settings::Mode::OnkyoDisplay) //only displays title
		{
			logger.println("DISPLAY\tOnkyoDisplay type");
		}
		/*display->flipScreenVertically();
		display->setTextAlignment(TEXT_ALIGN_LEFT);
		display->setFont(ArialMT_Plain_10);
		display->clear();
		display->drawString(0, 0, "Booting...");
		display->display();*/
		displayOffTime = 10000;
	}
	else
	{
		logger.println("DISPLAY\tno display");
	}

}


void Display::resetTimeout()
{
	if (!display)
		return;
	if (displayOffTime <= 0)
		display->setPower(true);
	displayOffTime = 3000;
}

void Display::update()
{
	if (!display)
		return;

	static long lastMillis = millis();
	long currentTime = millis();
	if (displayOffTime > 0)
	{
		displayOffTime -= currentTime - lastMillis;
		if (displayOffTime <= 0)
		{
			display->setPower(false);
			displayOffTime = 0;
		}
	}
	lastMillis = currentTime;


}



void Display::bootStart()
{
	if (!display)
		return;
/*	display->clear();
	display->drawString(offX, offY, "Booting...");
	display->display();*/
	delay(1000);
}

void Display::bootConnecting()
{
	if (!display)
		return;
/*	display->clear();
	display->drawString(offX, offY, "Connecting to wifi...");
	display->display();*/
}

void Display::bootConnected()
{
	if (!display)
		return;
/*	display->clear();
	display->drawString(offX, offY, "Connected to wifi...");
	display->display();*/
}

void Display::bootBooted()
{
	if (!display)
		return;
/*	display->clear();
	display->drawString(offX, offY, "Done booting");
	display->display();*/
}

void Display::setTextAlignment(int i) {/* if (display) display->setTextAlignment((OLEDDISPLAY_TEXT_ALIGNMENT)i);*/ }
void Display::setFont(const char* i) { /*if (display) display->setFont(i);*/ }
void Display::drawString(int x, int y, const String & text) { /*if (display) display->drawString(x + offX, y + offY, text);*/ }
void Display::drawStringMaxWidth(int x, int y, int maxWidth, const String & text) { /*if (display) display->drawStringMaxWidth(x + offX, y + offY, maxWidth, text); */}
void Display::clear() {
	if (!display)
		return;
//	display->clear();
}
void Display::swap() {
	if (!display)
		return;
	//display->display();
}


