PRAGMA foreign_keys = ON;

INSERT OR IGNORE INTO Users
    (UserId, FullName, Email, PasswordHash, Role, UserType, StaffId)
VALUES
    (1, 'System Admin', 'admin@example.com', '100000.km+0JMWqlzz+0QKeGut/vw==.D6zajVK5YSfZq9PY/PuvSYsVlMBpNlVED9x7n1B40gA=', 'Admin', 'Admin', 1001);

INSERT OR IGNORE INTO Users
    (UserId, FullName, Email, PasswordHash, Role, UserType, CompanyName, CompanyEmail)
VALUES
    (2, 'Grace Lee', 'employer@example.com', '100000.yGk0uOmH5H7hFJjW8ho1IA==.1fng4rXTlxU5alNIQmGQFhA1AHRUA1ZLTpjOemTQXkM=', 'Employer', 'Employer', 'Future Talent Pte Ltd', 'hr@futuretalent.example');

INSERT OR IGNORE INTO Users
    (UserId, FullName, Email, PasswordHash, Role, UserType, StudentId, DateOfBirth, Phone, Address, GPA)
VALUES
    (3, 'Alan Tan', 'student@example.com', '100000.bWiQYN6heht01OHGLtYCSw==.xu1MG6UXo00FBzLJ3AdMYrny3OLsOXvtdAm+RHIxthI=', 'Student', 'Student', 2026001, '2000-05-15 00:00:00', '+65 9123 4567', 'SkillsFuture Campus', 3.68);

INSERT OR IGNORE INTO Courses (CourseId, CourseName, Description)
VALUES
    (1, 'Diploma in Software Development', 'C# programming, object-oriented design, database systems, and web development.');

INSERT OR IGNORE INTO Modules (ModuleId, ModuleName, CreditValue, CourseId)
VALUES
    (1, 'Object-Oriented Design', 4, 1),
    (2, 'C# Programming', 4, 1),
    (3, 'Database Development', 3, 1);

INSERT OR IGNORE INTO Enrollments (EnrollmentId, StudentId, CourseId, EnrolledOn)
VALUES
    (1, 3, 1, '2026-05-13 02:50:41');

INSERT OR IGNORE INTO Transcripts (TranscriptId, GeneratedDate, GPA, FilePath, StudentId)
VALUES
    (1, '2026-05-06 02:50:41', 3.68, 'FileStorage/Transcripts/alan_tan_transcript.pdf', 3);

INSERT OR IGNORE INTO Grades (GradeId, ModuleName, Score, LetterGrade, CreditValue, TranscriptId)
VALUES
    (1, 'Object-Oriented Design', 88.0, 'A', 4, 1),
    (2, 'C# Programming', 82.0, 'B+', 4, 1),
    (3, 'Database Development', 76.0, 'B+', 3, 1);

INSERT OR IGNORE INTO Certificates
    (Id, CertificateId, AwardTitle, IssueDate, CompletionDate, Status, FilePath, StudentId, TranscriptId)
VALUES
    (1, 'SC-2026-0001', 'Diploma in Software Development', '2026-04-13 00:00:00', '2026-04-20 00:00:00', 'Valid', 'FileStorage/Certificates/SC-2026-0001.pdf', 3, 1),
    (2, 'SC-2026-0002', 'Certificate in Legacy Systems', '2025-05-13 00:00:00', '2025-05-03 00:00:00', 'Revoked', NULL, 3, NULL);
