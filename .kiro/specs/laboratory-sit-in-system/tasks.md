# Implementation Plan: Laboratory Sit-in System

## Overview

Incremental implementation of a WPF (C# .NET) desktop application for tracking student laboratory sit-in sessions. The app uses MVVM architecture, MySQL via XAMPP for persistence, and includes real-time session monitoring with alarm notifications. Tasks are ordered: solution scaffolding → data layer → core ViewModels → Views → real-time features → final wiring.

## Tasks

- [x] 1. Create Visual Studio solution structure and foundational infrastructure
  - [x] 1.1 Create the WPF solution (.sln) and project (.csproj) with MySql.Data NuGet reference
    - Create `LaboratorySitInSystem.sln` and `LaboratorySitInSystem.csproj` targeting .NET (WPF)
    - Add `MySql.Data` NuGet package reference
    - Create folder structure: `Models/`, `ViewModels/`, `Views/`, `DataAccess/`, `Helpers/`
    - _Requirements: 13 (Visual Studio solution structure)_

  - [x] 1.2 Implement MVVM base classes (`ViewModelBase`, `RelayCommand`)
    - Create `ViewModels/ViewModelBase.cs` with `INotifyPropertyChanged` and `SetProperty<T>` helper
    - Create `Helpers/RelayCommand.cs` implementing `ICommand` with `Action<object>` execute and `Predicate<object>` canExecute
    - _Requirements: MVVM pattern foundation_

  - [x] 1.3 Implement `DatabaseHelper` and MySQL connection setup
    - Create `DataAccess/DatabaseHelper.cs` with static `Initialize(string connectionString)` and `GetConnection()` methods
    - Read connection string from `App.config` or hardcoded default for XAMPP (`Server=localhost;Database=laboratory_sitin;Uid=root;Pwd=;`)
    - _Requirements: 12 (Database connectivity)_

  - [x] 1.4 Create the MySQL database schema script
    - Create `Database/schema.sql` with `CREATE DATABASE`, all 5 tables (`students`, `class_schedules`, `sitin_sessions`, `admin_users`, `system_settings`), foreign keys, and seed data (default admin, default settings)
    - _Requirements: 12 (Database connectivity)_

- [x] 2. Implement Models (domain entities)
  - [x] 2.1 Create all model classes
    - Create `Models/Student.cs` with `StudentId`, `FirstName`, `LastName`, `Course`, `YearLevel`, `FullName` computed property
    - Create `Models/ClassSchedule.cs` with `ScheduleId`, `StudentId`, `SubjectName`, `DayOfWeek`, `StartTime`, `EndTime`
    - Create `Models/SitInSession.cs` with `SessionId`, `StudentId`, `StudentName`, `SubjectName`, `StartTime`, `EndTime`, `IsScheduled`, `Duration` computed property
    - Create `Models/AdminUser.cs` with `AdminId`, `Username`, `PasswordHash`
    - Create `Models/SystemSettings.cs` with `SettingsId`, `AlarmThreshold`
    - _Requirements: 1.1, 4.1, 6.1, 1.3, 10.1_

- [x] 3. Implement Data Access Layer (Repositories)
  - [x] 3.1 Implement `IStudentRepository` and `StudentRepository`
    - Create `DataAccess/IStudentRepository.cs` interface with `GetAll()`, `GetById()`, `Search()`, `Add()`, `Update()`, `Delete()`
    - Create `DataAccess/StudentRepository.cs` with MySQL queries using `MySqlCommand` and parameterized queries
    - _Requirements: 4.1, 4.2, 4.3 (Student enrollment CRUD)_

  - [x] 3.2 Implement `IScheduleRepository` and `ScheduleRepository`
    - Create `DataAccess/IScheduleRepository.cs` interface with `GetByStudentId()`, `GetActiveSchedule()`, `Add()`, `Update()`, `Delete()`, `HasOverlap()`
    - Create `DataAccess/ScheduleRepository.cs` with MySQL queries; `GetActiveSchedule` matches by `DayOfWeek` and current time within `start_time`/`end_time`; `HasOverlap` checks for time range intersection on same student and day
    - _Requirements: 5.1, 5.2, 5.3, 5.4 (Class schedule management)_

  - [x] 3.3 Implement `ISessionRepository` and `SessionRepository`
    - Create `DataAccess/ISessionRepository.cs` interface with `GetActiveSessions()`, `GetActiveSessionByStudent()`, `GetHistory()`, `StartSession()`, `EndSession()`, `GetActiveSessionCount()`
    - Create `DataAccess/SessionRepository.cs` with MySQL queries; `GetActiveSessions` returns sessions where `end_time IS NULL`; `GetHistory` supports filtering by date range, student, and subject
    - _Requirements: 6.1, 6.2, 6.3, 7.1, 8.1, 9.1 (Session tracking, force-end, alarm, auto-complete)_

  - [x] 3.4 Implement `IAdminRepository` and `AdminRepository`
    - Create `DataAccess/IAdminRepository.cs` interface with `Authenticate()`
    - Create `DataAccess/AdminRepository.cs`; `Authenticate` compares `SHA2(password, 256)` hash against stored `password_hash`
    - _Requirements: 1.1, 1.2, 1.3 (Admin login and password reset)_

  - [x] 3.5 Implement `ISettingsRepository` and `SettingsRepository`
    - Create `DataAccess/ISettingsRepository.cs` interface with `GetSettings()`, `UpdateAlarmThreshold()`
    - Create `DataAccess/SettingsRepository.cs` with MySQL queries against `system_settings` table
    - _Requirements: 8.1, 8.2 (Alarm threshold configuration)_

  - [x] 3.6 Write unit tests for repository layer
    - Test `StudentRepository` CRUD operations
    - Test `ScheduleRepository.HasOverlap` with overlapping and non-overlapping time ranges
    - Test `SessionRepository.GetActiveSessions` returns only sessions with null `end_time`
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [x] 4. Checkpoint - Verify data layer
  - Ensure all repository implementations compile and tests pass, ask the user if questions arise.

- [x] 5. Implement navigation infrastructure and LoginViewModel
  - [x] 5.1 Implement navigation service for View switching
    - Create a simple navigation mechanism in `MainWindow` using a `ContentControl` bound to a `CurrentView` property on a `MainViewModel`
    - `MainViewModel` exposes `CurrentView` (object) and methods to switch between Views
    - _Requirements: Navigation between login, student sit-in, and admin dashboard_

  - [x] 5.2 Implement `LoginViewModel` with admin authentication
    - Create `ViewModels/LoginViewModel.cs` with `Username`, `Password` properties and `LoginCommand`
    - On login: call `AdminRepository.Authenticate()`; on success navigate to `AdminDashboardView`; on failure show error message
    - Include "Student Sit-In" button/command to navigate to `StudentSitInView`
    - _Requirements: 1.1, 1.2 (Admin login with username/password)_

  - [x] 5.3 Implement password reset functionality in `LoginViewModel`
    - Add `ResetPasswordCommand` that allows admin to reset password (e.g., via a dialog or inline form)
    - Hash new password with SHA256 and update via `AdminRepository`
    - _Requirements: 1.3 (Password reset)_

- [x] 6. Implement Student Sit-In flow
  - [x] 6.1 Implement `StudentSitInViewModel`
    - Create `ViewModels/StudentSitInViewModel.cs` with `StudentIdInput`, `CurrentStudent`, `MatchedSchedule`, `StatusMessage` properties and `LoginCommand`
    - On login: validate student exists via `StudentRepository.GetById()`; check no active session via `SessionRepository.GetActiveSessionByStudent()`; match schedule via `ScheduleRepository.GetActiveSchedule(studentId, today, now)`; start session via `SessionRepository.StartSession()`
    - Set `IsScheduled = true` if schedule matched, `false` for walk-in; populate `SubjectName` from matched schedule or leave null for walk-in
    - Show confirmation message with student name, subject, and session start time
    - _Requirements: 2.1, 2.2, 6.1, 6.2, 6.3 (Student login, sit-in application, session tracking)_

  - [x] 6.2 Write unit tests for `StudentSitInViewModel` login flow
    - Test successful sit-in with matching schedule sets `IsScheduled = true`
    - Test walk-in sit-in (no matching schedule) sets `IsScheduled = false`
    - Test duplicate active session is rejected
    - Test invalid student ID shows error
    - _Requirements: 2.1, 6.1, 6.2_

- [x] 7. Implement Admin Dashboard and Student Management
  - [x] 7.1 Implement `AdminDashboardViewModel`
    - Create `ViewModels/AdminDashboardViewModel.cs` with navigation commands to sub-views: Student Management, Schedule Management, Active Sessions, Sit-In History, Settings, About
    - Display summary counts (total students, active sessions) on dashboard
    - _Requirements: 1.2 (Admin dashboard access)_

  - [x] 7.2 Implement `StudentManagementViewModel`
    - Create `ViewModels/StudentManagementViewModel.cs` with `Students` (ObservableCollection), `SelectedStudent`, `SearchQuery` properties
    - Implement `AddCommand`, `UpdateCommand`, `DeleteCommand`, `SearchCommand` using `IStudentRepository`
    - Search filters by student ID, name, or course
    - _Requirements: 4.1, 4.2, 4.3 (Enroll, update, delete students)_

  - [x] 7.3 Implement `ScheduleManagementViewModel`
    - Create `ViewModels/ScheduleManagementViewModel.cs` with `Schedules` (ObservableCollection), `SelectedSchedule` properties
    - Implement `AddCommand`, `UpdateCommand`, `DeleteCommand`; validate no time overlap via `ScheduleRepository.HasOverlap()` before add/update
    - Allow selecting a student to view/manage their schedules
    - _Requirements: 5.1, 5.2, 5.3, 5.4 (CRUD schedules with overlap validation)_

- [x] 8. Implement Active Sessions monitoring and alarm
  - [x] 8.1 Implement `ActiveSessionsViewModel` with real-time refresh
    - Create `ViewModels/ActiveSessionsViewModel.cs` with `ActiveSessions` (ObservableCollection), `IsAlarmActive`, `AlarmThreshold` properties
    - Set up `DispatcherTimer` ticking every 30 seconds to refresh active sessions from `SessionRepository.GetActiveSessions()` and update `Duration` display
    - Check `GetActiveSessionCount()` against `AlarmThreshold` from `SettingsRepository`; set `IsAlarmActive = true` when threshold exceeded
    - _Requirements: 6.3, 8.1, 8.2 (Real-time session list, alarm notification)_

  - [x] 8.2 Implement admin force-end session
    - Add `ForceEndSessionCommand` to `ActiveSessionsViewModel`; calls `SessionRepository.EndSession(sessionId, DateTime.Now)` for selected session
    - Refresh session list after force-end
    - _Requirements: 7.1 (Admin force-end active session)_

  - [x] 8.3 Implement automatic session completion for scheduled sessions
    - In the `DispatcherTimer` tick handler, iterate active sessions where `IsScheduled = true`; if `DateTime.Now >= schedule.EndTime`, auto-end the session via `SessionRepository.EndSession()`
    - Requires cross-referencing active session's student schedule to determine end time
    - _Requirements: 9.1 (Automatic session end when scheduled time expires)_

  - [x] 8.4 Write unit tests for alarm threshold logic
    - Test alarm activates when active session count exceeds threshold
    - Test alarm deactivates when count drops below threshold
    - _Requirements: 8.1, 8.2_

- [x] 9. Checkpoint - Verify ViewModels and business logic
  - Ensure all ViewModel implementations compile and tests pass, ask the user if questions arise.

- [x] 10. Implement Sit-In History and Settings
  - [x] 10.1 Implement `SitInHistoryViewModel`
    - Create `ViewModels/SitInHistoryViewModel.cs` with `Sessions` (ObservableCollection), filter properties (`FromDate`, `ToDate`, `StudentIdFilter`, `SubjectFilter`), and `SearchCommand`
    - Load history from `SessionRepository.GetHistory()` with applied filters
    - Display session duration, student name, subject, and whether it was scheduled
    - _Requirements: 6.3 (Session history and reporting)_

  - [x] 10.2 Implement `SettingsViewModel`
    - Create `ViewModels/SettingsViewModel.cs` with `AlarmThreshold` property and `SaveCommand`
    - Load current threshold from `SettingsRepository.GetSettings()`; save via `SettingsRepository.UpdateAlarmThreshold()`
    - _Requirements: 8.2 (Admin configures alarm threshold)_

- [x] 11. Implement all Views (XAML)
  - [x] 11.1 Create `MainWindow.xaml` with navigation shell
    - `ContentControl` bound to `MainViewModel.CurrentView`
    - Basic window chrome, title, and sizing
    - _Requirements: 13 (Solution structure)_

  - [x] 11.2 Create `LoginView.xaml`
    - Username and password fields, Login button, Student Sit-In button, Reset Password link
    - Bind to `LoginViewModel` properties and commands
    - _Requirements: 1.1, 1.2, 1.3_

  - [x] 11.3 Create `StudentSitInView.xaml`
    - Student ID input field, Login/Start button, status message display, back-to-login button
    - Show matched schedule info and confirmation on successful sit-in
    - Bind to `StudentSitInViewModel`
    - _Requirements: 2.1, 6.1_

  - [x] 11.4 Create `AdminDashboardView.xaml`
    - Navigation menu/sidebar with links to sub-views (Students, Schedules, Active Sessions, History, Settings, About)
    - Summary panel with counts
    - Bind to `AdminDashboardViewModel`
    - _Requirements: 1.2_

  - [x] 11.5 Create `StudentManagementView.xaml`
    - DataGrid for student list, search bar, Add/Edit/Delete buttons, form fields for student details
    - Bind to `StudentManagementViewModel`
    - _Requirements: 4.1, 4.2, 4.3_

  - [x] 11.6 Create `ScheduleManagementView.xaml`
    - Student selector, DataGrid for schedules, Add/Edit/Delete buttons, form fields with day-of-week picker and time pickers
    - Bind to `ScheduleManagementViewModel`
    - _Requirements: 5.1, 5.2, 5.3, 5.4_

  - [x] 11.7 Create `ActiveSessionsView.xaml`
    - DataGrid showing active sessions with student name, subject, start time, duration (updating)
    - Force-End button per row, alarm indicator (visual highlight when threshold exceeded)
    - Bind to `ActiveSessionsViewModel`
    - _Requirements: 6.3, 7.1, 8.1_

  - [x] 11.8 Create `SitInHistoryView.xaml`
    - Filter controls (date range, student ID, subject), Search button, DataGrid for history records
    - Bind to `SitInHistoryViewModel`
    - _Requirements: 6.3_

  - [x] 11.9 Create `SettingsView.xaml`
    - Alarm threshold input, Save button
    - Bind to `SettingsViewModel`
    - _Requirements: 8.2_

  - [x] 11.10 Create About panel view
    - Display application name, version, developer info
    - Bind to a simple `AboutViewModel` or use code-behind for static content
    - _Requirements: 10.1 (About panel)_

- [x] 12. Wire everything together in `App.xaml.cs`
  - [x] 12.1 Initialize database connection and register dependencies in `App.xaml.cs`
    - Call `DatabaseHelper.Initialize()` with connection string on startup
    - Instantiate repositories and inject into ViewModels
    - Set `MainWindow.DataContext` to `MainViewModel` with initial `LoginView`
    - _Requirements: 12 (Database connectivity), 13 (Solution structure)_

  - [x] 12.2 Register DataTemplates for ViewModel-to-View mapping
    - In `App.xaml` or `MainWindow.xaml`, add `DataTemplate` entries mapping each ViewModel type to its corresponding View
    - _Requirements: Navigation infrastructure_

- [x] 13. Final checkpoint - Full integration verification
  - Ensure all code compiles, all tests pass, and Views are wired to ViewModels correctly. Ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- The DispatcherTimer in task 8 handles both alarm checking and automatic session completion
- All database operations use parameterized queries to prevent SQL injection
