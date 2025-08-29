select year,country_flag_url,city,start_date,end_date,competition_date from Games
where edition LIKE '%Summer Olympics' and competition_date != '—'
order by year desc;

select year,country_flag_url,city,start_date,end_date,competition_date from Games
where edition LIKE '%Winter Olympics' and competition_date != '—'
order by year desc;



