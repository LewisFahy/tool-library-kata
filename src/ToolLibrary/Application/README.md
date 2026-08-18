# Application

One class per interaction. Nothing else.

Namespace: `ToolLibrary.Application`

## The shape

```csharp
public class BorrowTool
{
    // dependencies injected via the constructor

    public void Execute(BorrowToolCommand command)
    {
        // load → delegate → save → maybe publish
    }
}
```

## The rules

- **One public method, called `Execute`.** A second public method means a second
  interaction; give it its own class.
- **Commands in, DTOs or void out.** Domain entities never cross this boundary.
- **No business rules.** No `if` about fines, limits or suspensions — that's the domain's
  job. This is the constraint that makes the kata work.
- **One unit of work.** `Execute` is a transaction boundary. Know where it starts and ends.
- **Register it** in `../Configuration/ToolLibraryModule.cs`, or the container tests fail.

## Naming

Name the class after what the *person* does, in their words: `BorrowTool`, `RenewLoan`,
`ReportToolDamaged`.

Not `LoanService`, `ToolManager` or `LoanProcessor` — those are nouns you'd have to explain
to the librarian. If you can't name it as a verb, you probably haven't found the
interaction yet.

## Discovery

The guard tests find your services by convention: a public, concrete class in this
namespace with a public `Execute` method. Commands and results can live here too — they're
ignored, because they don't execute anything.
