PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Users (
    UserId INTEGER NOT NULL CONSTRAINT PK_Users PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Email TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL,
    UserType TEXT NOT NULL,
    StudentId INTEGER NULL,
    DateOfBirth TEXT NULL,
    Phone TEXT NULL,
    Address TEXT NULL,
    GPA REAL NULL,
    StaffId INTEGER NULL,
    CompanyName TEXT NULL,
    CompanyEmail TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Email ON Users (Email);

CREATE TABLE IF NOT EXISTS Courses (
    CourseId INTEGER NOT NULL CONSTRAINT PK_Courses PRIMARY KEY AUTOINCREMENT,
    CourseName TEXT NOT NULL,
    Description TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Modules (
    ModuleId INTEGER NOT NULL CONSTRAINT PK_Modules PRIMARY KEY AUTOINCREMENT,
    ModuleName TEXT NOT NULL,
    CreditValue INTEGER NOT NULL,
    CourseId INTEGER NOT NULL,
    CONSTRAINT FK_Modules_Courses_CourseId FOREIGN KEY (CourseId)
        REFERENCES Courses (CourseId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Modules_CourseId ON Modules (CourseId);

CREATE TABLE IF NOT EXISTS Enrollments (
    EnrollmentId INTEGER NOT NULL CONSTRAINT PK_Enrollments PRIMARY KEY AUTOINCREMENT,
    StudentId INTEGER NOT NULL,
    CourseId INTEGER NOT NULL,
    EnrolledOn TEXT NOT NULL,
    CONSTRAINT FK_Enrollments_Users_StudentId FOREIGN KEY (StudentId)
        REFERENCES Users (UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Enrollments_Courses_CourseId FOREIGN KEY (CourseId)
        REFERENCES Courses (CourseId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Enrollments_CourseId ON Enrollments (CourseId);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Enrollments_StudentId_CourseId ON Enrollments (StudentId, CourseId);

CREATE TABLE IF NOT EXISTS Transcripts (
    TranscriptId INTEGER NOT NULL CONSTRAINT PK_Transcripts PRIMARY KEY AUTOINCREMENT,
    GeneratedDate TEXT NOT NULL,
    GPA REAL NOT NULL,
    FilePath TEXT NULL,
    StudentId INTEGER NOT NULL,
    CONSTRAINT FK_Transcripts_Users_StudentId FOREIGN KEY (StudentId)
        REFERENCES Users (UserId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Transcripts_StudentId ON Transcripts (StudentId);

CREATE TABLE IF NOT EXISTS Grades (
    GradeId INTEGER NOT NULL CONSTRAINT PK_Grades PRIMARY KEY AUTOINCREMENT,
    ModuleName TEXT NOT NULL,
    Score REAL NOT NULL,
    LetterGrade TEXT NOT NULL,
    CreditValue INTEGER NOT NULL,
    TranscriptId INTEGER NOT NULL,
    CONSTRAINT FK_Grades_Transcripts_TranscriptId FOREIGN KEY (TranscriptId)
        REFERENCES Transcripts (TranscriptId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Grades_TranscriptId ON Grades (TranscriptId);

CREATE TABLE IF NOT EXISTS Certificates (
    Id INTEGER NOT NULL CONSTRAINT PK_Certificates PRIMARY KEY AUTOINCREMENT,
    CertificateId TEXT NOT NULL,
    AwardTitle TEXT NOT NULL,
    IssueDate TEXT NOT NULL,
    CompletionDate TEXT NOT NULL,
    Status TEXT NOT NULL,
    FilePath TEXT NULL,
    StudentId INTEGER NOT NULL,
    TranscriptId INTEGER NULL,
    CONSTRAINT FK_Certificates_Users_StudentId FOREIGN KEY (StudentId)
        REFERENCES Users (UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Certificates_Transcripts_TranscriptId FOREIGN KEY (TranscriptId)
        REFERENCES Transcripts (TranscriptId) ON DELETE SET NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Certificates_CertificateId ON Certificates (CertificateId);
CREATE INDEX IF NOT EXISTS IX_Certificates_StudentId ON Certificates (StudentId);
CREATE INDEX IF NOT EXISTS IX_Certificates_TranscriptId ON Certificates (TranscriptId);
