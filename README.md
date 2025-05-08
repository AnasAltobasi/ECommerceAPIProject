# E-Commerce API with ASP.NET Core

A RESTful API for managing users, products, and invoices with JWT authentication and role-based authorization.

## Features

- **User Authentication**
  - JWT-based registration/login
  - Role-based access control (Admin/Visitor)
- **Product Management**
  - CRUD operations with soft delete
  - Paginated product listing
- **Invoice System**
  - Create invoices with product details
  - Calculate total amounts automatically
- **Database Seeding**
  - Preloaded admin user and sample products
- **Unit Tests**
  - Critical functionality tests (xUnit + Moq)

## Technologies

- ASP.NET Core 7.0
- Entity Framework Core
- JWT Authentication
- xUnit + Moq (Testing)
- SQL Server (In-memory for testing)

## Getting Started

### Prerequisites

- .NET 7 SDK
- SQL Server
- Postman (or similar API client)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/ecommerce-api.git
cd ECommerceAPIProject
