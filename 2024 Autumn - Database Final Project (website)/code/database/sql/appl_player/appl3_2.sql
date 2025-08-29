select d.edition as edition , d.sport as sport ,d.event as event , if(d.medal != '', d.medal, 'None') as medal
from Details d
inner join Athlete a on a.athlete_id = d.athlete_id
where a.name = 'Kuo Hsing-Chun'
order by edition desc , event;