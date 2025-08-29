select ar.sport,ar.athlete_id,a.country,ar.name,ar.grade,ar.year,ar.ascend from AthleteRecords ar
join Athlete a
where ar.athlete_id = a.athlete_id
order by ar.sport;