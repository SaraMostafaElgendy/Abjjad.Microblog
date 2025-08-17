1- It’s a .NET project named Abjjad.Microblog.

2- It has EF Core with AppDbContext.

3- It uses PostService, AuthService, background ImageProcessingQueue.

4- Tests are written using xUnit and EF Core InMemory provider.

5- You want clear setup instructions including how to set up the database.


**Getting Started**
1. Clone the Repository
git clone https://github.com/your-username/abjjad.microblog.git
cd abjjad.microblog
**Install Dependencies**
Make sure you have the .NET SDK installed (7.0 or later).
dotnet restore
**Configure the Database**
This project uses Entity Framework Core.
Two database providers are supported:
1- InMemory (for testing)
2- SQL Server (sqllite) (for development)
   to setup the db (as iam using sqllite) so it's just run the
       - Run the following command to create the database and schema:
            dotnet ef database update --project Abjjad.Microblog
**Run the Application**
dotnet run --project Abjjad.Microblog
then open http://localhost:5000/
**Testing Data**
username: bob , Password: password123
username: alice , Password: password123
**Run Tests**
This project uses xUnit and EF Core InMemory for unit testing.
dotnet test


**Project Structure**
Abjjad.Microblog/        → Main Web API project
Abjjad.Microblog.Data/   → EF Core DbContext and migrations
Abjjad.Microblog.Models/ → Entity models (User, Post, etc.)
Abjjad.Microblog.Services/ → Core services (PostService, AuthService, etc.)
Abjjad.Microblog.Tests/  → Unit tests (xUnit + InMemory DB)

**There's a quick Video Attached in the repo if you want to wach the application while running**

