-- Clean up any test data and verify current schedules
USE laboratory_sitin;

-- Remove any test schedules (optional - only run if you want to clean test data)
-- DELETE FROM class_schedules WHERE subject_name LIKE 'Test%';

-- Show current students
SELECT 'STUDENTS:' as info;
SELECT student_id, first_name, last_name, course, year_level FROM students;

-- Show current schedules with readable day names
SELECT 'SCHEDULES:' as info;
SELECT 
    schedule_id,
    student_id,
    subject_name,
    CASE day_of_week
        WHEN 0 THEN 'Sunday'
        WHEN 1 THEN 'Monday'
        WHEN 2 THEN 'Tuesday'
        WHEN 3 THEN 'Wednesday'
        WHEN 4 THEN 'Thursday'
        WHEN 5 THEN 'Friday'
        WHEN 6 THEN 'Saturday'
    END as day_name,
    day_of_week,
    start_time,
    end_time
FROM class_schedules
ORDER BY student_id, day_of_week, start_time;

-- Show what day today is (for reference)
SELECT 'TODAY IS:' as info, DAYOFWEEK(NOW()) - 1 as day_of_week_number,
CASE DAYOFWEEK(NOW()) - 1
    WHEN 0 THEN 'Sunday'
    WHEN 1 THEN 'Monday'
    WHEN 2 THEN 'Tuesday'
    WHEN 3 THEN 'Wednesday'
    WHEN 4 THEN 'Thursday'
    WHEN 5 THEN 'Friday'
    WHEN 6 THEN 'Saturday'
END as day_name;