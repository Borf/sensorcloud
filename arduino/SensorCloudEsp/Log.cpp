#include "Log.h"

Log logger;



size_t Log::write(uint8_t data)
{
	if (doPrintTime)
		printTime();
	/* if(data == '\n')
	   index = (index + 1)%100;
	 else
	   lines[index] += (char)data;*/
	int ret = Serial.write(data);
	//serverClient.write(data);

	if (data == '\n')
		doPrintTime = true;


	return ret;
}

size_t Log::write(const uint8_t *buffer, size_t size)
{
	if (doPrintTime)
		printTime();
	/* String data((char*)buffer);
	 while(data.indexOf("\n") >= 0)
	 {
	   lines[index] += data.substring(0, data.indexOf("\n")-1);
	   index = (index + 1)%100;
	   data = data.substring(data.indexOf("\n")+1);
	 }
	 lines[index] += data;*/

	int ret = Serial.write(buffer, size);
	//serverClient.write(buffer, size);
	if (buffer[size-1] == '\n' || buffer[size-2] == '\n')
		doPrintTime = true;

	return ret;
}

void Log::printTime()
{
	doPrintTime = false;
	time_t now = time(nullptr);
	/*tm* lt = localtime(&now);
	char buf[64];

	print(strftime(buf, 64, "%c", lt));*/

	char* timestr = asctime(localtime(&now));
	timestr[strlen(timestr) - 1] = 0;
	print(timestr);
	print("\t");
}

void Log::setType(int type)
{
	currentType = type;
}

