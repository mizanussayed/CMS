/*
  ClinicAppointments.sql
  - Creates Users, Doctors, Appointments tables
  - Adds stored procedures (USP_*) for Doctors and Appointments
  - Seeds minimal data for testing

  Intended for SQL Server 2019. Run in the target database context.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserID INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        Email NVARCHAR(200) NOT NULL UNIQUE,
        Password NVARCHAR(200) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        CreatedDate DATETIME2 DEFAULT SYSUTCDATETIME()
    );
END

IF OBJECT_ID('dbo.Doctors', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Doctors
    (
        DoctorID INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        Specialization NVARCHAR(150) NULL,
        AvailableSlots NVARCHAR(500) NULL, -- e.g. '09:00-12:00,14:00-17:00'
        CreatedDate DATETIME2 DEFAULT SYSUTCDATETIME()
    );
END

IF OBJECT_ID('dbo.Appointments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Appointments
    (
        AppointmentID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL,
        DoctorID INT NOT NULL,
        AppointmentDate DATETIME2 NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT('Pending'),
        CreatedDate DATETIME2 DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Appointments_Users FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_Appointments_Doctors FOREIGN KEY (DoctorID) REFERENCES dbo.Doctors(DoctorID),
        CONSTRAINT CHK_Appointment_Status CHECK (Status IN ('Pending','Confirmed','Cancelled'))
    );
END

/* ==========================
   Doctors Stored Procedures
   ========================== */

IF OBJECT_ID('USP_Doctor_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE USP_Doctor_GetAll;
GO
CREATE PROCEDURE USP_Doctor_GetAll
    @PageNumber INT,
    @PageSize INT,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @TotalRecords = COUNT(1) FROM dbo.Doctors;

    SELECT DoctorID, Name, Specialization, AvailableSlots, CreatedDate
    FROM dbo.Doctors
    ORDER BY Name
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID('USP_Doctor_GetById', 'P') IS NOT NULL
    DROP PROCEDURE USP_Doctor_GetById;
GO
CREATE PROCEDURE USP_Doctor_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DoctorID, Name, Specialization, AvailableSlots, CreatedDate
    FROM dbo.Doctors
    WHERE DoctorID = @Id;
END
GO

IF OBJECT_ID('USP_Doctor_Insert', 'P') IS NOT NULL
    DROP PROCEDURE USP_Doctor_Insert;
GO
CREATE PROCEDURE USP_Doctor_Insert
    @Id INT OUTPUT,
    @Name NVARCHAR(150),
    @Specialization NVARCHAR(150),
    @AvailableSlots NVARCHAR(500),
    @CreatedBy NVARCHAR(150) = NULL,
    @UserName NVARCHAR(150) = NULL,
    @UserRole NVARCHAR(50) = NULL,
    @IP NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Doctors(Name, Specialization, AvailableSlots)
    VALUES(@Name, @Specialization, @AvailableSlots);

    SET @Id = SCOPE_IDENTITY();
    -- Audit logging can be added here if required
END
GO

IF OBJECT_ID('USP_Doctor_Update', 'P') IS NOT NULL
    DROP PROCEDURE USP_Doctor_Update;
GO
CREATE PROCEDURE USP_Doctor_Update
    @Id INT,
    @Name NVARCHAR(150),
    @Specialization NVARCHAR(150),
    @AvailableSlots NVARCHAR(500),
    @LastModifiedBy NVARCHAR(150) = NULL,
    @UserName NVARCHAR(150) = NULL,
    @UserRole NVARCHAR(50) = NULL,
    @IP NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Doctors
    SET Name = @Name,
        Specialization = @Specialization,
        AvailableSlots = @AvailableSlots
    WHERE DoctorID = @Id;
    -- Audit logging can be added here
END
GO

IF OBJECT_ID('USP_Doctor_Delete', 'P') IS NOT NULL
    DROP PROCEDURE USP_Doctor_Delete;
GO
CREATE PROCEDURE USP_Doctor_Delete
    @Id INT,
    @UserName NVARCHAR(150) = NULL,
    @UserRole NVARCHAR(50) = NULL,
    @IP NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Doctors WHERE DoctorID = @Id;
    -- Audit logging can be added here
END
GO

IF OBJECT_ID('USP_Doctor_GetDistinctSpecializations', 'P') IS NOT NULL
    DROP PROCEDURE USP_Doctor_GetDistinctSpecializations;
GO
CREATE PROCEDURE USP_Doctor_GetDistinctSpecializations
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT Specialization FROM dbo.Doctors WHERE Specialization IS NOT NULL ORDER BY Specialization;
END
GO

/* =============================
   Appointments Stored Procedures
   ============================= */

IF OBJECT_ID('USP_Appointment_Book', 'P') IS NOT NULL
    DROP PROCEDURE USP_Appointment_Book;
GO
CREATE PROCEDURE USP_Appointment_Book
    @Id INT OUTPUT,
    @UserId INT,
    @DoctorId INT,
    @AppointmentDate DATETIME2,
    @Status NVARCHAR(20) = 'Pending',
    @UserName NVARCHAR(150) = NULL,
    @UserRole NVARCHAR(50) = NULL,
    @IP NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Simple conflict check: same doctor cannot have appointment at exact same datetime
    IF EXISTS (SELECT 1 FROM dbo.Appointments WHERE DoctorID = @DoctorId AND AppointmentDate = @AppointmentDate AND Status <> 'Cancelled')
    BEGIN
        RAISERROR('Requested slot is already booked', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Appointments(UserID, DoctorID, AppointmentDate, Status)
    VALUES(@UserId, @DoctorId, @AppointmentDate, @Status);

    SET @Id = SCOPE_IDENTITY();
    -- Audit logging can be added here
END
GO

IF OBJECT_ID('USP_Appointment_GetByUser', 'P') IS NOT NULL
    DROP PROCEDURE USP_Appointment_GetByUser;
GO
CREATE PROCEDURE USP_Appointment_GetByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AppointmentID, UserID, DoctorID, AppointmentDate, Status, CreatedDate
    FROM dbo.Appointments
    WHERE UserID = @UserId
    ORDER BY AppointmentDate DESC;
END
GO

IF OBJECT_ID('USP_Appointment_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE USP_Appointment_GetAll;
GO
CREATE PROCEDURE USP_Appointment_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AppointmentID, UserID, DoctorID, AppointmentDate, Status, CreatedDate
    FROM dbo.Appointments
    ORDER BY AppointmentDate DESC;
END
GO

IF OBJECT_ID('USP_Appointment_UpdateStatus', 'P') IS NOT NULL
    DROP PROCEDURE USP_Appointment_UpdateStatus;
GO
CREATE PROCEDURE USP_Appointment_UpdateStatus
    @Id INT,
    @Status NVARCHAR(20),
    @UserName NVARCHAR(150) = NULL,
    @UserRole NVARCHAR(50) = NULL,
    @IP NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Appointments SET Status = @Status WHERE AppointmentID = @Id;
    -- Audit logging can be added here
END
GO

IF OBJECT_ID('USP_Appointment_Cancel', 'P') IS NOT NULL
    DROP PROCEDURE USP_Appointment_Cancel;
GO
CREATE PROCEDURE USP_Appointment_Cancel
    @Id INT,
    @UserName NVARCHAR(150),
    @UserRole NVARCHAR(50) = NULL,
    @IP NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Ensure caller is owner of the appointment or called by admin (role check optional in app layer)
    IF EXISTS (SELECT 1 FROM dbo.Users u JOIN dbo.Appointments a ON u.UserID = a.UserID WHERE a.AppointmentID = @Id AND u.Name = @UserName)
    BEGIN
        UPDATE dbo.Appointments SET Status = 'Cancelled' WHERE AppointmentID = @Id;
    END
    ELSE
    BEGIN
        RAISERROR('Unauthorized to cancel this appointment', 16, 1);
    END
END
GO

/* ==========================
   Seed minimal data
   ========================== */

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'patient1@example.com')
BEGIN
    INSERT INTO dbo.Users (Name, Email, Password, Role) VALUES ('Patient One', 'patient1@example.com', 'Password123!', 'Patient');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'admin@example.com')
BEGIN
    INSERT INTO dbo.Users (Name, Email, Password, Role) VALUES ('System Admin', 'admin@example.com', 'AdminPass123!', 'SystemAdmin');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Name = 'Dr. Alice')
BEGIN
    INSERT INTO dbo.Doctors (Name, Specialization, AvailableSlots) VALUES ('Dr. Alice', 'General Practitioner', '09:00-12:00,13:30-17:00');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE Name = 'Dr. Bob')
BEGIN
    INSERT INTO dbo.Doctors (Name, Specialization, AvailableSlots) VALUES ('Dr. Bob', 'Cardiologist', '10:00-13:00,14:00-16:00');
END

PRINT 'ClinicAppointments objects and seed data created/verified.';
