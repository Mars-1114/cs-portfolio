load data local infile '\\database\\Olympic_Country_Profiles.csv'
INTO TABLE Country
FIELDS TERMINATED BY ',' ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;
/*
Hong Kong, China -> Hong Kong China
two ROC:
ROC	Russian Olympic Committee
ROC	ROC
/*
