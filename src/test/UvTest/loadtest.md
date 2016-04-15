# Load Test

## One server & one client

user count: 1500,2000,2300,2500

request times: 100

* Result

user&times	avg	avg50	avg90	avg95	avg99	min		max		error		QPS		kb/sec
1500*100	 3		2		8	11		21		0		3004	0.0		1876.8	216.2
2000*100	 6		5		15	20		30		0		3002	0.0		1916.4	220.8
2300*100	 9		6		22	29		40		0		3003	0.0		1876.5	216.2
2500*100	11		7		25	33		51		0		3074	0.0		1843.5	212.4