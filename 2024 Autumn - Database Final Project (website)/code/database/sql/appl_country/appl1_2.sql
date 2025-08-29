SELECT e.sport,
       SUM(e.medal = 'Gold') AS gold_medals,
       SUM(e.medal = 'Silver') AS silver_medals,
       SUM(e.medal = 'Bronze') AS bronze_medals
FROM Details e
WHERE e.country_noc = 'USA' -- Dynamically replace with selected country
GROUP BY e.sport
ORDER BY gold_medals DESC, silver_medals DESC, bronze_medals DESC;

