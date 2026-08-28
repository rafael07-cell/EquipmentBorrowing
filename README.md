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
