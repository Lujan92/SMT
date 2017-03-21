<?php

require_once dirname(__FILE__).'/PHPWord-master/src/PhpWord/Autoloader.php';
\PhpOffice\PhpWord\Autoloader::register();

use PhpOffice\PhpWord\TemplateProcessor;

$templateWord = new TemplateProcessor('plantilla.docx');
 
$nombre = $_POST['nombre'];
$tema = $_POST['tema'];
$escuela = $_POST['escuela'];

if($_POST['p1']<>""){
$pregunta_1 = "1.-".$_POST['p1'];
$I1 = $_POST['I1'];
$ra1 = "A)".$_POST['ra1'];
$rb1 = "B)".$_POST['rb1'];
$rc1 = "C)".$_POST['rc1'];
$rd1 = "D)".$_POST['rd1'];
$col_1 = $_POST['col_1'];
$col_11 = $_POST['col_11'];
}

if($_POST['p2']<>""){
$pregunta_2 = "2.-".$_POST['p2'];
$I2 = $_POST['I2'];
$ra2 = "A)".$_POST['ra2'];
$rb2 = "B)".$_POST['rb2'];
$rc2 = "C)".$_POST['rc2'];
$rd2 = "D)".$_POST['rd2'];}

if($_POST['p3']<>""){
$pregunta_3 = "3.-".$_POST['p3'];
$I3 = $_POST['I3'];
$ra3 = "A)".$_POST['ra3'];
$rb3 = "B)".$_POST['rb3'];
$rc3 = "C)".$_POST['rc3'];
$rd3 = "D)".$_POST['rd3'];}

if($_POST['p4']<>""){
$pregunta_4 = "4.-".$_POST['p4'];
$I4 = $_POST['I4'];
$ra4 = "A)".$_POST['ra4'];
$rb4 = "B)".$_POST['rb4'];
$rc4 = "C)".$_POST['rc4'];
$rd4 = "D)".$_POST['rd4'];}

if($_POST['p5']<>""){
$pregunta_5 = "5.-".$_POST['p5'];
$I5 = $_POST['I5'];
$ra5 = "A)".$_POST['ra5'];
$rb5 = "B)".$_POST['rb5'];
$rc5 = "C)".$_POST['rc5'];
$rd5 = "D)".$_POST['rd5'];}

if($_POST['p6']<>""){
$pregunta_6 = "6.-".$_POST['p6'];
$I6 = $_POST['I6'];
$ra6 = "A)".$_POST['ra6'];
$rb6 = "B)".$_POST['rb6'];
$rc6 = "C)".$_POST['rc6'];
$rd6 = "D)".$_POST['rd6'];}

if($_POST['p7']<>""){
$pregunta_7 = "7.-".$_POST['p7'];
$I7 = $_POST['I7'];
$ra7 = "A)".$_POST['ra7'];
$rb7 = "B)".$_POST['rb7'];
$rc7 = "C)".$_POST['rc7'];
$rd7 = "D)".$_POST['rd7'];}

if($_POST['p8']<>""){
$pregunta_8 = "8.-".$_POST['p8'];
$I8 = $_POST['I8'];
$ra8 = "A)".$_POST['ra8'];
$rb8 = "B)".$_POST['rb8'];
$rc8 = "C)".$_POST['rc8'];
$rd8 = "D)".$_POST['rd8'];}

if($_POST['p9']<>""){
$pregunta_9 = "9.-".$_POST['p9'];
$I9 = $_POST['I9'];
$ra9 = "A)".$_POST['ra9'];
$rb9 = "B)".$_POST['rb9'];
$rc9 = "C)".$_POST['rc9'];
$rd9 = "D)".$_POST['rd9'];}

if($_POST['p10']<>""){
$pregunta_10 = "10.-".$_POST['p10'];
$I10 = $_POST['I10'];
$ra10 = "A)".$_POST['ra10'];
$rb10 = "B)".$_POST['rb10'];
$rc10 = "C)".$_POST['rc10'];
$rd10 = "D)".$_POST['rd10'];}

if($_POST['p11']<>""){
$pregunta_11 = "11.-".$_POST['p11'];
$I11 = $_POST['I11'];
$ra11 = "A)".$_POST['ra11'];
$rb11 = "B)".$_POST['rb11'];
$rc11 = "C)".$_POST['rc11'];
$rd11 = "D)".$_POST['rd11'];}

if($_POST['p12']<>""){
$pregunta_12 = "12.-".$_POST['p12'];
$I12 = $_POST['I12'];
$ra12 = "A)".$_POST['ra12'];
$rb12 = "B)".$_POST['rb12'];
$rc12 = "C)".$_POST['rc12'];
$rd12 = "D)".$_POST['rd12'];}

if($_POST['p13']<>""){
$pregunta_13 = "13.-".$_POST['p13'];
$I13 = $_POST['I13'];
$ra13 = "A)".$_POST['ra13'];
$rb13 = "B)".$_POST['rb13'];
$rc13 = "C)".$_POST['rc13'];
$rd13 = "D)".$_POST['rd13'];}

if($_POST['p14']<>""){
$pregunta_14 = "14.-".$_POST['p14'];
$I14 = $_POST['I14'];
$ra14 = "A)".$_POST['ra14'];
$rb14 = "B)".$_POST['rb14'];
$rc14 = "C)".$_POST['rc14'];
$rd14 = "D)".$_POST['rd14'];}

if($_POST['p15']<>""){
$pregunta_15 = "15.-".$_POST['p15'];
$I15 = $_POST['I15'];
$ra15 = "A)".$_POST['ra15'];
$rb15 = "B)".$_POST['rb15'];
$rc15 = "C)".$_POST['rc15'];
$rd15 = "D)".$_POST['rd15'];}

if($_POST['p16']<>""){
$pregunta_16 = "16.-".$_POST['p16'];
$I16 = $_POST['I16'];
$ra16 = "A)".$_POST['ra16'];
$rb16 = "B)".$_POST['rb16'];
$rc16 = "C)".$_POST['rc16'];
$rd16 = "D)".$_POST['rd16'];}

if($_POST['p17']<>""){
$pregunta_17 = "17.-".$_POST['p17'];
$I17 = $_POST['I17'];
$ra17 = "A)".$_POST['ra17'];
$rb17 = "B)".$_POST['rb17'];
$rc17 = "C)".$_POST['rc17'];
$rd17 = "D)".$_POST['rd17'];}

if($_POST['p19']<>""){
$pregunta_19 = "19.-".$_POST['p19'];
$I19 = $_POST['I19'];
$ra19 = "A)".$_POST['ra19'];
$rb19 = "B)".$_POST['rb19'];
$rc19 = "C)".$_POST['rc19'];
$rd19 = "D)".$_POST['rd19'];}

if($_POST['p20']<>""){
$pregunta_20 = "20.-".$_POST['p20'];
$I20 = $_POST['I20'];
$ra20 = "A)".$_POST['ra20'];
$rb20 = "B)".$_POST['rb20'];
$rc20 = "C)".$_POST['rc20'];
$rd20 = "D)".$_POST['rd20'];}

if($_POST['p18']<>""){
$pregunta_18 = "18.-".$_POST['p18'];
$I18 = $_POST['I18'];
$ra18 = "A)".$_POST['ra18'];
$rb18 = "B)".$_POST['rb18'];
$rc18 = "C)".$_POST['rc18'];
$rd18 = "D)".$_POST['rd18'];}


// --- Asignamos valores a la plantilla
$templateWord->setValue('profesor',$nombre);
$templateWord->setValue('tema',$tema);
$templateWord->setValue('escuela',$escuela);



$templateWord->setValue('pregunta_1',$pregunta_1);
$templateWord->setValue('I1',$I1);
$templateWord->setValue('ra1',$ra1);
$templateWord->setValue('rb1',$rb1);
$templateWord->setValue('rc1',$rc1);
$templateWord->setValue('rd1',$rd1);
$templateWord->setValue('col_1',$col_1);
$templateWord->setValue('col_11',$col_11);

$templateWord->setValue('pregunta_2',$pregunta_2);
$templateWord->setValue('I2',$I2);
$templateWord->setValue('ra2',$ra2);
$templateWord->setValue('rb2',$rb2);
$templateWord->setValue('rc2',$rc2);
$templateWord->setValue('rd2',$rd2);
$templateWord->setValue('col_2',$col_2);
$templateWord->setValue('col_22',$col_22);

$templateWord->setValue('pregunta_3',$pregunta_3);
$templateWord->setValue('I3',$I3);
$templateWord->setValue('ra3',$ra3);
$templateWord->setValue('rb3',$rb3);
$templateWord->setValue('rc3',$rc3);
$templateWord->setValue('rd3',$rd3);
$templateWord->setValue('col3',$col3);
$templateWord->setValue('col33',$col33);

$templateWord->setValue('pregunta_4',$pregunta_4);
$templateWord->setValue('I4',$I4);
$templateWord->setValue('ra4',$ra4);
$templateWord->setValue('rb4',$rb4);
$templateWord->setValue('rc4',$rc4);
$templateWord->setValue('rd4',$rd4);
$templateWord->setValue('col4',$col4);
$templateWord->setValue('col44',$col44);

$templateWord->setValue('pregunta_5',$pregunta_5);
$templateWord->setValue('I5',$I5);
$templateWord->setValue('ra5',$ra5);
$templateWord->setValue('rb5',$rb5);
$templateWord->setValue('rc5',$rc5);
$templateWord->setValue('rd5',$rd5);
$templateWord->setValue('col5',$col5);
$templateWord->setValue('col55',$col55);

$templateWord->setValue('pregunta_6',$pregunta_6);
$templateWord->setValue('I6',$I6);
$templateWord->setValue('ra6',$ra6);
$templateWord->setValue('rb6',$rb6);
$templateWord->setValue('rc6',$rc6);
$templateWord->setValue('rd6',$rd6);
$templateWord->setValue('col6',$col6);
$templateWord->setValue('col66',$col66);

$templateWord->setValue('pregunta_7',$pregunta_7);
$templateWord->setValue('I7',$I7);
$templateWord->setValue('ra7',$ra7);
$templateWord->setValue('rb7',$rb7);
$templateWord->setValue('rc7',$rc7);
$templateWord->setValue('rd7',$rd7);
$templateWord->setValue('col7',$col7);
$templateWord->setValue('col77',$col77);

$templateWord->setValue('pregunta_8',$pregunta_8);
$templateWord->setValue('I8',$I8);
$templateWord->setValue('ra8',$ra8);
$templateWord->setValue('rb8',$rb8);
$templateWord->setValue('rc8',$rc8);
$templateWord->setValue('rd8',$rd8);
$templateWord->setValue('col8',$col8);
$templateWord->setValue('col88',$col88);

$templateWord->setValue('pregunta_9',$pregunta_9);
$templateWord->setValue('I9',$I9);
$templateWord->setValue('ra9',$ra9);
$templateWord->setValue('rb9',$rb9);
$templateWord->setValue('rc9',$rc9);
$templateWord->setValue('rd9',$rd9);
$templateWord->setValue('col9',$col9);
$templateWord->setValue('col99',$col99);

$templateWord->setValue('pregunta_10',$pregunta_10);
$templateWord->setValue('I10',$I10);
$templateWord->setValue('ra10',$ra10);
$templateWord->setValue('rb10',$rb10);
$templateWord->setValue('rc10',$rc10);
$templateWord->setValue('rd10',$rd10);
$templateWord->setValue('col10',$col10);
$templateWord->setValue('col1010',$col1010);

$templateWord->setValue('pregunta_11',$pregunta_11);
$templateWord->setValue('I11',$I11);
$templateWord->setValue('ra11',$ra11);
$templateWord->setValue('rb11',$rb11);
$templateWord->setValue('rc11',$rc11);
$templateWord->setValue('rd11',$rd11);
$templateWord->setValue('col11',$col11);
$templateWord->setValue('col11',$col11);

$templateWord->setValue('pregunta_12',$pregunta_12);
$templateWord->setValue('I12',$I12);
$templateWord->setValue('ra12',$ra12);
$templateWord->setValue('rb12',$rb12);
$templateWord->setValue('rc12',$rc12);
$templateWord->setValue('rd12',$rd12);
$templateWord->setValue('col12',$col12);
$templateWord->setValue('col1212',$col1212);

$templateWord->setValue('pregunta_13',$pregunta_13);
$templateWord->setValue('I13',$I13);
$templateWord->setValue('ra13',$ra13);
$templateWord->setValue('rb13',$rb13);
$templateWord->setValue('rc13',$rc13);
$templateWord->setValue('rd13',$rd13);
$templateWord->setValue('col13',$col13);
$templateWord->setValue('col1313',$col1313);

$templateWord->setValue('pregunta_14',$pregunta_14);
$templateWord->setValue('I14',$I14);
$templateWord->setValue('ra14',$ra14);
$templateWord->setValue('rb14',$rb14);
$templateWord->setValue('rc14',$rc14);
$templateWord->setValue('rd14',$rd14);
$templateWord->setValue('col14',$col14);
$templateWord->setValue('col1414',$col1414);

$templateWord->setValue('pregunta_15',$pregunta_15);
$templateWord->setValue('I15',$I15);
$templateWord->setValue('ra15',$ra15);
$templateWord->setValue('rb15',$rb15);
$templateWord->setValue('rc15',$rc15);
$templateWord->setValue('rd15',$rd15);
$templateWord->setValue('col15',$col15);
$templateWord->setValue('col1515',$col1515);

$templateWord->setValue('pregunta_16',$pregunta_16);
$templateWord->setValue('I16',$I16);
$templateWord->setValue('ra16',$ra16);
$templateWord->setValue('rb16',$rb16);
$templateWord->setValue('rc16',$rc16);
$templateWord->setValue('rd16',$rd16);
$templateWord->setValue('col16',$col16);
$templateWord->setValue('col1616',$col1616);

$templateWord->setValue('pregunta_17',$pregunta_17);
$templateWord->setValue('I17',$I17);
$templateWord->setValue('ra17',$ra17);
$templateWord->setValue('rb17',$rb17);
$templateWord->setValue('rc17',$rc17);
$templateWord->setValue('rd17',$rd17);
$templateWord->setValue('col17',$col17);
$templateWord->setValue('col1717',$col1717);

$templateWord->setValue('pregunta_18',$pregunta_18);
$templateWord->setValue('I18',$I18);
$templateWord->setValue('ra18',$ra18);
$templateWord->setValue('rb18',$rb18);
$templateWord->setValue('rc18',$rc18);
$templateWord->setValue('rd18',$rd18);
$templateWord->setValue('col18',$col18);
$templateWord->setValue('col1818',$col1818);

$templateWord->setValue('pregunta_19',$pregunta_19);
$templateWord->setValue('I19',$I19);
$templateWord->setValue('ra19',$ra19);
$templateWord->setValue('rb19',$rb19);
$templateWord->setValue('rc19',$rc19);
$templateWord->setValue('rd19',$rd19);
$templateWord->setValue('col19',$col19);
$templateWord->setValue('col1919',$col1919);

$templateWord->setValue('pregunta_20',$pregunta_20);
$templateWord->setValue('I20',$I20);
$templateWord->setValue('ra20',$ra20);
$templateWord->setValue('rb20',$rb20);
$templateWord->setValue('rc20',$rc20);
$templateWord->setValue('rd20',$rd20);
$templateWord->setValue('col20',$col20);
$templateWord->setValue('col2020',$col2020);
// --- Guardamos el documento
$templateWord->saveAs('Documento02.docx');

header("Content-Disposition: attachment; filename=Documento02.docx; charset=iso-8859-1");
echo file_get_contents('Documento02.docx');
        
?>