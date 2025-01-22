CatFactsExplorer

CatFactsExplorer is an example .NET solution designed to explore a variety of concepts and technologies, including API development, Background Workers, Database setup, and Dockerization. It supports two primary ways to run the entire system:
	1.	Using a Startup Console App that automates installing Node.js (on Windows), spinning up SQL Server (either locally or via Docker), creating the database with scripts, and then starting the API, Worker, and Frontend.
	2.	Using Docker Compose to build and run the entire stack (API, Worker, database, and possibly frontend) in containers.

Below is an overview of the project’s architecture, technologies used, and instructions to get it running.

Architecture Overview

The Solution has several projects under the src/ folder:
	•	CatFactsExplorer.API
	•	A REST API built with ASP.NET Core.
	•	Contains controllers (e.g., CatFactsController.cs) to expose endpoints.
	•	appsettings.json handles configuration such as connection strings.
	•	CatFactsExplorer.Worker
	•	A Worker Service (using .NET’s background service/host) that handles background tasks.
	•	Uses technologies like Hangfire (or equivalent) to schedule or process tasks in the background.
	•	CatFactsExplorer.Application, CatFactsExplorer.Domain, CatFactsExplorer.Infrastructure, CatFactsExplorer.Jobs
	•	These libraries implement a layered / clean architecture:
	•	Domain: Core business entities and rules.
	•	Application: Application logic, use cases, orchestrating domain objects.
	•	Infrastructure: Persistence, external services, EF Core, Hangfire configurations, etc.
	•	Jobs: Common job logic for scheduling or repeated tasks.
	•	StartupUtility
	•	A console utility that sets up the environment and then launches all the services (API, Worker, and frontend).
	•	This includes ensuring Node.js is installed (on Windows), spinning up SQL Server container (or local SQL Express), running the sql-scripts/init.sql, and updating appsettings.json connection strings.

Additionally, there is a sql-scripts/ folder where the init.sql script resides, responsible for creating or seeding the CatFacts database.

Technologies & Concepts
	1.	.NET 7 / C# – The core language/runtime for the API, Worker, and domain logic.
	2.	ASP.NET Core – Used for building the Web API (in CatFactsExplorer.API).
	3.	Worker Service – A background service in CatFactsExplorer.Worker that runs continuously to handle scheduled or asynchronous tasks.
	4.	Hangfire – Manages and schedules background jobs.
	5.	Entity Framework Core – (Likely in CatFactsExplorer.Infrastructure) for data access and migrations.
	6.	Docker – Containerization for running SQL Server, API, Worker, and optionally the frontend.
	7.	Node.js + npm – Required for the frontend build or dev server.
	8.	Clean Architecture / Layered Approach – Separation of concerns between Domain, Application, and Infrastructure layers.


 How to Run

Approach 1: Using the Startup Console App
	1.	Open the solution in Visual Studio (or any C# IDE).
	2.	Set the StartupUtility project as the startup project (right-click -> “Set as Startup Project”).
	3.	Run the app. It will:
	•	Check if Node.js is installed (on Windows). If not, it will attempt a silent install.
	•	Pull and start a SQL Server container (or use local SQL Express, depending on your setup).
	•	Run the init.sql script from sql-scripts/ to create the initial database/tables.
	•	Update the connection strings in your appsettings.json files for the API and Worker.
	•	Start the API, Worker, and npm dev server (frontend).


After these steps, the API and Worker should be running, and you can open the Frontend in your browser (usually http://localhost:3000 if it’s a React/Vue dev server).

Approach 2: Using Docker Compose
	1.	Install Docker (and Docker Compose) if not already available.
	2.	Navigate to the folder containing docker-compose.yml.
	3.	Run: docker-compose up --build

This will:
	•	Build Docker images for the API and Worker (and possibly the frontend).
	•	Spin up a SQL Server container with the database initialization if configured in the compose file.
	•	Expose the necessary ports on localhost for the API, Worker, and frontend.

Check your logs in the Docker console or CLI to see if everything started successfully. Then navigate to the exposed ports in your browser (e.g., http://localhost:5000 or similar).

Additional Notes
	•	Connection Strings: If you run the Docker-based approach, your environment might set ConnectionStrings__MSSQL to point to the container network name (e.g., mssql). If you run locally, it might be localhost. The StartupUtility tries to handle that automatically, but you can always tweak appsettings.json manually.
	•	Hangfire Dashboard: If you have Hangfire’s dashboard enabled, you can view it at [API_URL]/hangfire or [Worker_URL]/hangfire, depending on how you configure it.
	•	Frontend: If you prefer a separate, manual approach, you can cd into CatFactsExplorer.Frontend and run npm install && npm run dev (or npm run build) rather than using the console app or Docker Compose.
