<?php
    ##############################################
    ##  IMPORTANT INFO FOR DATABASE CONNECTION  ##
    ##############################################
    // here the password is empty (you may want to change it, or just set yours to empty as well)
    //     -> run "SET PASSWORD FOR root@localhost='';" in sql and restart the server
    // you -should not- need to change the servername and username, as they're the default names for a local server
    // the name for database is whatever tbh
    
    $servername = "localhost";
    $username = "root";
    $password = "";
    $database = "db";

    // create connection
    $conn = new mysqli($servername, $username, $password, $database);

    // Check connection
    if (!$conn) {
        die("Connection failed: " . mysqli_connect_error());
    }
?>
