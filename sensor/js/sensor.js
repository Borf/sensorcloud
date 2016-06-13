apiurl = "api/";

$(function() {
	$("a#btnWifi").click(function()
	{
		setPage("wifi.html").then(function()
		{
			$.ajax({
				url: apiurl + "wifi/settings",
				dataType: 'json',
				success : function(data)
				{
					$("#ssid").val(data.ssid);
					$("#password").val(data.password);
				},
				error : function(data, bla)
				{
					alert(bla + "\n" + data.responseText);
				}
			});


			$.ajax({
				url: apiurl + "wifi/scan",
				dataType: 'json',
				success : function(data)
				{
					$("#scan-desc").hide();
					table = $("#scan-table");
					table.empty();
					table.append('<table class="table table-striped table-hover"><thead><tr><th>#</th><th>SSID</th><th>Signal strength</th><th>Encryption</th><th>hidden</th><th>bssid</th><th>Channel</th><th>Actions</th></tr></thead><tbody></tbody></table>');
					table = $("#scan-table tbody");

					for(i = 0; i < data.length; i++)
					{
						var enc = "";
						if(data[i].encryption == 5) enc = '<i class="fa fa-lock"></i> WEP';
						else if(data[i].encryption == 2) enc = '<i class="fa fa-lock"></i> TKIP';
						else if(data[i].encryption == 4) enc = '<i class="fa fa-lock"></i> WPA CCMP';
						else if(data[i].encryption == 7) enc = '<i class="fa fa-unlock"></i> None';
						else if(data[i].encryption == 8) enc = '<i class="fa fa-lock"></i> Auto';
						else enc = '<i class="fa fa-question-circle"></i> Unknown : ' + data[i].encryption;

						table.append(
							"<tr><td>" + data[i].id + 
							"</td><td>" + data[i].ssid + 
							"</td><td>" + data[i].rssi + " dBm" +
							"</td><td>" + enc + 
							'</td><td class="'+(data[i].hidden ? "success" : "danger")+'">' + data[i].hidden + 
							"</td><td>" + data[i].BSSID + 
							"</td><td>" + data[i].channel + 
							"</td></tr>");
					}

					$('th').click(function(){
						    var table = $(this).parents('table').eq(0)
						    var rows = table.find('tr:gt(0)').toArray().sort(comparer($(this).index()))
						    this.asc = !this.asc
						    if (!this.asc){rows = rows.reverse()}
						    for (var i = 0; i < rows.length; i++){table.append(rows[i])}
						})
						function comparer(index) {
						    return function(a, b) {
						        var valA = getCellValue(a, index), valB = getCellValue(b, index)
						        return $.isNumeric(valA) && $.isNumeric(valB) ? valA - valB : valA.localeCompare(valB)
						    }
						}
						function getCellValue(row, index){ return $(row).children('td').eq(index).html() }



				},
				error : function(data, bla)
				{
					alert(bla + "\n" + data.responseText);
				}
			});
		});
	});
	$("a#btnDashboard").click(function()
	{
		setPage("dashboard.html").then(function()
		{
			showGraphs();
		});
	});
	$("a#btnSensor").click(function()
	{
		setPage("sensor.html").then(function()
		{
			$.ajax({
				url: apiurl + "sensors",
				dataType: 'json',
				success : function(data)
				{
					var container = $("#sensorContainer");
					container.empty();
					for(var i = 0; i < data.length; i++)
					{
   var html = '<div class="col-lg-4">' +
        '<div class="panel panel-default">' +
            '<div class="panel-heading">' +
                '<i class="fa fa-heartbeat fa-fw"></i> Sensor' +
                '<div class="pull-right">' +
                    '<div class="btn-group">' +
                        '<button type="button" class="btn btn-default btn-xs dropdown-toggle" data-toggle="dropdown">' +
                            'Actions' +
                            '<span class="caret"></span>' +
                        '</button>' +
                        '<ul class="dropdown-menu pull-right" role="menu">' +
                            '<li><a href="#">Delete</a></li>' +
                        '</ul>' +
                    '</div>' +
                '</div>' +
            '</div>' +
            '<!-- /.panel-heading -->' +
            '<div class="panel-body">' +
                '<table class="table">' +
                    '<tbody>';

	                    	for(var key in data[i])
	                        	html += '<tr><td>'+key+'</td><td>'+data[i][key]+'</td></tr>';
                   			html += 
                    '</tbody>' +
                '</table>' +
            '</div>' +
            '<!-- /.panel-body -->' +
        '</div>' +
        '<!-- /.panel -->' +
    '</div>						';
						container.append($(html));

					}
				},
				error : function(data, bla)
				{
					alert(bla + "\n" + data.responseText);
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


	if(location.hash == "#wifi")		$("a#btnWifi").click();
	else if(location.hash == "#log")	$("a#btnLog").click();
	else if(location.hash == "#sensor")	$("a#btnSensor").click();
	else								$("a#btnDashboard").click();

	function setPage(url)
	{
		return $.ajax({
			url : url,
			success : function(data, status, xhr)
			{
				$("div#page-wrapper").html(data);
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
        },{
            period: '2016-01-03',
            livingRoom: 20,
            kitchen: 19,
            bedroom: 18
        },{
            period: '2016-01-04',
            livingRoom: 19,
            kitchen: 15,
            bedroom: 18
        },{
            period: '2016-01-05',
            livingRoom: 20,
            kitchen: 19,
            bedroom: 18
        },{
            period: '2016-01-06',
            livingRoom: 19,
            kitchen: 19,
            bedroom: 18
        },{
            period: '2016-01-07',
            livingRoom: 20,
            kitchen: 19,
            bedroom: 18
        },{
            period: '2016-01-08',
            livingRoom: 19,
            kitchen: 19,
            bedroom: 18
        },


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
        },{
            period: '2016-01-03',
            livingRoom: 80,
            kitchen: 19,
            bedroom: 80
        },{
            period: '2016-01-04',
            livingRoom: 19,
            kitchen: 15,
            bedroom: 18
        },{
            period: '2016-01-05',
            livingRoom: 20,
            kitchen: 19,
            bedroom: 18
        },{
            period: '2016-01-06',
            livingRoom: 80,
            kitchen: 19,
            bedroom: 18
        },{
            period: '2016-01-07',
            livingRoom: 20,
            kitchen: 19,
            bedroom: 18
        },{
            period: '2016-01-08',
            livingRoom: 19,
            kitchen: 19,
            bedroom: 18
        },


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
