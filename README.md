# Campus Equipment Borrowing System

## Laboratory Activity 1

**ITSD 81 – Desktop Application Development**

**Prepared by:**

Deniel Bern M. Miasco

Rafael Sofian O. Guipetacio



---

# Part A – Requirements and Use Case Analysis

## A. Actors

**Student**

The student expects the system to allow them to request available equipment, borrow equipment when the requirements are satisfied, and return borrowed equipment.

---

## B. Use Cases

**Use Case: Borrow Equipment**

| Item | Description |
|---|---|
| Primary Actor | Student |
| Preconditions | Student exists and is allowed to borrow; equipment exists and is available; student has not reached the maximum number of active borrowings. |
| Main Action | Student requests to borrow a specific piece of equipment. |
| Expected Result | A new borrowing record is created with status Active; the equipment becomes unavailable. |
| Possible Failure | Student does not exist, student is not allowed to borrow, equipment does not exist, equipment is unavailable, or student has reached the maximum active borrowings limit. |

**Use Case: Return Equipment**

| Item | Description |
|---|---|
| Primary Actor | Student |
| Preconditions | An active borrowing record exists linking the student and the equipment. |
| Main Action | Student returns previously borrowed equipment. |
| Expected Result | The borrowing record's status is set to Returned; the equipment becomes available again. |
| Possible Failure | No matching active borrowing record found for that student/equipment pair. |

**Use Case: Check Availability of Equipment**

| Item | Description |
|---|---|
| Primary Actor | Student |
| Preconditions | Requested equipment exists in the system. |
| Main Action | Student checks whether a specific piece of equipment is currently available. |
| Expected Result | System returns the equipment's current availability status. |
| Possible Failure | Equipment ID does not exist in the system. |

---

## C. Domain Concepts

**Student**
1. Information it must contain: Id, Name, IsAllowedToBorrow.
2. Rules or state it owns: Whether the student is currently eligible to borrow.
3. Not its responsibility: Tracking which items it currently holds that belongs to Borrowing.

**Equipment**
1. Information it must contain: Id, Name, IsAvailable.
2. Rules or state it owns: Marking itself as borrowed or returned.
3. Not its responsibility: Knowing who borrowed it and tracking separately via Borrowing.

**Borrowing**
1. Information it must contain: Id, StudentId, EquipmentId, DateBorrowed, ExpectedReturnDate, Status.
2. Rules or state it owns: Its own lifecycle transition from Active to Returned.
3. Not its responsibility: Validating whether the student or equipment involved actually exist or are eligible that belongs to the application service.

---

# Part B – The .NET Solution

The solution is organized into the following projects, placed directly in the repository root (a flat structure rather than `src/`/`tests/` subfolders, as explicitly permitted by the activity):

- `EquipmentBorrowing.Domain`
- `EquipmentBorrowing.Application`
- `EquipmentBorrowing.Infrastructure`
- `EquipmentBorrowing.Tests`
- `EquipmentBorrowing.ConsoleDemo`

**Project Responsibilities**

**Domain** — Contains the important concepts and rules belonging to the problem itself: `Student`, `Equipment`, `Borrowing`, `BorrowingStatus`. No dependencies on any other project.

**Application** — Contains the operations performed by the application. `BorrowEquipmentService` coordinates domain objects and repository interfaces to execute the borrowing use case. Depends only on Domain.

**Infrastructure** — Contains implementations concerned with external technical mechanisms. For this activity, in-memory repositories using C# `List<T>` collections stand in for a database.

**Tests** — Contains the initial test project structure for Domain and Application behavior.

---

# Part C – Domain Models

```csharp
namespace EquipmentBorrowing.Domain;
public class Student
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAllowedToBorrow { get; set; }
    public Student(int id, string name, bool isAllowedToBorrow = true)
    {
        Id = id;
        Name = name;
        IsAllowedToBorrow = isAllowedToBorrow;
    }
}
```

```csharp
namespace EquipmentBorrowing.Domain;
public class Equipment
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAvailable { get; private set; }
    public Equipment(int id, string name, bool isAvailable = true)
    {
        Id = id;
        Name = name;
        IsAvailable = isAvailable;
    }
    public void MarkBorrowed() => IsAvailable = false;
    public void MarkReturned() => IsAvailable = true;
}
```

```csharp
namespace EquipmentBorrowing.Domain;
public class Borrowing
{
    public int Id { get; }
    public int StudentId { get; }
    public int EquipmentId { get; }
    public DateTime DateBorrowed { get; }
    public DateTime ExpectedReturnDate { get; }
    public BorrowingStatus Status { get; private set; }
    public Borrowing(int id, int studentId, int equipmentId,
        DateTime dateBorrowed, DateTime expectedReturnDate)
    {
        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }
    public void MarkReturned() => Status = BorrowingStatus.Returned;
}
```

```csharp
namespace EquipmentBorrowing.Domain;
public enum BorrowingStatus
{
    Active,
    Returned
}
```

---

# Part D – Repository Abstractions

```csharp
public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<Borrowing?> GetActiveByStudentAndEquipmentAsync(int studentId, int equipmentId, CancellationToken cancellationToken = default);
}
```

Each method exists because a specific application operation currently needs it.

---

# Part E – Application Service

**Chosen use case:** Borrow Equipment
**Service:** `BorrowEquipmentService`

The service coordinates `IStudentRepository`, `IEquipmentRepository`, and `IBorrowingRepository` to execute the borrowing operation, checking the following in order:

1. Does the student exist?
2. Is the student allowed to borrow?
3. Does the equipment exist?
4. Is the equipment currently available?
5. Has the student reached the allowed number of active borrowings (maximum of 3)?
6. If all rules are satisfied, a borrowing record is created.

The service contains no database connections, SQL, or user-interface code.

---

# Part F – Manual Dependency Injection

`BorrowEquipmentService` does not instantiate its own repository dependencies. Instead, it receives them through its constructor:

```csharp
public class BorrowEquipmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }
}
```

The concrete repository instances are constructed and injected in `Program.cs` (the composition root), not inside the service itself. No dependency injection container is used — dependencies are wired manually, as required by this activity.

---

# Part G – In-Memory Repository

Three in-memory repositories were implemented in `EquipmentBorrowing.Infrastructure/Repositories/`, each backed by a `List<T>` collection instead of a database:

- `InMemoryStudentRepository`
- `InMemoryEquipmentRepository`
- `InMemoryBorrowingRepository`

These demonstrate that `BorrowEquipmentService` can operate correctly without knowing how or where the data is actually stored.

---

# Part H – Application Flow Demonstration

The console demo (`EquipmentBorrowing.ConsoleDemo/Program.cs`) seeds two students and two pieces of equipment, then executes three scenarios:

**Successful Case:** Student 1 (allowed to borrow) borrows Equipment 1 (available) → `Success: Borrowing #<id> created.`

**Failure Case 1:** Student 2 (`IsAllowedToBorrow = false`) attempts to borrow → `Failed: Student is not allowed to borrow.`

**Failure Case 2:** Student 1 attempts to borrow Equipment 2 (`IsAvailable = false`) → `Failed: Equipment is not available.`

All three cases were run and verified via `dotnet run --project EquipmentBorrowing.ConsoleDemo`, confirming the full path from console input through `BorrowEquipmentService`, the repository interfaces, and the in-memory Infrastructure implementations.

---

# Part I – Architecture Explanation

## 1. Solution Structure

**Domain** — the important concepts and rules belonging to the problem itself, independent of storage or execution.

**Application** — the operations performed by the application, coordinating domain objects and repository interfaces.

**Infrastructure** — the concrete, technical implementations of Application's interfaces; currently simple in-memory storage.

**Tests** — the test project for Domain and Application behavior.

## 2. Dependency Direction

```
        EquipmentBorrowing.ConsoleDemo
        (executable / future UI)
                    │
                    ▼
        EquipmentBorrowing.Application
                    │        ▲
                    ▼        │
          EquipmentBorrowing.Domain
                             │
        EquipmentBorrowing.Infrastructure
```

Domain depends on nothing. Application depends only on Domain. Infrastructure depends on Application and Domain. ConsoleDemo references all three layers to construct and inject the concrete dependencies at startup.

## 3. Use Case Mapping

**Actor:** Student
**Use Case:** Borrow Equipment
**Application Service:** `BorrowEquipmentService`
**Domain Objects Used:** `Student`, `Equipment`, `Borrowing`
**Repository Interfaces Used:** `IStudentRepository`, `IEquipmentRepository`, `IBorrowingRepository`
**Infrastructure Implementations Used:** `InMemoryStudentRepository`, `InMemoryEquipmentRepository`, `InMemoryBorrowingRepository`

## 4. Reflection

**1. Why should the application service depend on a repository interface instead of directly depending on a database implementation?**

So the the BorrowEquipmentService will depend on what is need not on how it will be fulfilled. That separation allows you to run, build and test the entire borrowing workflow without a database ever existing.

**2. Which parts of your current solution could remain unchanged if SQLite were added later?**

EquipmentBorrowing.Domain and EquipmentBorrowing.Application would remain completely unchanged because both of them are not relying on database to store data.

**3. Which project would eventually contain Avalonia Views?**

A new project, like EquipmentBorrowing.Desktop, would hold the Avalonia Views and it would replace ConsoleDemo's role as the entry point, referencing Application to call the business logic and Infrastructure only to wire up dependencies at startup, while Domain, Application, and Infrastructure themselves stay completely untouched.

**4. Should an Avalonia button directly execute database queries? Why or why not?**

No. A button's click handler only role should be like the user do something and forward the intent to the layer that knows how to handle it. All the business roles that was in the BorrowEquipmentServices might get duplicated across every UI element that needs them or skipped entirely somewhere and the UI layer would become responsible for decisions it has no business making.

**5. What part of your implementation represents the actual business operation requested by the actor?**
The BorrowEquipmentService.ExecuteAsync(int studentId, int equipmentId, CancellationToken cancellationToken) represent the actual operation, checking eligibility, checking availability, enforcing the borrowing limit, and creating the borrowing records.


---

# Project Status

| Part | Description | Status |
|---|---|---|
| A | Analysis — Actors, Use Cases, Domain Concepts | ✅ Done |
| B | .NET Solution Scaffolding | ✅ Done |
| C | Domain Models | ✅ Done |
| D | Repository Abstractions | ✅ Done |
| E | Application Service (`BorrowEquipmentService`) | ✅ Done |
| F | Manual Dependency Injection | ✅ Done |
| G | In-Memory Repository | ✅ Done |
| H | Application Flow Demonstration | ✅ Done |
| I | Architecture Explanation (README) | ✅ Done |

**Build status:** Solution builds successfully across all 5 projects (Domain, Application, Infrastructure, Tests, ConsoleDemo).

**Git history:** Repository contains incremental, meaningful commits reflecting development progression — initial solution structure, domain models, repository interfaces, borrowing service, in-memory repository, demonstration with data seeding, and architecture documentation.


---

## Laboratory Activity 2

**Extending the Application with Avalonia UI and MVVM**

---

# Part J – Desktop Project

`EquipmentBorrowing.Desktop` is the new presentation layer added in this activity. It is responsible for:

- displaying equipment and active borrowing information to the user;
- collecting user input (student selection, equipment selection, expected return date);
- handling presentation state such as the currently selected item and status messages;
- invoking existing Application operations through ViewModels; and
- providing user-facing feedback for both success and failure outcomes.

Desktop references `EquipmentBorrowing.Application` and `EquipmentBorrowing.Infrastructure` directly. It does **not** duplicate any domain models, repository interfaces, or borrowing rules — it reuses `BorrowEquipmentService`, the new `ReturnEquipmentService`, and the existing in-memory repositories exactly as they were built (and extended) for the Application/Infrastructure layers. `EquipmentBorrowing.Domain` and `EquipmentBorrowing.Application` have no reference to Avalonia in either direction.

To support the Desktop project, the following repository methods were added to the Application interfaces and their in-memory implementations, since Lab 1 only exposed single-record lookups:

- `IStudentRepository.GetAllAsync()` — needed to populate the student selection dropdown.
- `IEquipmentRepository.GetAllAsync()` — needed to populate the equipment list.
- `IBorrowingRepository.GetActiveAsync()` — needed to populate the Active Borrowings screen.
- `IBorrowingRepository.GetByIdAsync()` — needed so a specific borrowing can be located and returned.

A new `ReturnEquipmentService` was also added to `EquipmentBorrowing.Application/Services/`, following the same pattern as `BorrowEquipmentService`: it depends only on repository interfaces, receives them through constructor injection, and returns a result object (`ReturnResult`) rather than throwing exceptions for expected failure cases.

---

# Part K – Updated Architecture

Avalonia View
│
│ Binding / Command
▼
ViewModel
│
│ Application Operation
▼
Application Service
│
├──────────► Domain
│
▼
Repository Interface
▲
│
Infrastructure Implementation


**View** (`EquipmentView.axaml`, `BorrowingsView.axaml`, `MainWindow.axaml`) — contains only XAML layout, controls, bindings, and styles. No business logic.

**ViewModel** (`EquipmentViewModel`, `BorrowingsViewModel`, `MainWindowViewModel`) — holds presentation state (selected items, observable collections, status messages) and exposes `RelayCommand`s that call into Application services. Never queries a repository directly and never re-implements a business rule.

**Application Service** (`BorrowEquipmentService`, `ReturnEquipmentService`) — unchanged in responsibility from Lab 1: coordinates repositories, enforces business rules, and returns a result.

**Domain** — unchanged from Lab 1.

**Repository Interface / Infrastructure Implementation** — unchanged from Lab 1, with the additional query methods described in Part J.

**Composition Root** — `App.axaml.cs` registers all repositories, services, and ViewModels using `Microsoft.Extensions.DependencyInjection`'s `ServiceCollection`, and constructs the initial `MainWindow` with its `DataContext` resolved from the container. Repositories and ViewModels are registered as Singleton so that in-memory application state (available equipment, active borrowings) is preserved when the user navigates between views, rather than being reset each time a view is shown.

---

# Part L – Borrow Equipment Flow

1. The user selects a student, selects a piece of equipment, and picks an expected return date on the Equipment view, then presses **Borrow Equipment**.
2. The button's `Command` binding fires `EquipmentViewModel.BorrowAsync()` (exposed via `[RelayCommand]`).
3. `BorrowAsync()` first performs presentation validation only — checking that a student and equipment were actually selected. No business rule is evaluated here.
4. If presentation validation passes, the ViewModel calls `BorrowEquipmentService.ExecuteAsync(studentId, equipmentId)`.
5. The service performs the actual business validation from Lab 1 (student exists, student is allowed to borrow, equipment exists, equipment is available, active borrowing limit) using the injected repository interfaces, and either creates a new `Borrowing` or returns a failure result with a message.
6. The result flows back to the ViewModel, which sets `StatusMessage` to either a success or failure message.
7. The ViewModel reloads equipment data (`LoadAsync()`), which refreshes the `ObservableCollection` bound to the Equipment list — so a successfully borrowed item is immediately shown as unavailable.
8. Data binding updates the View automatically; no manual UI refresh code is written anywhere.

---

# Part M – Return Equipment Flow

1. The user navigates to Active Borrowings, selects a borrowing record from the list, and presses **Return Equipment**.
2. The button's `Command` binding fires `BorrowingsViewModel.ReturnAsync()`.
3. `ReturnAsync()` performs presentation validation only — checking that a borrowing is actually selected.
4. If valid, the ViewModel calls `ReturnEquipmentService.ExecuteAsync(borrowingId)`.
5. The service locates the borrowing by id, checks it hasn't already been returned, locates the associated equipment, marks the borrowing as `Returned`, and marks the equipment as available again — returning a result object either way.
6. The ViewModel sets `StatusMessage` based on the result.
7. The ViewModel reloads active borrowings, which removes the now-returned record from the visible list via the `ObservableCollection`.
8. Switching back to the Equipment view shows the equipment as available again, since both views ultimately read from the same singleton repository instances.

---

# Part N – Architectural Reflection

**1. Why should the View not call a repository directly?**

The View is only responsible for displaying information and collecting user input. If it called a repository directly, it would bypass the business rules that live in the Application service (such as the borrowing limit or availability checks), and the same logic would either need to be duplicated in every View that needs it or would end up missing entirely somewhere. It would also make the UI impossible to test independently of storage.

**2. Why should business rules not be implemented in the ViewModel?**

A business rule implemented in the ViewModel only protects that one screen. If another part of the application (or a future screen) needed to trigger the same operation, the rule would either need to be copied or would be skipped. Keeping rules in the Application/Domain layer means there is exactly one place where "can this student borrow this equipment" is decided, and every caller — UI or otherwise — gets the same answer.

**3. What is the responsibility of the ViewModel?**

The ViewModel holds presentation state (selected items, loaded lists, status messages), exposes commands that respond to user actions, performs presentation-level validation (has something been selected, is a required field filled), and translates a user action into a call to the appropriate Application service. It does not decide whether an operation is allowed to succeed — it only reports the outcome the service returns.

**4. Why can the existing Application layer work without knowing that Avalonia is being used?**

`BorrowEquipmentService` and `ReturnEquipmentService` depend only on repository interfaces and Domain objects, both of which existed before Avalonia was ever introduced. Avalonia is layered on top through the Desktop project, which depends on Application — not the other way around. Because the dependency only points one direction, the Application layer has no reference to, and no awareness of, the UI framework calling it.

**5. What advantage is gained from registering dependencies in one composition point?**

Every dependency — which repository implementation to use, which services exist, how ViewModels are constructed — is defined in exactly one place (`App.axaml.cs`). If an implementation needs to change (for example, swapping an in-memory repository for a different one), only that one registration needs to be updated. Nothing else in the application needs to know or care how its dependencies were constructed.

**6. If the in-memory repository were replaced by SQLite later, which parts of the current interface should remain largely unchanged?**

The Views, ViewModels, Application services, Domain models, and the repository interfaces themselves would all remain unchanged. Only the Infrastructure layer would change — new SQLite-backed repository classes would be written to implement the same `IStudentRepository`, `IEquipmentRepository`, and `IBorrowingRepository` interfaces, and the only other change needed would be updating the registrations in `App.axaml.cs` to point to the new classes instead of the in-memory ones.

---

# Project Status (Updated)

| Part | Description | Status |
|---|---|---|
| A | Analysis — Actors, Use Cases, Domain Concepts | ✅ Done |
| B | .NET Solution Scaffolding | ✅ Done |
| C | Domain Models | ✅ Done |
| D | Repository Abstractions | ✅ Done |
| E | Application Service (`BorrowEquipmentService`) | ✅ Done |
| F | Manual Dependency Injection | ✅ Done |
| G | In-Memory Repository | ✅ Done |
| H | Application Flow Demonstration | ✅ Done |
| I | Architecture Explanation (README) | ✅ Done |
| J | Desktop Project (Avalonia) | ✅ Done |
| K | Updated Architecture Diagram | ✅ Done |
| L | Borrow Equipment Flow (UI) | ✅ Done |
| M | Return Equipment Flow (UI) | ✅ Done |
| N | Architectural Reflection | ✅ Done |

**Build status:** Solution builds successfully across all 6 projects (Domain, Application, Infrastructure, Tests, ConsoleDemo, Desktop).

**Manual testing:** Borrow Equipment and Return Equipment were tested end-to-end through the Avalonia UI, including a handled business-rule failure (student not allowed to borrow) and a handled presentation-validation failure (no borrowing selected before returning).

**Git history:** Repository contains incremental, meaningful commits reflecting development progression through Lab 2 — repository query methods, in-memory implementations, return service, dependency injection, equipment view model and view, borrowings view model and view, navigation layout, shared styles, and bug fixes discovered during manual testing.