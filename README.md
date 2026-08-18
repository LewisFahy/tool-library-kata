# The Community Tool Library Kata

An Interaction-Driven Design kata.

The goal is **not** to finish the features. The goal is to practise turning user
interactions into application services with a well-modelled domain underneath.

---

## Getting started

```bash
dotnet test
```

Five tests, two red. That's your starting line.

The two failures tell you the same thing from different angles: you haven't modelled an
interaction yet.

### Where things live

```
src/ToolLibrary/
├── Domain/                 your model — entities, value objects, domain services
├── Application/            one class per interaction
└── Configuration/          ToolLibraryModule — the composition root
tests/ToolLibrary.Tests/
├── BorrowToolShould.cs     start here
├── ContainerShould.cs      guard tests (don't delete)
└── *.cs                    test harness plumbing
```

---

## Learning objectives

By the end you should be able to:

1. Identify the **interactions** (things an actor does to the system) in a problem statement.
2. Write an application service that models **one interaction**, with a single `Execute` method.
3. Keep the application service **thin** — orchestration only — and push rules into the domain.
4. Choose a **transaction/unit-of-work boundary** deliberately.
5. Talk about the domain using the **business's words**, not technical nouns.

---

## The domain

A neighbourhood **tool library**. Members borrow tools — drills, ladders, wallpaper
steamers — instead of buying them.

### Glossary (use these words in your code)

| Term | Meaning |
|---|---|
| **Member** | Someone who has joined the library. Pays an annual subscription. |
| **Tool** | A single physical item, identified by an asset tag. |
| **Tool Type** | The kind of thing, e.g. "18V cordless drill". The library may own several. |
| **Loan** | A member has a tool out, from a start date, due back on a due date. |
| **Reservation** | A member has claimed the *next* available tool of a type. |
| **Suspension** | A member is temporarily barred from borrowing. |
| **Steward** | A volunteer who runs the counter — hands tools out and takes them back. |

### The rules, as the librarian would tell you

- A member can have at most **3 tools on loan** at once.
- The standard loan period is **7 days**. Ladders and scaffolding go out for **2 days**.
- A member can **renew** a loan once, for the same period again — but only if nobody is
  waiting for that tool type, and only if the loan isn't already overdue.
- Tools returned late incur a fine of **£1 per day**, capped at the tool's replacement cost.
- If a member owes more than **£20** in fines, they can't borrow anything else.
- If a member is more than **14 days** overdue on anything, they're **suspended** until it's back.
- A member can **reserve** a tool type when all copies are out. When one comes back it's
  held for them for **48 hours**.
- A steward can mark a tool **damaged** or **missing** on return. Damaged tools go out of
  circulation until repaired; that cancels nothing, the queue just keeps waiting.
- A member's subscription must be **current** to borrow. It lapses on its anniversary.

> These rules are deliberately underspecified, and in places slightly contradictory.
> **That's the point.** The facilitator plays the librarian — ask them.

---

## Format

- **Timebox:** 90 minutes, or three 45-minute sessions run as rounds.
- **Pairs or triples.** The pairing matters more than the code.
- **C#, NUnit, FakeItEasy.** Already set up in this repo.
- **No database.** Repository interfaces only, faked in tests. Persistence is a distraction.
- **A real container** — `Microsoft.Extensions.DependencyInjection`, wired through a single
  composition root in `src/ToolLibrary/Configuration/ToolLibraryModule.cs`.
- **No other frameworks.** No mapping library, no ORM, no mediator.

### The container rules

1. **One composition root.** `ToolLibraryModule` is the only place allowed to know how to
   build anything.
2. **Constructor injection only.** No `IServiceProvider` in an application service, no
   static access to the container, no service locator.
3. **Register as you go.** Each interaction you build gets registered. `ContainerShould`
   fails until it is.
4. **Tests don't use the container.** Unit tests `new` up the service under test and pass
   fakes by hand. If a test needs the container to build its subject, the constructor is
   telling you something.

Rule 4 catches people out. The container is for the *application*; the tests are where you
find out whether your dependencies are honest.

---

## Guard tests

Three tests enforce the constraints so the facilitator doesn't have to nag:

| Test | What it's protecting |
|---|---|
| `ContainerShould.Know_about_at_least_one_interaction` | You've modelled something as an interaction |
| `ContainerShould.Resolve_every_interaction_it_has_been_asked_to_build` | Registrations and lifetimes are real |
| `ApplicationServicesShould.Expose_one_public_method_each_named_Execute` | One service, one interaction |
| `ContainerShould.Only_ever_be_touched_by_the_composition_root` | No service locator |

They're part of the kata, not scaffolding — if one is in your way, that's the lesson
talking. Ports with no implementation (repositories, clock, email) are faked automatically
by the test harness, so you never have to write a throwaway stub to get green.

---

## Round 1 — Find the interactions (20 min, no code)

On paper or a whiteboard, list every **interaction** with the system. An interaction is
something an actor *does*, phrased as a verb from their point of view.

Prompts:
- Who are the actors? Member, Steward, ...anyone else? What about time itself?
- What does each one *do*?
- Which of these are actually the **same** interaction wearing different clothes?

If you find yourself writing `ToolService` or `LoanManager`, ask: *which single thing does a
person do when they call that?*

---

## Round 2 — Build one interaction, outside-in (45 min)

Pick **`BorrowTool`**. It's the richest. There's a red test waiting for you in
`tests/ToolLibrary.Tests/BorrowToolShould.cs`.

Write the test first. It should read like the interaction:

```csharp
[Test]
public void Lend_the_tool_to_the_member_and_set_the_due_date_seven_days_out()
```

Constraints for this round:

1. The application service has **one public method, `Execute`**.
2. `Execute` takes a **command/DTO**, not domain entities, and returns a DTO or void.
3. `Execute` contains **no business rules** — no `if` statements about fines, limits or
   suspensions. If you need one, it belongs in the domain.
4. Everything it touches from the outside world sits behind an **interface you own**.
5. `Execute` is **one unit of work**. Decide where the transaction starts and ends, and be
   able to justify it.
6. The service is **registered in the composition root** and resolves from the container,
   with a lifetime you can defend.

Constraint 3 is the whole exercise. Expect it to hurt.

A finished `Execute` should read roughly *load → delegate → save → maybe publish*. If it's
60 lines long, your domain is anaemic. If its constructor has seven parameters, it's doing
more than one interaction's worth of work — the container will happily build it anyway,
which is exactly why constructor size is worth watching.

---

## Round 3 — Pressure test the model (25 min)

Your facilitator will drop new requirements on you, one at a time, without warning.

The measure of a good model isn't how it handles round 2. It's how cheaply it absorbs
round 3.

---

## Round 4 (optional) — Constraints round

Re-run `BorrowTool` from a blank page with one added constraint:

- **No primitives** in the domain — no bare `string`, `int`, `DateTime` crossing a boundary.
- **No getters** on domain entities — tell, don't ask.
- **Mute pairing** — no talking; only the test names communicate.
- **Ping-pong** — I write a failing test, you make it pass and write the next failing test.

---

## Debrief (20 min — do not skip)

This is where the learning happens.

1. Show your `Execute` method. Read it aloud. Does it read as the interaction?
2. Where did the *"can this member borrow?"* decision end up? Why there?
3. Who else could have made that decision? What would that have cost you?
4. What did you name things? Would the librarian recognise the words?
5. Where's your transaction boundary? What happens if the email send fails after the commit?
6. How many constructor parameters does your service have? What would a seventh mean?
7. What lifetimes did you pick, and what breaks if you get one wrong?
8. Which round 3 change was cheapest? Which hurt? What does that tell you?

Then map it back: *where in our own codebase do we do the opposite of this?*

---

## A note on "finishing"

You won't finish, and you're not meant to. A kata that gets completed was too easy.
Stop on time, not on done-ness — then run the whole thing again from a blank page a few
weeks later. The second run is where the pattern actually sticks.
