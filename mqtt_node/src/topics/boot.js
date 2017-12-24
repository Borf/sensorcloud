var db = require('../db.js'),
    util = require('util');


module.exports = function(router)
{
    router.on(/boot\/whoami\/(.*)/i, function(client, topicmatch, payload)
    {
        console.log("Got a whoami for ID " + topicmatch[1]);
        var hwid = topicmatch[1];
        db.getConnection(function(err, connection)
        {
            if(err)
                throw err;

            connection.query('SELECT * FROM `nodes` WHERE `hwid` = ?', [ hwid ], function (error, results, fields) {
                // error will be an Error if one occurred during the query
                // results will contain the results of the query
                // fields will contain information about the returned results fields (if any)

                if(err)
                    client.publish('/boot/whoami/' + hwid, JSON.stringify(err))
                else if(results.length != 1)
                    client.publish('/boot/whoami/' + hwid, JSON.stringify({}));
                else
                    client.publish('/boot/whoami/' + hwid, JSON.stringify(results[0]));
                
            });
        });
    });
    
    

}