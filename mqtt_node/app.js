var mqtt = require('mqtt'),
    topicrouter = require('./src/topicrouter.js'),
    logger = require('winston'),
    config = require('config.json')('./config.json'),
    _ = require('lodash'),
    express = require('express'),
    bodyParser = require('body-parser'),
    util = require('util'),
    db = require('./src/db.js'),
    telegram = require('./src/telegram.js')
    Jvc = require('./src/jvc.js');


logger.remove(logger.transports.Console);
logger.add(logger.transports.Console, {'timestamp':true});

db.connect(config);

var client  = mqtt.connect('mqtt://192.168.2.201', {
    'clientId' : 'SensorCloudServer',
    'clean' : false,
    'will' :
    {
        'topic' : 'boot/server',
        'payload' : 'dead',
        'retain' : true
    }
})
router = new topicrouter(client);
require('./src/topics/boot')(router);
var jvc = new Jvc(client, router);
client.on('connect', function () {
    jvc.start();

});
client.subscribe('boot/whoami');
client.subscribe('ping');
client.subscribe('report');
client.publish("boot/server", "alive", { retain: true });

require('./src/onkyo2mqtt.js')(client, router);
require('./src/coin2mqtt.js')(client, router);





var app = express();
app.use(bodyParser.json());

app.post('/', function(req, res)
{
    logger.info(util.inspect(req.body));
    bot.sendMessage(config.chat_id, JSON.stringify(req.body));
    res.status(200).send();
});

app.listen(1337, function()
{
    logger.info("Listening on port 1337");
});


console.log("Running...");
telegram.sendMessage("Running");