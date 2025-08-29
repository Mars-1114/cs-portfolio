load data local infile '/home/wayme/database_HW/final_project/Olympic_Athlete_Biography.csv'
INTO TABLE Athlete
FIELDS TERMINATED BY ',' ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;
