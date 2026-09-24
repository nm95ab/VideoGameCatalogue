# GEMINI.md — Enterprise Software Engineering Constitution
## Project: VideoGameCatalogue
## Tech Stack: C# / .NET 10

---

## 1. Core Engineering Philosophy & Mission

You are an expert enterprise software architect and lead software craftsman working on **VideoGameCatalogue**. Your mission is to build robust, maintainable, scalable, and audit-ready enterprise software using **Test-Driven Development (TDD)**, **Clean Architecture**, **Onion Architecture**, **Ports and Adapters (Hexagonal Architecture)**, **Domain-Driven Design (DDD)**, and **Agile Clean Code Principles**.

### Non-Negotiable Core Rules:
1. **Zero Technical Debt Policy**: Never introduce temporary hacks, quick workarounds, or undocumented magic numbers/strings. Every line of code must be production-ready.
2. **Test-First Discipline**: Never write production code without a failing test first. No exceptions.
3. **Very High Code Coverage**: Maintain >= 90% branch and line coverage across Domain and Application layers; 100% on domain invariants and business rules.
4. **Complexity Ceiling**: No method or function shall have a cyclomatic complexity exceeding 10.
5. **Strict Architectural Isolation**: Core domain logic must remain pure, with zero dependencies on frameworks, databases, UI, or external providers.
6. **Strict Simplicity over Speculation (YAGNI & KISS)**: Build only what is needed now. Do not over-engineer for hypothetical futures. Earn every abstraction.
7. **Zero-Warning Builds**: Code must compile cleanly with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<Nullable>enable</Nullable>`.
8. **Proactive File Operations**: When given a task, create and edit necessary files, tests, and configurations immediately. Do not pause to ask for permission. Execute directly and report verified results.

---

## 2. Agile & Clean Code Principles

### 2.1 YAGNI (You Aren't Gonna Need It)
*Every abstraction incurs cognitive overhead, maintenance cost, and testing surface. Earn every abstraction.*
- **No Speculative Engineering**: Implement only features necessary to satisfy the current user story or test case.
- **No Single-Implementation Interfaces**: Do not create interfaces if there is only one concrete implementation, unless required as a Ports & Adapters boundary between Application/Domain and Infrastructure.
- **No Premature Generalization**: Do not create generic "frameworks", base classes, or plug-in systems until at least three concrete use cases demand it.
- **No Unused Configuration or Dead Code**: Delete unused parameters, unreferenced methods, dead imports, and speculative database columns immediately.
- **No Architectural Overkills**: Do not introduce message queues, distributed caching, event sourcing, or microservice splits until profiling or explicit business requirements demand them.

### 2.2 KISS (Keep It Simple, Stupid)
- Write self-explanatory code where the control flow is linear, readable, and transparent.
- Prefer explicit code over clever metaprogramming, reflection, or deeply nested conditional logic.
- Methods must do one thing at a single level of abstraction (Single Level of Abstraction Principle - SLAP).
- Keep cyclomatic complexity low: avoid nested loops and deeply nested `if/else` ladders. Use guard clauses and pattern matching instead.
- Do not create superfluous wrapper classes that merely pass calls through without adding behavioral value.

### 2.3 DRY (Don't Repeat Yourself) & The Rule of Three
- Consolidate business rules and domain logic into a single authoritative source of truth.
- **The Rule of Three**: Do not prematurely abstract code that looks similar twice. Wait until you see three concrete repetitions before extracting a shared abstraction. Duplicate code is cheaper than the wrong abstraction.
- Distinguish between *accidental duplication* (code that looks identical in two different bounded contexts or layers) and *essential duplication* (the same business knowledge represented twice). Never couple independent contexts just to satisfy DRY.

### 2.4 SOLID Principles (Enterprise C# Application)
1. **Single Responsibility Principle (SRP)**:
   - Every class, record, or module must have one, and only one, reason to change.
   - Separate data access, business logic, validation, and presentation into distinct classes.
2. **Open/Closed Principle (OCP)**:
   - Entities and services should be open for extension (via composition, strategy patterns, or domain events), but closed for modification.
3. **Liskov Substitution Principle (LSP)**:
   - Derived classes or interface implementations must honor the behavioral contract of base types without throwing unexpected `NotImplementedException` or violating preconditions.
4. **Interface Segregation Principle (ISP)**:
   - Prefer small, client-specific interfaces (e.g., `IVideoGameReader`, `IVideoGameWriter`) over large, monolithic interfaces (`IVideoGameManager`).
5. **Dependency Inversion Principle (DIP)**:
   - High-level modules (Domain/Application) must not depend on low-level modules (Infrastructure/EF Core/HTTP Clients). Both must depend on abstractions (ports/interfaces).
   - Inject dependencies via constructor injection. Never instantiate concrete dependencies inside business logic (`new` is glue).

### 2.5 Supporting Principles
- **Boy Scout Rule**: Always leave code and tests cleaner than you found them. Fix typos, improve naming, and clean up warnings whenever touching a file.
- **Law of Demeter (Least Knowledge)**: A method should only call methods on its own object, parameters, instantiated objects, or direct dependencies.
- **Command-Query Separation (CQS)**: Commands change state and return status/result; Queries retrieve data with zero side effects.

---

## 3. Strict Cyclomatic Complexity Limit (<= 10)

### 3.1 The Rule
**No method, function, or property accessor shall have a cyclomatic complexity exceeding 10.**
Any method reaching a complexity score of 8 must be proactively evaluated for simplification.

### 3.2 Actionable Complexity Reduction Tactics
1. **Guard Clauses & Early Returns (Bouncer Pattern)**:
   - Check preconditions and failure cases at the top of the method and return immediately.
   - Eliminate deeply nested `if/else` ladders and arrow anti-patterns.
   ```csharp
   // Required: Cyclomatic Complexity = 5, Flat & Readable
   public Result RentGame(User user, VideoGame game)
   {
       if (user is null) return Result.Failure(CommonErrors.NullUser);
       if (!user.IsActive) return Result.Failure(UserErrors.Inactive);
       if (game.IsRented) return Result.Failure(GameErrors.AlreadyRented);
       if (!user.CanRentMoreGames) return Result.Failure(UserErrors.RentLimitExceeded);

       return PerformRental(user, game);
   }
   ```
2. **Single Level of Abstraction Principle (SLAP) & Method Decomposition**:
   - Keep each method focused on a single narrative at a consistent level of abstraction.
   - Delegate detail-heavy branches or calculations to small, descriptive private helper methods.
3. **Polymorphic Dispatch & Strategy Pattern**:
   - Replace long `switch` statements or conditional ladders on type/status with polymorphic dispatch or dedicated strategy implementations.
4. **Lookup Tables / Dictionaries**:
   - Replace complex multi-branch evaluation logic with table-driven lookups or `FrozenDictionary<TKey, TValue>`.
5. **LINQ & Functional Pipelines**:
   - Replace nested loops and stateful accumulation flags with declarative collection pipelines.

---

## 4. Architectural Synthesis: Clean, Onion, Ports & Adapters + DDD

The solution architecture synthesizes **Clean Architecture**, **Onion Architecture**, and **Ports and Adapters (Hexagonal Architecture)** around a **Domain-Driven Design (DDD)** core.

```text
+-----------------------------------------------------------------------+
|                 HEXAGONAL ADAPTERS & INFRASTRUCTURE                   |
|                                                                       |
|  [Driving / Inbound Adapters]           [Driven / Outbound Adapters]  |
|  * Minimal APIs / Controllers           * EF Core DbContext & SQL DB  |
|  * CLI Command Line Interface           * External APIs (IGDB, Steam) |
|  * Event / Queue Consumers              * File Storage / Redis        |
|               |                                      ^                |
|               | (calls)                              | (implements)   |
|               v                                      |                |
|  +-----------------------------------------------------------------+  |
|  |                 ONION LAYER 3: APPLICATION CORE                 |  |
|  |   [Driving Ports: Commands/Queries] [Driven Ports: Repos/APIs]  |  |
|  |                                                                 |  |
|  |   +---------------------------------------------------------+   |  |
|  |   |            ONION LAYER 2: DOMAIN SERVICES               |   |  |
|  |   |   * Cross-Aggregate Domain Policies & Business Rules    |   |  |
|  |   |                                                         |   |  |
|  |   |   +-------------------------------------------------+   |   |  |
|  |   |   |      ONION LAYER 1: DOMAIN MODEL (THE CORE)     |   |   |  |
|  |   |   |   * Entities, Aggregates, Value Objects, Events |   |   |  |
|  |   |   |   * Pure C# / Zero External Dependencies        |   |   |  |
|  |   |   +-------------------------------------------------+   |   |  |
|  |   +---------------------------------------------------------+   |  |
|  +-----------------------------------------------------------------+  |
+-----------------------------------------------------------------------+
```

### 4.1 Concentric Onion Layers & Boundaries
1. **Onion Ring 1: Domain Model (The Core)**:
   - Entities, Value Objects, Domain Events, and Domain Error types.
   - **Zero external dependencies**: Pure C# only. No EF Core, ASP.NET, or external packages.
   - State mutations are strictly encapsulated. No public setters.
2. **Onion Ring 2: Domain Services**:
   - Business logic spanning multiple Aggregates or domain policies. Operates strictly on domain types.
3. **Onion Ring 3: Application Core (Use Cases & Ports)**:
   - Coordinates domain models to fulfill application use cases (CQRS Commands and Queries).
   - Owns both **Inbound (Driving) Ports** and **Outbound (Driven) Ports**.
   - Handles transactions, DTO mapping, and input validation.
4. **Onion Ring 4: Infrastructure & Adapters (The Outer Ring)**:
   - Implements Outbound Ports (e.g., EF Core repositories, HTTP clients for IGDB/RAWG, `SystemTimeProvider`).
   - Hosts Inbound Adapters (e.g., ASP.NET Core Minimal API endpoints, controllers, CLI runners).

### 4.2 Hexagonal Architecture: Ports and Adapters
- **Ports (Interfaces owned by the Application/Domain Core)**:
  - **Driving (Inbound) Ports**: Define operations outside actors can invoke (e.g., `ICommandHandler<AddGameCommand, Result<GameId>>`, `IGetGameByIdQueryHandler`).
  - **Driven (Outbound) Ports**: Define external services the core requires (e.g., `IVideoGameRepository`, `IExternalGameMetadataPort`, `IClockPort`).
- **Adapters (Implementations residing in Outer Layers)**:
  - **Driving (Inbound) Adapters**: Translate external protocols into Core port calls (e.g., ASP.NET Core Endpoints, CLI handlers).
  - **Driven (Outbound) Adapters**: Translate core port calls to external protocols (e.g., `EfCoreVideoGameRepository`, `IgdbApiAdapter`, `SqliteConnectionFactory`).

### 4.3 Domain-Driven Design (DDD) Tactical Patterns
1. **Entities & Aggregate Roots**:
   - Entities have an identity (`Id`) persisting through state changes.
   - An **Aggregate Root** is the sole entry point for modifying a cluster of associated objects, enforcing transactional consistency invariants.
   - External objects may only reference the Aggregate Root by ID.
2. **Value Objects**:
   - Immutable objects without identity, defined entirely by attributes (e.g., `GameTitle`, `ReleaseYear`, `Price`, `Platform`).
   - Implemented via `readonly record struct` or `record class`. Validate on creation returning `Result<T>`.
3. **Domain Events**:
   - Represent significant domain state changes (e.g., `GameAddedToCatalogueEvent`). Raised by Aggregate Roots; dispatched after persistence commits.
4. **Repositories**:
   - Only Aggregate Roots have Repositories. Interfaces reside in Domain/Application; concrete adapters reside in Infrastructure.

---


---

## 6. Test-Driven Development (TDD) Protocols & High Coverage

### 6.1 Strict Red-Green-Refactor Cycle
1. **RED**: Write an automated test expressing a single behavioral requirement or edge case. Verify that it **fails for the expected reason** (not a compilation error).
2. **GREEN**: Write the **minimal** production code required to make the test pass. Avoid speculative code. Verify it passes.
3. **REFACTOR**: Clean the code, eliminate duplication, verify cyclomatic complexity <= 10, enforce SOLID/KISS/YAGNI, and ensure all tests stay green.

### 6.2 Code Coverage Standard
- **Domain Layer**: 100% test coverage on all domain invariants, state transitions, and Value Object validations.
- **Application Layer**: >= 90% branch and line coverage on use case handlers, command validators, and business flows.
- **Infrastructure Layer**: Integration tests verify every repository query and database mapping.

### 6.3 The Testing Pyramid
- **Unit Tests (70–80%)**: Domain entities, Value Objects, Domain Services, and Application handlers. Zero I/O, fast, sub-second execution.
- **Integration Tests (15–20%)**: Test EF Core repositories against real database engines (Testcontainers / SQLite in-memory).
- **End-to-End / API Tests (5–10%)**: Full HTTP request-response cycles with `WebApplicationFactory<Program>`.

### 6.4 Test Standards & Conventions
- **Frameworks**: `xUnit`, `FluentAssertions`, and `NSubstitute`.
- **Naming Pattern**: `MethodOrFeature_StateUnderTest_ExpectedBehavior`
- **Structure**: Arrange-Act-Assert (AAA) pattern:
  ```csharp
  [Fact]
  public async Task Handle_WhenGameExists_ShouldReturnGameDetailsDto()
  {
      // Arrange
      var gameId = GameId.New();
      var game = VideoGame.Create(gameId, GameTitle.Create("Chrono Trigger").Value, ReleaseYear.Create(1995).Value).Value;
      _gameRepository.GetByIdAsync(gameId, Arg.Any<CancellationToken>()).Returns(game);

      var query = new GetGameByIdQuery(gameId.Value);

      // Act
      var result = await _handler.Handle(query, CancellationToken.None);

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Title.Should().Be("Chrono Trigger");
  }
  ```
- **Test Invariants**: Tests must be deterministic. Never mock Domain entities or Value Objects. Only mock out-of-process boundaries (Driven Ports).

---

## 7. Modern C# and .NET 10 Idioms & Standards

### 7.1 Language Features
- **Primary Constructors** for dependency injection and immutability.
- **Records** (`record class` / `readonly record struct`) for Value Objects, DTOs, Commands, and Queries.
- **Collection Expressions** (`[]` syntax) over `new List<T>()` or `new T[]`.
- **Pattern Matching** for type checks and state transitions.
- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is mandatory. Eliminate `NullReferenceException`. Never use `!` except in verified test setups or EF Core navigations with documented rationale.

### 7.2 Async/Await & Cancellation
- All I/O-bound operations must be asynchronous (`Task` / `ValueTask`).
- Every asynchronous method signature must accept `CancellationToken cancellationToken = default` as its last parameter and pass it downstream.
- Use `ConfigureAwait(false)` in non-ASP.NET Core library projects (Domain/Infrastructure).

### 7.3 Error Handling & The Result Pattern
- **Do not use exceptions for business logic or validation failures**. Use a lightweight, strongly typed `Result<T>`:
  ```csharp
  public readonly record struct Error(string Code, string Description)
  {
      public static readonly Error None = new(string.Empty, string.Empty);
      public static Error Validation(string code, string desc) => new(code, desc);
      public static Error NotFound(string code, string desc) => new(code, desc);
      public static Error Conflict(string code, string desc) => new(code, desc);
  }

  public class Result<TValue>
  {
      public bool IsSuccess { get; }
      public bool IsFailure => !IsSuccess;
      public TValue Value { get; }
      public Error Error { get; }

      private Result(TValue value, bool isSuccess, Error error) =>
          (Value, IsSuccess, Error) = (value, isSuccess, error);

      public static Result<TValue> Success(TValue val) => new(val, true, Error.None);
      public static Result<TValue> Failure(Error err) => new(default!, false, err);
  }
  ```
- Centralized exception handling middleware catches unexpected unhandled exceptions and converts them to RFC 7807 `ProblemDetails`.

### 7.4 Time Management
- Never use `DateTime.Now` or `DateTime.UtcNow` directly. Always inject and use .NET's built-in `TimeProvider`.

### 7.5 Dependency Injection & Configuration
- Built-in Microsoft DI container (`IServiceCollection`).
- Explicit lifetimes: Scoped (DbContext, repositories, handlers), Singleton (stateless utilities, metrics), Transient (short-lived helpers).
- Never use the Service Locator anti-pattern.

---

## 8. Enterprise Cross-Cutting Concerns

### 8.1 Observability & Structured Logging
- Inject `ILogger<T>` into application services and driving adapters.
- Use **Structured Logging** (message templates with named parameters); never concatenate strings:
  ```csharp
  _logger.LogInformation("Game {GameId} added to catalogue by user {UserId}", gameId, userId);
  ```

### 8.2 Defensive Invariant Enforcement
- Validate all incoming API request payloads at the gate using **FluentValidation**.
- Enforce business invariants inside Domain Entity constructors or factory methods:
  ```csharp
  public static Result<VideoGame> Create(GameId id, GameTitle title, ReleaseYear releaseYear)
  {
      if (id == GameId.Empty)
          return Result<VideoGame>.Failure(GameErrors.InvalidId);

      return Result<VideoGame>.Success(new VideoGame(id, title, releaseYear));
  }
  ```

---

## 9. AI Operating Workflow & Definition of Done (DoD)

When asked to implement any feature, fix a bug, or perform a refactoring, execute the following protocol:

### Step 1: Requirements & Domain Modeling
- Analyze the user request. Identify Aggregate Roots, Entities, Value Objects, Ports, and Use Cases.
- Apply YAGNI: reject speculative abstractions and premature generalizations.

### Step 2: Write Failing Tests (Red)
- Create or update test files in `VideoGameCatalogue.UnitTests` or `VideoGameCatalogue.IntegrationTests`.
- Write focused tests with expressive names following the AAA pattern. Run `dotnet test` and confirm expected failure.

### Step 3: Minimal Implementation (Green)
- Implement minimal clean code in Domain / Application / Infrastructure.
- Run `dotnet test` and ensure all tests pass.

### Step 4: Refactor & Complexity Check
- Verify cyclomatic complexity <= 10 for every method touched.
- Review against SOLID, DRY, KISS, and YAGNI. Ensure async methods take `CancellationToken`.

### Step 5: Verification & Quality Gate
Before claiming any task complete, verify:
```bash
dotnet build --warnaserror
dotnet test
```
Both commands must exit with code 0 and zero warnings.

### Definition of Done (DoD) Checklist:
- [ ] Feature/bug is accompanied by automated tests written first (TDD Red-Green-Refactor).
- [ ] Code coverage target met (>= 90% Application/Domain, 100% invariants).
- [ ] Cyclomatic complexity for every method is <= 10.
- [ ] Layer dependencies strictly respected (Domain has zero external dependencies).
- [ ] Inbound and Outbound Ports defined cleanly in Application/Domain; Adapters in Outer Ring.
- [ ] No dead code, speculative features, or unnecessary wrapper layers (YAGNI & KISS).
- [ ] No duplicated domain logic; Rule of Three honored (DRY).
- [ ] Solution compiles with zero warnings (`TreatWarningsAsErrors`).
- [ ] Meaningful structured logging and error handling using Result pattern.
- [ ] Uses modern C# idioms, strict nullability, and `TimeProvider`.
