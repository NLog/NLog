use master;
go

create database NLogDatabase
/*
on primary (name='NLogDatabase
filename='***insert_path_here***\NLogDatabase.mdf', 
size=10MB) 
log on (name='NLogDatabase_log',
filename='***insert_path_here***\NLogDatabase_log.ldf',
size=10MB)
*/
go

exec sp_addlogin 'nloguser','nlogpassword',NLogDatabase
go

use NLogDatabase;
go

create table LogTable
(
    sequence_id integer not null primary key identity(1,1),
    time_stamp datetime not null,
    level varchar(5) not null,
    logger varchar(80) not null,
    message varchar(4095) not null,
)
go

exec sp_grantdbaccess 'nloguser','nloguser'
go

grant insert,select on LogTable to nloguser
go
