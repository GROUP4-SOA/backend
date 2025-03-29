# Bookstore Inventory Management System

## Overview
The **Bookstore Inventory Management System** is a robust and scalable solution designed to streamline book inventory management, user authentication, and category organization within a bookstore. Built on **.NET 8 Minimal API** with **MongoDB**, this system follows a **Layered Architecture**, ensuring modularity, maintainability, and scalability.

## System Architecture
The project is structured into four primary layers:

1. **Bookstore.API (Presentation Layer)**
   - Manages HTTP requests and responses.
   - Implements controllers for books, categories, and authentication.
   - Integrates middleware for exception handling and authentication.
   
2. **Bookstore.Application (Business Logic Layer)**
   - Encapsulates core business logic via services and interfaces.
   - Utilizes Data Transfer Objects (DTOs) for structured data management.
   
3. **Bookstore.Infrastructure (Data Access Layer)**
   - Implements repository patterns for MongoDB interactions.
   - Defines database context and seed data mechanisms.
   
4. **Bookstore.Domain (Domain Layer)**
   - Defines core domain entities, including `Book`, `Category`, and `User`.

## Key Features
- **Book Management**: Supports CRUD operations for books.
- **Category Management**: Enables book categorization.
- **User Authentication**: Implements role-based authentication (Admin, Storekeeper) with secure password hashing.
- **RESTful API**: Provides endpoints for seamless frontend integration.
- **Middleware Support**: Handles authentication and exception management.

## Technology Stack
- **Backend**: C# .NET 8 Minimal API
- **Database**: MongoDB
- **Authentication**: JWT-based authentication
- **Dependency Injection**: Built-in .NET DI
- **Version Control**: Git & GitHub

## Installation & Setup
### Prerequisites
- **.NET 8 SDK**
- **MongoDB** (or MongoDB Atlas for cloud-based database hosting)

### updating...
