load data local infile '/home/wayme/database_HW/final_project/Olympic_Event_Results.csv'
INTO TABLE Results
FIELDS TERMINATED BY ',' ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

/*
replace True with 1 , False with 0
*/
