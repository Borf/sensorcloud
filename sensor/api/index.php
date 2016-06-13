<?
$method = $_SERVER["REQUEST_METHOD"];
$uri = $_SERVER["REQUEST_URI"];
if($uri[strlen($uri)-1] == '/')
	$uri = substr($uri, 0, strlen($uri)-1);

header("Content-type: application/json");

$ch = curl_init();
curl_setopt($ch,CURLOPT_URL, "http://192.168.2.29" . $uri);

$result = curl_exec($ch);
curl_close($ch);
?>