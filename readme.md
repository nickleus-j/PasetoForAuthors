# PasetoForAuthors

PasetoForAuthors is a modern ASP.NET 10 MVC application designed to demonstrate secure authentication using **PASETO (Platform-Agnostic Security Tokens)**. Unlike JWTs, PASETOs are designed to be "cryptographically resilient," eliminating common vulnerabilities like the "none" algorithm or key-confusion attacks.

This project serves as a reference implementation for developers looking to integrate PASETO into .NET web applications. An identity service will integrate with other services.

## 🚀 Features

  - **PASETO Integration**: Implements secure token-based authentication (v3/v4) using C\#.
  - **ASP.NET 10 MVC**: Built on the latest .NET framework for high performance.
  - **Dockerized Deployment**: Includes a production-ready Docker configuration.

## 🛠 Prerequisites

  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
  - [Docker Desktop](https://www.docker.com/products/docker-desktop)
  - A tool for testing APIs (like Postman or curl)

## 🐳 Deployment with Docker

The application is containerized for easy deployment. You can spin up the environment using Docker Compose.

### 1\. Configure the Environment

The `docker-compose.yml` file handles the orchestration. Ensure your environment variables for token secrets are configured correctly in your `appsettings.json` or as environment overrides.

### 2\. Run the Application

From the root directory, execute:

```bash
docker-compose up -d --build
```

### 3\. Verify the Deployment

Once the containers are running, the MVC application will be accessible at:

  - **HTTP**: `https://localhost:5001`

> **Note**: If the MVC app does not start as expected, ensure the `ENTRYPOINT` in your `Dockerfile` matches the assembly name (e.g., `ENTRYPOINT ["dotnet", "PasetoForAuthors.dll"]`) and that the ports in `docker-compose.yml` are not already in use.

## 📁 Project Structure

  - `/src`: Contains the primary MVC project and PASETO logic.
  - `/docs`: Additional documentation on PASETO implementation details.
  - `docker-compose.yml`: Orchestration file for the application environment.

## 🔒 Security Configuration

To modify the PASETO token settings (such as expiration or version), navigate to the `Security` configuration section in `appsettings.json`:

```json
"PasetoSettings": {
  "Issuer": "PasetoForAuthors",
  "Audience": "AuthorsAPI",
  "ExpirationInMinutes": 60
}
```

## 📄 License

This project is licensed under the [MIT License](https://www.google.com/search?q=LICENSE).
