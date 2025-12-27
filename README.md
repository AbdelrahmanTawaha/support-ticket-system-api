# Support Ticket System – API (ASP.NET Core)

ASP.NET Core Web API for a **Support Ticket System** (T2 training project).  
Built with a layered architecture and includes authentication, ticket workflows, attachments, real-time updates, and AI-powered reporting (Gemini).

---

## Key Features
- **Layered Architecture**: DataAccessLayer, BusinessLayer, API
- **Authentication & Authorization**
  - JWT access tokens
  - Role-based authorization (Manager / Employee / Client)
- **Tickets Module**
  - Create / update / status workflow
  - Comments & ticket details
- **Attachments**
  - Upload/download ticket attachments
  - File storage under `wwwroot` (local)
- **Real-time Updates**
  - SignalR hub for live ticket updates/notifications
- **AI Reports (Gemini)**
  - Generate safe SQL-based report queries using allowed views only
  - Query validation & whitelisting for security

---

## Tech Stack
- **ASP.NET Core Web API**
- **EF Core** (Code First + Migrations)
- **SQL Server**
- **JWT Bearer Authentication**
- **SignalR**
- **Swagger / OpenAPI**

---

## Project Structure
