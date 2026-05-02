-- Test script to verify schedule validation
USE laboratory_sitin;

-- Show current time info
SELECT 
    'CURRENT TIME INFO' as info,
    NOW() as current_datetime,
    DAYOFWEEK(NOW()) - 1 as current_day_number,
    CASE DAYOFWEEK(NOW()) - 1
        WHEN 0 THEN 'Sunday'
        WHEN 1 THEN 'Monday'
        WHEN 2 THEN 'Tuesday'
        WHEN 3 THEN 'Wednesday'
        WHEN 4 THEN 'Thursday'
        WHEN 5 THEN 'Friday'
        WHEN 6 THEN 'Saturday'
    END as current_day_name,
    TIME(NOW()) as current_time_value;

-- Test case 1: Student with no schedules (should be denied)
SELECT 'TEST CASE 1: Students with no schedules' as test_case;
SELECT 
    s.student_id,
    s.first_name,
    s.last_name,
    COUNT(cs.schedule_id) as schedule_count,
    CASE 
        WHEN COUNT(cs.schedule_id) = 0 THEN 'DENIED - No schedules'
        ELSE 'Has schedules'
    END as access_status
FROM students s
LEFT JOIN class_schedules cs ON s.student_id = cs.student_id
GROUP BY s.student_id, s.first_name, s.last_name
HAVING schedule_count = 0
LIMIT 3;

-- Test case 2: Students with schedules but not for current day/time
SELECT 'TEST CASE 2: All students with their schedule status' as test_case;
SELECT 
    s.student_id,
    s.first_name,
    s.last_name,
    COUNT(cs.schedule_id) as total_schedules,
    COUNT(CASE 
        WHEN cs.day_of_week = DAYOFWEEK(NOW()) - 1 
        AND cs.start_time <= TIME(NOW()) 
        AND cs.end_time > TIME(NOW()) 
        THEN 1 
    END) as active_schedules_now,
    CASE 
        WHEN COUNT(cs.schedule_id) = 0 THEN 'DENIED - No schedules'
        WHEN COUNT(CASE 
            WHEN cs.day_of_week = DAYOFWEEK(NOW()) - 1 
            AND cs.start_time <= TIME(NOW()) 
            AND cs.end_time > TIME(NOW()) 
            THEN 1 
        END) = 0 THEN 'DENIED - No active schedule now'
        ELSE 'ALLOWED - Has active schedule'
    END as access_status
FROM students s
LEFT JOIN class_schedules cs ON s.student_id = cs.student_id
GROUP BY s.student_id, s.first_name, s.last_name
ORDER BY s.student_id;

-- Test case 3: Show what schedules exist for today
SELECT 'TEST CASE 3: Schedules for today' as test_case;
SELECT 
    cs.student_id,
    s.first_name,
    s.last_name,
    cs.subject_name,
    cs.start_time,
    cs.end_time,
    CASE 
        WHEN cs.start_time <= TIME(NOW()) AND cs.end_time > TIME(NOW()) 
        THEN 'ACTIVE NOW'
        WHEN cs.start_time > TIME(NOW()) 
        THEN 'FUTURE'
        ELSE 'PAST'
    END as time_status
FROM class_schedules cs
JOIN students s ON cs.student_id = s.student_id
WHERE cs.day_of_week = DAYOFWEEK(NOW()) - 1
ORDER BY cs.student_id, cs.start_time;