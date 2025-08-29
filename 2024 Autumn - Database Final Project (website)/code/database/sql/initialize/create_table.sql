--updated: 12/19 Athlete table : website no need to input athlete_id for insert new athlete 

CREATE TABLE AthleteRecords (
    sport VARCHAR(255),
    result_id INT,
    athlete_id INT,
    country VARCHAR(50),
    name VARCHAR(255),
    grade VARCHAR(20), -- Used VARCHAR for mixed formats (e.g., times, distances)
    year YEAR,
    ascend TINYINT, -- Indicates whether the record is ascending (0 or 1)
    
    FOREIGN KEY (athlete_id) REFERENCES Athlete(athlete_id)
);



CREATE TABLE Athlete (
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
    special_notes TEXT
);
/*
year can't use type YEAR ,the min value of YEAR is 1901
*/
CREATE TABLE Medal (
    edition_id VARCHAR(255),
    edition VARCHAR(255),
    year INT,
    country VARCHAR(255),
    country_noc CHAR(3),
    gold INT DEFAULT 0,
    silver INT DEFAULT 0,
    bronze INT DEFAULT 0,
    total INT GENERATED ALWAYS AS (gold + silver + bronze) STORED,
    PRIMARY KEY (edition, country_noc)
);

CREATE TABLE Games (
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
    isHeld VARCHAR(25)
);


CREATE TABLE Details (
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
    FOREIGN KEY (athlete_id) REFERENCES Athlete(athlete_id)
    
);



CREATE TABLE Results (
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
    FOREIGN KEY (edition_id) REFERENCES Games(edition_id)
);

CREATE TABLE Country (
    noc CHAR(3) PRIMARY KEY,
    country VARCHAR(255)
);


