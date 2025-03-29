Bookstore Inventory Management System

Overview

The Bookstore Inventory Management System is a robust and scalable solution designed to streamline book inventory management, user authentication, and category organization within a bookstore. Built on .NET 8 Minimal API with MongoDB, this system follows a Layered Architecture, ensuring modularity, maintainability, and scalability.

System Architecture

The project is structured into four primary layers:

Bookstore.API (Presentation Layer)

Manages HTTP requests and responses.

Implements controllers for books, categories, and authentication.

Integrates middleware for exception handling and authentication.

Bookstore.Application (Business Logic Layer)

Encapsulates core business logic via services and interfaces.

Utilizes Data Transfer Objects (DTOs) for structured data management.

Bookstore.Infrastructure (Data Access Layer)

Implements repository patterns for MongoDB interactions.

Defines database context and seed data mechanisms.

Bookstore.Domain (Domain Layer)

Defines core domain entities, including Book, Category, and User.

Key Features

Book Management: Supports CRUD operations for books.

Category Management: Enables book categorization.

User Authentication: Implements role-based authentication (Admin, Storekeeper) with secure password hashing.

RESTful API: Provides endpoints for seamless frontend integration.

Middleware Support: Handles authentication and exception management.

Technology Stack

Backend: C# .NET 8 Minimal API

Database: MongoDB

Authentication: JWT-based authentication

Dependency Injection: Built-in .NET DI

Version Control: Git & GitHub

Installation & Setup

Prerequisites

.NET 8 SDK

MongoDB (or MongoDB Atlas for cloud-based database hosting)

Steps to Run

Clone the repository:

git clone https://github.com/your-repo/bookstore-inventory.git

Navigate to the backend directory:

cd bookstore-inventory/backend

Configure appsettings.json with your MongoDB connection string.

Run the API:

dotnet run --project Bookstore.API

API Documentation

Book Endpoints

Method

Endpoint

Description

GET

/api/books

Retrieve all books

GET

/api/books/{id}

Retrieve a specific book by ID

POST

/api/books

Add a new book

PUT

/api/books/{id}

Update book details

DELETE

/api/books/{id}

Remove a book

Authentication Endpoints

Method

Endpoint

Description

POST

/api/auth/login

Authenticate user credentials

POST

/api/auth/register

Register a new user
