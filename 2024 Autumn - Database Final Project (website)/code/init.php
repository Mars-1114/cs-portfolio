<?php
    include "database_connection.php";

    // dataset full reset
    $reset = $_REQUEST['reset'];
    if ($reset == '1') {
        $conn->query("DROP DATABASE $database");
    }
    
    $conn->query("CREATE DATABASE IF NOT EXISTS $database");

    // link to database
    $conn->select_db($database);
    
    // create tables
    // NOTE: mysqli does not support (from what I know of) multi-query
    //       so, create table one by one

    // yea i only check one table since all of them should be created in one execution
    // (keep this in mind if you gonna modify the database structure)
    $table = $conn->query("SHOW TABLES LIKE 'Country'");
    $table_status = $table->num_rows > 0;

    $conn->query("CREATE TABLE IF NOT EXISTS Athlete (
                            -- will get the max+1 value in the table , when insert new athlete without athlete_id
                            athlete_id INT AUTO_INCREMENT PRIMARY KEY, 
                            name VARCHAR(255),
                            sex VARCHAR(10),
                            born VARCHAR(80),
                            height VARCHAR(10),
                            weight VARCHAR(10),
                            country VARCHAR(255),
                            country_noc CHAR(3),
                            description TEXT,
                            special_notes TEXT)");
    $conn->query("CREATE TABLE IF NOT EXISTS Medal (
                            edition VARCHAR(255),
                            edition_id VARCHAR(255),
                            year INT,
                            country VARCHAR(255),
                            country_noc CHAR(3),
                            gold INT DEFAULT 0,
                            silver INT DEFAULT 0,
                            bronze INT DEFAULT 0,
                            total INT GENERATED ALWAYS AS (gold + silver + bronze) STORED,
                            PRIMARY KEY (edition, country_noc))");
    $conn->query("CREATE TABLE IF NOT EXISTS Games (
                            edition VARCHAR(255),
                            edition_id INT PRIMARY KEY,
                            edition_url VARCHAR(255),
                            year INT,
                            city VARCHAR(255),
                            country_flag_url VARCHAR(255),
                            country_noc CHAR(3),
                            start_date CHAR(25),
                            end_date CHAR(25),
                            competition_date VARCHAR(255),
                            isHeld VARCHAR(25))");
    $conn->query("CREATE TABLE IF NOT EXISTS Details (
                            edition VARCHAR(255),
                            edition_id INT,
                            country_noc CHAR(3),   
                            sport VARCHAR(255),
                            event VARCHAR(255),
                            result_id INT,
                            athlete VARCHAR(255),
                            athlete_id INT,
                            pos VARCHAR(15),
                            medal VARCHAR(10) DEFAULT NULL,
                            isTeamSport BOOLEAN,
                            PRIMARY KEY (athlete_id, result_id),
                            FOREIGN KEY (edition_id) REFERENCES Games(edition_id),
                            FOREIGN KEY (athlete_id) REFERENCES Athlete(athlete_id))");
    $conn->query("CREATE TABLE IF NOT EXISTS Results (
                            result_id INT PRIMARY KEY,
                            event_title text,
                            edition VARCHAR(255),
                            edition_id INT,
                            sport VARCHAR(255),
                            sport_url VARCHAR(255),
                            result_date text,
                            result_location text,
                            result_participants VARCHAR(255),
                            result_format TEXT,
                            result_detail TEXT,
                            result_description TEXT,
                            FOREIGN KEY (edition_id) REFERENCES Games(edition_id))");
    $conn->query("CREATE TABLE IF NOT EXISTS Country (
                            noc CHAR(3) PRIMARY KEY,
                            country VARCHAR(255))");
    $conn->query("CREATE TABLE IF NOT EXISTS AthleteRecords (
                            sport VARCHAR(255),
                            result_id INT,
                            athlete_id INT,
                            country VARCHAR(50),
                            name VARCHAR(255),
                            grade VARCHAR(20), -- Used VARCHAR for mixed formats (e.g., times, distances)
                            year YEAR,
                            ascend TINYINT, -- Indicates whether the record is ascending (0 or 1)
                            
                            FOREIGN KEY (athlete_id) REFERENCES Athlete(athlete_id))");

    // only import when the table is newly created
    // (this takes a long time (~30s for my pc))
    if (!$table_status) {
        // import csv
        // NOTE: use a "pseudo-relative" path so user don't need to move csv files to some weird location
        //       also need to load one by one (i'm gonna cry)
        $dir = __DIR__."/database";
        $dir = str_replace('\\', '/', $dir);
    
        $conn->query("SET GLOBAL local_infile=1");
        $conn->query("load data local infile '$dir/Olympic_Country_Profiles.csv'
                             INTO TABLE Country
                             FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
                             LINES TERMINATED BY '\n'
                             IGNORE 1 ROWS");
        $conn->query("load data local infile '$dir/Olympic_Medal_Tally_History.csv'
                             INTO TABLE Medal
                             FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
                             LINES TERMINATED BY '\n'
                             IGNORE 1 ROWS");
        $conn->query("load data local infile '$dir/Olympic_Athlete_Biography.csv'
                             INTO TABLE Athlete
                             FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
                             LINES TERMINATED BY '\n'
                             IGNORE 1 ROWS");
        $conn->query("load data local infile '$dir/Olympic_Games_Summary.csv'
                             INTO TABLE Games
                             FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
                             LINES TERMINATED BY '\n'
                             IGNORE 1 ROWS");
        $conn->query("load data local infile '$dir/Olympic_Event_Results.csv'
                             INTO TABLE Results
                             FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
                             LINES TERMINATED BY '\n'
                             IGNORE 1 ROWS");
        $conn->query("load data local infile '$dir/Olympic_Athlete_Event_Details.csv'
                             INTO TABLE Details
                             FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
                             LINES TERMINATED BY '\n'
                             IGNORE 1 ROWS");
        $conn->query("load data local infile '$dir/Olympic_Record.csv'
                             INTO TABLE AthleteRecords
                             FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
                             LINES TERMINATED BY '\n'
                             IGNORE 1 ROWS");
    }

    // debug
    /*$debug = $conn->query("SELECT * FROM Country");
    if ($debug->num_rows > 0) {
        while ($row = $debug->fetch_assoc()) {
            echo "NOC: ".$row["noc"].", Country: ".$row["country"]."<br>";
        }
    }*/

    // close connection
    if ($reset == 1) {
        echo 'Database Reset Successfully';
    }
    $conn->close();
?>