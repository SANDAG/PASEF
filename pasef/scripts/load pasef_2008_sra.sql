truncate table pasef_2008_sra_ep;

insert into pasef_2008_sra_ep
select sra,ethnicity,sex,age,sum(pop),sum(spop),sum(nspop)
from pasef_2008_ct_ep
group by sra,ethnicity,sex,age;