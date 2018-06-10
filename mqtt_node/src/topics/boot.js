var db = require('../db.js'),
    util = require('util'),
    logger = require('winston');


module.exports = function(router)
{
    router.on(/boot\/whoami/i, function(client, topicmatch, payload)
    {
        var hwid = payload.hwid;
        logger.info("Got a whoami for ID " + hwid);
        db.getConnection(function(err, connection)
        {
            if(err)
                throw err;

            connection.query('SELECT `nodes`.*, `rooms`.`topic` as `roomtopic` FROM `nodes` LEFT JOIN `rooms` ON `nodes`.`room` = `rooms`.`id` WHERE `hwid` = ?', [ hwid ], function (error, results, fields) {
                if(err)
                {
                    console.log("Error " + err);
                    client.publish('boot/whoami/' + hwid, JSON.stringify(err))
                }
                else if(results.length != 1)
                {
                    console.log("id not found");
                    client.publish('boot/whoami/' + hwid, JSON.stringify({}));
                }
                else
                {
                    var nodeData = results[0];
                    nodeData.config = JSON.parse(nodeData.config);
                    connection.release();
                    db.getConnection(function(err, connection)
                    {
                        connection.query('SELECT * FROM `sensors` WHERE `nodeid` = ?', [ nodeData.id ], function(error, results, fields)
                        {
                            for(var i in results)
                                results[i].config = JSON.parse(results[i].config);
                            nodeData.sensors = results;

                            client.publish('boot/whoami/' + hwid, JSON.stringify(nodeData));
                            client.subscribe(nodeData.roomtopic + "/" + nodeData.topic + "/#");
                            router.remove(new RegExp("(" + nodeData.roomtopic + "/" + nodeData.topic + ")/(.*)", 'i')); //TODO: move all these to the will of this client
                            router.on(new RegExp("(" + nodeData.roomtopic + "/" + nodeData.topic + ")/(.*)", 'i'), require('./node.js')(nodeData.id, router));
                            connection.release();
                        });
                    });


                    
                }
                
            });
        });
    });
    
    

}