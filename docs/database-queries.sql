-- Campus Equipment Borrowing System: sample queries (SQLite)

-- 1. RETRIEVAL: list all equipment
SELECT Id, Name, IsAvailable
FROM Equipment
ORDER BY Id;

-- 2. FILTER: students who are allowed to borrow
SELECT Id, Name
FROM Students
WHERE IsAllowedToBorrow = 1;

-- 3. JOIN: active borrowings with student and equipment details
SELECT b.Id AS BorrowingId,
       s.Name AS Student,
       e.Name AS Equipment,
       b.DateBorrowed,
       b.ExpectedReturnDate
FROM Borrowings b
INNER JOIN Students s ON s.Id = b.StudentId
INNER JOIN Equipment e ON e.Id = b.EquipmentId
WHERE b.Status = 'Active'
ORDER BY b.DateBorrowed;

-- 4. AGGREGATE: number of borrowings per student
SELECT s.Name AS Student, COUNT(b.Id) AS TotalBorrowings
FROM Students s
LEFT JOIN Borrowings b ON b.StudentId = s.Id
GROUP BY s.Id, s.Name
ORDER BY TotalBorrowings DESC;

-- 5. UPDATE: block a student from borrowing
UPDATE Students
SET IsAllowedToBorrow = 0
WHERE Name = 'Carla Santos';