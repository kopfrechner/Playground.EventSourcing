# Event Sourcing with Marten

This repository demonstrates event sourcing patterns and practices using [Marten](https://martendb.io/), a .NET document database and event store library for PostgreSQL.

## Overview

Event sourcing is an architectural pattern where application state changes are captured as a sequence of events. Instead of storing the current state, we store the history of events that led to that state. This approach provides:

- Complete audit trail and history
- Ability to reconstruct the state at any point in time
- Separation of write and read models (CQRS)
- Enhanced debugging and system analysis

## Project Structure

- **Playground.EventSourcing**: Core library containing aggregates, events, and projections
- **Playground.EventSourcing.Tests**: Test suite demonstrating various event sourcing scenarios

## Key Concepts Demonstrated

### Aggregates
The repository showcases how to implement aggregates that maintain their state through events:
- `Product` - A domain aggregate that manages product information and lifecycle

### Event Types
Various domain events are modeled, including:
- Product creation events
- Product revision events (upload, approval, decline)
- Product locking/unlocking events

### Projections
The code demonstrates different projection types:
- Single-stream projections (focusing on a single aggregate)
- Multi-stream projections (combining data from multiple aggregates)

## Test Scenarios

The test suite showcases several key event sourcing scenarios:

1. **Aggregate Creation and Event Application**
   - Building aggregates from event streams
   - Applying new events to existing aggregates

2. **Event History and Replay**
   - Querying event history
   - Rebuilding state from historical events

3. **Projections**
   - Creating read models from events
   - Handling different event types in projections

4. **Business Rules**
   - Product locking/unlocking
   - Revision approval workflows

## Getting Started

1. Ensure you have PostgreSQL installed or use the provided Docker container setup
2. Run the tests to see event sourcing in action

```bash
dotnet test
```

## Technologies

- **.NET Core**: Modern C# features including records for immutable data
- **Marten**: Document database and event store for PostgreSQL
- **xUnit**: Testing framework
- **Testcontainers**: For integration testing with real PostgreSQL instances
