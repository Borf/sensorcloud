'use strict';


var TelegramBot = require('node-telegram-bot-api'),
    logger = require('winston'),
    config = require('config.json')('./config.json'),
    _ = require('lodash'),
    express = require('express'),
    bodyParser = require('body-parser'),
    util = require('util');



logger.remove(logger.transports.Console);
logger.add(logger.transports.Console, {'timestamp':true});
var bot = new TelegramBot(config.api_token,
    {
        polling : true
    });

bot.onText(/\/start/i, function(msg)
{
    if(msg.chat.id != config.chat_id)
        return;
    logger.info(util.inspect(msg));
    id = msg.chat.id;
    logger.info("ID: " + id);
    bot.sendMessage(id, "Hello World");
});

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