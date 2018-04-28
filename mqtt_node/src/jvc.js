var util = require('util'),
    logger = require('winston'),
    net = require('net');

// http://support.jvc.com/consumer/support/documents/DILAremoteControlGuide.pdf

module.exports = class Jvc
{
    constructor(client, router)
    {
        this.ready = false;
        this.lastStatus = "";
        this.client = client;
        this.client.subscribe("projector/#");
        this.callback = null;

        var that = this;
        router.on(/projector\/power\/set$/i, function(client, topicmatch, payload)
        {
            if(payload == "1")
                that.send("PW1", function() { that.updatestatus(); });
            if(payload == "0")
                that.send("PW0", function() { that.updatestatus(); });
        });


        var socket = new net.Socket();
        this.socket = socket;
        socket.setEncoding("binary");
        socket.connect(20554, '192.168.2.39', function() {
        });
        socket.on('error', function()
        {
            console.error("error connecting to projector");
        });

        socket.on('connect', function()
        {
            logger.info("Connected to projector");
        });
        
        socket.on('data', function(data) {
            if(data == "PJ_OK")
                socket.write("PJREQ");
            else if(data == "PJACK")
            {
                logger.info("Projector connection ready");//ready
                //socket.write("\x21\x89\x01"+command+"\x0A", "latin1");
                that.ready = true;
                that.updatestatus();
            }
            else
            {
                //status reply: 06 89 01 50 57 0a 40 89 01 50 57 30 0a
                if(data.charCodeAt(0) == 0x6 && data.charCodeAt(1) == 0x89 && data.charCodeAt(2) == 0x1 && data.length > 11)
                {
                    var status = "error";
                    switch(data.charCodeAt(11))
                    {
                        case 0x30: status = "standby"; break;
                        case 0x31: status = "poweron"; break;
                        case 0x32: status = "cooling"; break;
                        case 0x34: status = "emergency"; break;
                        default: status = "Unknown: " + data.charCodeAt(11); break;
                    }
                    if(that.lastStatus != status)
                    {
                        that.client.publish("projector/power", status, { retain : true });
                        logger.info("Publishing new projector state");
                    }
                    that.lastStatus = status;
                }
                else
                {
                    logger.info("Got info from projector");
                    if(that.callback)
                        that.callback();
                    that.callback = null;
                }
            }


        });

    }

    start()
    {
        var that = this;
        setInterval(function() { that.updatestatus() }, 10000);
    }


//power: 21 89 01 50 57 30 0A  = PW0  PW1

    send(command, callback)
    {
        console.info("Sending " + command + " to projector");
        this.socket.write("\x21\x89\x01"+command+"\x0A", "latin1");
        this.callback = callback;
    }


//request: 3F 89 01 50 57 0A    
    updatestatus()
    {
        /*var that = this;
        var socket = new net.Socket();
        socket.connect(20554, '192.168.2.39', function() {
        });
        socket.setEncoding("binary");
        socket.on('error', function()
        {
            logger.error("Error while connecting to projector");
        })
        socket.on('data', function(data) {
            if(data == "PJ_OK")
                socket.write("PJREQ");
            else if(data == "PJACK")
                socket.write("\x3f\x89\x01PW\x0A", "latin1");
            else
            {
                var status = "error";
                switch(data[11])
                {
                    case "\x30": status = "standby"; break;
                    case "\x31": status = "poweron"; break;
                    case "\x32": status = "cooling"; break;
                    case "\x34": status = "emergency"; break;
                    default: status = "Unknown: " + data[11]; break;
                }
                if(that.lastStatus != status)
                    that.client.publish("projector/power/status", status, { retain : true });
                that.lastStatus = status;
                socket.destroy();
            }


        });*/

        if(this.ready)
            this.socket.write("\x3f\x89\x01PW\x0A", "binary");
        else
            logger.error("Trying to get projector status while not ready");

    }



};