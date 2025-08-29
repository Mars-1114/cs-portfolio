select distinct d.sport
from  Details d
where d.edition like '2020%'
order by d.sport;

select d.sport, d.event,
    max(if(d.medal = 'Gold', c.country, null)) as champion_country,
    group_concat(if(d.medal = 'Gold', d.athlete, null) order by d.athlete separator ', ') as champion_athletes,
    max(if(d.medal = 'Silver',c.country, null)) as second_country,
    group_concat(if(d.medal = 'Silver', d.athlete, null) order by d.athlete separator ', ') as second_athletes,
    max(if(d.medal = 'Bronze',c.country, null)) as third_country,
    group_concat(if(d.medal = 'Bronze', d.athlete, null) order by d.athlete separator ', ') as third_athletes
from Details d
join Country c on d.country_noc = c.noc
where d.edition like '2020%' and d.sport = 'Artistic Gymnastics'
and d.medal in ('Gold', 'Silver', 'Bronze')
group by d.sport, d.event
order by d.sport, d.event;