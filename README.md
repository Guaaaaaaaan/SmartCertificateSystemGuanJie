# Smart Certificate Verification & Student Records Management System

ASP.NET Core MVC final project for certificate verification and student records management.

## Tech Stack

- .NET 10 / ASP.NET Core MVC
- SQLite + EF Core
- Custom session authentication
- xUnit tests

## Run

```powershell
dotnet restore .\SmartCertificateSystem.slnx
dotnet run --project .\SmartCertificateSystem\SmartCertificateSystem.csproj
```

Open the URL shown by `dotnet run`, or use `http://localhost:5221` when launched with the included profile.

## Demo Accounts

- Admin: `admin@example.com` / `Admin123!`
- Student: `student@example.com` / `Student123!`
- Employer: `employer@example.com` / `Employer123!`

## Demo Certificate

- Certificate ID: `SC-2026-0001`
- Student Name: `Alan Tan`
- Date of Birth: `2000-05-15`

## Tests

```powershell
dotnet test .\SmartCertificateSystem.slnx
```

## Included Deliverables

- MVC source code
- xUnit test cases
- UML PlantUML source files and exported PNG diagrams
- Original project requirements PDF
