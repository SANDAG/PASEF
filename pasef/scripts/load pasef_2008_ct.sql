truncate table pasef_2008_ct_ep;

insert into pasef_2008_ct_ep
select x.ct,x.sra,ethnicity,sex,age,sum(pop),0,0
from pasef_2008_mgra_ep p, xref_mgra_sr12 x
where p.mgra = x.mgra 
group by x.ct,x.sra,ethnicity,sex,age;

update pasef_2008_ct_ep
set spop = pop where ct in (select ct00 from mil_census_tract_codes);

update pasef_2008_ct_ep
set nspop = pop where spop = 0;

