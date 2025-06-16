# Orders & Payments API

## Overview

This project is an implementation of a distributed backend system featuring two core microservices: **Payments Service** and **Orders Service**. The system is designed to handle the creation and payment processing of customer orders in a reliable and scalable manner.

-   **Payments Service:** Responsible for managing user accounts, balances, and processing deposits.
-   **Orders Service:** Responsible for managing the lifecycle of orders, from creation to completion or cancellation.

All communication from the outside world is routed through a dedicated **API Gateway**, which acts as a single entry point to the system. The project assumes the existence of an external user management service, and `user_id` is passed as a known parameter.

## Supported API Endpoints

The API is accessible through the API Gateway.

### Payments Service (`/api/accounts`)

-   `POST /api/accounts`: Create a new account for a user with a zero balance.
-   `POST /api/accounts/deposit`: Deposit funds into a user's account.
-   `GET /api/accounts/{id}`: Get account details by its unique ID.
-   `GET /api/accounts/user/{userId}`: Get account details by the user's ID.

### Orders Service (`/api/orders`)

-   `POST /api/orders`: Create a new order, which asynchronously triggers the payment process.
-   `GET /api/orders`: Get a list of all orders for a specific user.
-   `GET /api/orders/{id}`: Get the details and current status of a specific order.
-   `PATCH /api/orders/{id}/cancel`: Cancel an order by its ID (if it has not been completed or already cancelled).

## Asynchronous Communication Flow

The core business process of creating and paying for an order is fully asynchronous to ensure system resilience and responsiveness:

1.  A client sends a `POST` request to the `Orders Service` to create an order. The order is immediately created with a `NEW` status.
2.  The `Orders Service` atomically saves the new order and publishes an `OrderPaymentRequest` message to a **RabbitMQ** message broker.
3.  The `Payments Service` consumes this message, finds the user's account, and attempts to withdraw the required amount.
4.  Based on the outcome, the `Payments Service` publishes a corresponding event: `OrderPaymentSucceeded` or `OrderPaymentFailed`.
5.  The `Orders Service` consumes the result event and updates the order's status to `FINISHED` or `CANCELLED`, thus completing the cycle.

## How to Run

### Prerequisites

-   Docker and Docker Compose (Docker Desktop is recommended).
-   A `.env` file for configuration.

### Steps

1.  Clone the repository.
2.  Create a `.env` file in the root directory by copying `.env.example`. Adjust the values if necessary.
3.  Run the following command from the root directory:
    ```bash
    docker-compose up --build -d
    ```
4.  Wait for all containers to start. The system is now running.
5.  To interact with the API, open the unified Swagger UI hosted on the API Gateway: **`http://localhost:8080/swagger`** (use the port you configured in `.env`).
6.  Alternatively, you can use the `.http` files in each service's `Web` project or the `APIGateway` project to send requests directly from your IDE (e.g., VS Code with the REST Client extension or JetBrains Rider).

## Architecture & Design Patterns

The system is built upon a modern, robust set of architectural principles and design patterns to ensure scalability, maintainability, and reliability.

### High-Level Architecture

-   **Microservices Architecture:** The system is decomposed into independent, deployable services (`OrdersService`, `PaymentsService`) with a single point of entry via an **API Gateway** (implemented with YARP).
-   **Database-per-Service:** Each microservice has its own private PostgreSQL database, ensuring loose coupling and independent data management.

### Core Application Design

-   **Clean Architecture:** Each service follows a strict separation of concerns, with dependencies directed inwards. This isolates business logic from external frameworks and technologies.
    -   **Domain Layer:** Contains rich domain entities (`Order`, `Account`) that encapsulate business rules and protect their own state (invariants). This follows the **Domain-Driven Design (DDD)** principle of a Rich Domain Model.
    -   **Application Layer:** Orchestrates the use cases of the application. It is built upon the **CQRS (Command Query Responsibility Segregation)** pattern, using **MediatR** to separate commands (state-changing operations) from queries (data-retrieval operations).
    -   **Infrastructure Layer:** Contains implementations for external concerns, such as database access (**Repository Pattern** and **Unit of Work Pattern** with Entity Framework Core) and message bus integration.
    -   **Web API Layer:** The entry point for external requests, containing thin controllers that delegate all work to the Application Layer.

### Reliability and Messaging Patterns

To ensure data consistency and reliable communication in a distributed environment, the project heavily relies on advanced messaging patterns implemented with **MassTransit** and **RabbitMQ**.

-   **Saga Pattern (Choreography-based):** The entire order processing flow is implemented as a Saga. There is no central orchestrator; instead, services communicate by publishing and subscribing to events. `OrdersService` initiates the Saga by publishing a payment request, and `PaymentsService` continues it by publishing a payment result, ensuring the entire business transaction either completes successfully or fails gracefully.
-   **Transactional Outbox:** This pattern guarantees that a message is sent to the message broker **if and only if** the corresponding database transaction is successfully committed. When an order is created, the `Order` entity and the `OrderPaymentRequest` message are saved atomically. This prevents scenarios where an order is saved but the payment request is never sent.
-   **Transactional Inbox (Idempotent Consumers):** This pattern ensures that incoming messages are processed **exactly once**. MassTransit uses an `InboxState` table to track received messages. If a message is delivered more than once (e.g., due to a network issue), the consumer will process it only the first time, preventing duplicate operations like charging a customer twice for the same order.
-   **At-Most-Once / Exactly-Once Semantics:** The combination of the Transactional Outbox and Inbox patterns effectively achieves **exactly-once** message processing, fulfilling and exceeding the project requirements.

### Other Implemented Patterns

-   **Options Pattern:** For strongly-typed, safe, and validated application configuration.
-   **Custom Middleware:** For centralized, uniform exception handling across the API.
-   **Dependency Injection:** Extensively used throughout the application to achieve loose coupling and high testability.