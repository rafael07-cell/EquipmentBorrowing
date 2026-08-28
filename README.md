# EquipmentBorrowing — Campus Equipment Borrowing System

**Course:** ITSD 81 – Desktop Application Development
**Activity:** Laboratory Activity 1 — From Requirements to Application Structure
**Section:** BSIT 3C
**Team:** [Your Name] (Infrastructure) & [Partner's Name] (Domain / Application)

---

## 1. Solution Structure

The solution is organized into four projects, each with a distinct, isolated responsibility:

- **`EquipmentBorrowing.Domain`** — Contains the core concepts and rules that belong to the problem itself, independent of how the application runs or where data is stored: `Student`, `Equipment`, `Borrowing`, and `BorrowingStatus`. This project has no dependencies on any other project in the solution.

- **`EquipmentBorrowing.Application`** — Contains the operations (use cases) the system performs, and the abstractions (interfaces) it needs to perform them. `BorrowEquipmentService` lives here and coordinates domain objects and repository interfaces to execute the borrowing use case. This layer depends only on Domain — it does not know how data is actually stored.

- **`EquipmentBorrowing.Infrastructure`** — Contains the concrete, technical implementations of the abstractions defined in Application. For this activity, that means simple in-memory repositories (`InMemoryStudentRepository`, `InMemoryEquipmentRepository`, `InMemoryBorrowingRepository`) using C# `List<T>` collections instead of a real database.

- **`EquipmentBorrowing.Tests`** — Contains the initial test project structure for testing Domain and Application behavior.

- **`EquipmentBorrowing.ConsoleDemo`** — The executable entry point (`Program.cs`). This is the composition root: the only place where concrete Infrastructure classes are actually instantiated and wired into the Application service.

---

## 2. Dependency Direction

```
        EquipmentBorrowing.ConsoleDemo
        (executable / composition root)
                    │
                    ▼
        EquipmentBorrowing.Application
                    │
                    ▼
          EquipmentBorrowing.Domain
                    ▲
                    │
        EquipmentBorrowing.Infrastructure


   EquipmentBorrowing.Tests ──→ Domain
   EquipmentBorrowing.Tests ──→ Application
```

**Explanation:**

- **Domain** depends on nothing. It is the innermost layer and has no knowledge of Application, Infrastructure, or how the program is run.
- **Application** depends only on **Domain**. It defines *what* the system needs to do (via `BorrowEquipmentService`) and *what data operations it needs* (via `IStudentRepository`, `IEquipmentRepository`, `IBorrowingRepository`), without knowing how those operations are actually carried out.
- **Infrastructure** depends on **Application** (to implement its interfaces) and **Domain** (to work with `Student`, `Equipment`, and `Borrowing` objects). Application, however, has no dependency on Infrastructure — it only knows the interfaces exist, not which class implements them.
- **ConsoleDemo** is the only project that references all three layers, because it is responsible for constructing the concrete Infrastructure repositories and injecting them into the Application service (manual dependency injection, Part F).
- **Tests** depends on Domain and Application, allowing tests to exercise business rules and service logic directly.

This arrangement means the storage mechanism (currently in-memory) can later be replaced with a real database implementation without requiring any changes to Domain or Application — only a new Infrastructure implementation and a one-line change in `Program.cs`.

---

## 3. Use Case Mapping

**Actor:** Student

**Use Case:** Borrow Equipment

**Application Service:** `BorrowEquipmentService.ExecuteAsync(int studentId, int equipmentId, CancellationToken cancellationToken)`

**Domain Objects Used:**
- `Student` (checked via `IsAllowedToBorrow`)
- `Equipment` (checked via `IsAvailable`, updated via `MarkBorrowed()`)
- `Borrowing` (created upon successful validation)

**Repository Interfaces Used:**
- `IStudentRepository.GetByIdAsync(int id, CancellationToken)`
- `IEquipmentRepository.GetByIdAsync(int id, CancellationToken)`
- `IBorrowingRepository.CountActiveByStudentIdAsync(int studentId, CancellationToken)`
- `IBorrowingRepository.AddAsync(Borrowing borrowing, CancellationToken)`

**Infrastructure Implementations Used:**
- `InMemoryStudentRepository`
- `InMemoryEquipmentRepository`
- `InMemoryBorrowingRepository`

**Validation rules enforced by `BorrowEquipmentService`, in order:**
1. Student exists (`GetByIdAsync` does not return null).
2. Student is allowed to borrow (`IsAllowedToBorrow == true`).
3. Equipment exists (`GetByIdAsync` does not return null).
4. Equipment is currently available (`IsAvailable == true`).
5. Student has not reached the maximum number of active borrowings (`CountActiveByStudentIdAsync` < `MaxActiveBorrowings`, set to 3).
6. If all rules pass, a new `Borrowing` is created, the equipment is marked borrowed, and the record is persisted via `AddAsync`.

This directly satisfies the six validation checks required by the lab specification (Part E).

---

## 4. Reflection

**1. Why should the application service depend on a repository interface instead of directly depending on a database implementation?**

Depending on an interface (`IStudentRepository`, `IEquipmentRepository`, `IBorrowingRepository`) instead of a concrete class means `BorrowEquipmentService` only knows *what* operations are available (e.g., "get a student by ID"), not *how* those operations are carried out. This is what allowed us to run and fully test the service using in-memory repositories with zero knowledge, on the Application side, that a database doesn't exist yet. If the service depended directly on a concrete database class, swapping storage technology later — or even just testing the service without a real database — would require changing the service itself. With the interface in place, we can add a `SqliteBorrowingRepository` later that implements the same interface, and `BorrowEquipmentService` would not need a single line changed.

**2. Which parts of your current solution could remain unchanged if SQLite were added later?**

`EquipmentBorrowing.Domain` and `EquipmentBorrowing.Application` would remain completely unchanged. The domain models (`Student`, `Equipment`, `Borrowing`, `BorrowingStatus`) describe the problem itself, not storage, so they don't need to know SQLite exists. `BorrowEquipmentService` and the repository interfaces would also stay untouched, since they were written against abstractions rather than the in-memory implementation. Only `EquipmentBorrowing.Infrastructure` would need new classes (e.g., `SqliteStudentRepository`), and `Program.cs` would need a one-line change to construct those new classes instead of the in-memory ones.

**3. Which project would eventually contain Avalonia Views?**

`EquipmentBorrowing.ConsoleDemo` would effectively be replaced by a new UI project (e.g., `EquipmentBorrowing.Desktop` or similar), which would contain the Avalonia Views. This project — like `ConsoleDemo` today — would sit at the outermost layer, referencing `Application` (and indirectly `Domain`) to call `BorrowEquipmentService`, and referencing `Infrastructure` only at startup to wire up the real dependencies. Neither `Domain`, `Application`, nor `Infrastructure` would need to change to support a new UI layer.

**4. Should an Avalonia button directly execute database queries? Why or why not?**

No. A button's click handler should call into the Application layer (e.g., invoke `BorrowEquipmentService.ExecuteAsync(...)`), not talk to a database directly. If UI code executed SQL or repository logic itself, the business rules (student eligibility, equipment availability, borrowing limits) would either be duplicated across every UI element that needs them, or skipped entirely in some parts of the interface. Keeping the UI layer only responsible for displaying data and forwarding user actions to the Application service ensures the validation rules exist in exactly one place, regardless of how many different UI screens eventually call the same use case.

**5. What part of your implementation represents the actual business operation requested by the actor?**

`BorrowEquipmentService.ExecuteAsync(int studentId, int equipmentId, CancellationToken cancellationToken)`, in the Application layer, represents the actual business operation. This is the single method that carries out everything the Student actor is really asking for when they "borrow equipment" — checking eligibility, checking availability, enforcing the borrowing limit, and creating the borrowing record — independent of whether that request came from a console demo, a future Avalonia UI, or an automated test.

---

## 5. Working Demonstration

The console demo (`EquipmentBorrowing.ConsoleDemo/Program.cs`) seeds two students and two pieces of equipment, then executes three scenarios:

1. **Successful case** — Student 1 (allowed to borrow) borrows Equipment 1 (available) → `Success: Borrowing #<id> created.`
2. **Failure case** — Student 2 (`IsAllowedToBorrow = false`) attempts to borrow → `Failed: Student is not allowed to borrow.`
3. **Failure case** — Student 1 attempts to borrow Equipment 2 (`IsAvailable = false`) → `Failed: Equipment is not available.`

All three cases were run and verified via `dotnet run --project EquipmentBorrowing.ConsoleDemo`, confirming the full path from console input through `BorrowEquipmentService`, the repository interfaces, and the in-memory Infrastructure implementations.

---

## 6. Git History

Meaningful commits reflecting incremental development (see repository commit history for full detail), including:
- Initial solution structure
- Add domain models
- Add repository interfaces
- Implement borrowing service
- Add in-memory repository implementations
- Add data seeding and verify console demo success/failure cases
- Complete architecture documentation
