<?php
include("../db.php");

$data = explode(",", $_POST["data"], 3);

mysqli_query($db, 
	"INSERT INTO `data` 
		(`date`, `nodeid`, `sensorid`, `data`) 
			VALUES 
		(NOW(), ".(int)$data[0].", " . (int)$data[1] . ", '".mysqli_real_escape_string($db, $data[2])."')") or die(mysqli_error($db));

?>