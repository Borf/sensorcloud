var TelegramBot = require('node-telegram-bot-api'),
    config = require('config.json')('./config.json');



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
    bot.sendMessage(config.chat_id, "Hello World");
});


module.exports = 
{

    sendMessage : function(text)
    {
        bot.sendMessage(config.chat_id, text);
    },

    sendPhoto : function(data, caption)
    {
        console.log("Sending photo");
        bot.sendPhoto(config.chat_id, data, 
            { 
                caption : caption,
                contentType : "image/jpeg"
            }, {
                filename : "cam.jpg",
                contentType : "image/jpeg"
            });
    }
}



