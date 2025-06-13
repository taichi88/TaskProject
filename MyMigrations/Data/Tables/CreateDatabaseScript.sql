
CREATE DATABASE HealthcareApi;
GO

USE HealthcareApi;
GO

CREATE SCHEMA Core;
GO

CREATE SCHEMA Clinical;
GO

CREATE SCHEMA Billing;
GO


CREATE TABLE Clinical.Clinics (
    ClinicId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255)
);

CREATE TABLE Core.Persons (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL, -- Changed to NVARCHAR for broader character support
    Surname NVARCHAR(100) NOT NULL, -- Changed to NVARCHAR
    Email NVARCHAR(100), -- Changed to NVARCHAR
    PersonalNumber VARCHAR(20) NOT NULL UNIQUE,
    DateOfBirth DATE,
    Phone VARCHAR(20), -- Consider NVARCHAR if phone numbers might have non-ASCII characters (rare, but possible)
    Address NVARCHAR(255) -- Changed to NVARCHAR
);

CREATE TABLE Core.DeskStaff (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PersonId INT UNIQUE NOT NULL,
    CONSTRAINT FK_DeskStaff_Person FOREIGN KEY (PersonId) REFERENCES Core.Persons(Id)
);

CREATE TABLE Core.Patients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PersonId INT UNIQUE NOT NULL,
    InsuranceNumber VARCHAR(50),
    EmergencyContactName NVARCHAR(100), -- Changed to NVARCHAR
    EmergencyContactPhone VARCHAR(20),
    BloodType VARCHAR(3),
    Allergies NVARCHAR(MAX), -- Changed to NVARCHAR(MAX) from TEXT
    CONSTRAINT FK_Patients_Person FOREIGN KEY (PersonId) REFERENCES Core.Persons(Id)
);

CREATE TABLE Core.Doctors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PersonId INT UNIQUE NOT NULL,
    Specialty NVARCHAR(100), -- Changed to NVARCHAR
    LicenseNumber VARCHAR(50),
    YearsOfExperience INT,
    CONSTRAINT FK_Doctors_Person FOREIGN KEY (PersonId) REFERENCES Core.Persons(Id)
);

CREATE TABLE Clinical.Appointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    AppointmentDateTime DATETIME NOT NULL,
    ReasonForVisit NVARCHAR(255), -- Changed to NVARCHAR
    Status VARCHAR(50), -- Consider an ENUM type or lookup table if specific statuses are required
    Notes NVARCHAR(MAX), -- Changed to NVARCHAR(MAX) from TEXT
    CONSTRAINT FK_Appointments_Patient FOREIGN KEY (PatientId) REFERENCES Core.Patients(Id), -- *** FIXED: References Core.Patients(Id) ***
    CONSTRAINT FK_Appointments_Doctor FOREIGN KEY (DoctorId) REFERENCES Core.Doctors(Id) -- *** FIXED: References Core.Doctors(Id) ***
);

CREATE TABLE Clinical.Diagnoses (
    DiagnosisId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    DiagnosisDate DATE NOT NULL,
    Description NVARCHAR(255), -- Changed to NVARCHAR
    PrescribedTreatment NVARCHAR(MAX), -- Changed to NVARCHAR(MAX) from TEXT
    DoctorId INT NOT NULL,
    CONSTRAINT FK_Diagnoses_Patient FOREIGN KEY (PatientId) REFERENCES Core.Patients(Id), -- *** FIXED: References Core.Patients(Id) ***
    CONSTRAINT FK_Diagnoses_Doctor FOREIGN KEY (DoctorId) REFERENCES Core.Doctors(Id) -- *** FIXED: References Core.Doctors(Id) ***
);

CREATE TABLE Billing.Accounts (
    AccountId INT PRIMARY KEY IDENTITY(1,1),
    PersonId INT NULL,
    ClinicId INT NULL,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,

    CONSTRAINT FK_Account_Person FOREIGN KEY (PersonId) REFERENCES Core.Persons(Id),
    CONSTRAINT FK_Account_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinical.Clinics(ClinicId),
    -- Constraint to ensure an account is linked to EITHER a Person OR a Clinic, but not both, and not neither.
    CONSTRAINT CHK_Account_Type CHECK (
        (PersonId IS NOT NULL AND ClinicId IS NULL) OR
        (PersonId IS NULL AND ClinicId IS NOT NULL)
    )
);

CREATE TABLE Billing.Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    FromAccountId INT NOT NULL, -- Patient’s account
    ToAccountId INT NOT NULL,   -- Clinic’s account
    Amount DECIMAL(18, 2) NOT NULL,
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
    PaymentMethod VARCHAR(50), -- Consider NVARCHAR if methods might include non-ASCII chars
    Status VARCHAR(50), -- Consider an ENUM type or lookup table for specific statuses
    Notes NVARCHAR(MAX), -- Changed to NVARCHAR(MAX) from TEXT

    CONSTRAINT FK_Payments_Appointment FOREIGN KEY (AppointmentId) REFERENCES Clinical.Appointments(AppointmentId),
    CONSTRAINT FK_Payments_FromAccount FOREIGN KEY (FromAccountId) REFERENCES Billing.Accounts(AccountId),
    CONSTRAINT FK_Payments_ToAccount FOREIGN KEY (ToAccountId) REFERENCES Billing.Accounts(AccountId),
    CONSTRAINT CHK_Accounts_NotEqual CHECK (FromAccountId <> ToAccountId)
);
