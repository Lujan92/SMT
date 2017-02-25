<?php 


//Conexion a base de datos
$host='smts.cjup94k2g7mf.us-east-1.rds.amazonaws.com'; // para conectarnos a localhost o el ip del servidor de postgres
$db='SMTs';// seleccionar la base de datos que vamos a utilizar
$port='5432';//Puerto
$user='smts'; // seleccionar el usuario con el que nos vamos a conectar
$pass='JRG9Jx$UDtOGXG5hQjfR8*ta.W0vkl1C2JAMk%Vg9,Ywmfd1!SOD9?IX8fC99Gye'; // la clave del usuario
$conexionstring="host=$host port=$port dbname=$db user=$user password=$pass";  //donde se guardara la conexión

$dbcon=pg_connect($conexionstring) or die ('
	No se ha podido conectar a la base de datos: ' .pg_last_error());

$query= 'SELECT * FROM "Grupos";';
$result= pg_query($query) or die ('No se pudieron obtener registros: ' .pg_last_error());

$json = array();
while ($row = pg_fetch_assoc($result)) {
 $json['Grupos'] []=$row;
}

pg_close($dbcon);

echo json_encode($json);


 ?>