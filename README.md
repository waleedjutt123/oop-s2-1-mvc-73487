# Community Library Desk

Internal system for a small community library: track Books, Members, and Loans. Staff can lend books, assign members, and check overdue items.



# Tech stack

Technology  Version  detail 

.NET 9.0
- ASP.NET Core MVC, 9.0 
- Database  SQLite (no server needed) 
- ORM | Entity Framework Core 9 
- Auth  ASP.NET Core Identity (with Roles) 
- Tests xUnit 
- CI GitHub Actions 



# Prerequisites

.NET 9 SDK – [Download](https://dotnet.microsoft.com/download/dotnet/9.0)



# Database setup (SQLite)

Out of the box the app uses **SQLite** (`app.db` in the project folder). No server needed.

1. Apply migrations:

bash
   cd src/CommunityLibrary
   dotnet ef database update


2. Run the app – the database is ready.



## Run the application

bash
cd src/CommunityLibrary
dotnet run


Then open the URL shown (e.g. `https://localhost:5001` or `http://localhost:5000`).



## Project structure



├── README.md

└── src/
    └── CommunityLibrary/          # Main MVC app
        ├── Controllers/
        ├── Data/
        │   ├── ApplicationDbContext.cs
        │   ├── DesignTimeDbContextFactory.cs   # For EF tools (migrations)
        │   └── Migrations/
        ├── Models/               # Book, Member, Loan
        ├── Views/
        └── ...


## Entities

Book – Id, Title, Author, Isbn, Category, IsAvailable  
Member – Id, FullName, Email, Phone  
Loan – Id, BookId, MemberId, LoanDate, DueDate, ReturnedDate (nullable)  

Relationships: Book 1 Loan, Member 1 Loan.



## .NET version in use

This project targets .NET 9.0 (`net9.0` in `CommunityLibrary.csproj`).  
Ensure you have the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed:

bash
dotnet --version
# Should show 9.x.x




## EF Core commands

With default SQLite, these work without any database server:

bash
cd src/CommunityLibrary
dotnet ef migrations add SyncModelWithSnapshot
dotnet ef migrations remove
dotnet ef database update
dotnet clean
dotnet restore





Admin: admin@community.local / Admin123!
Staff: staff@community.local / Staff123!