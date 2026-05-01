-- Laboratory Sit-In System Database Schema
-- Run this script in MySQL (XAMPP) to create the database and seed initial data.

CREATE DATABASE IF NOT EXISTS laboratory_sitin;
USE laboratory_sitin;

-- Students table
CREATE TABLE students (
    student_id VARCHAR(20) PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    course VARCHAR(100) NOT NULL,
    year_level INT NOT NULL
);

-- Class schedules table
CREATE TABLE class_schedules (
    schedule_id INT AUTO_INCREMENT PRIMARY KEY,
    student_id VARCHAR(20) NOT NULL,
    subject_name VARCHAR(100) NOT NULL,
    day_of_week INT NOT NULL,          -- 0=Sunday, 1=Monday, ..., 6=Saturday
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    FOREIGN KEY (student_id) REFERENCES students(student_id) ON DELETE CASCADE
);

-- Sit-in sessions table
CREATE TABLE sitin_sessions (
    session_id INT AUTO_INCREMENT PRIMARY KEY,
    student_id VARCHAR(20) NOT NULL,
    subject_name VARCHAR(100),
    start_time DATETIME NOT NULL,
    end_time DATETIME NULL,
    is_scheduled BOOLEAN NOT NULL DEFAULT FALSE,
    early_ended BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (student_id) REFERENCES students(student_id) ON DELETE CASCADE
);

-- Admin users table
CREATE TABLE admin_users (
    admin_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL
);

-- System settings table
CREATE TABLE system_settings (
    settings_id INT PRIMARY KEY DEFAULT 1,
    alarm_threshold INT NOT NULL DEFAULT 30
);

-- Seed default admin (username: admin, password: admin123)
INSERT INTO admin_users (username, password_hash) VALUES ('admin', SHA2('admin123', 256));

-- Seed default settings (alarm threshold: 30)
INSERT INTO system_settings (settings_id, alarm_threshold) VALUES (1, 30);
