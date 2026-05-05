-- ============================================================
-- Laboratory Sit-In System - Database Schema
-- Import this file directly into phpMyAdmin.
-- ============================================================

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";

CREATE DATABASE IF NOT EXISTS `laboratory_sitin` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `laboratory_sitin`;

-- Table: students
CREATE TABLE IF NOT EXISTS `students` (
    `student_id` VARCHAR(20) NOT NULL,
    `first_name` VARCHAR(100) NOT NULL,
    `last_name` VARCHAR(100) NOT NULL,
    `course` VARCHAR(100) NOT NULL,
    `year_level` INT NOT NULL,
    PRIMARY KEY (`student_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: class_schedules
CREATE TABLE IF NOT EXISTS `class_schedules` (
    `schedule_id` INT AUTO_INCREMENT NOT NULL,
    `student_id` VARCHAR(20) NOT NULL,
    `subject_name` VARCHAR(100) NOT NULL,
    `day_of_week` INT NOT NULL,
    `start_time` TIME NOT NULL,
    `end_time` TIME NOT NULL,
    PRIMARY KEY (`schedule_id`),
    FOREIGN KEY (`student_id`) REFERENCES `students`(`student_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: sitin_sessions
CREATE TABLE IF NOT EXISTS `sitin_sessions` (
    `session_id` INT AUTO_INCREMENT NOT NULL,
    `student_id` VARCHAR(20) NOT NULL,
    `subject_name` VARCHAR(100) DEFAULT NULL,
    `start_time` DATETIME NOT NULL,
    `end_time` DATETIME DEFAULT NULL,
    `is_scheduled` TINYINT(1) NOT NULL DEFAULT 0,
    `early_ended` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`session_id`),
    FOREIGN KEY (`student_id`) REFERENCES `students`(`student_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: admin_users
CREATE TABLE IF NOT EXISTS `admin_users` (
    `admin_id` INT AUTO_INCREMENT NOT NULL,
    `username` VARCHAR(50) NOT NULL,
    `password_hash` VARCHAR(255) NOT NULL,
    PRIMARY KEY (`admin_id`),
    UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: system_settings
CREATE TABLE IF NOT EXISTS `system_settings` (
    `settings_id` INT NOT NULL DEFAULT 1,
    `alarm_threshold` INT NOT NULL DEFAULT 30,
    PRIMARY KEY (`settings_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Default admin (username: admin, password: admin123)
INSERT INTO `admin_users` (`username`, `password_hash`) VALUES
('admin', SHA2('admin123', 256));

-- Default settings
INSERT INTO `system_settings` (`settings_id`, `alarm_threshold`) VALUES
(1, 30);

COMMIT;
