## Topic description:

- /boot/whoami

  Sent from clients to server to request information about themselves. Server will respond with a message on the /boot/whoami/<hwid> topic Data:

  ```json
    {
        "hwid" : "id"
    }
  ```

- /boot/whoami/<id>

  Sent from server to client with information about this node. Data:

  ```json
    {
        "hwid" : "id",
        "name" : "",
        "id" : 0,
        "topic" : "",
        "sensors" :
        [
            {
                "id" : 0,
                "type" : 0,
                "config" : {}
            }
        ],
        "config" :
        {
            "display" : 0,
            "mode" : 0
        }
    }
  ```

    Type can be:

    - 1: DHT11, "config" : { "pin" : 0 }
    - 2: DHT21, "config" : { "pin" : 0 }
    - 3: 
    - 4: Switch, "config" :  {}
    - 5: AnalogSensor, "config" : {}
    - 8: ActuatorSwitch, "config" : {}
    - 9: ActuatorRGB, "config" : {}
- /boot/ping

  Sent from clients to server to post some information about themselves. Information includes heapspace and signal strength. Data:

  ```json
    {
        "id" : 0,
        "heapspace" : 0,
        "rssi" : 0
    }
  ```

- /boot/timeout

  Sent from clients to server when the client disconnects or times out. Data:

  ```json
    {
        "id" : 0
    }
  ```