# Requirements: Laboratory Sit-in System

## Requirement 1

**User Story:** As a student, I want to log into the sit-in system using my student ID, so that I can record my attendance in the computer laboratory.

### Acceptance Criteria

1. WHEN a student enters a valid student ID THEN the system SHALL authenticate the student and display their information (name, course, year level)
2. WHEN a student enters an invalid or unregistered student ID THEN the system SHALL display an error message and prevent login
3. WHEN a student is already logged in with an active session THEN the system SHALL prevent duplicate sit-in sessions
4. WHEN a student logs in THEN the system SHALL record the session start time based on the current system clock

## Requirement 2

**User Story:** As a student, I want to sit in during my scheduled laboratory class, so that my attendance is properly tracked for the correct subject and time slot.

### Acceptance Criteria

1. WHEN a student logs in THEN the system SHALL match the current day and time to the student's class schedule to determine the active subject
2. WHEN a student has no scheduled class at the current time THEN the system SHALL still allow sit-in but mark it as "unscheduled"
3. WHEN a class schedule is matched THEN the system SHALL display the subject name, scheduled start time, and scheduled end time
4. WHEN the scheduled end time is reached THEN the system SHALL automatically end the sit-in session

## Requirement 3

**User Story:** As an administrator, I want to manage student records, so that I can maintain an accurate database of registered students.

### Acceptance Criteria

1. WHEN an administrator adds a new student THEN the system SHALL store the student ID, first name, last name, course, and year level in the database
2. WHEN an administrator edits a student record THEN the system SHALL update the corresponding fields in the database
3. WHEN an administrator deletes a student record THEN the system SHALL remove the student and all associated data from the database
4. WHEN an administrator searches for a student THEN the system SHALL filter results by student ID, name, or course

## Requirement 4

**User Story:** As an administrator, I want to manage class schedules for students, so that sit-in sessions can be automatically matched to the correct subject.

### Acceptance Criteria

1. WHEN an administrator creates a class schedule THEN the system SHALL store the subject name, day of week, start time, and end time linked to a student
2. WHEN an administrator edits a class schedule THEN the system SHALL update the schedule entry in the database
3. WHEN an administrator deletes a class schedule THEN the system SHALL remove the schedule entry from the database
4. WHEN a schedule has overlapping time slots for the same student on the same day THEN the system SHALL reject the entry and display a validation error

## Requirement 5

**User Story:** As an administrator, I want to view and manage active sit-in sessions, so that I can monitor laboratory usage in real time.

### Acceptance Criteria

1. WHEN the administrator opens the active sessions view THEN the system SHALL display all currently active sit-in sessions with student name, subject, and elapsed time
2. WHEN an administrator force-ends an active session THEN the system SHALL immediately close the session and record the end time
3. WHEN the active sessions view is open THEN the system SHALL refresh the session list at a regular interval
4. WHEN a session is automatically ended by schedule THEN the system SHALL update the active sessions view accordingly

## Requirement 6

**User Story:** As an administrator, I want to view sit-in history and generate reports, so that I can analyze laboratory usage patterns.

### Acceptance Criteria

1. WHEN an administrator views sit-in history THEN the system SHALL display past sessions with student name, subject, date, start time, end time, and duration
2. WHEN an administrator filters sit-in history by date range THEN the system SHALL display only sessions within the specified range
3. WHEN an administrator filters sit-in history by student or subject THEN the system SHALL display only matching sessions
4. WHEN an administrator exports sit-in history THEN the system SHALL generate a report in a standard format (e.g., CSV)

## Requirement 7

**User Story:** As an administrator, I want to configure system settings including the alarm threshold, so that I can be notified when laboratory capacity is nearing its limit.

### Acceptance Criteria

1. WHEN an administrator sets the alarm threshold THEN the system SHALL store the threshold value in the database
2. WHEN the number of active sit-in sessions reaches or exceeds the alarm threshold THEN the system SHALL display a visual alarm notification
3. WHEN the number of active sessions drops below the threshold THEN the system SHALL dismiss the alarm notification
4. WHEN the alarm threshold is updated THEN the system SHALL immediately apply the new threshold to the current session count

## Requirement 8

**User Story:** As an administrator, I want to log in with secure credentials, so that only authorized personnel can access administrative functions.

### Acceptance Criteria

1. WHEN an administrator enters valid credentials THEN the system SHALL grant access to the admin dashboard
2. WHEN an administrator enters invalid credentials THEN the system SHALL display an error and deny access
3. WHEN an administrator logs out THEN the system SHALL return to the login screen and clear the session
4. WHEN the application starts THEN the system SHALL display the login screen as the default view

## Requirement 9

**User Story:** As a developer, I want the application to follow MVVM architecture with a MySQL data access layer, so that the codebase is maintainable and testable.

### Acceptance Criteria

1. WHEN the application is structured THEN the system SHALL separate concerns into Models, ViewModels, and Views following the MVVM pattern
2. WHEN data is accessed THEN the system SHALL use a dedicated data access layer with MySql.Data for all database operations
3. WHEN database connections are used THEN the system SHALL properly open and close connections to prevent resource leaks
4. WHEN configuration values (e.g., connection string) are needed THEN the system SHALL read them from a centralized configuration source
