load data local infile '/home/wayme/database_HW/final_project/Olympic_Medal_Tally_History.csv'
INTO TABLE Medal
FIELDS TERMINATED BY ',' ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;
