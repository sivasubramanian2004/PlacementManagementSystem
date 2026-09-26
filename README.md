# Placement Management System (PMS)

A role-based Placement Management System built using **ASP.NET Core Web API (.NET 10)** and **SQL Server**.

The system manages users, students, departments, companies, placement drives, applications, academic details, authentication, authorization, email notifications, file uploads, search/filter/sort/pagination, and Excel exports.

---

## 🚀 Project Overview

The Placement Management System manages the complete placement workflow:

```text
User Registration / Login
        ↓
Student Profile
        ↓
Placement Drive
        ↓
Student Application
        ↓
Eligibility Validation
        ↓
Shortlisting
        ↓
Interview
        ↓
Selection / Rejection
```

---

## 🛠️ Tech Stack

### Backend

- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- BCrypt.Net-Next
- FluentValidation
- Serilog
- Mapster
- MailKit / MimeKit
- ClosedXML
- Swagger / OpenAPI

### Testing

- xUnit
- Moq
- MockQueryable.Moq
- Microsoft.NET.Test.Sdk

### Tools

- Visual Studio 2026
- SQL Server Management Studio
- Postman
- Git
- GitHub
- Docker

---

# 🏗️ Architecture

The project follows a layered architecture.

```text
PlacementManagementSystem
│
├── PMS.API
│   ├── Controllers
│   ├── Middleware
│   └── Program.cs
│
├── PMS.Core
│   ├── DTOs
│   ├── Interfaces
│   ├── Validators
│   ├── Models
│   ├── Helpers
│   └── Extensions
│
├── PMS.Data
│   ├── Entities
│   ├── Context
│   ├── Configurations
│   └── Repositories
│
├── PMS.Service
│   ├── Services
│   ├── Mappings
│   ├── Security
│   └── Email
│
└── PMS.Tests
    ├── Unit Tests
    └── Service Tests
```

---

# 📦 Core Modules

## 1. Authentication & User Management

Features:

- User registration
- Login
- JWT authentication
- Role-based authorization
- Forgot password
- OTP verification
- Reset password
- Account activation/deactivation
- BCrypt password hashing
- Authentication rate limiting

### User Roles

```text
Student
Admin
PlacementOfficer
Recruiter
```

---

## 2. Department Management

Features:

- Create Department
- Get Department
- Update Department
- Delete Department
- Search
- Filter
- Sorting
- Pagination
- Unique Department Code
- Soft Delete

Example:

```text
Computer Science and Engineering
CSE
```

---

## 3. Student Management

Features:

- Create Student Profile
- Get Student
- Update Student
- Delete Student
- Department mapping
- Resume upload
- Profile photo upload
- Skills
- Placement status
- Education details

Relationship:

```text
User 1 : 1 Student
Department 1 : N Student
Student 1 : N EducationDetails
```

---

## 4. Education Details

A student can have multiple education records.

```text
Student 1 : N EducationDetails
```

### Education Types

```text
SSLC
HSC
Diploma
Undergraduate
Postgraduate
```

Education details include:

- Education Type
- Institution
- Percentage / CGPA
- Backlogs
- Year of Passing
- Location

---

## 5. Company Management

Features:

- Create Company
- Get Company
- Update Company
- PATCH Company
- Soft Delete
- Industry classification
- Website
- Email
- Phone
- Description
- Location

### Industry Types

```text
Software
Hardware
BPO
Marketing
HR
Electronics
Electrical
Mechanical
Finance
Others
```

If:

```text
IndustryType = Others
```

then:

```text
OtherIndustry
```

stores the custom industry value.

---

## 6. Placement Drive Management

A company can have multiple placement drives.

```text
Company 1 : N PlacementDrive
```

Placement Drive contains:

- Company
- Job Title
- Job Description
- Employment Type
- Work Mode
- Location
- Minimum CGPA
- Maximum Backlogs
- Graduation Year
- Salary
- Required Skills
- Application Deadline
- Drive Date
- Status

### Employment Type

```text
FullTime
PartTime
Internship
Contract
```

### Work Mode

```text
OnSite
Hybrid
Remote
```

### Placement Drive Status

```text
Draft
Open
Closed
Cancelled
```

---

## 7. Eligible Departments

A placement drive can be eligible for multiple departments.

```text
PlacementDrive N : N Department
```

Using:

```text
PlacementDriveDepartment
```

Example:

```text
.NET Developer Drive
        ↓
CSE
IT
AI & DS
```

---

# 8. Application Management

Students can apply to placement drives.

### Application Workflow

```text
Student
   ↓
Placement Drive
   ↓
Eligibility Check
   ↓
Application
   ↓
Shortlisted / Rejected
   ↓
Interview
   ↓
Selected / Rejected
```

### Application Status

```text
Applied
Shortlisted
Rejected
Selected
Withdrawn
```

### Application Validation

Before creating an application:

- Student profile must exist
- Placement Drive must exist
- Placement Drive must be open
- Application deadline must not be passed
- Student must not have already applied
- Department eligibility must match
- Minimum CGPA must be satisfied
- Maximum backlog requirement must be satisfied
- Graduation year must match

---

# 🔢 Application Number

Each successful application receives a unique application number.

## Format

```text
A + Last 2 Digits of Year + Sequence
```

Examples:

```text
2026 → A260001
2026 → A260002
2026 → A260003

2027 → A270001
2027 → A270002

2028 → A280001
2028 → A280002
```

Each year has its own sequence.

Example:

```text
Year    LastSequence
2026    10000
2027    2
2028    15
```

2026 application numbers remain unchanged when 2027 begins.

---

# 📧 Application Email Notification

After successful application submission, an HTML email is sent to the student.

Email includes:

- Student name
- Application Number
- Company Name
- Job Title
- Applied Date
- Application Status

Example:

```text
Application No: A260001
Company: Zoho Corporation
Position: .NET Developer
Status: Applied
```

Email body is HTML-enabled using:

```csharp
mailMessage.IsBodyHtml = true;
```

---

# 🔎 Search, Filtering, Sorting & Pagination

The project uses reusable query parameter models.

Supported functionality:

```text
Search
Filter
Sort
Pagination
```

Example:

```http
GET /api/applications?pageNumber=1&pageSize=10
```

Search:

```http
GET /api/applications?searchTerm=Zoho
```

Filter:

```http
GET /api/applications?status=Shortlisted
```

Sort:

```http
GET /api/applications?sortBy=AppliedDate&sortDescending=true
```

---

# 📥 Application Excel Export

Applications can be exported to Excel.

```http
GET /api/applications/download
```

The download API reuses:

```text
Search
Filter
Sort
```

but does **not** apply pagination.

Flow:

```text
GET /applications/download
        ↓
Search
        ↓
Filter
        ↓
Sort
        ↓
Query Database
        ↓
ClosedXML
        ↓
MemoryStream
        ↓
Excel File Response
```

The generated Excel file is streamed directly to the client and is not permanently stored on the backend.

---

# 🗑️ Soft Delete

The project uses soft deletion instead of physical deletion.

Common audit fields:

```text
IsDeleted
DeletedBy
DeletedDate
CreatedDate
CreatedBy
UpdatedDate
UpdatedBy
```

Example:

```csharp
entity.IsDeleted = true;
entity.DeletedBy = deletedBy;
entity.DeletedDate = DateTime.UtcNow;

await _unitOfWork.SaveChangesAsync();
```

---

# 🔐 JWT Authentication

JWT includes claims such as:

```text
NameIdentifier
Email
Role
GivenName
Surname
Jti
```

The logged-in user's `UserId` is taken from:

```csharp
ClaimTypes.NameIdentifier
```

For student-specific operations:

```text
JWT UserId
    ↓
Student.UserId
    ↓
Student.Id
```

This prevents students from accessing other students' application data.

---

# 🔒 Role-Based Authorization

Example:

```csharp
[Authorize(Roles = "Admin,PlacementOfficer")]
```

Typical authorization responsibilities:

```text
Student
- View Placement Drives
- Apply for Placement Drives
- View Own Applications

Admin
- Manage Users
- Manage Departments
- Manage Companies

Placement Officer
- Manage Placement Drives
- Review Applications
- Shortlist Students
- Schedule Interviews

Recruiter
- Access recruitment-related information
```

---

# 📂 File Upload

Student module supports:

```text
Resume
Profile Photo
```

Example folders:

```text
StudentDocuments/
├── StudentPhotos/
└── StudentResumes/
```

The file service handles:

- File validation
- File size validation
- Extension validation
- Unique filename generation
- Folder creation
- File deletion
- Error logging

---

# ✅ Validation

FluentValidation is used for request validation.

Examples:

```text
Required fields
Maximum field length
Enum validation
Numeric validation
URL validation
```

Business/database checks remain in service layer.

Examples:

```text
Company exists?
Department exists?
Student eligible?
Placement Drive open?
Application already exists?
Application deadline passed?
```

---

# 🧩 Repository & Unit of Work

The project uses:

```text
Repository Pattern
Unit of Work Pattern
```

Tracked entity example:

```csharp
var company = await _companyRepo.Table
    .FirstOrDefaultAsync(c => c.Id == id);

company.Name = "Updated Company";

await _unitOfWork.SaveChangesAsync();
```

Because `Table` uses EF Core tracking, `UpdateAsync()` is not required for the already tracked entity.

---

# 🧯 Global Exception Handling

Global exception middleware handles exceptions centrally.

Common exceptions include:

```text
KeyNotFoundException
InvalidOperationException
UnauthorizedAccessException
ArgumentException
```

The middleware converts exceptions into appropriate API responses.

---

# 📝 Logging

Serilog is used for structured logging.

Example:

```csharp
_logger.LogInformation(
    "Application {ApplicationId} deleted successfully by {DeletedBy}.",
    application.Id,
    deletedBy);
```

---

# 🚦 Rate Limiting

.NET built-in rate limiting is used for sensitive APIs such as authentication.

Example:

```text
5 requests
per 1 minute
per IP address
```

---

# 🔎 Lookup / Enum API

A lookup API is used to provide enum values to frontend applications.

```http
GET /api/lookup/enums
```

Example response:

```json
{
  "educationType": [
    {
      "id": 1,
      "name": "SSLC"
    },
    {
      "id": 2,
      "name": "HSC"
    },
    {
      "id": 4,
      "name": "Undergraduate"
    }
  ]
}
```

---

# 🌐 API Endpoints

## Authentication

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/forgot-password
POST /api/auth/reset-password
```

## Departments

```http
GET    /api/departments
GET    /api/departments/{id}
POST   /api/departments
PATCH  /api/departments/{id}
DELETE /api/departments/{id}
```

## Students

```http
GET    /api/students
GET    /api/students/{id}
POST   /api/students
PATCH  /api/students/{id}
DELETE /api/students/{id}
```

## Companies

```http
GET    /api/companies
GET    /api/companies/{id}
POST   /api/companies
PATCH  /api/companies/{id}
DELETE /api/companies/{id}
```

## Placement Drives

```http
GET    /api/placementdrives
GET    /api/placementdrives/{id}
POST   /api/placementdrives
PATCH  /api/placementdrives/{id}
DELETE /api/placementdrives/{id}
```

## Applications

```http
GET    /api/applications
GET    /api/applications/{id}
GET    /api/applications/my
POST   /api/applications
DELETE /api/applications/{id}
GET    /api/applications/download
```

---

# 🗄️ Database Relationships

```text
User
 │
 └── 1 : 1 ── Student
                │
                ├── N : 1 ── Department
                │
                └── 1 : N ── EducationDetails
                │
                └── 1 : N ── Application
                                   │
                                   ├── N : 1 ── PlacementDrive
                                   │                    │
                                   │                    ├── N : 1 ── Company
                                   │                    │
                                   │                    └── N : N ── Department
                                   │                              via PlacementDriveDepartment
                                   │
                                   └── 1 : N ── Interview
```

---

# 🧪 Testing

Run all tests:

```bash
dotnet test
```

Testing stack:

```text
xUnit
Moq
MockQueryable.Moq
Microsoft.NET.Test.Sdk
```

Areas covered include:

```text
Authentication
Service business logic
Validation
Search
Filtering
Sorting
Pagination
```

---

# 🗃️ Database Migration

Using Package Manager Console:

```powershell
Add-Migration MigrationName
Update-Database
```

Using .NET CLI:

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

---

# ⚙️ Local Setup

## Prerequisites

Install:

- .NET 10 SDK
- SQL Server
- SQL Server Management Studio
- Visual Studio 2026
- Git
- Postman
- Docker Desktop

---

## Clone Repository

```bash
git clone <YOUR_GITHUB_REPOSITORY_URL>
cd PlacementManagementSystem
```

---

## Configure Database

Example connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PlacementManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Do not commit production credentials or secrets to GitHub.

---

## Restore Packages

```bash
dotnet restore
```

---

## Build Project

```bash
dotnet build
```

---

## Apply Migrations

```bash
dotnet ef database update
```

---

## Run Application

```bash
dotnet run --project PMS.API
```

Swagger:

```text
/swagger
```

---

# 🐳 Redis Development Setup

Redis is planned for caching.

For local development, Redis can be run using Docker.

```bash
docker run -d \
  --name pms-redis \
  -p 6379:6379 \
  redis:latest
```

Check container:

```bash
docker ps
```

Open Redis CLI:

```bash
docker exec -it pms-redis redis-cli
```

Test:

```text
PING
```

Expected response:

```text
PONG
```

---

# 🌳 Git Workflow

Recommended workflow:

```text
feature/*
    ↓
Pull Request
    ↓
develop
    ↓
Pull Request
    ↓
main
```

Bug fixes:

```text
bugfix/*
    ↓
Pull Request
    ↓
develop
```

Multiple features and bug fixes can be merged into `develop`.

When `develop` becomes release-ready:

```text
develop
    ↓
GitHub Pull Request
    ↓
main
```

---

# 📊 Project Status

## Completed

- Authentication
- User Management
- Department CRUD
- Student CRUD
- Education Details
- Company CRUD
- Placement Drive CRUD
- Application CRUD
- Application Eligibility Validation
- Application Number Generation
- Application Email Notification
- Excel Export
- Search
- Filtering
- Sorting
- Pagination
- Soft Delete
- JWT Authentication
- Role-Based Authorization
- Global Exception Handling
- Serilog Logging
- FluentValidation
- Repository Pattern
- Unit of Work
- Unit Testing Foundation
- Lookup / Enum API

## Future Enhancements

- Interview Module
- Redis Caching
- Production deployment
- Advanced integration testing
- CI/CD pipeline enhancements

---

# 📜 License

This project is developed as an academic and portfolio project for learning and demonstrating:

- Backend API Development
- Database Design
- Authentication & Authorization
- Business Logic
- Repository & Unit of Work
- Testing
- File Upload
- Email Integration
- Excel Export
- Caching
- Git Workflow
- Clean Architecture Practices
