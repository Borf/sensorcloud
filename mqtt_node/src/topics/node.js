var util = require('util'),
    db = require('../db.js'),
    logger = require('winston'),
    telegram = require('../telegram'),
    request = require('request'),
    sharp = require('sharp');

module.exports = function(nodeid, router)
{
    return function(client, topicmatch, payload)
    {
        switch(topicmatch[2])
        {
            case "ping":
                console.log("Ping: " + topicmatch[1]);
                db.getConnection(function(err, connection)
                {
                    if(err)
                        throw err;
                    connection.query('INSERT INTO `pings` (`stamp`, `nodeid`, `heapspace`, `rssi`, `ip`) VALUES (NOW(), ?, ?, ?, ?)', [ nodeid, payload.heapspace, payload.rssi, payload.ip ], function (error, results, fields) {
                        connection.release();
                    });
                });

                break;

            case "temperature":
            case "humidity":
            console.log(topicmatch[2] + " at " + topicmatch[1] + " is " + payload);
                db.getConnection(function(err, connection)
                {
                    if(err)
                        throw err;
                    connection.query('INSERT INTO `sensordata` (`stamp`, `nodeid`, `type`, `value`) VALUES (NOW(), ?, ?, ?)', [ nodeid, topicmatch[2], payload ], function (error, results, fields) {
                        connection.release();
                    });
                });
        
                break;
            case "switch":
                console.log(topicmatch[2] + " at " + topicmatch[1] + " is " + payload);
                if(payload == "1")
                {
                    router.on(/hallway\/camera\/image/i, (client, result, payload, packet) =>
                    {
                        sharp(payload).rotate(180).toBuffer().then(buffer => telegram.sendPhoto(buffer, "Someone's at the door"));
                        router.remove(/hallway\/camera\/image/i);
                    });
                    client.subscribe('hallway/camera/image');
                    client.publish("hallway/camera/snapshot", "1");
                }
                break;
            default:
                console.log("Unhandled message from a node: ");
                console.log(util.inspect(topicmatch));
                console.log(payload);
        
            }


    }
}