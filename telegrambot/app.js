'use strict';

var TelegramBot = require('node-telegram-bot-api'),
    logger = require('winston'),
    config = require('config.json')('./config.json'),
    _ = require('lodash'),
    express = require('express'),
    bodyParser = require('body-parser'),
    jsonParser = bodyParser.json();



logger.remove(logger.transports.Console);
logger.add(logger.transports.Console, {'timestamp':true});
var bot = new TelegramBot(config.api_token,
    {
        polling : true
    });

var id;

bot.onText(/\/start/i, function(msg)
{
    id = msg.from.id;
    bot.sendMessage(msg.from.id, "Hello World");
});

var app = express();

app.use(jsonParser);
app.post('/', function(req, res)
{
    bot.sendMessage(id, JSON.stringify(req.body));
    res.status(200).send();
});

app.listen(1337, function()
{
    logger.info("Listening on port 1337");
});