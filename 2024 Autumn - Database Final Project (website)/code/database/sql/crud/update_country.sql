update Country
set country = 'Japan'
where noc = 'JPN';

update Country
set noc = 'JPN'
where country = 'Japan';