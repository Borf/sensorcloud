#include <ESP8266WiFi.h>
#include "Settings.h"
#include "Log.h"
#include <EEPROM.h>

Settings settings;

bool Settings::loadFromEeprom()
{
	int index = 0;
	crc = EEPROM.read(index++);

	id = EEPROM.read(index++);
	int nameLen = EEPROM.read(index++);
	if (nameLen < 0 || nameLen > 32)
	{
		logger.println("SETTING\tInvalid name in EEPROM");
		logger.print("SETTING\tName length is ");
		logger.println(nameLen);
		reset();
		return false;
	}
	name = "";
	for (int i = 0; i < nameLen; i++)
	{
		char c = EEPROM.read(index++);
		name += c;
	}

	mode = (Mode)EEPROM.read(index++);
	display = (DisplayType)EEPROM.read(index++);

	logger.println("SETTING\tFound name in EEPROM: " + name);

	int crcCalc = calcCrc();
	if (crcCalc != crc)
	{
		logger.print("SETTING\tInvalid CRC. Found ");
		logger.print(crc);
		logger.print(" but should be ");
		logger.println(crcCalc);
		return false;
	}
	return true;
}

void Settings::reset()
{
	mode = Mode::Sensor;
	display = DisplayType::None;
	id = 0;
	name = "";
	crc = -1;
}


bool Settings::saveToEeprom()
{
	int index = 0;
	unsigned char newCrc = calcCrc();
	if (crc == newCrc)
	{
		logger.println("SETTING\tNothing changed, not saving to eeprom");
		return false;
	}
	logger.println("SETTING\tSaving settings to eeprom");
	crc = newCrc;

	EEPROM.write(index++, crc);
	EEPROM.write(index++, id);
	EEPROM.write(index++, name.length());
	for (size_t i = 0; i < name.length(); i++)
		EEPROM.write(index++, name[i]);

	EEPROM.write(index++, (int)mode);
	EEPROM.write(index++, (int)display);

	logger.print("SETTING\tCRC calculated: ");
	logger.println(crc);

	EEPROM.commit();
	return true;
}



unsigned char Settings::calcCrc()
{
	int crcCalc = 0;
	//  for(int i = 0; i < sizeof(Settings); i++)
	//    crcCalc ^= ((unsigned char*)&settings)[i];

	crcCalc ^= id;
	for (size_t i = 0; i < name.length(); i++)
		crcCalc ^= name[i];

	crcCalc ^= (int)mode * 37;
	crcCalc = (crcCalc * 37) ^ (int)display;

	return crcCalc;
}

