apiurl = "http://api.sensorcloud.borf.nl/";

if (location.hostname === "localhost")
    apiurl = "http://" + location.host + "/";

var cards = [];

$(function() {
	$.ajax({
		url: apiurl + "user/login",
		method: "post",
		data: JSON.stringify({ 'username' : 'borf', 'password' : 'borf' }),
		contentType: "application/json",
		dataType: "json",
		success : function(data)
        {
			if(!data.auth)
				alert("Could not log in. Something is wrong");


			//build up nodes in menu
            $.ajax({
                url: apiurl + "dash/cards",
                dataType: "json",
                success: function (data) {
                    var cardDeck = $("#dashnodes");
                    for (i in data) {
                        var card = $("<li>", {
                            "class": "card"
                        });
                        cardDeck.append(card);

                        card.append($("<h2>", { "class": "card-title" }).text(data[i].title));
                        card.append(data[i].list = $("<ul>", { "class": "list-group" }));

                        if (data[i].columns > 1)
                            data[i].list.addClass("flex-row flex-wrap");

                        data[i].elem = card;
                        data[i].items = [];
                        cards.push(data[i]);
                    }

                    setTimeout(updateCards, 0);
                }
            });
		}
	});
});


function updateCards()
{
	updateCard(0);
}

function updateCard(cardIndex)
{
    $.ajax({
        url: apiurl + "dash/card/" + cards[cardIndex].id,
        dataType: "json",
        success: function (data) {
            $("#error").hide();
            var card = cards[cardIndex];

            var colClass = "";
            if (card.columns > 1)
                colClass = "col-sm-" + Math.floor(12 / card.columns);

            for (var i in data) {
                var found = -1;
                for (var ii in card.items)
                    if (card.items[ii].id == data[i].id)
                        found = ii;
                if (found == -1) {
                    var item = {
                        id: data[i].id
                    };
                    if (data[i].displaytype == "circle") {
                        var v = parseInt(data[i].value);
                        card.list.append(item.elem = $("<li>", { "class": "list-group-item " + colClass })
                            .append(item.badge = $("<canvas>")));
                        item.chart = new Chart(item.badge,
                            {
                                type: 'doughnut',
                                data: {
                                    datasets: [{
                                        data: [v, 100-v],
                                        backgroundColor: ['rgb('+2*v+','+(200-2*v)+',0)', 'rgba(0,0,0,0)']
                                    }],
                                    labels: [data[i].name, 'free']
                                },
                                options: {
                                    legend: { display: false },
                                    title: { text: data[i].value + '\n' + data[i].name },
                                    circumference: Math.PI,
                                    rotation: -Math.PI
                                }
                            });
                    }
                    else {
                        card.list.append(item.elem = $("<li>", { "class": "list-group-item" })
                            .append(data[i].name)
                            .append(item.badge = $("<span>", { "class": "badge float-right" }).text("???")));
                    }
                    card.items.push(item);
                    found = card.items.length - 1;
				}

				if(data[i].type == 'image')
				{
					if(card.items[found].image) {
						card.items[found].image.attr('src', data[i].parameter +"?" + Math.random());
					}
					else {
						card.items[found].badge.empty();
						card.items[found].badge.append(card.items[found].image = $('<img>', { src : data[i].parameter +"?" + Math.random() }));
					}
				}
				else if(data[i].value == null)
				{
					card.items[found].badge.removeClass("badge-danger");
					card.items[found].badge.removeClass("badge-success");
					card.items[found].badge.removeClass("badge-warning");
					card.items[found].badge.addClass("badge-secondary");
					card.items[found].badge.text("Not sensed");
				}
				else
                {
                    if (data[i].displaytype == "circle") {
                        var v = parseInt(data[i].value);
                        card.items[found].chart.data.datasets[0].data[0] = v;
                        card.items[found].chart.data.datasets[0].data[1] = 100 - v;
                        card.items[found].chart.data.datasets[0].backgroundColor[0] = 'rgb(' + 2 * v + ',' + (200 - 2 * v) + ',0)';
                        card.items[found].chart.update();
                    }
                    else {
                        card.items[found].badge.removeClass("badge-danger");
                        card.items[found].badge.removeClass("badge-secondary");
                        card.items[found].badge.removeClass("badge-success");
                        card.items[found].badge.removeClass("badge-warning");
                        if (data[i].value == "ok")
                            card.items[found].badge.addClass("badge-success");
                        else if (data[i].value == "error")
                            card.items[found].badge.addClass("badge-danger");
                        else if (data[i].type == "pingtime") {
                            if (data[i].value > 15)
                                card.items[found].badge.addClass("badge-warning");
                            else
                                card.items[found].badge.addClass("badge-success");
                        }
                        else if (data[i].type == "ping") {
                            if (data[i].value == "offline")
                                card.items[found].badge.addClass("badge-warning");
                        } else if (/^\d+(\.\d+)?%$/.test(data[i].value.trim())) {
                            if (parseInt(data[i].value) > 95)
                                card.items[found].badge.addClass("badge-danger");
                            else
                                card.items[found].badge.addClass("badge-secondary");
                        }
                        if (card.title == "Power Status") {
                            if (data[i].value == "on")
                                card.items[found].badge.addClass("badge-danger");
                            else if (data[i].value == "off")
                                card.items[found].badge.addClass("badge-success");
                        }

                        card.items[found].badge.text(data[i].value);
                    }
				}
			}
			cardIndex++;
			if(cardIndex < cards.length)
				setTimeout("updateCard("+cardIndex+");", 1);
			else
				setTimeout(updateCards, 10000);
		},
		error : function(xhr, status, error)
		{
			$("#error").show();
			setTimeout(updateCards, 1000);
		},
		complete : function(xhr, status)
		{
		}
	})
}