SELECT e.sport,
       e.isTeamSport,
       COUNT(DISTINCT CASE WHEN e.isTeamSport = 1 AND e.medal = 'Gold' THEN CONCAT(e.edition_id, '-', e.sport, '-', e.medal) END) AS team_gold_medals,
       COUNT(DISTINCT CASE WHEN e.isTeamSport = 1 AND e.medal = 'Silver' THEN CONCAT(e.edition_id, '-', e.sport, '-', e.medal) END) AS team_silver_medals,
       COUNT(DISTINCT CASE WHEN e.isTeamSport = 1 AND e.medal = 'Bronze' THEN CONCAT(e.edition_id, '-', e.sport, '-', e.medal) END) AS team_bronze_medals,
       SUM(CASE WHEN e.isTeamSport = 0 AND e.medal = 'Gold' THEN 1 ELSE 0 END) AS individual_gold_medals,
       SUM(CASE WHEN e.isTeamSport = 0 AND e.medal = 'Silver' THEN 1 ELSE 0 END) AS individual_silver_medals,
       SUM(CASE WHEN e.isTeamSport = 0 AND e.medal = 'Bronze' THEN 1 ELSE 0 END) AS individual_bronze_medals
FROM Details e
WHERE e.country_noc = 'USA' -- Replace 'USA' dynamically with the selected country
GROUP BY e.sport, e.isTeamSport
ORDER BY e.isTeamSport DESC, team_gold_medals DESC, team_silver_medals DESC, team_bronze_medals DESC, individual_gold_medals DESC, individual_silver_medals DESC, individual_bronze_medals DESC;

