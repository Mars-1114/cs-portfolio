-- this is code is for website use , 2024/12/19
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
WHERE (A.sex = :sex OR :sex IS NULL)
  AND (A.name LIKE CONCAT('%', :name, '%') OR :name IS NULL)
  AND (A.country LIKE CONCAT('%', :country, '%') OR :country IS NULL)
GROUP BY A.athlete_id;

-- this is code is for sql use , 2024/12/19
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
WHERE (A.sex = 'Female' OR 'Female' IS NULL)
  AND (A.name LIKE CONCAT('%', 'Tzu-Ying', '%') OR 'Phelps' IS NULL)
  AND (A.country = CONCAT('%', ' Chinese Taipei', '%') OR 'USA' IS NULL);
GROUP BY A.athlete_id;


-- this is code is for sql use , 2024/12/19
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
WHERE (A.sex = 'Male' OR 'Male' IS NULL)
  AND (A.name LIKE CONCAT('%', 'Phelps', '%') OR 'Phelps' IS NULL)
  AND (A.country = CONCAT('%', 'USA', '%') OR 'USA' IS NULL);
GROUP BY A.athlete_id;
