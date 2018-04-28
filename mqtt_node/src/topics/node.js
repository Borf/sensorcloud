var util = require('util'),
    db = require('../db.js'),
    logger = require('winston');

module.exports = function(nodeid)
{
    return function(client, topicmatch, payload)
    {

        switch(topicmatch[2])
        {
            case "ping":
                logger.info("Ping: " + topicmatch[1]);
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
                logger.info(topicmatch[2] + " at " + topicmatch[1] + " is " + payload);
                db.getConnection(function(err, connection)
                {
                    if(err)
                        throw err;
                    connection.query('INSERT INTO `sensordata` (`stamp`, `nodeid`, `type`, `value`) VALUES (NOW(), ?, ?, ?)', [ nodeid, topicmatch[2], payload ], function (error, results, fields) {
                        connection.release();
                    });
                });
        
                break;

            default:
                logger.log("Unhandled message from a node: ");
                logger.log(util.inspect(topicmatch));
                logger.log(payload);
        
            }


    }
}