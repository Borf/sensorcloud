var util = require('util'),
    eiscp = require('eiscp'),
    logger = require('winston');



module.exports = function(client, router)
{
 //   eiscp.on("debug", util.log);
    eiscp.on("error", util.log);

    var shuffle;
    var repeat;
    var playstatus;
    var artist = "";
    var title = "";
    var album = "";


    eiscp.on('volume', function (arg) {
        client.publish("onkyo/volume", arg + "", { retain : true });
    });

    eiscp.on('system-power', function (arg) {
        client.publish("onkyo/system-power", arg + "", { retain : true });
        if(arg == '00')
        {
            client.publish("onkyo/playtime", "", { retain : true }); 
            client.publish("onkyo/status/artist", "", { retain : true });
            client.publish("onkyo/status/album", "", { retain : true });
//            client.publish("onkyo/status/title", "", { retain : true });
            client.publish("onkyo/status", "stopped", { retain : true });
        }
    });

    eiscp.on('net-usb-time-info', function(arg)
    {
        client.publish("onkyo/playtime", arg + "", { retain : true });
    });

    eiscp.on('net-usb-artist-name-info', function(arg)
    {
        if(arg.trim() != "")
        {
            client.publish("onkyo/status/artist", arg.trim(), { retain : true });
            artist = arg.trim();
            logger.info("Got artist");
        }
    });
    eiscp.on('net-usb-album-name-info', function(arg)
    {
        if(arg.trim() != "")
        {
            client.publish("onkyo/status/album", arg.trim(), { retain : true });
            album = arg.trim();
            logger.info("Got album");
        }
    });
    eiscp.on('net-usb-title-name-info', function(arg)
    {
        //never called!?!?!?
        if(arg.trim() != "" && arg.length > 1)
        {
            client.publish("onkyo/status/title", arg.trim(), { retain : true });
            title = arg.trim();
            logger.info("Got title");
        }
    });
    
    eiscp.on('net-usb-play-status', function(arg)
    {
        if(playstatus != arg[0])
        {
            if(arg[0] == 'p')
                client.publish("onkyo/status", "paused", { retain : true });
            else if(arg[0] == 'P')
            {
                client.publish("onkyo/status", "playing", { retain : true });
                client.publish("log", "Playing " + artist + " - " + title + " (" + album + ")");
                logger.info("Got play");
            }
            else if(arg[0] == '-')
                client.publish("onkyo/status", "stopped", { retain : true });
        }

        if(repeat != arg[1])
        {
            if(arg[1] == '-')
                client.publish("onkyo/status/repeat", "no", { retain : true });
            else if(arg[1] == '1')
                client.publish("onkyo/status", "repeat single", { retain : true });
            else if(arg[1] == 'R')
                client.publish("onkyo/status", "repeat", { retain : true });
        }

        if(shuffle != arg[2])
        {
            if(arg[2] == '-')
                client.publish("onkyo/status/shuffle", "no", { retain : true });
            else if(arg[2] == 'S')
                client.publish("onkyo/status/shuffle", "shuffle", { retain : true });
        }

        playstatus = arg[0];
        repeat = arg[1];
        shuffle = arg[2];
    });

    

    eiscp.on('net-usb-title-name', function(arg)
    {
        if(arg.trim() != "" && arg.length > 1)
        {
            client.publish("onkyo/status/title", arg.trim(), { retain : true });
            title = arg.trim();
            logger.info("Got title");
        }
    });
    

    eiscp.on('connect', function () {
        logger.info("Connected to onkyo");      
        
        
        eiscp.get_commands('main', function (err, cmds) {

            console.log(cmds);
            cmds.forEach(function (cmd) {
                console.log(cmd);
                eiscp.get_command(cmd, function (err, values) {
                    console.log(values);
                });
            });
        });
    
    });
    eiscp.connect({host: "192.168.2.200", verify_commands : false});

    client.subscribe("onkyo/#");

    router.on(/onkyo\/volume\/set/i, function(client, result, data, packet)
    {
        try {       eiscp.command("volume " + data); } catch(e) { logger.info(e); }
    });

    router.on(/onkyo\/power\/set/i, function(client, result, data, packet)
    {
        logger.info("Setting power to '" + data + "'");
        try {        eiscp.command("system-power " + data); } catch(e) { logger.info(e); }
    });

    router.on(/onkyo\/action/i, function(client, result, data, packet)
    {
        if(data == "next")
            try {        eiscp.command("network-usb trup"); } catch(e) { logger.info(e); }
        if(data == "prev")
            try {        eiscp.command("network-usb trdn"); } catch(e) { logger.info(e); }
    });


}