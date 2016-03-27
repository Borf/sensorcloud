class Log : public Print
{
  int currentType;
  
public:
  int index = 0;
  String lines[100];

  
  void begin()
  {
    
  }

  
  size_t write(uint8_t data)
  {
    if(data == '\n')
      index++;
    else
      lines[index] += (char)data;
    Serial.write(data);
  }

  size_t write(const uint8_t *buffer, size_t size)
  {
    String data((char*)buffer);
    while(data.indexOf("\n") >= 0)
    {
      lines[index] += data.substring(0, data.indexOf("\n")-1);
      index++;
      data = data.substring(data.indexOf("\n")+1);
    }
    lines[index] += data;
        
    Serial.write(buffer, size);    
  }

  void setType(int type)
  {
    currentType = type;  
  }
  
  
};


