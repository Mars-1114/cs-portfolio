select ar.sport,ar.year,ar.grade,ar.ascend
from AthleteRecords ar
inner join Athlete a on a.athlete_id = ar.athlete_id
where a.name = 'David Rudisha'
order by ar.year desc , ar.sport;