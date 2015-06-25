var key = null;
var password = null;


var activeAccount = -1;
var accounts = [];
var currentPage = "";

($(document).ready(function()
{
	$(".form-signin").submit(function()
	{
		var username = $("#inputUsername").val();
		password = $("#inputPassword").val();
		$("#inputUsername").val("Logging in....").prop('disabled', true);
		$("#inputPassword").val("").prop('disabled', true);
		$(".form-signin button").prop('disabled', true);
	
		$.ajax({
			type: "POST",
			url:"/api/login",
			data: JSON.stringify({ 'username' : username, 'password' : password }),
			contentType: "application/json",
			dataType: "json",
			success:	function(data) 
			{ 
				if(data["auth"] == false)
				{
					alert("Invalid login");
					location.reload();
					return;
				}
					$.ajax({
						url:"layout.htm",
						success : function(data)
						{
							$("#main").html(data);
							
							
							$("#setpage_overview").click(function() { setPage("overview"); });
							$("#setpage_triggers").click(function() { setPage("triggers"); });
							
		/*					var row = $("#mainpanel").empty();
							
							
							$("#uploadform").submit(uploadFile);
							$("#managelabels").click(function() { setPage("managelabels"); });
							$("#overview").click(function() { setPage("overview"); });
							*/

							setPage("overview");
							
						}
					});
		},
			failure: function(data) { alert('Error logging in'); }
		});
		return false;
	});

	$(".form-signin").submit();

}));


var timer = null;

function setPage(page)
{
	if(timer != null)
		clearTimeout(timer);
	var oldPage = currentPage;
	currentPage = page;	
	
	if(currentPage != oldPage)
	{
		$("#setpage_overview").removeClass("active");
		$("#setpage_triggers").removeClass("active");
		if(currentPage == "overview")		$("#setpage_overview").addClass("active");
		if(currentPage == "triggers")		$("#setpage_triggers").addClass("active");
		
	}
	

	if(page == "overview")
	{
		timer = setTimeout(function() { setPage("overview"); }, 60000);
		$("#mainpanel").empty();	
		{
			var row = $("#mainpanel").addRow();
			var cell = row.addCell(6);
			var panel = cell.addPanel('Nodes<span class="pull-right"><button type="button" class="btn btn-default btn-xs"><span class="glyphicon glyphicon-cog"></span></button><button type="button" class="btn btn-default btn-xs"><span class="glyphicon glyphicon-pencil"></span></button></span>');
			
			var table = $('<table>', { class : 'table table-striped table-hover', id : 'transactions' });
			panel.append(table);
			
			var thead = $('<thead>');
			var tbody = $('<tbody>');
			table.append(thead, tbody);
			thead.append('<tr><th>Node ID</th><th>Name</th><th>Connection Status</th></tr>');
			$.ajax({
				type: "GET",
				url:"/api/nodes",
				contentType: "application/json",
				dataType: "json",
				success:	function(data) 
				{ 
					var statusCells = {};
					for(var i in data)
					{
						var node = $('<tr>');
						node.append($('<td>' + data[i].address + '</td>'));
						node.append($('<td>' + data[i].name + '</td>'));
						statusCells[data[i].address] = $('<td>polling....</td>');
						node.append(statusCells[data[i].address]);
						tbody.append(node);
					}
					$.ajax({
						type: "GET",
						url:"api/list",
						contentType: "application/json",
						dataType: "json",
						success:	function(data) 
						{
							for(var i in data)
							{
								if(data[i].lastHello > 30)
								{
									statusCells[data[i].id].empty();
									statusCells[data[i].id].append('Timed Out (' + data[i].lastHello + ')');
									statusCells[data[i].id].addClass("danger");
								}
								else
								{
									statusCells[data[i].id].empty();
									statusCells[data[i].id].append('connected');
									statusCells[data[i].id].addClass("success");
								}
							}
							for(var i in statusCells)
							{
								if(!statusCells[i].hasClass("success") && !statusCells[i].hasClass("danger"))
								{
									statusCells[i].empty();
									statusCells[i].append('Not connected');
									statusCells[i].addClass("warning");
								}
							}
						}
					
					});
				}
			});
			
			
			
			
			
		
			var cell = row.addCell(6);
			var panel  = cell.addPanel('Temperature past 24 hours<span class="pull-right"><button type="button" class="btn btn-default btn-xs"><span class="glyphicon glyphicon-cog"></span></button><button type="button" class="btn btn-default btn-xs"><span class="glyphicon glyphicon-pencil"></span></button></span>');
			var last24 = $('<div>', { id : 'graph2', 'height' : '300px', 'outline' : '1px solid black' });
			panel.append(last24);
			var timer24hours = null;
			$.ajax({
				type: "GET",
				url:"api/last24",
				contentType: "application/json",
				dataType: "json",
				success:	function(data)
				{
				  last24.highcharts({
						credits: { enabled : false },
				        chart: {
				            type: 'spline',
							zoomType: 'x'
				        },
				        plotOptions: {
						    spline: {
						        marker: {
						            enabled: false
						        }
						    }
						},
				        title: {
				            text:'',
				        },
				        xAxis: {
				            type: 'datetime',
							dateTimeLabelFormats: { // don't display the dummy year
				                month: '%e. %b',
				                year: '%b'
				            },
				            title: {
				                text: ''
				            }
				        },
				        yAxis: {
				            title: {
				                text: ''
				            }
				        },
				        legend: { enabled : false },
				        series: [
						{
				            name: 'Temperature',
				            data: $.map(data, function(a) { return [ [ Date.parse(a.time), a.data ] ]; }),
				        }
						]
				    });
   				}
			});

			
			
			
		}	

		{
			var row = $("#mainpanel").addRow();
			var cell = row.addCell(6);
			var panel = cell.addPanel("Actuators");
			
			var actuatorList = $("<ul>");
			panel.append(actuatorList);
			$.ajax({
				type: "GET",
				url:"/api/actuators",
				contentType: "application/json",
				dataType: "json",
				success:	function(data) 
				{
					for(var i in data)
					{
						var item = $('<li>', { "style" : "clear: both" });
						item.append(data[i].name);
						var buttons = $('<div>', { "class" : "btn-group pull-right", "role" : "group" });
						item.append(buttons);
						
						for(var ii in data[i].actions)
						{
							//var btn = $('<button type="button" class="btn btn-default" onclick="activate('+data[i].nodeId+',' + data[i].sensorId + ',' + data[i].actions[ii].value + ')"><span class="glyphicon '+data[i].actions[ii].icon+'"></span></button>');
							var btn = $('<button type="button" class="btn btn-default"><span class="glyphicon '+data[i].actions[ii].icon+'"></span></button>');
							btn.click(function(d1, d2)
							{
								return function()
								{
									activate(d1, d2);
								}
								
							}(data[i], data[i].actions[ii]));
							btn.css('color', data[i].actions[ii].color);
							buttons.append(btn);
						}
						
						actuatorList.append(item);
					}
				}
			});
		
			var cell = row.addCell(6);
			var panel  = cell.addPanel("Current Values");

			var list = $("<ul>");
			list.append($('<li>Temperature Outside	<span class="pull-right">10°C		<span class="glyphicon glyphicon-arrow-up"></span></li>'));
			list.append($('<li>Humidity Outside		<span class="pull-right">50%		<span class="glyphicon glyphicon-arrow-down"></span></li>'));
			list.append($('<li>Air Pressure			<span class="pull-right">1007.0 hPa	<span class="glyphicon glyphicon-arrow-up"></span></li>'));
			list.append($('<li>Temperature Inside 	<span class="pull-right">20°C		<span class="glyphicon glyphicon-arrow-down"></span></li>'));
			list.append($('<li>Humidity Inside		<span class="pull-right">70%		<span class="glyphicon glyphicon-arrow-down"></span></li>'));
			list.append($('<li>Humidity Shed		<span class="pull-right">50%		<span class="glyphicon glyphicon-arrow-up"></span></li>'));
			panel.append(list);
		}	
		{
			var row = $("#mainpanel").addRow();
			var cell = row.addCell(6);
			var panel = cell.addPanel("Cookies");
			
			//var chart = $('<div>', { id : 'graph1', 'height' : '400px', 'width' : '500px', 'outline' : '1px solid black' });
			//panel.append(chart);
			
		
			var cell = row.addCell(6);
			var panel  = cell.addPanel("Status info");

			//var chart = $('<div>', { id : 'graph2', 'height' : '400px', 'width' : '500px', 'outline' : '1px solid black' });
			//panel.append(chart);
		}	

		
	}
	else if(page == "triggers")
	{
		$("#mainpanel").empty();	
		$("#mainpanel").append($("<div>", {  'id' : 'blocklyDiv', 'style' : 'height: 800px; width: 1140px; background-color: gray'} ));
		$("#mainpanel").append($('<xml id="toolbox" style="display: none">\
			<block type="ontick"></block>\
			<block type="controls_if"></block>\
			<block type="logic_compare"></block>\
			<block type="controls_repeat_ext"></block>\
			<block type="math_number"></block>\
			<block type="math_arithmetic"></block>\
			<block type="text"></block>\
			<block type="text_print"></block>\
			<block type="sensor_value"></block>\
		</xml>'));


		var workspace = Blockly.inject('blocklyDiv',
        {		
        	media: 'blockly/media/',
        	toolbox: document.getElementById('toolbox')
        });

		var xmlData = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="ontick" id="10" x="135" y="83"><field name="tickCount">5</field><statement name="statements"><block type="controls_if" id="11"><value name="IF0"><block type="logic_compare" id="14"><field name="OP">NEQ</field><value name="A"><block type="sensor_value" id="15"><field name="NODE">1</field><field name="SENSOR">0</field></block></value><value name="B"><block type="math_number" id="16"><field name="NUM">0</field></block></value></block></value><statement name="DO0"><block type="text_print" id="12"><value name="TEXT"><block type="text" id="13"><field name="TEXT">Triggered!</field></block></value></block></statement></block></statement></block></xml>';
		var xmlDom = Blockly.Xml.textToDom(xmlData);
		workspace.clear();
  		Blockly.Xml.domToWorkspace(workspace, xmlDom);			


		var savedData;
		var btn = $('<button>');
		btn.append('save');
		btn.click(function()
		{
			 var xml = Blockly.Xml.workspaceToDom(workspace);
		    var text = Blockly.Xml.domToText(xml);
		    savedData = text;
		    alert(savedData);
		    console.log(savedData);
		});
		$("#mainpanel").append(btn);

		var btn = $('<button>');
		btn.append('load');
		btn.click(function()
		{
			var xmlDom = Blockly.Xml.textToDom(savedData);
			workspace.clear();
      		Blockly.Xml.domToWorkspace(workspace, xmlDom);			
		});
		$("#mainpanel").append(btn);

	}


	
}



function activate(d1, d2)
{

	

	if(d2.macro)
	{
		for(var i in d2.macro)
			activate(d1, d2.macro[i]);
		return;
	}
	
	var nodeId = 0;
	if(d1.nodeId)	nodeId = d1.nodeId;
	if(d2.nodeId)	nodeId = d2.nodeId;
	var sensorId = 0;
	if(d1.sensorId)	sensorId = d1.sensorId;
	if(d2.sensorId)	sensorId = d2.sensorId;

	if(nodeId == 0)
	{
		if(d2.command)
		{
			$.ajax({
				type: "POST",
				url:"http://192.168.2.12:8080/command",
				data: JSON.stringify({ 'command' : d2.command }),
			});
		}
		else
			alert("node 0 can only process commands...");
	}
	else
	{
		console.log("Setting sensor " + sensorId + " on node " + nodeId + " to value " + d2.value);
		$.ajax({
			type: "POST",
			url:"http://192.168.2.12:8080/activate/:" + nodeId,
			data: JSON.stringify({ 'sensor' : sensorId, 'value' : d2.value }),
		});
	}
	
	
}


var JsonFormatter = {
    stringify: function (cipherParams) {
        // create json object with ciphertext
        var jsonObj = {
            ct: cipherParams.ciphertext.toString(CryptoJS.enc.Base64)
        };

        // optionally add iv and salt
        if (cipherParams.iv) {
            jsonObj.iv = cipherParams.iv.toString();
        }
        if (cipherParams.salt) {
            jsonObj.s = cipherParams.salt.toString();
        }

        // stringify json object
        return JSON.stringify(jsonObj);
    },

    parse: function (jsonStr) {
        // parse json string
        var jsonObj = JSON.parse(jsonStr);

        // extract ciphertext from json object, and create cipher params object
        var cipherParams = CryptoJS.lib.CipherParams.create({
            ciphertext: CryptoJS.enc.Base64.parse(jsonObj.ct)
        });

        // optionally extract iv and salt
        if (jsonObj.iv) {
            cipherParams.iv = CryptoJS.enc.Hex.parse(jsonObj.iv)
        }
        if (jsonObj.s) {
            cipherParams.salt = CryptoJS.enc.Hex.parse(jsonObj.s)
        }

        return cipherParams;
    }
};
    


jQuery.fn.addRow = function()
{
	var row = $('<div>', { class : 'row' });
	$(this).append(row);
	return row;
}


jQuery.fn.addCell = function(width)
{
	var cell = $('<div>', { class : 'col-md-' + width });
	$(this).append(cell);
	return cell;
}


jQuery.fn.addPanel = function(titleText)
{
	var o = $(this[0]);
	var panel = $('<div>', { class : 'panel panel-default'});
	var header = $('<div>', { class : 'panel-heading' });
	var title = $('<h3>', { class : 'panel-title' });
	var b = $('<div>', { class : 'panel-body' });
	title.append(titleText);
	
	header.append(title);
	panel.append(header, b);
	
	
	$(this).append(panel);
	return b;
}

