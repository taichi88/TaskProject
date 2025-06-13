-- Clean up existing data (optional, for re-running)
-- Be careful with DELETE statements in a production environment!
DELETE FROM Billing.Payments;
DELETE FROM Clinical.Diagnoses;
DELETE FROM Clinical.Appointments;
DELETE FROM Core.Doctors;
DELETE FROM Core.Patients;
DELETE FROM Core.DeskStaff;
DELETE FROM Billing.Accounts;
DELETE FROM Clinical.Clinics;
DELETE FROM Core.Persons;

-- ---
-- Sample Data Insertion
-- ---

-- 1. Insert into Core.Persons
INSERT INTO Core.Persons (Name, Surname, Email, PersonalNumber, DateOfBirth, Phone, Address) VALUES
('John', 'Doe', 'john.doe@example.com', '12345678901', '1980-05-15', '555-111-2222', '123 Oak Ave'),
('Jane', 'Smith', 'jane.smith@example.com', '23456789012', '1992-11-23', '555-333-4444', '456 Pine St'),
('Alice', 'Johnson', 'alice.j@example.com', '34567890123', '1975-01-01', '555-555-6666', '789 Maple Rd'),
('Bob', 'Williams', 'bob.w@example.com', '45678901234', '1988-07-30', '555-777-8888', '101 Elm Blvd'),
('Emily', 'Brown', 'emily.b@example.com', '56789012345', '1995-03-10', '555-999-0000', '202 Birch Ln'),
('Michael', 'Green', 'michael.g@example.com', '67890123456', '1982-09-05', '555-123-4567', '303 Cedar Ct');

-- 2. Insert into Core.Doctors (using PersonId from above)
INSERT INTO Core.Doctors (PersonId, Specialty, LicenseNumber, YearsOfExperience) VALUES
((SELECT Id FROM Core.Persons WHERE PersonalNumber = '12345678901'), 'Cardiology', 'LIC12345', 15), -- John Doe
((SELECT Id FROM Core.Persons WHERE PersonalNumber = '34567890123'), 'Pediatrics', 'LIC67890', 20); -- Alice Johnson

-- 3. Insert into Core.Patients (using PersonId from above)
INSERT INTO Core.Patients (PersonId, InsuranceNumber, EmergencyContactName, EmergencyContactPhone, BloodType, Allergies) VALUES
((SELECT Id FROM Core.Persons WHERE PersonalNumber = '23456789012'), 'INS987654321', 'David Smith', '555-333-4445', 'A+', 'Penicillin'), -- Jane Smith
((SELECT Id FROM Core.Persons WHERE PersonalNumber = '45678901234'), 'INS123456789', 'Sarah Williams', '555-777-8889', 'O-', NULL), -- Bob Williams
((SELECT Id FROM Core.Persons WHERE PersonalNumber = '56789012345'), 'INS555666777', 'Tom Brown', '555-999-0001', 'B+', 'Latex'); -- Emily Brown

-- 4. Insert into Core.DeskStaff (using PersonId from above)
INSERT INTO Core.DeskStaff (PersonId) VALUES
((SELECT Id FROM Core.Persons WHERE PersonalNumber = '67890123456')); -- Michael Green

-- 5. Insert into Clinical.Clinics
INSERT INTO Clinical.Clinics (Name, Address) VALUES
('City Central Clinic', '100 Main St, Anytown'),
('Pediatric Care Center', '200 Oak Ave, Anytown');

-- 6. Insert into Billing.Accounts (linking to Persons and Clinics)
INSERT INTO Billing.Accounts (PersonId, ClinicId, Balance) VALUES
((SELECT Id FROM Core.Persons WHERE PersonalNumber = '23456789012'), NULL, 100000.00), -- Jane Smith's Account
((SELECT Id FROM Core.Persons WHERE PersonalNumber = '45678901234'), NULL, 500000.00), -- Bob Williams's Account (initial balance owed)
((SELECT Id FROM Core.Persons WHERE PersonalNumber = '56789012345'), NULL, 900000.00), -- Emily Brown's Account
(NULL, (SELECT ClinicId FROM Clinical.Clinics WHERE Name = 'City Central Clinic'), 5214700.00), -- City Central Clinic Account
(NULL, (SELECT ClinicId FROM Clinical.Clinics WHERE Name = 'Pediatric Care Center'), 54125487.00); -- Pediatric Care Center Account

-- 7. Insert into Clinical.Appointments
INSERT INTO Clinical.Appointments (PatientId, DoctorId, AppointmentDateTime, ReasonForVisit, Status, Notes) VALUES
((SELECT Id FROM Core.Patients WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '23456789012')), -- Jane Smith's PatientId
 (SELECT Id FROM Core.Doctors WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '12345678901')), -- John Doe's DoctorId
 '2025-06-15 10:00:00', 'Routine check-up', 'Scheduled', NULL),
((SELECT Id FROM Core.Patients WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '45678901234')), -- Bob Williams's PatientId
 (SELECT Id FROM Core.Doctors WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '34567890123')), -- Alice Johnson's DoctorId
 '2025-06-16 14:30:00', 'Childhood Vaccination', 'Completed', 'Patient tolerated vaccine well.'),
((SELECT Id FROM Core.Patients WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '56789012345')), -- Emily Brown's PatientId
 (SELECT Id FROM Core.Doctors WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '12345678901')), -- John Doe's DoctorId
 '2025-06-17 09:00:00', 'Chest pain evaluation', 'Scheduled', 'Patient reported intermittent chest pain.');

-- 8. Insert into Clinical.Diagnoses (Linked to PatientId and DoctorId from Core.Patients/Core.Doctors)
INSERT INTO Clinical.Diagnoses (PatientId, DiagnosisDate, Description, PrescribedTreatment, DoctorId) VALUES
((SELECT Id FROM Core.Patients WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '45678901234')), -- Bob Williams's PatientId
 '2025-06-16', 'Routine pediatric visit', 'Vaccination (MMR)',
 (SELECT Id FROM Core.Doctors WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '34567890123'))); -- Alice Johnson's DoctorId

-- 9. Insert into Billing.Payments
-- Example: Bob Williams pays for his vaccination appointment
INSERT INTO Billing.Payments (AppointmentId, FromAccountId, ToAccountId, Amount, PaymentDate, PaymentMethod, Status, Notes) VALUES
(
    (SELECT AppointmentId FROM Clinical.Appointments WHERE PatientId = (SELECT Id FROM Core.Patients WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '45678901234')) AND AppointmentDateTime = '2025-06-16 14:30:00'), -- Bob Williams's appointment
    (SELECT AccountId FROM Billing.Accounts WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '45678901234')), -- Bob Williams's Account
    (SELECT AccountId FROM Billing.Accounts WHERE ClinicId = (SELECT ClinicId FROM Clinical.Clinics WHERE Name = 'Pediatric Care Center')), -- Pediatric Care Center's Account
    75.00,
    '2025-06-16 15:00:00',
    'Credit Card',
    'Completed',
    'Paid by patient for vaccination'
);

-- ---
-- Verification Queries
-- ---

SELECT * FROM Core.Persons;
SELECT * FROM Core.Doctors;
SELECT * FROM Core.Patients;
SELECT * FROM Core.DeskStaff;
SELECT * FROM Clinical.Clinics;
SELECT * FROM Billing.Accounts;
SELECT * FROM Clinical.Appointments;
SELECT * FROM Clinical.Diagnoses;
SELECT * FROM Billing.Payments;

-- Example: Check a patient's account balance (manual update after payment)
-- In a real application, a trigger or stored procedure would update the balance automatically
-- For now, let's show how you'd manually update based on the payment
-- UPDATE Billing.Accounts
-- SET Balance = Balance - 75.00
-- WHERE AccountId = (SELECT AccountId FROM Billing.Accounts WHERE PersonId = (SELECT Id FROM Core.Persons WHERE PersonalNumber = '45678901234')); -- Bob Williams's account

-- UPDATE Billing.Accounts
-- SET Balance = Balance + 75.00
-- WHERE AccountId = (SELECT AccountId FROM Billing.Accounts WHERE ClinicId = (SELECT ClinicId FROM Clinical.Clinics WHERE Name = 'Pediatric Care Center')); -- Pediatric Care Center's account
