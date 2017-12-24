


module.exports = class TopicRouter
{
    constructor(client)
    {
        this.client = client;
        this.topics = [];
        var that = this;

        client.on('message', function(topic, payload)
        {
            that.topics.some(reg =>
            {
                var result = reg.regex.exec(topic);
                if(!result)
                    return false;

                reg.regex.lastIndex = 0; //reset
                reg.callback(client, result, payload);
                return false;
            });
        });
      

    }


    on(topicRegex, callback)
    {
        this.topics.push(
            {
                regex : topicRegex,
                callback : callback
            }
        );
    }


};