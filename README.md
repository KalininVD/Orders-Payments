# Orders & Payments API

## Overview

This project is an implementation of a backend API system with two main services:

- Payments Service
  - Responsible for managing accounts and their funds
- Orders Service
  - Responsible for managing orders and their statuses

Both services assume that there is an external service for managing users and `user_id` property is passed to both services as a known parameter.

For a controlled access to the API, there is a reverse proxy project `API Gateway` that is responsible for routing requests to the appropriate services. In the future it will be upgraded to support load balancing and managing multiple instances of each service.

## Supported API endpoints

### Payments Service

- `POST /api/accounts` - Create a new account with zero balance
- `GET /api/accounts/{id}` - Get account by ID
- `GET /api/accounts/user/{userId}` - Get account by user ID
- `POST /api/accounts/deposit` - Deposit funds to an account

### Orders Service

- `POST /api/orders` - Create a new order and start its payment process
- `GET /api/orders/{id}` - Get order by its ID (including its status)
- `GET /api/orders/user/{userId}` - See all orders for a specific user
- `PATCH /api/orders/{id}/cancel` - Cancel an order by its ID (if it is not already cancelled or finished)

## Asynchronous communication

Since the order is created in `NEW` status, it needs to be confirmed by the payment service to become `FINISHED`. So after the order is created, `Orders Service` sends a request to `Payments Service` to try to withdraw money from the account linked to `user_id`. In case of success, the order status is changed to `FINISHED`, otherwise it is changed to `CANCELLED` (if the account has less funds than the order amount or the specified user does not have an account).

## Requirements to run locally

0. Install Docker (Docker Desktop is recommended)
1. Clone the repository
2. Copy `.env.example` and rename it to `.env`, change the values to your needs
3. Run `docker compose up --build -d` in the root directory, wait for all containers to start
4. To see Swagger UI, open `http://localhost:8080/swagger` in your browser (make sure to use appropriate port)
5. Or use HTTP file inside `APIGateway` directory to test the API by manually sending requests (in VS Code you should install the `REST Client` extension)

## Microservices architecture

Both microservices (`Payments Service` and `Orders Service`) are implemented in C# using .NET 9. Main used patterns include Clean Architecture, CQRS and Domain-Driven Design (Rich Domain Model). Both services consist of four layers as projects within a single .NET solution:

- Domain layer
  - Contains the domain entities and their logic
  - Rich Domain Model - domain objects check the business rules by themselves
- Application layer
  - Contains the application logic, including all realizations for known use cases
  - CQRS pattern for each use case - commands and queries are separated and handled by their own handler
  - For commands there are Validators for checking data correctness before trying to execute the command with real data access
- Infrastructure layer
  - Contains the implementation of Entity Framework Core DB context to save entities to the database (PostgreSQL is used in this project)
  - Configuration for RabbitMQ, used for message communication between services
- Web API
  - Controllers for handling HTTP requests on the API endpoints
  - Matching HTTP requests to the corresponding use cases
  - Middleware for handling exceptions and logging

For the ability to send and receive messages from other services, there is an additional project `Shared` containing `Contracts` folder with specific record classes indicating the status of the operation performed by the service. In the case of `Payments Service`, it returns the status of the payment operation: if money was successfully withdrawn from the account, `Orders Service` will change the order status to `FINISHED`, otherwise (if the account does not exist or does not have enough funds) the order will be cancelled - that is `Orders Service` responsibility.