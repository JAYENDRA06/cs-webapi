USE DotNetCourseDatabase
GO

CREATE OR ALTER PROCEDURE TutorialAppSchema.spUser_Upsert
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email NVARCHAR(50),
    @Gender NVARCHAR(50),
    @JobTitle NVARCHAR(50),
    @Department NVARCHAR(50),
    @Salary DECIMAL(18,4),
    @Active BIT = 1,
    @UserId INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT * FROM TutorialAppSchema.Users AS Users WHERE Users.UserId = @UserId)
        BEGIN
            IF NOT EXISTS (SELECT * FROM TutorialAppSchema.Users AS Users WHERE Users.Email = @Email)
                BEGIN
                    DECLARE @OutputUserId INT

                    INSERT INTO TutorialAppSchema.Users (
                        [Users].[FirstName],
                        [Users].[LastName],
                        [Users].[Email],
                        [Users].[Gender],
                        [Users].[Active]
                    ) VALUES (
                        @FirstName,
                        @LastName,
                        @Email,
                        @Gender,
                        @Active
                    )
                    SET @OutputUserId = @@IDENTITY -- to get IDENTITY of row we just inserted

                    INSERT INTO TutorialAppSchema.UserSalary (
                        UserId,
                        Salary
                    ) VALUES (
                        @OutputUserId,
                        @Salary
                    )

                    INSERT INTO TutorialAppSchema.UserJobInfo (
                        UserId,
                        JobTitle,
                        Department
                    ) VALUES (
                        @OutputUserId,
                        @JobTitle,
                        @Department
                    )
                END
        END
    ELSE
        BEGIN
            UPDATE TutorialAppSchema.Users
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Gender = @Gender,
                    Active = @Active
                WHERE UserId = @UserId
            
            UPDATE TutorialAppSchema.UserSalary
                SET Salary = @Salary
                WHERE UserId = @UserId

            UPDATE TutorialAppSchema.UserJobInfo
                SET Department = @Department,
                    JobTitle = @JobTitle
                WHERE UserId = @UserId
        END
END