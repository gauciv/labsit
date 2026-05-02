-- Simple schedule validation check
USE laboratory_sitin;

-- 1. Show current time information
SELECT 
    NOW() as current_datetime,
    DAYOFWEEK(NOW()) - 1 as current_day_number,
    TIME(NOW()) as current_time_value;

-- 2. Show all students and their schedule counts
SELECT 
    s.student_id,
    CONCAT(s.first_name, ' ', s.last_name) as full_name,
    COUNT(cs.schedule_id) as total_schedules
FROM students s
LEFT JOIN class_schedules cs ON s.student_id = cs.student_id
GROUP BY s.student_id, s.first_name, s.last_name
ORDER BY s.student_id;

-- 3. Show all existing schedules
SELECT 
    cs.student_id,
    s.first_name,
    s.last_name,
    cs.subject_name,
    cs.day_of_week,
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
FROM class_schedules cs
JOIN students s ON cs.student_id = s.student_id
ORDER BY cs.student_id, cs.day_of_week, cs.start_time;

-- 4. Show schedules for current day only
SELECT 
    'Schedules for today' as info,
    cs.student_id,
    s.first_name,
    s.last_name,
    cs.subject_name,
    cs.start_time,
    cs.end_time
FROM class_schedules cs
JOIN students s ON cs.student_id = s.student_id
WHERE cs.day_of_week = DAYOFWEEK(NOW()) - 1
ORDER BY cs.student_id, cs.start_time;

-- 5. Check which students can access NOW
SELECT 
    'Access check for current time' as info,
    cs.student_id,
    s.first_name,
    s.last_name,
    cs.subject_name,
    cs.start_time,
    cs.end_time,
    'ALLOWED NOW' as access_status
FROM class_schedules cs
JOIN students s ON cs.student_id = s.student_id
WHERE cs.day_of_week = DAYOFWEEK(NOW()) - 1
  AND cs.start_time <= TIME(NOW())
  AND cs.end_time > TIME(NOW())
ORDER BY cs.student_id;