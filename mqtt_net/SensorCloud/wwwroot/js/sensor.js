apiurl = "http://api.sensorcloud.borf.info/";

if (location.hostname === "localhost")
    apiurl = "http://" + location.host + "/";

function alert(msg, className) {
    if (!className)
        className = "danger";
    var alert = $(`<li class="list-group-item list-group-item-`+className+` px-4">` + msg + `</li>`);
    $("#alerts").append(alert);
    alert.slideDown();
    setTimeout(function () {
        alert.slideUp(() => alert.remove());
    },2000);
    alert.click(e => alert.slideUp(() => alert.remove()));
}


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
        },
        error: function (data) {
            alert("Could not log in. Something is wrong");
        }
	});


	$("a#btnDashboard").click(function()
	{
		setPage("dashboard.html").then(function()
		{
			showGraphs();
		});
	});
	$("a#btnNodes").click(function()
	{
		setPage("nodes.html").then(function()
		{
			$.ajax({
				url: apiurl + "nodelist",
				dataType: "json",
				success : function(data)
				{
					container = $("#nodesContainer");
					container.empty();
					for(var i = 0; i < data.length; i++)
					{
						var memclass = "";
						console.log(data[i].heapspace);
                        if (!data[i].heapspace)
                            memclass = "danger";
                        else if (data[i].heapspace < 20000)
                            memclass = "warning";
                        else if (data[i].heapspace < 15000)
                            memclass = "fatal";
                        else
                            memclass = "success";


						if(navigator.userAgent.toLowerCase().indexOf('firefox') > -1)
							data[i].stamp = data[i].stamp.replace(" ", "T");
						var time = "";
						if(data[i].stamp)
							time = '<abbr title="'+data[i].stamp+'">'+Math.round((Date.now() - Date.parse(data[i].stamp)) / 100)/10+' seconds ago</abbr>';

						var sensorTime = "";
						if(data[i].lastsensordata)
							sensorTime = '<abbr title="'+data[i].lastsensordata+'">'+Math.round((Date.now() - Date.parse(data[i].lastsensordata)) / 100)/10+' seconds ago</abbr>';


						container.append('<div class="col-lg-4">' +
											'<div class="panel'+(data[i].ip ? " panel-default" : ' panel-danger')+'">' +
				'<div class="panel-heading">' +
					'<i class="fa fa-heartbeat fa-fw"></i> Node' +
					'<div class="pull-right">' +
						'<div class="btn-group">' +
							'<button type="button" class="btn btn-default btn-xs dropdown-toggle" data-toggle="dropdown">' +
								'Actions' +
								'<span class="caret"></span>' +
							'</button>' +
							'<ul class="dropdown-menu pull-right" role="menu">' +
								'<li><a href="#">Edit</a></li>' +
								'<li><a href="#">Delete</a></li>' +
							'</ul>' +
						'</div>' +
					'</div>' +
				'</div>' +
				'<!-- /.panel-heading -->' +
				'<div class="panel-body">' +
					'<table class="table">' +
						'<tbody>' +
							'<tr><td>Id</td><td>'+data[i].id+'</td></tr>' +
							'<tr><td>Hardware Id</td><td>'+data[i].hwid+'</td></tr>' +
							'<tr><td>Name</td><td>'+data[i].name+'</td></tr>' +
							'<tr><td>Room</td><td>'+data[i].room+'</td></tr>' +
							'<tr><td>IP</td><td><a href="http://'+data[i].ip+'">'+data[i].ip+'</a></td></tr>' +
							'<tr><td>Last ping</td><td>'+time+'</td></tr>' +
							'<tr><td>Last sensor update</td><td>'+sensorTime+'</td></tr>' +
							'<tr class="'+memclass+'"><td>Memory</td><td>'+data[i].heapspace+' bytes</td></tr>' +
							'<tr><td>RSSI</td><td>'+data[i].rssi+' Dbm</td></tr>' +
							'<tr><td>Sensors</td><td>'+data[i].sensorcount+'</td></tr>' +
							'<tr><td>Actuators</td><td>'+data[i].actcount+'</td></tr>' +
						'</tbody>' +
					'</table>' +
				'</div>' +
				'<!-- /.panel-body -->' +
			'</div>' +
			'<!-- /.panel -->' +
		'</div>');
					}
				}
			});
		});
	});
	$("a#btnSensors").click(function()
	{
		setPage("sensors.html").then(function()
		{
			$.ajax({
				url: apiurl + "sensordata/type:temperature",
				dataType: 'json',
				success : function(data)
				{
				   	Morris.Line({
						element: 'tempGraph',
						data: data.data,
						xkey: 'time',
						ykeys: data.nodes,
						labels: data.nodes,
						pointSize: 0,
						hideHover: 'auto',
						resize: true
					});
				}
			});
			$.ajax({
				url: apiurl + "sensordata/type:humidity",
				dataType: 'json',
				success : function(data)
				{
				   	Morris.Line({
						element: 'humGraph',
						data: data.data,
						xkey: 'time',
						ykeys: data.nodes,
						labels: data.nodes,
						pointSize: 0,
						hideHover: 'auto',
						resize: true
					});
				}
			});


		});
	});
	$("a#btnMap").click(function()
	{
		setPage("map.html").then(function()
		{
			$.ajax({
				url: apiurl + "roomlist",
				dataType: "json",
				success : function(data)
				{
					var R = Raphael("mapje");

					var attr = {
						fill: "#faa",
						"fill-opacity":0.3,
						stroke: "#f00",
						"stroke-width": 10,
						"stroke-linejoin": "round"
					};
					for(var i in data)
					{
						data[i].el = R.path(data[i].area).attr(attr);

						var bbox = data[i].el.getBBox();
						var center = { x : bbox.x + bbox.width/2, y : bbox.y + bbox.height/2 };
						R.text(center.x, center.y, data[i].name).attr({
							'font-size' : 20
						});
						if(data[i].nodes)
						{
							R.text(center.x, center.y+20, data[i].nodes.length + " node").attr({
								'font-size' : 10
							});
						}
					}

/*					plan.dining = R.path("M285,83 l190,0 l0,240 l-190,0z").attr(attr);
					plan.kitchen = R.path("M285,323 l190,0 l0,382 l-177,0 l-13,-250z").attr(attr);
					plan.livingroom = R.path("M19,455 l266,0 l13,250 l0,330 l-279,0z").attr(attr);
					plan.hallway = R.path("M298,705 l177,0 l0,170 l-90,0 l0,10 l10,0 l0,150 l-97,0z").attr(attr);
					plan.toilet = R.path("M395,932 l80,0 l0,103 l-80,0z").attr(attr);
					plan.meter = R.path("M395,885 l80,0 l0,47 l-80,0z").attr(attr);

					attr.stroke = '#0f0';

					plan.smallbedroom = R.path("M755,300 l190,0 l0,411 l-73,0 l0,-30 l-106,0 l0,-226 l-11,0z").attr(attr);
					plan.masterbedroom = R.path("M489,455 l277,0 l0,305 l-279,0z").attr(attr);
					plan.hallway2 = R.path("M766,681 l105,0 l0,30 l75,0 l0,174 l-180,0z").attr(attr);
					plan.bathroom = R.path("M766,885 l180,0 l0,155 l-180,0z").attr(attr);
					plan.bedroom = R.path("M489,760 l277,0 l0,280 l-277,0z").attr(attr);
					attr.stroke = '#00f';*/

/*
		 			var current = null;
					for (var room in plan) {
						plan[room].color = "#afa";


						var bbox = plan[room].getBBox();
						var center = { x : bbox.x + bbox.width/2, y : bbox.y + bbox.height/2 };
						R.text(center.x, center.y, room).attr({
							'font-size' : 20
						});

						(function (st, room) {
							st[0].style.cursor = "pointer";
							st[0].onmouseover = function () {
								current && plan[current].animate({fill: "#faa"}, 150)
								st.animate({fill: st.color}, 150);
								st.toFront();
								R.safari();
								//document.getElementById(room).style.display = "block";
								current = room;
							};
							st[0].onmouseout = function () {
								st.animate({fill: "#faa"}, 150);
								st.toFront();
								R.safari();
							};
						})(plan[room], room);
					}*/
				}
			});

		});
	});
	$("a#btnLog").click(function()
	{
		setPage("log.html").then(function()
		{
				$.ajax({
				url: apiurl + "log",
				dataType: 'json',
				success : function(data)
				{
					log = $("#log");
					log.empty();
					for(var i = 0; i < data.length; i++)
					{
						log.append(	'<div class="row">' +
									'<div class="col-lg-2"><i class="fa fa-wifi fa-fw"></i> 2016-01-25 12:12:12</div>' +
									'<div class="col-lg-10">'+data[i]+'</div>' +
									'</div>');
					}
				},
				error : function(data, bla)
				{
					alert(bla + "\n" + data.responseText);
				}
			});
		});
    });

    


    $("a#btnRules").click(function () {
        setPage("rules.html").then(function () {

        });
    });

	if(location.hash == "#nodes")		    $("a#btnNodes").click();
    else if (location.hash == "#sensors")   $("a#btnSensors").click();
	else if (location.hash == "#rules")     $("a#btnRules").click();
    else if (location.hash == "#map") $("a#btnMap").click();
    else if (location.hash == "#nodeedit") setPage("nodeedit.html").then(function () {});
	else								    $("a#btnDashboard").click();

	function setPage(url)
	{
		return $.ajax({
			url : url,
			success : function(data, status, xhr)
			{
				$("main").html(data);
			}
		});
	}



	function showGraphs()
	{
   	Morris.Line({
		element: 'morris-area-chart',
		data: [{
			period: '2016-01-01',
			livingRoom: 20,
			kitchen: 19,
			bedroom: 18
		},{
			period: '2016-01-02',
			livingRoom: 21,
			kitchen: 19,
			bedroom: 18
		}
		],
		xkey: 'period',
		ykeys: ['livingRoom', 'kitchen', 'bedroom'],
		labels: ['living room', 'kitchen', 'bedroom'],
		pointSize: 2,
		hideHover: 'auto',
		resize: true
	});

   Morris.Line({
		element: 'morris-area-chart-hum',
		data: [{
			period: '2016-01-01',
			livingRoom: 80,
			kitchen: 80,
			bedroom: 80
		},{
			period: '2016-01-02',
			livingRoom: 21,
			kitchen: 19,
			bedroom: 18
		}
		],
		xkey: 'period',
		ykeys: ['livingRoom', 'kitchen', 'bedroom'],
		labels: ['living room', 'kitchen', 'bedroom'],
		pointSize: 2,
		hideHover: 'auto',
		resize: true
	});
	}

});
