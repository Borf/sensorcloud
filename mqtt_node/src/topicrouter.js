var isEqualRegex = require('is-equal-regex');


module.exports = class TopicRouter
{
    constructor(client)
    {
        this.client = client;
        this.topics = [];
        var that = this;

        client.on('message', function(topic, payload, packet)
        {
            that.topics.some(reg =>
            {
                var result = reg.regex.exec(topic);
                if(!result)
                    return false;

                reg.regex.lastIndex = 0; //reset

                var data
                try
                {
                    data = JSON.parse(payload);
                }
                catch(e)
                {
                    data = payload;
                }
                reg.callback(client, result, data, packet);
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

    remove(topicRegex)
    {
        var index = this.topics.findIndex(e => isEqualRegex(e.regex, topicRegex));
        if(index > -1)
            this.topics.splice(index, 1);
        else
            console.log("Error removing topic to listen to");
    }

};