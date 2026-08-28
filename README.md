# EquipmentBorrowing — Campus Equipment Borrowing System

**Course:** ITSD 81 – Desktop Application Development
**Activity:** Laboratory Activity 1 — From Requirements to Application Structure
**Section:** BSIT 3C
**Team:** [Your Name] (Infrastructure) & [Partner's Name] (Domain / Application)

---

## 1. Overview

EquipmentBorrowing is a console-based simulation of a campus equipment borrowing system, built using layered architecture in C#/.NET. The system has a single actor — the **Student** — and is fully automatic (no staff/admin actor, no manual approval step). The solution has no UI and no database; data is held in memory for the duration of the program.

---

## 2. Actor

- **Student** — the only actor in the system. Students borrow equipment, return equipment, and check equipment availability. All borrowing rules (e.g., eligibility, availability checks) are enforced automatically by the system with no human intervention.

---

## 3. Use Cases

### 3.1 Borrow Equipment

| Field | Description |
|---|---|
| **Preconditions** | Student exists and `IsAllowedToBorrow` is `true`; requested equipment exists and `IsAvailable` is `true`. |
| **Main Action** | Student requests to borrow a specific piece of equipment. |
| **Expected Result** | A new `Borrowing` record is created with `Status = Active`; the equipment's `IsAvailable` is set to `false`. |
| **Possible Failure** | Student is not allowed to borrow, equipment is already borrowed/unavailable, or student/equipment does not exist. |

### 3.2 Return Equipment

| Field | Description |
|---|---|
| **Preconditions** | An active `Borrowing` record exists linking the student and the equipment. |
| **Main Action** | Student returns previously borrowed equipment. |
| **Expected Result** | The `Borrowing` record's `Status` is set to `Returned`; the equipment's `IsAvailable` is set to `true`. |
| **Possible Failure** | No matching active borrowing record found for that student/equipment pair. |

### 3.3 Check Availability of Equipment

| Field | Description |
|---|---|
| **Preconditions** | Requested equipment exists in the system. |
| **Main Action** | Student checks whether a specific piece of equipment is currently available. |
| **Expected Result** | System returns the equipment's current `IsAvailable` status. |
| **Possible Failure** | Equipment ID does not exist in the system. |

---

## 4. Domain Concepts

| Concept | Data It Holds | Rules It Owns | Not Responsible For |
|---|---|---|---|
| **Student** | `Id`, `Name`, `IsAllowedToBorrow` | Whether the student is eligible to borrow at all. | Tracking which items it currently holds — that's the Borrowing record's job. |
| **Equipment** | `Id`, `Name`, `IsAvailable` | Marking itself as borrowed/returned (`MarkBorrowed()`, `MarkReturned()`). | Knowing *who* borrowed it — that's tracked via `Borrowing`, not stored on Equipment. |
| **Borrowing** | `Id`, `StudentId`, `EquipmentId`, `DateBorrowed`, `ExpectedReturnDate`, `Status` | Its own lifecycle transition from Active → Returned (`MarkReturned()`). | Validating whether the student/equipment involved actually exist — that's the repository/service layer's job. |

---

## 5. Solution Structure

```
EquipmentBorrowing-shared/
│
├── EquipmentBorrowing.Domain/              → Core business entities, no dependencies
│   ├── Student.cs
│   ├── Equipment.cs
│   ├── Borrowing.cs
│   └── BorrowingStatus.cs
│
├── EquipmentBorrowing.Application/         → Business rules & contracts, depends on Domain
│   └── Interfaces/
│       ├── IStudentRepository.cs
│       ├── IEquipmentRepository.cs
│       └── IBorrowingRepository.cs
│   (BorrowEquipmentService.cs — in progress)
│
├── EquipmentBorrowing.Infrastructure/      → Concrete implementations, depends on Application + Domain
│   └── Repositories/
│       ├── InMemoryStudentRepository.cs
│       ├── InMemoryEquipmentRepository.cs
│       └── InMemoryBorrowingRepository.cs
│
├── EquipmentBorrowing.Tests/                → Unit tests, depends on Domain + Application
│
├── EquipmentBorrowing.ConsoleDemo/          → Entry point, wires everything together (Program.cs)
│
└── EquipmentBorrowing.slnx
```

### 5.1 Dependency Direction

```
   Domain
     ↑
 Application
     ↑
Infrastructure

Tests ──→ Domain
Tests ──→ Application

ConsoleDemo ──→ all layers (composition root)
```

The dependency arrows point **inward**, toward Domain. Domain has zero dependencies on any other project — it knows nothing about how data is stored or how business rules are orchestrated. Application depends only on Domain, defining *what* operations are needed (via interfaces) without knowing *how* they're implemented. Infrastructure depends on Application, providing the concrete "how" (in-memory storage, and later, potentially a real database) without Application ever needing to know Infrastructure exists. This is what allows the storage mechanism to be swapped later (e.g., in-memory → EF Core) without touching business logic.

---

## 6. Use Case → Code Mapping

| Use Case | Domain | Application | Infrastructure |
|---|---|---|---|
| Borrow Equipment | `Student.IsAllowedToBorrow`, `Equipment.MarkBorrowed()`, `new Borrowing(...)` | `IStudentRepository.GetByIdAsync`, `IEquipmentRepository.GetByIdAsync`, `IBorrowingRepository.CountActiveByStudentIdAsync`, `IBorrowingRepository.AddAsync` *(orchestrated in `BorrowEquipmentService`, in progress)* | `InMemoryStudentRepository`, `InMemoryEquipmentRepository`, `InMemoryBorrowingRepository` |
| Return Equipment | `Equipment.MarkReturned()`, `Borrowing.MarkReturned()` | `IBorrowingRepository.GetActiveByStudentAndEquipmentAsync` *(orchestrated in service, in progress)* | `InMemoryBorrowingRepository` |
| Check Availability | `Equipment.IsAvailable` | `IEquipmentRepository.GetByIdAsync` | `InMemoryEquipmentRepository` |

---

## 7. Repository Interfaces (Application Layer)

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

## 8. In-Memory Repository Implementations (Infrastructure Layer)

Each interface above is implemented using a simple in-memory `List<T>`, simulating storage without a real database. Repositories start empty; sample data is seeded from `Program.cs` at demo runtime. `Task.FromResult(...)` / `Task.CompletedTask` are used to satisfy the async interface signatures since no real I/O is happening yet — this keeps the contract identical to what a future database-backed implementation (e.g., EF Core) would need, so the Application layer never has to change when storage does.

---

## 9. Reflection Questions

*(To be completed after all parts are done.)*

1.
2.
3.
4.
5.

---

## 10. Status

- [x] Part A — Analysis (Actors, Use Cases, Domain Concepts)
- [x] Part B — Solution Scaffolding
- [x] Domain Layer
- [x] Part D — Repository Interfaces
- [x] Part G — In-Memory Repository Implementations
- [ ] Part E — BorrowEquipmentService
- [ ] Part F — Manual Dependency Injection
- [ ] Part H — Console Demo (success + failure case)
- [ ] Part I — Final Reflection Questions
