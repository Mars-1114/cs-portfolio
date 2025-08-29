load data local infile '/home/wayme/database_HW/final_project/Olympic_Athlete_Event_Details.csv'
INTO TABLE Details
FIELDS TERMINATED BY ',' ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

/*
replace True with 1 , False with 0
*/
