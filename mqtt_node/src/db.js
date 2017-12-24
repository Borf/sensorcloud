var mysql = require('mysql');
var pool;



exports.connect = function(config)
{
    var dbconfig = {
        host :      config.database.host,
        user:       config.database.user,
        database :  config.database.daba,
        password :  config.database.pass,
        port:       config.database.port, 
        max:        10,
        idleTimeoutMillis: 30000,
      };
    pool = mysql.createPool(dbconfig);
}


exports.getConnection = function(callback)
{
    return pool.getConnection(callback);
}