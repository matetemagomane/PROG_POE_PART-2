# RaceDay RESTful API — PROG6212 POE Part 2

![Build Status](https://github.com/matetemagomane/PROG_POE_PART-2/actions/workflows/main.yml/badge.svg)

## Project Overview
**RaceDay** is an event management platform built for sports event organizers and participants (running, cycling, walking). This repository contains **Part 2: RESTful API**, built using **ASP.NET Core Web API**, **Entity Framework Core**, and **SQL Server**. 

The API acts as the core backend service responsible for managing user authentication, role restrictions, event organization, race category setup, event enrolments, and participant finish results.

---

## User Roles & Access Control
The API enforces strict Role-Based Access Control (RBAC) via server-side session management:

| Endpoint Resource | Organiser Role | Participant Role | Unauthenticated |
| :--- | :--- | :--- | :--- |
| **Authentication** (`/api/auth`) | Register / Login / Logout | Register / Login / Logout | Register / Login |
| **Events** (`/api/events`) | Create, Update, Delete | View All / View Details | View All / View Details |
| **Categories** (`/api/categories`) | Create, Delete Categories | View Categories | View Categories |
| **Enrolments** (`/api/enrolments`) | View Event Roster | Enrol in Race / My Enrolments | Denied (401/403) |
| **Results** (`/api/results`) | Record Times & Positions | View My Results / Standings | View Public Standings |

* **Password Security:** All passwords are standard BCrypt-hashed before persistence in SQL Server.
* **Session Management:** Authenticated user IDs and roles are stored in `HttpContext.Session`.

---

## Technical Architecture
- **Framework:** ASP.NET Core Web API (.NET 8.0)
- **Database Engine:** Microsoft SQL Server via Entity Framework Core (Code-First)
- **Documentation:** Interactive OpenAPI / Swagger UI
- **Security:** BCrypt.Net-Next for password hashing, Distributed Memory Cache Session State
- **Unit Testing:** xUnit Framework with EF Core In-Memory Database provider
- **CI/CD:** Automated GitHub Actions build and test pipeline

---

## Setup & Local Execution Guide

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Microsoft SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

### Installation Steps

1. **Clone the Repository:**
   ```bash
   git clone [https://github.com/matetemagomane/PROG_POE_PART-2.git](https://github.com/matetemagomane/PROG_POE_PART-2.git)
   cd PROG_POE_PART-2

 2. **Youtube Link**
   [https://youtu.be/_0MrGwneH58]




   
