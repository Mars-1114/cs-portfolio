SELECT A.name, 
       A.sex, 
       A.born, 
       A.height, 
       A.weight, 
       A.country, 
       GROUP_CONCAT(DISTINCT E.sport SEPARATOR '  ') AS sport,
       COUNT(E.athlete_id) AS total_events, -- Total events participated in
       COUNT(CASE WHEN E.medal = 'Gold' THEN 1 END) AS gold_medals, -- Gold medal count
       COUNT(CASE WHEN E.medal = 'Silver' THEN 1 END) AS silver_medals, -- Silver medal count
       COUNT(CASE WHEN E.medal = 'Bronze' THEN 1 END) AS bronze_medals, -- Bronze medal count
       GROUP_CONCAT(DISTINCT E.event SEPARATOR '  ') AS events -- List of distinct events
FROM Athlete A
LEFT JOIN Details E ON A.athlete_id = E.athlete_id
WHERE A.name LIKE '%Hsing-Chun%'
GROUP BY A.athlete_id;


SELECT A.name, 
       A.sex, 
       A.born, 
       A.height, 
       A.weight, 
       A.country,
       GROUP_CONCAT(DISTINCT E.sport SEPARATOR '  ') AS sport,
       COUNT(E.athlete_id) AS total_events, -- Total events participated in
       COUNT(CASE WHEN E.medal = 'Gold' THEN 1 END) AS gold_medals, -- Gold medal count
       COUNT(CASE WHEN E.medal = 'Silver' THEN 1 END) AS silver_medals, -- Silver medal count
       COUNT(CASE WHEN E.medal = 'Bronze' THEN 1 END) AS bronze_medals, -- Bronze medal count
       GROUP_CONCAT(DISTINCT E.event SEPARATOR '  ') AS events -- List of distinct events
FROM Athlete A
LEFT JOIN Details E ON A.athlete_id = E.athlete_id
WHERE A.name LIKE '%Tzu-Ying%'
GROUP BY A.athlete_id;



SELECT A.name, 
       A.sex, 
       A.born, 
       A.height, 
       A.weight, 
       A.country,
       GROUP_CONCAT(DISTINCT E.sport SEPARATOR '  ') AS sport,
       COUNT(E.athlete_id) AS total_events, -- Total events participated in
       COUNT(CASE WHEN E.medal = 'Gold' THEN 1 END) AS gold_medals, -- Gold medal count
       COUNT(CASE WHEN E.medal = 'Silver' THEN 1 END) AS silver_medals, -- Silver medal count
       COUNT(CASE WHEN E.medal = 'Bronze' THEN 1 END) AS bronze_medals, -- Bronze medal count
       GROUP_CONCAT(DISTINCT E.event  SEPARATOR '  ') AS events -- List of distinct events
FROM Athlete A
LEFT JOIN Details E ON A.athlete_id = E.athlete_id
WHERE A.name LIKE '%Phelps%'
GROUP BY A.athlete_id;