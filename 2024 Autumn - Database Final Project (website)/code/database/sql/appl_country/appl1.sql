
/*
Retrieve medal tally and rankings by points:
*/
SELECT country, country_noc, 
       SUM(gold * 3 + silver * 2 + bronze * 1) AS points,
       SUM(gold) AS gold, SUM(silver) AS silver, SUM(bronze) AS bronze
      
FROM Medal
GROUP BY country, country_noc
ORDER BY points DESC;


