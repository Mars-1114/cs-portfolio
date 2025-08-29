/* INFORMATION
** -----------
** This serves as a fail safe if init.php doesn't work as intended
** Open the MySQL shell and type "SOURCE file/location/reset.sql;"
** The query should build the entire table
*/

DROP DATABASE db;
CREATE DATABASE db;
USE db;

-- CREATE TABLES --
CREATE TABLE Athlete (  athlete_id INT PRIMARY KEY,
                        name VARCHAR(255),
                        sex VARCHAR(10),
                        born VARCHAR(80),
                        height VARCHAR(10),
                        weight VARCHAR(10),
                        country VARCHAR(255),
                        country_noc CHAR(3),
                        description TEXT,
                        special_notes TEXT);

CREATE TABLE Medal (edition_id VARCHAR(255),
                    edition VARCHAR(255),
                    year INT,
                    country VARCHAR(255),
                    country_noc CHAR(3),
                    gold INT DEFAULT 0,
                    silver INT DEFAULT 0,
                    bronze INT DEFAULT 0,
                    total INT GENERATED ALWAYS AS (gold + silver + bronze) STORED,
                    PRIMARY KEY (edition, country_noc));

CREATE TABLE Games (edition VARCHAR(255),
                    edition_id INT PRIMARY KEY,
                    edition_url VARCHAR(255),
                    year INT,
                    city VARCHAR(255),
                    country_flag_url VARCHAR(255),
                    country_noc CHAR(3),
                    start_date CHAR(25),
                    end_date CHAR(25),
                    competition_date VARCHAR(255),
                    isHeld VARCHAR(25));

CREATE TABLE Details (  edition VARCHAR(255),
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
                        FOREIGN KEY (athlete_id) REFERENCES Athlete(athlete_id));

CREATE TABLE Results (  result_id INT PRIMARY KEY,
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
                        FOREIGN KEY (edition_id) REFERENCES Games(edition_id));

CREATE TABLE Country (  noc CHAR(3) PRIMARY KEY,
                        country VARCHAR(255));

CREATE TABLE AthleteRecords (   sport VARCHAR(255),
                                athlete_id INT,
                                country VARCHAR(50),
                                name VARCHAR(255),
                                grade VARCHAR(20), -- Used VARCHAR for mixed formats (e.g., times, distances)
                                year YEAR,
                                ascend TINYINT, -- Indicates whether the record is ascending (0 or 1)
                                FOREIGN KEY (athlete_id) REFERENCES Athlete(athlete_id));

-- LOAD CSV FILE --
--# Replace the file path with their location in your PC #--
--# (Found Problem: file path must not contain Chinese characters ) #--

SET GLOBAL local_infile=1;

LOAD DATA LOCAL infile 'D:/dataset/Olympic_Country_Profiles.csv'
INTO TABLE Country
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

LOAD DATA LOCAL infile 'D:/dataset/Olympic_Medal_Tally_History.csv'
INTO TABLE Medal
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

LOAD DATA LOCAL infile 'D:/dataset/Olympic_Athlete_Biography.csv'
INTO TABLE Athlete
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

LOAD DATA LOCAL infile 'D:/dataset/Olympic_Games_Summary.csv'
INTO TABLE Games
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

LOAD DATA LOCAL infile 'D:/dataset/Olympic_Event_Results.csv'
INTO TABLE Results
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

LOAD DATA LOCAL infile 'D:/dataset/Olympic_Athlete_Event_Details.csv'
INTO TABLE Details
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

LOAD DATA LOCAL infile 'D:/dataset/Olympic_Record.csv'
INTO TABLE AthleteRecords
FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;