<?
$config = json_decode(file_get_contents("../config.json"), true);

$db = mysqli_connect($config["mysql"]["host"], $config["mysql"]["user"], $config["mysql"]["pass"]) or die(mysqli_error($db));;;
mysqli_select_db($db, $config["mysql"]["daba"]) or die(mysqli_error($db));
session_start();


if(isset($_SERVER["PATH_INFO"]))
{
	$page = explode("/", $_SERVER["PATH_INFO"]);
	unset($page[0]);
	foreach($page as $id => $name)
		if(empty($name) && $name !== "0")
			unset($page[$id]);
	$page = array_values($page);
}
else
	die("404 not found");
//if(isset($_SESSION["login"]))
$cruds = array(
	"nodes" => array(
		"table" => "nodes",
		"fields" => array("id", "name", "hwid"),
		"key" => "id"
	),
	"sensors" => array(
		"table" => "sensors",
		"fields" => array("id", "nodeid", "type", "config"),
		"fieldconfig" => array("config" => "json"),
		"key" => "id",

	),
);

$returnType = "json";
$method = $_SERVER["REQUEST_METHOD"];

$types = array("json", "xml", "csv", "url", "txt");

if(in_array($page[0], $types ))
{
	$returnType = $page[0];
	unset($page[0]);
	$page = array_values($page);
}

if(isset($cruds[$page[0]]))
{
	$crud = $cruds[$page[0]];
	if(!isset($page[1]) && $method == "GET")
	{
		$result = [];


		$sql = "SELECT * FROM `" . $crud["table"] . "` WHERE TRUE";
		if(isset($crud["sql"]))
			$sql .= $crud["sql"];

		$res = mysqli_query($db, $sql) or die(mysqli_error($db));
		while($line = mysqli_fetch_assoc($res))
		{
			$l = array();
			foreach($crud["fields"] as $f)
				$l[$f] = $line[$f];
			$result[] = $l;
		}
		showResult($returnType, $result);
	}
	if($method == "GET" && $page[1][0] == ":")
	{
		$keys = explode(",", $crud["key"]);
		$ids = explode(",", substr($page[1],1));
		$ids = array_map(function($i) { return (int)$i; }, $ids);

		$sql = "SELECT * FROM `" . $crud["table"] . "` WHERE TRUE ";
		for($i = 0; $i < count($keys); $i++)
			$sql .= " AND `" . $keys[$i] . "` = " . (int)$ids[$i];

		if(isset($crud["sql"]))
			$sql .= $crud["sql"];

		$res = mysqli_query($db, $sql) or die(mysqli_error($db));
		if($line = mysqli_fetch_assoc($res))
			showResult($returnType, $line);
		else
			exit();
	}
	else
	{
		if(strpos($page[1], ":") != -1)
		{
			$key = explode(":", $page[1])[0];
			$val = explode(":", $page[1])[1];

			if(!in_array($key, $crud["fields"]))
				die("Error: " . $key . " is not a field");

			$sql = "SELECT * FROM `" . $crud["table"] . "` WHERE `" . mysqli_escape_string($db, $key) . "` = '" . mysqli_escape_string($db, $val) . "'";
			$result = array();
			$res = mysqli_query($db, $sql) or die(mysqli_error($db));
			while($line = mysqli_fetch_assoc($res))
			{
				$l = array();
				foreach($crud["fields"] as $f)
				{
					if(isset($crud["fieldconfig"][$f]))
					{
						if($crud["fieldconfig"][$f] == "json")
							$l[$f] = json_decode($line[$f]);
					}
					else
						$l[$f] = $line[$f];
				}
				$result[] = $l;
			}
			showResult($returnType, array("res" => "ok", "data" => $result));


		}
	}
	if($method == "POST")
	{
		$data = decodeData();

		handlePost($data);
		echo "ok";
		exit();

	}

	print_r($crud);

	die();

}
if($page[0] == "login")
{
	$data = decodeData();
	if($data["username"] == "borf" && $data["password"] == "borf")
	{
		$_SESSION["login"] = true;
		die(showResult("json", array("auth" => true)));
	}
	else
	{
		$_SESSION["login"] = false;
		die(showResult("json", array("auth" => false)));
	}

}


if($page[0] == "report")
{
	if($page[1][0] == ":")
	{
		$id = (int)substr($page[1],1);
		$data = decodeData();
//		echo json_encode($data);
		foreach($data as $row)
		{
			$sensorid = $row["id"];
			unset($row["id"]);
			foreach($row as $key => $value)
			{
				$type = "";
				if($key == "temperature")
					$type = "TEMPERATURE";
				else if($key == "humidity")
					$type = "HUMIDITY";
				else if($key == "switch")
					$type = "SWITCH";
				else if($key == "value")
					$type = "VALUE";
				else
					die("Unknown sensor value");

				$sql = "INSERT INTO `sensordata` (`stamp`, `sensorid`, `type`, `value`) VALUES (NOW(3), ".(int)$sensorid.", '".mysqli_escape_string($db, $type)."', ".(float)$value.")";
				mysqli_query($db, $sql) or die(mysqli_error($db));
			}
		}
		echo '{ "res" : "ok" }';

	}
}
if($page[0] == "ping")
{
	if($page[1][0] == ":")
	{
		$id = (int)substr($page[1],1);
		$data = decodeData();

		$ip = $_SERVER["REMOTE_ADDR"];
		if(isset($data["ip"]))
			$ip = mysqli_escape_string($db, $data["ip"]);

		$sql = "INSERT INTO `pings` (`nodeid`, `ip`, `heapspace`, `rssi`) VALUES (".(int)$id.", '".$ip."', ".(int)$data["heapspace"].", " . (int)$data["rssi"]. ")";
		mysqli_query($db, $sql) or die(mysqli_error($db));
		echo '{ "res" : "ok" }';

	}
}
if($page[0] == "sensordata")
{
	$key = explode(":", $page[1])[0];
	$val = explode(":", $page[1])[1];

	if($val == "temperature")
		$type = "TEMPERATURE";
	else if($val == "humidity")
		$type = "HUMIDITY";


	$sql = "select round(avg(`value`)*10)/10 as `value`, `name`, date_format(min(stamp), '%Y-%m-%d %k:%i') as `time`, round(unix_timestamp(`stamp`)/60/10) as `groupie` from `sensordata`
		LEFT JOIN `sensors` ON `sensors`.`id` = `sensordata`.`sensorid`
		LEFT JOIN `nodes` ON `nodes`.`id` = `sensors`.`nodeid`
		where `sensordata`.`type` = '".$type."' AND `stamp` >= now() - interval 2 day
		group by `name`,`groupie`
			 order by `stamp` DESC, `name`";

	$data = array();
	$res = mysqli_query($db, $sql) or die(mysqli_error($db));
	while($line = mysqli_fetch_assoc($res))
		$data[$line["time"]][$line["name"]] = $line["value"];

	$returnData = array("nodes" => array(), "data" => array());
	foreach($data as $time => $value)
	{
		$returnData["nodes"] = array_values(array_unique(array_merge(array_keys($value), $returnData["nodes"])));
		$value["time"] = $time;
		$returnData["data"][] = $value;
	}

	showResult("$returnType", $returnData);
	exit();
}




















if($page[0] == "fullnode")
{
	$id = (int)substr($page[1],1);
	$res = mysqli_query($db, "SELECT * FROM `nodes` WHERE `address` = " . $id) or die(mysqli_error($db));
	$node = mysqli_fetch_assoc($res);
	$sensors = array();
	$res = mysqli_query($db, "SELECT * FROM `sensors` WHERE `node` = " . $id) or die(mysqli_error($db));
	while($line = mysqli_fetch_assoc($res))
		$sensors[] = $line;


	$data = array();

	$data[] = $node["name"];
	$data[] = count($sensors);
	for($i = 0; $i < count($sensors); $i++)
	{
		$data[] = $sensors[$i]["type"];
		$data[] = $sensors[$i]["pin"];
		$data[] = $sensors[$i]["value1"];
		$data[] = $sensors[$i]["value2"];
		$data[] = $sensors[$i]["value3"];
	}
	echo implode(",", $data);
	echo "\n";
	die();
}

if($page[0] == "current")
{
	$res = mysqli_query($db, "(SELECT `data` FROM `data` WHERE `nodeid` = 2 AND `sensorid` =  3 ORDER BY `date` DESC LIMIT 1) UNION (SELECT avg(`data`) FROM `data` WHERE `nodeid` = 2 AND `sensorid` = 3 AND `date` > NOW() - 300)") or die(mysqli_error($db));
	$temp = mysqli_fetch_assoc($res);
	$avg = mysqli_fetch_assoc($res);
	$data["insidetemp"] = $temp["data"];
	$data["insidetempAvg"] = $avg["data"];
	showResult($returnType, $data);
	exit();
}

if($page[0] == "nodelist")
{
	//SELECT * FROM nodes n LEFT JOIN (SELECT distinct nodeid, ip, stamp, hwid FROM pings p order by stamp desc limit 1) as bla ON bla.nodeid = n.id
/*	$res = mysqli_query($db, "SELECT
								`nodes`.*,
								(SELECT `stamp` FROM `pings` WHERE `pings`.`nodeid` = `id` ORDER BY `stamp` DESC LIMIT 1) as `stamp`,
								(SELECT `hwid` FROM `pings` WHERE `pings`.`nodeid` = `id` ORDER BY `stamp` DESC LIMIT 1) as `hwid`,
								(SELECT `ip` FROM `pings` WHERE `pings`.`nodeid` = `id` ORDER BY `stamp` DESC LIMIT 1) as `ip`,
								(SELECT `heapspace` FROM `pings` WHERE `pings`.`nodeid` = `id` ORDER BY `stamp` DESC LIMIT 1) as `heapspace`
								FROM `nodes`") or die(mysqli_error($db));*/


$res = mysqli_query($db, "SELECT
 n.id,
    n.name,
    n.hwid,
    p.stamp,
    p.ip,
    p.heapspace,
    p.rssi,
    sen.stamp as lastsensordata,
    rooms.name as room,
    (select count(*) from `sensors` LEFT JOIN `sensortypes` ON `sensortypes`.`id` = `sensors`.`type` WHERE `sensors`.`nodeid` = `n`.`id` AND `sensortypes`.`name` LIKE 'SEN%') as `sensorcount`,
    (select count(*) from `sensors` LEFT JOIN `sensortypes` ON `sensortypes`.`id` = `sensors`.`type` WHERE `sensors`.`nodeid` = `n`.`id` AND `sensortypes`.`name` LIKE 'ACT%') as `actcount`
FROM
 nodes AS n
LEFT JOIN (
 select `nodeid`, `stamp`, `ip`, `heapspace`,`rssi` from `pings` as `p1`
 	inner join (
 		select MAX(`p3`.`id`) as `id` from `pings` as `p3` group by `nodeid`
 	) as `p2`
 	ON `p1`.`id` = `p2`.`id`
) as `p`
ON `n`.`id` = `p`.`nodeid`
LEFT JOIN (
select `nodeid`, max(`stamp`) as `stamp` from `sensors` left join (
    select `sensorid`, `stamp` from `sensordata` as `s1` INNER JOIN (
    	select MAX(`s3`.`id`) as `id` from
    	`sensordata` as `s3` group by `sensorid`
    	) as `s2`
    	ON `s1`.`id` = `s2`.`id`
    ) as `s`
ON `s`.`sensorid` = `sensors`.`id`
GROUP BY `nodeid`
) as `sen`
ON `sen`.`nodeid` = `n`.`id`
left join `rooms` on `rooms`.`id` = `n`.`room`

") or die(mysqli_error($db));
	$data = array();
	while($line = mysqli_fetch_assoc($res))
		$data[] = $line;
	showResult($returnType, $data);
	exit();
}

if($page[0] == "roomlist")
{
	$res = mysqli_query($db, "
		SELECT
			`rooms`.`id` as `roomid`,
			`rooms`.`name` as `roomname`,
			`rooms`.`area` as `area`,
			`nodes`.`id` as `nodeid`,
			`nodes`.`name` as `nodename`,
			`sensors`.`id` as `sensorid`,
			`sensors`.`type` as `sensortype`,
			`sensortypes`.`name` as `sensorname`
		FROM `rooms`
		LEFT JOIN `nodes` ON `nodes`.`room` = `rooms`.`id`
		LEFT JOIN `sensors` ON `sensors`.`nodeid` = `nodes`.`id`
		LEFT JOIN `sensortypes` ON `sensors`.`type` = `sensortypes`.`id`

		ORDER BY `rooms`.`id`, `nodes`.`id`") or die(mysqli_error($db));

	$data = array();
	$room = array();
	$node = array();
	$lastRoom = "";
	$lastNode = "";
	while($line = mysqli_fetch_assoc($res))
	{
		if($lastNode != $line["nodeid"])
		{
			if(count($node) > 0)
				$room["nodes"][] = $node;
			$node = array();
		}

		if($lastRoom != $line["roomid"])
		{
			if($lastRoom != "")
			{
				if(count($node) > 0)
					$room["nodes"][] = $node;
				$data[] = $room;
				$room = array();
				$node = array();
			}
		}

		$room["name"] = $line["roomname"];
		$room["area"] = $line["area"];
		$room["id"] = $line["roomid"];

		if(!is_null($line["nodename"]))
		{
			$node["name"] = $line["nodename"];
			$node["id"] = $line["nodeid"];
		}
		if(!is_null($line["sensortype"]))
		{
			$node["sensors"][] = array("id" => $line["sensorid"], "type" => $line["sensorname"]);
		}


		$lastRoom = $line["roomid"];
		$lastNode = $line["nodeid"];
	}

	if(count($node) > 0)
		$room["nodes"][] = $node;
	$data[] = $room;


	showResult($returnType, $data);
	exit();
}




if($page[0] == "temp")
{
	$res = mysqli_query($db, "SELECT * FROM (SELECT date_format(`date`, '%Y-%m-%d %H') as `d`, `nodeid`, `sensorid`, avg(`data`)  as `temp` FROM `data` WHERE `nodeid` = 1 GROUP BY `d`, `sensorid` ORDER BY `d` DESC LIMIT 400) as `bla` ORDER BY `d` ASC") or die(mysqli_error($db));
	$data = array("0" => array(), "1" => array(), "2" => array(), "3" => array());
	while($line = mysqli_fetch_assoc($res))
		$data[$line["sensorid"]][] = array("time" => $line["d"], "data" => $line["temp"]);

		showResult($returnType, $data);
	exit();
}
if($page[0] == "temp2")
{
	$res = mysqli_query($db, "SELECT * FROM (SELECT date_format(`date`, '%Y-%m-%d %H %i') as `d`, `nodeid`, `sensorid`, `data`, avg(`data`)  as `temp` FROM `data` WHERE `nodeid` = 2 AND `sensorid` = 0 GROUP BY `d` ORDER BY `d` DESC LIMIT 100) as `bla` ORDER BY `d` ASC") or die(mysqli_error($db));
	$data = array();
	while($line = mysqli_fetch_assoc($res))
		$data[] = array("time" => $line["d"], "data" => $line["data"]);
	showResult($returnType, $data);
	exit();
}

if($page[0] == "last24")
{
	$res = mysqli_query($db, "SELECT * FROM (SELECT date_format(`date`, '%Y-%m-%dT%H:%i:00') as `d`, `nodeid`, `sensorid`, `data`, avg(`data`)  as `temp` FROM `data` WHERE `nodeid` = 2 AND `sensorid` = 3 AND `date` > DATE_SUB(NOW(), INTERVAL 24 HOUR) GROUP BY `d` ORDER BY `d`) as `bla` ORDER BY `d` ASC") or die(mysqli_error($db));
	$data = array();
	while($line = mysqli_fetch_assoc($res))
		$data[] = array("time" => $line["d"], "data" => $line["data"]);

	$data2 = array();
	foreach($data as $t)
	{
		$group = substr($t["time"], 0, 14) . sprintf("%02d", 10*(int)(((int)substr($t["time"], 14, 2) / 10))) . ":00";
		$data2[$group][] = $t["data"];
	}


	$res = mysqli_query($db, "SELECT * FROM (SELECT date_format(`date`, '%Y-%m-%dT%H:%i:00') as `d`, `nodeid`, `sensorid`, `data`, avg(`data`)  as `temp` FROM `data` WHERE `nodeid` = 3 AND `sensorid` = 3 AND `date` > DATE_SUB(NOW(), INTERVAL 24 HOUR) GROUP BY `d` ORDER BY `d`) as `bla` ORDER BY `d` ASC") or die(mysqli_error($db));
	$data = array();
	while($line = mysqli_fetch_assoc($res))
		$data[] = array("time" => $line["d"], "data" => $line["data"]);

	$data3 = array();
	foreach($data as $t)
	{
		$group = substr($t["time"], 0, 14) . sprintf("%02d", 10*(int)(((int)substr($t["time"], 14, 2) / 10))) . ":00";
		$data3[$group][] = $t["data"];
	}


	$data = array();
	foreach($data2 as $time => $value)
	{
		$temp2 = null;
		if(isset($data3[$time]))
			$temp2 = round(array_sum($data3[$time]) / count($data3[$time]), 2);

		$data[] = array("time" => $time, "temp1" => round(array_sum($value) / count($value), 2),
		"temp2" => $temp2);
	}
	showResult($returnType, $data);
	exit();



}

if($page[0] == "list")
{
	readfile("http://192.168.2.12:8080/list");
	exit();
}
if($page[0] == "actuators")
{
	showResult($returnType, $config["actuators"]);
}



function decodeData()
{
	$postdata = file_get_contents("php://input");

	return json_decode($postdata, true, 512, JSON_BIGINT_AS_STRING);
}


function showResult($type, $data)
{
	header("Access-Control-Allow-Origin: *");
	header("Access-Control-Allow-Methods: GET,POST,PUT");
	header("Access-Control-Allow-Headers: Content-Type");
	if($type == "json")
	{
		header("Content-type: application/json");
		echo json_encode($data, JSON_NUMERIC_CHECK);
	}
	else if($type == "xml")
	{
		echo "not implemented";
	}
	else if ($type == "url")
	{
		echo http_build_query($data);
	}
	else if ($type == "csv")
	{
		foreach($data as $key => $value)
			echo $value . ",";
	}


	exit();
}






function handlePost($data)
{
	global $crud;
	global $db;
	if(isset($data[0]))
	{
		for($i = 0; $i < count($data); $i++)
			handlePost($data[$i]);
		return;
	}

	foreach($crud["fields"] as $field)
		if(!isset($data[$field]))
			die($field . " is not set");

	$sql = "";
	if(isset($page[1]))
		$sql = "UPDATE `" . $crud["table"] . "` SET ";
	else
		$sql = "INSERT INTO `" . $crud["table"] . "` SET ";

	foreach($crud["fields"] as $field)
		$sql .= "`" . $field . "` = '" . mysqli_escape_string($db, $data[$field]) . "', ";
	$sql = substr($sql, 0, -2);

	if(isset($page[1]))
	{
		$sql .= " WHERE TRUE ";
		$keys = explode(",", $crud["key"]);
		$ids = explode(",", substr($page[1],1));
		$ids = array_map(function($i) { return (int)$i; }, $ids);

		for($i = 0; $i < count($keys); $i++)
			$sql .= " AND `" . $keys[$i] . "` = " . (int)$ids[$i];

	}
	mysqli_query($db, $sql) or die($sql . "\n" . mysqli_error($db));
}




?>
