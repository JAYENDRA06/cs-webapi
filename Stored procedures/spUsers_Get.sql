USE DotNetCourseDatabase
GO

-- don't use sp_ since it's understood as a system stored procedure
-- naming convention(not mandatory): sp + name of object + action it will perform
-- CREATE PROCEDURE TutorialAppSchema.spUsers_Get
ALTER PROCEDURE TutorialAppSchema.spUsers_Get
-- EXEC TutorialAppSchema.spUsers_Get @Active=1, @UserId=100
    @UserId INT = NULL -- Default value
    , @Active BIT = NULL
AS
BEGIN
    DROP TABLE IF EXISTS #AverageDeptSalary

    SELECT UserJobInfo.Department, 
        AVG(UserSalary.Salary) AvgSalary
        INTO #AverageDeptSalary -- Temporary table, to make it global do ##
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
        GROUP BY UserJobInfo.Department

    CREATE CLUSTERED INDEX cix_#AverageDeptSalary_Department ON #AverageDeptSalary(Department)


    SELECT [Users].[UserId],
        [Users].[FirstName],
        [Users].[LastName],
        [Users].[Email],
        [Users].[Gender],
        [Users].[Active],
        [UserSalary].[Salary],
        [UserJobInfo].[Department],
        [UserJobInfo].[JobTitle],
        [AvgSalary].[AvgSalary]
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
        LEFT JOIN #AverageDeptSalary AS AvgSalary 
            ON AvgSalary.Department = UserJobInfo.Department
        -- OUTER APPLY (
        --     SELECT AVG(UserSalary2.Salary) AvgSalary
        --     FROM TutorialAppSchema.Users AS Users2
        --         LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary2
        --             ON UserSalary2.UserId = Users2.UserId
        --         LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo2
        --             ON UserJobInfo2.UserId = Users2.UserId
        --         WHERE UserJobInfo2.Department = UserJobInfo.Department
        --         GROUP BY UserJobInfo2.Department
        -- ) AvgSalary
    WHERE Users.UserId = ISNULL(@UserId, Users.UserId) -- Make it explicity true is it's NULL and will give full list of users
        AND ISNULL(Users.Active, 0) = COALESCE(@Active, Users.Active, 0)
END


ALTER TABLE TutorialAppSchema.UserSalary DROP COLUMN AvgSalary