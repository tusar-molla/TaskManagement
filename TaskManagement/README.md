# Task Management Web Application

## Overview
This is a Task Management Web Application developed as part of the .NET Core Developer Coding Test for CSM Bangladesh LTD. The application allows users to register, log in, and manage personal task lists with features including task creation, editing, deletion, completion status updates, filtering by completion status, and sorting by due date. Built using ASP.NET Core 8 MVC, it leverages Entity Framework Core, SQL Server LocalDB, and ASP.NET Core Identity for secure user authentication and data persistence.

## Features
- **User Authentication**: Register and log in and log out using ASP.NET Core Identity.
- **Task Management**:
  - Create, view, edit, and delete tasks.
  - Mark tasks as completed or not completed.
  - Tasks are user-specific, ensuring users only access their own tasks.
- **Filtering and Sorting**:
  - Filter tasks by completion status (All, Completed, Not Completed).
  - Sort tasks by due date (ascending or descending).
- **Validation**:
  - Client-side validation using jQuery Validation.
  - Server-side validation with data annotations (e.g., required title).
- **Responsive UI**: Styled with Bootstrap 5.3.2 for a clean, user-friendly interface.

## Technical Stack
- **Framework**: ASP.NET Core 8 MVC
- **ORM**: Entity Framework Core 8
- **Database**: SQL Server LocalDB
- **Authentication**: ASP.NET Core Identity
- **Frontend**:
  - Bootstrap 5.3.2 (via CDN)
  - jQuery 3.6.0 and jQuery Validation 1.19.5 (via CDN)
- **Development Environment**: Visual Studio 2022, .NET 8 SDK

## Prerequisites
- **.NET 8 SDK**
- **Visual Studio 2022**
- **SQL Server LocalDB**: Included with Visual Studio or install via SQL Server Express.

## Setup Instructions
1. **Clone the Project**:   

2. ## Setup the Database      
   ** Add-Migration InitialCreate
   ** Update-Database
3. ## Run the Application.

  ** Register a new user, log in, and start managing tasks