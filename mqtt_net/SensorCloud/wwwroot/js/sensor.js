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


$(function () {
    feather.replace();
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


	$("a#btnDashboard").click(function(e)
    {
        setPage("dashboard.html", $(e.target)).then(function()
		{
			showGraphs();
		});
	});
	$("a#btnNodes").click(function(e)
    {
        setPage("nodes.html", $(e.target)).then(function()
		{
			$.ajax({
				url: apiurl + "nodes",
				dataType: "json",
				success : function(data)
                {
					container = $("#nodesContainer");
                    container.empty();
                    var row = $(`<div class="row"></div>`);
                    container.append(row);

					for(var i = 0; i < data.length; i++)
                    {
                        var lastPing = Math.round((Date.now() - Date.parse(data[i].stamp) - (60 * 60 * 2 * 1000)) / 100) / 10;

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
                            time = '<abbr title="' + data[i].stamp + '">' + Math.round((Date.now() - Date.parse(data[i].stamp) - (60 * 60 * 2*1000)) / 100)/10+'sec ago</abbr>';

						var sensorTime = "";
						if(data[i].lastsensordata)
							sensorTime = '<abbr title="'+data[i].lastsensordata+'">'+Math.round((Date.now() - Date.parse(data[i].lastsensordata)) / 100)/10+'sec ago</abbr>';

                        var inDanger = !data[i].ip || !data[i].stamp || lastPing > 120;

						row.append('<div class="col-2">' +
				'<div class="card border-primary text-white bg-dark">' +
				'<div class="card-heading bg-primary">' +
					'<h2 class="card-title"><i class="fa fa-heartbeat fa-fw"></i>' + data[i].name + '</h2>' +
				'</div>' +
				'<!-- /.card-heading -->' +
				'<div class="card-body">' +
					'<table class="table text-white">' +
						'<tbody>' +
							'<tr><td>Id</td><td>'+data[i].id+'</td></tr>' +
							'<tr><td>Hardware Id</td><td>'+data[i].hwid+'</td></tr>' +
							'<tr><td>Name</td><td>'+data[i].name+'</td></tr>' +
							'<tr><td>Room</td><td>'+data[i].room+'</td></tr>' +
                            '<tr' + (inDanger ? ' class="bg-danger"' : '') + '><td>IP</td><td><a href="http://' + data[i].ip + '">' + data[i].ip + '</a></td></tr>' +
                            '<tr' + (inDanger?' class="bg-danger"':'') + '><td>Last ping</td><td>' + time + '</td></tr>' +
							'<tr><td>Last sensor update</td><td>'+sensorTime+'</td></tr>' +
							'<tr class="'+memclass+'"><td>Memory</td><td>'+data[i].heapspace+' bytes</td></tr>' +
							'<tr><td>RSSI</td><td>'+data[i].rssi+' Dbm</td></tr>' +
							'<tr><td>Sensors</td><td>'+data[i].sensorcount+'</td></tr>' +
							'<tr><td>Actuators</td><td>'+data[i].actcount+'</td></tr>' +
						'</tbody>' +
					'</table>' +
				'</div>' +
				'<!-- /.card-body -->' +
			'</div>' +
			'<!-- /.card -->' +
		'</div>');
                       /* if (i % 4 == 3) {
                            row = $(`<div class="row"></div>`);
                            container.append(row);
                        }*/
					}
				}
			});
		});
	});
	$("a#btnSensors").click(function(e)
	{
        setPage("sensors.html", $(e.target)).then(function()
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
	$("a#btnMap").click(function(e)
	{
        setPage("map.html", $(e.target)).then(function()
        {
			$.ajax({
				url: apiurl + "room",
				dataType: "json",
				success : function(data)
                {
					var R = Raphael("mapje");

                    var r = R.rect(0, 0, 1048, 1084);
                    r.attr({
                        fill: "#252A31",
                        "fill-opacity": 0.5
                    });

					var attr = {
                        fill: "#323842",
						"fill-opacity":0.3,
                        stroke: "#4F75FC",
						"stroke-width": 2,
						"stroke-linejoin": "round"
					};
					for(var i in data)
					{
						data[i].el = R.path(data[i].area).attr(attr);

						var bbox = data[i].el.getBBox();
						var center = { x : bbox.x + bbox.width/2, y : bbox.y + bbox.height/2 };
						R.text(center.x, center.y, data[i].name).attr({
                            'font-size': 20,
                            fill: '#fff'
						});
						if(data[i].nodes)
                        {
                            R.text(center.x, center.y + 20, data[i].nodes.length + " node" + (data[i].nodes.length > 1 ? "s" : "")).attr({
                                'font-size': 10,
                                fill: '#fff'
							});
						}
					}
				}
			});

		});
	});
	$("a#btnLog").click(function(e)
	{
        setPage("log.html", $(e.target)).then(function()
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

    


    $("a#btnRules").click(function (e) {
        setPage("rules.html", $(e.target)).then(function () {

        });
    });

	if(location.hash == "#nodes")		    $("a#btnNodes").click();
    else if (location.hash == "#sensors")   $("a#btnSensors").click();
	else if (location.hash == "#rules")     $("a#btnRules").click();
    else if (location.hash == "#map") $("a#btnMap").click();
	else								    $("a#btnDashboard").click();

	function setPage(url, src)
    {
        $("a.nav-link").removeClass("active");
        src.addClass("active");
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
