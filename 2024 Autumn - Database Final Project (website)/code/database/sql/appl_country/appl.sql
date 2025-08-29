-- some test sql not final
/*
Retrieve medal tally and rankings by points:
*/
SELECT country, country_noc, 
       
       SUM(gold) AS gold, SUM(silver) AS silver, SUM(bronze) AS bronze,
      SUM(gold * 3 + silver * 2 + bronze * 1) AS points
FROM Medal
GROUP BY country, country_noc
ORDER BY points DESC;

/*

/*
List events and top performers for a specific year:
*/
SELECT event, sport, athlete, medal
FROM Details
WHERE edition_id IN (SELECT edition_id FROM Games WHERE year = 2004)
ORDER BY medal;

/*
Retrieve details of an athlete, including medal achievements:
*/
SELECT A.name, A.sex, A.born, A.height, A.weight, A.country, COUNT(E.medal) AS medals, 
       GROUP_CONCAT(DISTINCT E.event) AS events
FROM Athlete A
LEFT JOIN Details E ON A.athlete_id = E.athlete_id
WHERE A.name LIKE '%Ivanka%'
GROUP BY A.athlete_id;

/*
Show world records and filter by sport:
*/

SELECT event_title, sport, result_description
FROM Results
WHERE sport = 'Weightlifting' AND result_description LIKE '%record%';



*/


