load data local infile '/home/wayme/database_HW/final_project/best_record.csv'
INTO TABLE AthleteRecords
FIELDS TERMINATED BY ',' ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;
