<?
$config = json_decode(file_get_contents("../../config.json"), true);


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
if(isset($_SESSION["login"]))
$cruds = array(
	"nodes" => array(
		"table" => "nodes",
		"fields" => array("address", "name"),
		"key" => "address"
	),
	"transactions" => array(
		"table" => "transactions",
		"fields" => array("accountid", "date", "data"),
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
	
	$data = array();
	foreach($data2 as $time => $value)
	{
		$data[] = array("time" => $time, "data" => round(array_sum($value) / count($value), 2));
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
	
	return json_decode($postdata, true);
}


function showResult($type, $data)
{
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