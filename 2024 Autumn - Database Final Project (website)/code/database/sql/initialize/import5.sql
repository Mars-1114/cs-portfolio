load data local infile '/home/wayme/database_HW/final_project/Olympic_Games_Summary.csv'
INTO TABLE Games
FIELDS TERMINATED BY ',' ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;
