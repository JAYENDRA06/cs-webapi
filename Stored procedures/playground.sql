CREATE DATABASE DotNetCourseDatabase
GO
 
USE DotNetCourseDatabase
GO
 
CREATE SCHEMA TutorialAppSchema
GO
 
CREATE TABLE TutorialAppSchema.Computer(
    ComputerId INT IDENTITY(1,1) PRIMARY KEY,
    Motherboard NVARCHAR(50),
    CPUCores INT,
    HasWifi BIT,
    HasLTE BIT,
    ReleaseDate DATE,
    Price DECIMAL(18,4),
    VideoCard NVARCHAR(50)
);


SELECT [ComputerId],
[Motherboard],
[CPUCores],
[HasWifi],
[HasLTE],
[ReleaseDate],
[Price],
[VideoCard] FROM DotNetCourseDatabase.TutorialAppSchema.Computer;

-- type SELECT * ___ then put cursor after * and do ctrl+space to get  fully qualified column names 
SELECT [UserId],
[FirstName],
[LastName],
[Email],
[Gender],
[Active] FROM DotNetCourseDatabase.TutorialAppSchema.Users;

INSERT INTO TutorialAppSchema.Users(
    [FirstName],
    [LastName],
    [Email],
    [Gender],
    [Active]
) VALUES (

)

UPDATE TutorialAppSchema.Users
    SET [FirstName] = '',
    [LastName] = '',
    [Email] = '',
    [Gender] = '',
    [Active] = ''
    WHERE UserId = 



SELECT [UserId],
[JobTitle],
[Department] FROM DotNetCourseDatabase.TutorialAppSchema.UserJobInfo;

SELECT [UserId],
[Salary] FROM DotNetCourseDatabase.TutorialAppSchema.UserSalary;

TRUNCATE TABLE DotNetCourseDatabase.TutorialAppSchema.Computer;



CREATE TABLE TutorialAppSchema.Auth (
    Email NVARCHAR(50),
    PasswordHash VARBINARY(MAX),
    PasswordSalt VARBINARY(MAX)
)

SELECT * FROM TutorialAppSchema.Users WHERE Email='jayendra@gmail.com';

SELECT * FROM TutorialAppSchema.Auth;


CREATE TABLE TutorialAppSchema.Posts (
    PostId INT IDENTITY(1,1),
    UserId INT,
    PostTitle NVARCHAR(255),
    PostContent NVARCHAR(MAX),
    PostCreated DATETIME,
    PostUpdated DATETIME
)

-- setting up clustered index

CREATE CLUSTERED INDEX cix_Posts_UserId_PostId ON TutorialAppSchema.Posts(UserId, PostId)

SELECT [PostId],
[UserId],
[PostTitle],
[PostContent],
[PostCreated],
[PostUpdated] FROM TutorialAppSchema.Posts

INSERT INTO TutorialAppSchema.Posts(
    [PostId],
    [UserId],
    [PostTitle],
    [PostContent],
    [PostCreated],
    [PostUpdated]
) VALUES ()

UPDATE TutorialAppSchema.Posts 
    SET PostContent = '', PostTitle = '', PostUpdated = GETDATE() 
    WHERE PostId = 32

DELETE FROM TutorialAppSchema.Posts WHERE PostId = ''


SELECT * FROM TutorialAppSchema.Posts 
    WHERE PostTitle LIKE '%search%'
    OR PostContent LIKE '%search%'