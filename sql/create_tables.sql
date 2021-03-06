CREATE TABLE [xref_mgra](
	[MGRA] [int] NOT NULL,
	[City] [tinyint] NULL,
	[ct10] [int] NULL,
	[cityct10] [int] NOT NULL,
	[supersplit] [int] NOT NULL,
	[SG] [int] NULL,
	[ZIP] [int] NULL,
	[Sphere] [smallint] NULL,
	[CPA] [smallint] NULL,
	[CPASG] [int] NULL,
	[Council] [smallint] NULL,
	[Super] [smallint] NULL,
	[LUZ] [smallint] NULL,
	[Elem] [smallint] NULL,
	[Unif] [smallint] NULL,
	[High] [smallint] NULL,
	[Coll] [smallint] NULL,
	[Transit] [smallint] NULL,
	[ct10bg] [int] NULL,
	[cityct10bg] [int] NOT NULL,
	[sra] [tinyint] NULL,
	[msa] [tinyint] NULL,
	[taz] [int] NULL,
	[x] [int] NULL,
	[y] [int] NULL,
 CONSTRAINT [PK_xref_mgra_sr13] PRIMARY KEY CLUSTERED 
(
	[MGRA] ASC
))
GO

CREATE TABLE [special_pop_tracts](
	[ct10] [int] NOT NULL,
	[base_code] [tinyint] NULL,
	[sra] [tinyint] NULL
)
GO

CREATE TABLE [reg_fcst](
	[scenario] [tinyint] NOT NULL,
	[year] [smallint] NOT NULL,
	[civ] [int] NULL,
	[mil] [int] NULL,
	[min] [int] NULL,
	[cons] [int] NULL,
	[mfg] [int] NULL,
	[whtrade] [int] NULL,
	[retrade] [int] NULL,
	[twu] [int] NULL,
	[info] [int] NULL,
	[fre] [int] NULL,
	[pbs] [int] NULL,
	[edhs_oth] [int] NULL,
	[edhs_health] [int] NULL,
	[lh_amuse] [int] NULL,
	[lh_hotel] [int] NULL,
	[lh_restaur] [int] NULL,
	[osv_oth] [int] NULL,
	[osv_rel] [int] NULL,
	[gov_fed] [int] NULL,
	[gov_sloth] [int] NULL,
	[gov_sled] [int] NULL,
	[sedw] [int] NULL,
	[hs] [int] NULL,
	[hs_sf] [int] NULL,
	[hs_mf] [int] NULL,
	[hs_mh] [int] NULL,
	[hh] [int] NULL,
	[hh_sf] [int] NULL,
	[hh_mf] [int] NULL,
	[hh_mh] [int] NULL,
	[pop] [int] NULL,
	[hhp] [int] NULL,
	[gq] [int] NULL,
	[gq_civ] [int] NULL,
	[gq_civ_college] [int] NULL,
	[gq_civ_other] [int] NULL,
	[gq_mil] [int] NULL,
	[er] [int] NULL,
	[inc_mean] [int] NULL,
	[inc_median] [int] NULL,
	[i1] [int] NULL,
	[i2] [int] NULL,
	[i3] [int] NULL,
	[i4] [int] NULL,
	[i5] [int] NULL,
	[i6] [int] NULL,
	[i7] [int] NULL,
	[i8] [int] NULL,
	[i9] [int] NULL,
	[i10] [int] NULL,
	[hhs1] [int] NULL,
	[hhs2] [int] NULL,
	[hhs3] [int] NULL,
	[hhs4] [int] NULL,
	[hhs5] [int] NULL,
	[hhs6] [int] NULL,
	[hhs7] [int] NULL,
	[hhwoc] [int] NULL,
	[hhwc] [int] NULL,
	[hhworkers0] [int] NULL,
	[hhworkers1] [int] NULL,
	[hhworkers2] [int] NULL,
	[hhworkers3] [int] NULL,
	[enroll_K8] [int] NULL,
	[enroll_HS] [int] NULL,
	[enroll_MajCol] [int] NULL,
	[enroll_OtherCol] [int] NULL,
	[enroll_AdultEd] [int] NULL,
 CONSTRAINT [PK_reg_fcst_ep] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[year] ASC
))
GO

CREATE TABLE [pop_defm](
	[scenario] [tinyint] NOT NULL,
	[year] [int] NOT NULL,
	[age] [tinyint] NOT NULL,
	[hispm] [int] NULL,
	[hispf] [int] NULL,
	[nhwm] [int] NULL,
	[nhwf] [int] NULL,
	[nhbm] [int] NULL,
	[nhbf] [int] NULL,
	[nhim] [int] NULL,
	[nhif] [int] NULL,
	[nham] [int] NULL,
	[nhaf] [int] NULL,
	[nhhm] [int] NULL,
	[nhhf] [int] NULL,
	[nhom] [int] NULL,
	[nhof] [int] NULL,
	[nh2m] [int] NULL,
	[nh2f] [int] NULL,
 CONSTRAINT [PK_pop_defm] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[year] ASC,
	[age] ASC
))
GO

CREATE TABLE [pasef_sra_chgshr](
	[sra] [tinyint] NOT NULL,
	[hispm] [float] NULL,
	[hispf] [float] NULL,
	[nhwm] [float] NULL,
	[nhwf] [float] NULL,
	[nhbm] [float] NULL,
	[nhbf] [float] NULL,
	[nhim] [float] NULL,
	[nhif] [float] NULL,
	[nham] [float] NULL,
	[nhaf] [float] NULL,
	[nhhm] [float] NULL,
	[nhhf] [float] NULL,
	[nhom] [float] NULL,
	[nhof] [float] NULL,
	[nh2m] [float] NULL,
	[nh2f] [float] NULL,
 CONSTRAINT [PK_pasef_sra_chgshr] PRIMARY KEY CLUSTERED 
(
	[sra] ASC
))
GO

CREATE TABLE [pasef_sra](
	[scenario] [tinyint] NOT NULL,
	[year] [int] NOT NULL,
	[sra] [tinyint] NOT NULL,
	[ethnicity] [tinyint] NOT NULL,
	[sex] [tinyint] NOT NULL,
	[age] [tinyint] NOT NULL,
	[pop] [int] NULL,
	[spop] [int] NULL,
	[nspop] [int] NULL,
 CONSTRAINT [PK_pasef_2008_sra_ep] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[year] ASC,
	[sra] ASC,
	[ethnicity] ASC,
	[sex] ASC,
	[age] ASC
))
GO

CREATE TABLE [pasef_mgra_tab](
	[scenario] [tinyint] NOT NULL,
	[year] [int] NOT NULL,
	[mgra] [smallint] NOT NULL,
	[ethnicity] [tinyint] NOT NULL,
	[pop] [int] NULL,
	[pop_0to4] [int] NULL,
	[pop_5to9] [int] NULL,
	[pop_10to14] [int] NULL,
	[pop_15to17] [int] NULL,
	[pop_18to19] [int] NULL,
	[pop_20to24] [int] NULL,
	[pop_25to29] [int] NULL,
	[pop_30to34] [int] NULL,
	[pop_35to39] [int] NULL,
	[pop_40to44] [int] NULL,
	[pop_45to49] [int] NULL,
	[pop_50to54] [int] NULL,
	[pop_55to59] [int] NULL,
	[pop_60to61] [int] NULL,
	[pop_62to64] [int] NULL,
	[pop_65to69] [int] NULL,
	[pop_70to74] [int] NULL,
	[pop_75to79] [int] NULL,
	[pop_80to84] [int] NULL,
	[pop_85plus] [int] NULL,
	[popm] [int] NULL,
	[popm_0to4] [int] NULL,
	[popm_5to9] [int] NULL,
	[popm_10to14] [int] NULL,
	[popm_15to17] [int] NULL,
	[popm_18to19] [int] NULL,
	[popm_20to24] [int] NULL,
	[popm_25to29] [int] NULL,
	[popm_30to34] [int] NULL,
	[popm_35to39] [int] NULL,
	[popm_40to44] [int] NULL,
	[popm_45to49] [int] NULL,
	[popm_50to54] [int] NULL,
	[popm_55to59] [int] NULL,
	[popm_60to61] [int] NULL,
	[popm_62to64] [int] NULL,
	[popm_65to69] [int] NULL,
	[popm_70to74] [int] NULL,
	[popm_75to79] [int] NULL,
	[popm_80to84] [int] NULL,
	[popm_85plus] [int] NULL,
	[popf] [int] NULL,
	[popf_0to4] [int] NULL,
	[popf_5to9] [int] NULL,
	[popf_10to14] [int] NULL,
	[popf_15to17] [int] NULL,
	[popf_18to19] [int] NULL,
	[popf_20to24] [int] NULL,
	[popf_25to29] [int] NULL,
	[popf_30to34] [int] NULL,
	[popf_35to39] [int] NULL,
	[popf_40to44] [int] NULL,
	[popf_45to49] [int] NULL,
	[popf_50to54] [int] NULL,
	[popf_55to59] [int] NULL,
	[popf_60to61] [int] NULL,
	[popf_62to64] [int] NULL,
	[popf_65to69] [int] NULL,
	[popf_70to74] [int] NULL,
	[popf_75to79] [int] NULL,
	[popf_80to84] [int] NULL,
	[popf_85plus] [int] NULL,
 CONSTRAINT [PK_pasef_2008_tab_ep] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[year] ASC,
	[mgra] ASC,
	[ethnicity] ASC
))
GO

CREATE TABLE [pasef_mgra](
	[scenario] [tinyint] NOT NULL,
	[year] [int] NOT NULL,
	[mgra] [smallint] NOT NULL,
	[ethnicity] [tinyint] NOT NULL,
	[sex] [tinyint] NOT NULL,
	[age] [tinyint] NOT NULL,
	[pop] [int] NULL,
 CONSTRAINT [PK_pasef_2008_mgra_ep] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[year] ASC,
	[mgra] ASC,
	[ethnicity] ASC,
	[sex] ASC,
	[age] ASC
))
GO

CREATE TABLE [pasef_HHdetail_mgra](
	[scenario] [int] NOT NULL,
	[year] [int] NOT NULL,
	[ct10] [int] NULL,
	[mgra] [int] NOT NULL,
	[hhs1] [int] NULL,
	[hhs2] [int] NULL,
	[hhs3] [int] NULL,
	[hhs4] [int] NULL,
	[hhs5] [int] NULL,
	[hhs6] [int] NULL,
	[hhs7] [int] NULL,
	[hhwoc] [int] NULL,
	[hhwc] [int] NULL,
	[hhworkers0] [int] NULL,
	[hhworkers1] [int] NULL,
	[hhworkers2] [int] NULL,
	[hhworkers3] [int] NULL,
	[hh] [int] NULL,
	[hhs] [float] NULL,
	[hhp_implied] [int] NULL,
 CONSTRAINT [PK_pasef_HHdetail_mgra] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[year] ASC,
	[mgra] ASC
))
GO

CREATE TABLE [pasef_HHdetail_ct](
	[scenario] [int] NOT NULL,
	[year] [int] NOT NULL,
	[ct10] [int] NOT NULL,
	[hhs1] [int] NULL,
	[hhs2] [int] NULL,
	[hhs3] [int] NULL,
	[hhs4] [int] NULL,
	[hhs5] [int] NULL,
	[hhs6] [int] NULL,
	[hhs7] [int] NULL,
	[hhwc] [int] NULL,
	[hhwoc] [int] NULL,
	[hhworkers0] [int] NULL,
	[hhworkers1] [int] NULL,
	[hhworkers2] [int] NULL,
	[hhworkers3] [int] NULL,
 CONSTRAINT [PK_pasef_HHdetail_ct] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[year] ASC,
	[ct10] ASC
))
GO

CREATE TABLE [pasef_enrollment_mgra](
	[scenario] [tinyint] NOT NULL,
	[year] [int] NOT NULL,
	[mgra] [int] NOT NULL,
	[enroll_k8] [int] NULL,
	[enroll_HS] [int] NULL,
	[enroll_MajCol] [int] NULL,
	[enroll_OtherCol] [int] NULL,
	[enroll_AdultEd] [int] NULL,
 CONSTRAINT [PK_pasef_enrollment_mgra] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[year] ASC,
	[mgra] ASC
))
GO

CREATE TABLE [pasef_ct](
	[scenario] [tinyint] NOT NULL,
	[year] [int] NOT NULL,
	[ct10] [smallint] NOT NULL,
	[sra] [tinyint] NOT NULL,
	[ethnicity] [tinyint] NOT NULL,
	[sex] [tinyint] NOT NULL,
	[age] [tinyint] NOT NULL,
	[pop] [int] NULL,
	[spop] [int] NULL,
	[nspop] [int] NULL,
 CONSTRAINT [PK_pasef_ct] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[year] ASC,
	[ct10] ASC,
	[ethnicity] ASC,
	[sex] ASC,
	[age] ASC
))
GO

CREATE TABLE [overrides_HHSdetail](
	[year] [int] NOT NULL,
	[ct10] [int] NOT NULL,
	[HH1] [float] NULL,
	[HH2] [float] NULL,
	[HH3] [float] NULL,
	[HH4] [float] NULL,
	[HH5] [float] NULL,
	[HH6] [float] NULL,
	[HH7] [float] NULL,
 CONSTRAINT [PK_overrides_HHSdetail] PRIMARY KEY CLUSTERED 
(
	[year] ASC,
	[ct10] ASC
))
GO

CREATE TABLE [mgrabase](
	[scenario] [tinyint] NOT NULL,
	[increment] [int] NOT NULL,
	[mgra] [int] NOT NULL,
	[luz] [smallint] NULL,
	[pop] [int] NULL,
	[hhp] [int] NULL,
	[er] [int] NULL,
	[gq] [int] NULL,
	[gq_civ] [int] NULL,
	[gq_civ_college] [int] NULL,
	[gq_civ_other] [int] NULL,
	[gq_mil] [int] NULL,
	[hs] [int] NULL,
	[hs_sf] [int] NULL,
	[hs_mf] [int] NULL,
	[hs_mh] [int] NULL,
	[hh] [int] NULL,
	[hh_sf] [int] NULL,
	[hh_mf] [int] NULL,
	[hh_mh] [int] NULL,
	[emp] [int] NULL,
	[emp_civ] [int] NULL,
	[emp_mil] [int] NULL,
	[emp_agmin] [int] NULL,
	[emp_cons] [int] NULL,
	[emp_mfg] [int] NULL,
	[emp_whtrade] [int] NULL,
	[emp_retrade] [int] NULL,
	[emp_twu] [int] NULL,
	[emp_info] [int] NULL,
	[emp_fre] [int] NULL,
	[emp_pbs] [int] NULL,
	[emp_edhs_oth] [int] NULL,
	[emp_edhs_health] [int] NULL,
	[emp_lh_amuse] [int] NULL,
	[emp_lh_hotel] [int] NULL,
	[emp_lh_restaur] [int] NULL,
	[emp_osv_oth] [int] NULL,
	[emp_osv_rel] [int] NULL,
	[emp_gov_fed] [int] NULL,
	[emp_gov_sloth] [int] NULL,
	[emp_gov_sled] [int] NULL,
	[emp_sedw] [int] NULL,
	[emp_indus_lu] [int] NULL,
	[emp_comm_lu] [int] NULL,
	[emp_office_lu] [int] NULL,
	[emp_other_lu] [int] NULL,
	[i1] [int] NULL,
	[i2] [int] NULL,
	[i3] [int] NULL,
	[i4] [int] NULL,
	[i5] [int] NULL,
	[i6] [int] NULL,
	[i7] [int] NULL,
	[i8] [int] NULL,
	[i9] [int] NULL,
	[i10] [int] NULL,
	[dev_ldsf] [float] NULL,
	[dev_sf] [float] NULL,
	[dev_mf] [float] NULL,
	[dev_mh] [float] NULL,
	[dev_oth] [float] NULL,
	[dev_ag] [float] NULL,
	[dev_indus] [float] NULL,
	[dev_comm] [float] NULL,
	[dev_office] [float] NULL,
	[dev_schools] [float] NULL,
	[dev_roads] [float] NULL,
	[dev_parks] [float] NULL,
	[dev_mil] [float] NULL,
	[dev_water] [float] NULL,
	[dev_mixed_use] [float] NULL,
	[vac_ldsf] [float] NULL,
	[vac_sf] [float] NULL,
	[vac_mf] [float] NULL,
	[vac_mh] [float] NULL,
	[vac_oth] [float] NULL,
	[vac_ag] [float] NULL,
	[vac_indus] [float] NULL,
	[vac_comm] [float] NULL,
	[vac_office] [float] NULL,
	[vac_schools] [float] NULL,
	[vac_roads] [float] NULL,
	[vac_mixed_use] [float] NULL,
	[vac_parks] [float] NULL,
	[redev_sf_mf] [float] NULL,
	[redev_sf_emp] [float] NULL,
	[redev_mf_emp] [float] NULL,
	[redev_mh_sf] [float] NULL,
	[redev_mh_mf] [float] NULL,
	[redev_mh_emp] [float] NULL,
	[redev_ag_ldsf] [float] NULL,
	[redev_ag_sf] [float] NULL,
	[redev_ag_mf] [float] NULL,
	[redev_ag_indus] [float] NULL,
	[redev_ag_comm] [float] NULL,
	[redev_ag_office] [float] NULL,
	[redev_ag_schools] [float] NULL,
	[redev_ag_roads] [float] NULL,
	[redev_emp_res] [float] NULL,
	[redev_emp_emp] [float] NULL,
	[infill_sf] [float] NULL,
	[infill_mf] [float] NULL,
	[infill_emp] [float] NULL,
	[acres] [float] NULL,
	[dev] [float] NULL,
	[vac] [float] NULL,
	[unusable] [float] NULL,
 CONSTRAINT [PK_mgrabase] PRIMARY KEY CLUSTERED 
(
	[scenario] ASC,
	[increment] ASC,
	[mgra] ASC
))
GO

CREATE TABLE [error_factors_HHSDetail](
	[year] [int] NOT NULL,
	[ct10] [int] NOT NULL,
	[hhs1_erf] [float] NULL,
	[hhs2_erf] [float] NULL,
	[hhs3_erf] [float] NULL,
	[hhs4_erf] [float] NULL,
	[hhs5_erf] [float] NULL,
	[hhs6_erf] [float] NULL,
	[hhs7_erf] [float] NULL,
 CONSTRAINT [PK_error_factors_HHS] PRIMARY KEY CLUSTERED 
(
	[year] ASC,
	[ct10] ASC
))
GO

CREATE TABLE [distribution_HHworkers_region](
	[year] [int] NOT NULL,
	[row] [int] NOT NULL,
	[HHW0] [float] NULL,
	[HHW1] [float] NULL,
	[HHW2] [float] NULL,
	[HHW3] [float] NULL,
 CONSTRAINT [PK_distribution_HHworkers_region] PRIMARY KEY CLUSTERED 
(
	[year] ASC,
	[row] ASC
))
GO

CREATE TABLE [distribution_HH_wo_children](
	[year] [int] NOT NULL,
	[ct10] [int] NOT NULL,
	[hhwoc1] [float] NULL,
	[hhwoc2] [float] NULL,
	[hhwoc3] [float] NULL,
	[hhwoc4] [float] NULL,
 CONSTRAINT [PK_distribution_HH_wo_children] PRIMARY KEY CLUSTERED 
(
	[year] ASC,
	[ct10] ASC
))
GO

CREATE TABLE [census2010_ct_kids](
	[ct10] [int] NOT NULL,
	[kids_hh_wkids] [float] NULL,
 CONSTRAINT [PK_census2010_ct10_kids] PRIMARY KEY CLUSTERED 
(
	[ct10] ASC
))
GO
