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
				success : function(data)
				{
					var cardDeck = $("#dashnodes");
					for(i in data)
					{
						var card = $("<li>", {
							"class" : "card"
						});
						cardDeck.append(card);

						card.append($("<h2>", { "class" : "card-title"}).text(data[i].title));
						card.append(data[i].list = $("<ul>", { "class" : "list-group"}));

						data[i].elem = card;
						data[i].items = [];
						cards.push(data[i]);
					}

					setTimeout(updateCards, 0);
                }
			})
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
		success : function(data)
		{
			$("#error").hide();
			var card = cards[cardIndex];


			for(var i in data)
			{
				var found = -1;
				for(var ii in card.items)
					if(card.items[ii].id == data[i].id)
						found = ii;
				if(found == -1)
				{
					var item, badge;
					card.list.append(item = $("<li>", { "class" : "list-group-item" })
							.append(data[i].name)
							.append(badge = $("<span>", { "class" : "badge float-right"}).text("???")));
					card.items.push(
						{
							id : data[i].id,
							elem : item,
							badge : badge
						});
						found = card.items.length-1;
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
					card.items[found].badge.removeClass("badge-danger");
					card.items[found].badge.removeClass("badge-secondary");
					card.items[found].badge.removeClass("badge-success");
					card.items[found].badge.removeClass("badge-warning");
					if(data[i].value == "ok")
						card.items[found].badge.addClass("badge-success");
					else if(data[i].value == "error")
						card.items[found].badge.addClass("badge-danger");
					else if(data[i].type == "pingtime")
					{
						if(data[i].value > 15)
							card.items[found].badge.addClass("badge-warning");
						else
							card.items[found].badge.addClass("badge-success");
					}
					else if(data[i].type == "ping")
					{
						if(data[i].value == "offline")
							card.items[found].badge.addClass("badge-warning");
					} else if(/^\d+(\.\d+)?%$/.test(data[i].value.trim()))
					{
						if(parseInt(data[i].value) > 95)
							card.items[found].badge.addClass("badge-danger");
						else
						card.items[found].badge.addClass("badge-secondary");
					}
					if(card.title == "Power Status") {
						if(data[i].value == "on")
							card.items[found].badge.addClass("badge-danger");
						else if(data[i].value == "off")
							card.items[found].badge.addClass("badge-success");
					}
	
					card.items[found].badge.text(data[i].value);
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