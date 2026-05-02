-- Check schedules for debugging
-- Run this to see what schedules exist and for which days

USE laboratory_sitin;

-- Show all students
SELECT * FROM students;

-- Show all schedules with day names
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

-- Check for specific student (replace '23123' with actual student ID)
SELECT 
    s.student_id,
    s.first_name,
    s.last_name,
    cs.subject_name,
    CASE cs.day_of_week
        WHEN 0 THEN 'Sunday'
        WHEN 1 THEN 'Monday'
        WHEN 2 THEN 'Tuesday'
        WHEN 3 THEN 'Wednesday'
        WHEN 4 THEN 'Thursday'
        WHEN 5 THEN 'Friday'
        WHEN 6 THEN 'Saturday'
    END as day_name,
    cs.start_time,
    cs.end_time
FROM students s
LEFT JOIN class_schedules cs ON s.student_id = cs.student_id
WHERE s.student_id = '23123';
