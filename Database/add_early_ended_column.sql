-- Add the missing early_ended column to sitin_sessions table
-- Run this in phpMyAdmin or MySQL Workbench

USE laboratory_sitin;

ALTER TABLE sitin_sessions 
ADD COLUMN early_ended BOOLEAN NOT NULL DEFAULT FALSE;

-- Verify the column was added
DESCRIBE sitin_sessions;
