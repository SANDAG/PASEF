truncate table pasef_2008_ethnic_totals_ep;
insert into pasef_2008_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2008_mgra_tab_ep p0,
pasef_2008_mgra_tab_ep p1,
pasef_2008_mgra_tab_ep p2,
pasef_2008_mgra_tab_ep p3,
pasef_2008_mgra_tab_ep p4,
pasef_2008_mgra_tab_ep p5,
pasef_2008_mgra_tab_ep p6,
pasef_2008_mgra_tab_ep p7,
pasef_2008_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2008_ethnic_totals_ep set nhisp = pop - hisp;

truncate table pasef_2011_ethnic_totals_ep;
insert into pasef_2011_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2011_mgra_tab_ep p0,
pasef_2011_mgra_tab_ep p1,
pasef_2011_mgra_tab_ep p2,
pasef_2011_mgra_tab_ep p3,
pasef_2011_mgra_tab_ep p4,
pasef_2011_mgra_tab_ep p5,
pasef_2011_mgra_tab_ep p6,
pasef_2011_mgra_tab_ep p7,
pasef_2011_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2011_ethnic_totals_ep set nhisp = pop - hisp;

truncate table pasef_2015_ethnic_totals_ep;
insert into pasef_2015_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2015_mgra_tab_ep p0,
pasef_2015_mgra_tab_ep p1,
pasef_2015_mgra_tab_ep p2,
pasef_2015_mgra_tab_ep p3,
pasef_2015_mgra_tab_ep p4,
pasef_2015_mgra_tab_ep p5,
pasef_2015_mgra_tab_ep p6,
pasef_2015_mgra_tab_ep p7,
pasef_2015_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2015_ethnic_totals_ep set nhisp = pop - hisp;

truncate table pasef_2020_ethnic_totals_ep;
insert into pasef_2020_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2020_mgra_tab_ep p0,
pasef_2020_mgra_tab_ep p1,
pasef_2020_mgra_tab_ep p2,
pasef_2020_mgra_tab_ep p3,
pasef_2020_mgra_tab_ep p4,
pasef_2020_mgra_tab_ep p5,
pasef_2020_mgra_tab_ep p6,
pasef_2020_mgra_tab_ep p7,
pasef_2020_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2020_ethnic_totals_ep set nhisp = pop - hisp;

truncate table pasef_2025_ethnic_totals_ep;
insert into pasef_2025_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2025_mgra_tab_ep p0,
pasef_2025_mgra_tab_ep p1,
pasef_2025_mgra_tab_ep p2,
pasef_2025_mgra_tab_ep p3,
pasef_2025_mgra_tab_ep p4,
pasef_2025_mgra_tab_ep p5,
pasef_2025_mgra_tab_ep p6,
pasef_2025_mgra_tab_ep p7,
pasef_2025_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2025_ethnic_totals_ep set nhisp = pop - hisp;

truncate table pasef_2030_ethnic_totals_ep;
insert into pasef_2030_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2030_mgra_tab_ep p0,
pasef_2030_mgra_tab_ep p1,
pasef_2030_mgra_tab_ep p2,
pasef_2030_mgra_tab_ep p3,
pasef_2030_mgra_tab_ep p4,
pasef_2030_mgra_tab_ep p5,
pasef_2030_mgra_tab_ep p6,
pasef_2030_mgra_tab_ep p7,
pasef_2030_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2030_ethnic_totals_ep set nhisp = pop - hisp;

truncate table pasef_2035_ethnic_totals_ep;
insert into pasef_2035_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2035_mgra_tab_ep p0,
pasef_2035_mgra_tab_ep p1,
pasef_2035_mgra_tab_ep p2,
pasef_2035_mgra_tab_ep p3,
pasef_2035_mgra_tab_ep p4,
pasef_2035_mgra_tab_ep p5,
pasef_2035_mgra_tab_ep p6,
pasef_2035_mgra_tab_ep p7,
pasef_2035_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2035_ethnic_totals_ep set nhisp = pop - hisp;

truncate table pasef_2040_ethnic_totals_ep;
insert into pasef_2040_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2040_mgra_tab_ep p0,
pasef_2040_mgra_tab_ep p1,
pasef_2040_mgra_tab_ep p2,
pasef_2040_mgra_tab_ep p3,
pasef_2040_mgra_tab_ep p4,
pasef_2040_mgra_tab_ep p5,
pasef_2040_mgra_tab_ep p6,
pasef_2040_mgra_tab_ep p7,
pasef_2040_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2040_ethnic_totals_ep set nhisp = pop - hisp;

truncate table pasef_2045_ethnic_totals_ep;
insert into pasef_2045_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2045_mgra_tab_ep p0,
pasef_2045_mgra_tab_ep p1,
pasef_2045_mgra_tab_ep p2,
pasef_2045_mgra_tab_ep p3,
pasef_2045_mgra_tab_ep p4,
pasef_2045_mgra_tab_ep p5,
pasef_2045_mgra_tab_ep p6,
pasef_2045_mgra_tab_ep p7,
pasef_2045_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2045_ethnic_totals_ep set nhisp = pop - hisp;

truncate table pasef_2050_ethnic_totals_ep;
insert into pasef_2050_ethnic_totals_ep
select p0.mgra,pop = p0.pop,hisp = p1.pop,nhisp = 0,nhw = p2.pop,nhb = p3.pop,nhi = p4.pop,nha = p5.pop,
nhh = p6.pop,nho = p7.pop,nh2 = p8.pop
from 
pasef_2050_mgra_tab_ep p0,
pasef_2050_mgra_tab_ep p1,
pasef_2050_mgra_tab_ep p2,
pasef_2050_mgra_tab_ep p3,
pasef_2050_mgra_tab_ep p4,
pasef_2050_mgra_tab_ep p5,
pasef_2050_mgra_tab_ep p6,
pasef_2050_mgra_tab_ep p7,
pasef_2050_mgra_tab_ep p8
where 
      p1.mgra = p0.mgra and
      p1.mgra = p2.mgra and
      p1.mgra = p3.mgra and
      p1.mgra = p4.mgra and
      p1.mgra = p5.mgra and
      p1.mgra = p6.mgra and
      p1.mgra = p7.mgra and
      p1.mgra = p8.mgra and
      p0.ethnicity = 0 and
      p1.ethnicity = 1 and
      p2.ethnicity = 2 and
      p3.ethnicity = 3 and
      p4.ethnicity = 4 and
      p5.ethnicity = 5 and
      p6.ethnicity = 6 and
      p7.ethnicity = 7 and
      p8.ethnicity = 8
order by p0.mgra;
update pasef_2050_ethnic_totals_ep set nhisp = pop - hisp;