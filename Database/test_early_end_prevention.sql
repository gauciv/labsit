-- Test script for early end prevention functionality
USE laboratory_sitin;

-- Show current sessions and their early_ended status
SELECT 
    'CURRENT SESSIONS' as info,
    session_id,
    student_id,
    subject_name,
    start_time,
    end_time,
    early_ended,
    CASE 
        WHEN end_time IS NULL THEN 'ACTIVE'
        WHEN early_ended = TRUE THEN 'ENDED EARLY'
        ELSE 'COMPLETED NORMALLY'
    END as session_status
FROM sitin_sessions
ORDER BY start_time DESC;

-- Check for students who ended sessions early today
SELECT 
    'STUDENTS WHO ENDED EARLY TODAY' as info,
    student_id,
    subject_name,
    start_time,
    end_time,
    'CANNOT REJOIN TODAY' as rejoin_status
FROM sitin_sessions
WHERE DATE(start_time) = CURDATE()
  AND early_ended = TRUE
ORDER BY student_id, subject_name;

-- Test query: Check if a specific student ended early for a subject today
-- (This is what the HasEndedSessionEarlyToday method does)
SELECT 
    'TEST: Check if student ended early today' as test_case,
    COUNT(*) as early_end_count,
    CASE 
        WHEN COUNT(*) > 0 THEN 'BLOCKED - Cannot rejoin'
        ELSE 'ALLOWED - Can sit in'
    END as access_status
FROM sitin_sessions 
WHERE student_id = '23123'  -- Replace with actual student ID
  AND subject_name = 'Computer Programming'  -- Replace with actual subject
  AND DATE(start_time) = CURDATE()
  AND early_ended = TRUE;