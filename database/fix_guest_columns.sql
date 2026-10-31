-- Hotel Keeper: bring an older `hotelkeeper` database up to the current schema.
--
-- Main fix: "Register guest" failed with "A database error occurred". The app log showed
--   MySqlException: Column 'UserId' cannot be null   (INSERT INTO Guests)
-- because the old Guests table declared UserId NOT NULL, while a guest registered as a record
-- (no sign-in account) is stored with UserId = NULL.
--
-- Safe to run more than once. No app rebuild or restart is needed for the UserId fix.
-- Mirrors the additive migrations in src/HotelKeeper.Core/Data/DatabaseSetup.cs.
-- Run in MySQL Workbench or:  mysql -u root -p < database/fix_guest_columns.sql

USE hotelkeeper;

-- 1) Guests.UserId must allow NULL (keeps its UNIQUE key and foreign key to Users).
ALTER TABLE Guests MODIFY COLUMN UserId INT NULL;

-- 2) Guests.IdDocumentType (added only if missing)
SET @sql = IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Guests' AND COLUMN_NAME = 'IdDocumentType') = 0,
  'ALTER TABLE Guests ADD COLUMN IdDocumentType VARCHAR(40) NOT NULL DEFAULT '''' AFTER Nationality',
  'SELECT ''Guests.IdDocumentType already exists'' AS info');
PREPARE st FROM @sql; EXECUTE st; DEALLOCATE PREPARE st;

-- 3) Guests.StayDuration (added only if missing)
SET @sql = IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Guests' AND COLUMN_NAME = 'StayDuration') = 0,
  'ALTER TABLE Guests ADD COLUMN StayDuration INT NOT NULL DEFAULT 0 AFTER IdDocument',
  'SELECT ''Guests.StayDuration already exists'' AS info');
PREPARE st FROM @sql; EXECUTE st; DEALLOCATE PREPARE st;

-- 4) Stays.ExpectedCheckOutAt (added only if missing)
SET @sql = IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Stays' AND COLUMN_NAME = 'ExpectedCheckOutAt') = 0,
  'ALTER TABLE Stays ADD COLUMN ExpectedCheckOutAt DATETIME NULL AFTER ExpectedCheckOutDate',
  'SELECT ''Stays.ExpectedCheckOutAt already exists'' AS info');
PREPARE st FROM @sql; EXECUTE st; DEALLOCATE PREPARE st;

-- Check: in the Guests listing, UserId should show Null = YES.
SHOW COLUMNS FROM Guests;
