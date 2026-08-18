# Facilitator guide

Don't share this file with participants before the session. Everything they need is in
`README.md`; this is the stuff that only works if it's a surprise.

> **Setup check:** run `dotnet test` before the session. You should see **5 tests, 2 failing**
> (`Lend_the_tool_to_the_member_...` and `Know_about_at_least_one_interaction`). The other
> three pass vacuously and start biting as soon as code appears.

---

## Your job

You are **the librarian**. You are not a teacher and you are definitely not an architect.

- Answer questions in business language. "I don't know, we've never had that happen" is a
  legitimate answer and a very useful one.
- Be occasionally inconsistent. Real stakeholders are. If someone catches you contradicting
  yourself, that's a win — congratulate them and pick a ruling.
- Never hand out a design. If asked "should this be a domain service?", bounce it back:
  "I don't know what that is. Where would *you* put it?"

---

## Timings

| Round | Length | What you're doing |
|---|---|---|
| Setup + framing | 10 min | Read the domain aloud. Take questions. |
| Round 1 — interactions | 20 min | Circulate. Say almost nothing. |
| Round 2 — build `BorrowTool` | 45 min | Answer as the librarian. Watch for constraint 3 breaking. |
| Round 3 — curveballs | 25 min | Drop the requirements below, one at a time. |
| Debrief | 20 min | The actual point of the day. |

If you only have 90 minutes, cut round 3 to two curveballs and protect the debrief.

---

## Round 1 checkpoints

You're looking for a list roughly like:

`BorrowTool`, `ReturnTool`, `RenewLoan`, `ReserveToolType`, `CancelReservation`,
`ReportToolDamaged`, `JoinLibrary`, `RenewSubscription`, `PayFine`

Things to watch for and *not* correct immediately:

- **Noun-shaped services** — `ToolService`, `LoanManager`, `MemberHandler`. Let them write
  it. Ask later: "which single thing does a person do when they call that?"
- **CRUD leakage** — `CreateLoan`, `UpdateLoan`, `DeleteLoan`. Ask what the steward would
  call it. Nobody at a counter says "update loan"; they say "they want it another week".
- **Missing actors** — most teams forget that *time* triggers things. Who suspends a member
  at 14 days overdue? Nobody clicks a button. That's an interaction too.
- **Merged interactions** — is "return a tool" the same interaction as "return a damaged
  tool"? There's no right answer. Make them defend one.

Resist the urge to teach here. Let them build `LoanManager` and feel it in round 2.

---

## Round 2 checkpoints

Walk the room. The tells:

- **`Execute` is 60 lines.** The domain is anaemic — entities are bags of properties and the
  service is doing all the thinking. Ask: "if I wanted this rule in a second place, what
  would I copy?"
- **`if` statements about fines or limits inside `Execute`.** This breaks constraint 3.
  Don't just say so — ask "who should know whether a member can borrow?" and wait.
- **Domain entities in the command.** `Execute(Member member, Tool tool)` means the caller
  already did the loading, and probably the deciding.
- **Repository returning DTOs.** Then the domain has nothing to be rich *with*.
- **No transaction boundary at all.** Ask what happens if the save succeeds and the
  reservation release fails.
- **Stuck on infrastructure.** Someone will start writing a fake in-memory repository with
  real query logic. Redirect: FakeItEasy, one line, move on.
- **Fighting the guard tests.** Someone will try to delete `ContainerShould` or add a second
  public method "just for the test". Don't allow it — ask what the test is telling them.

### On the container specifically

The container is there to make dependencies *visible*, not to teach DI. Most of the
learning is in what it exposes:

- **Constructor creep.** The container will cheerfully build a service with nine
  dependencies. Nothing fails. Ask the pair to read their constructor parameters aloud as a
  sentence — if it doesn't sound like one interaction, it isn't one.
- **Reaching for the container in a test.** A few pairs will try to resolve their service
  from a `ServiceCollection` inside a unit test. That's the tell that construction is
  painful, and the fix is in the design, not the test.
- **`IServiceProvider` injected into a service.** There's a guard test for it. When it
  fires, connect it to the codebase: this is exactly the ServiceResolver pattern, and it's
  only legitimate where serialisation forces it.
- **Lifetimes.** Most pairs pick `AddScoped` without thinking. Ask what a singleton
  application service would mean when two members borrow at once. `ValidateScopes` is on,
  so captive dependencies fail loudly — that's a good five minutes if it happens.
- **Registering the domain.** Watch for entities being registered in the container. A
  `Member` is loaded from a repository, not resolved. If they've registered one, the model
  has drifted towards services-with-data.

A good `Execute` ends up near:

```
load the member and the tool
ask the member to borrow it (domain decides, may refuse)
save
publish that it happened
```

If a pair gets there early, don't let them idle — give them a round 3 curveball ahead of
schedule.

---

## Round 3 — the curveballs

Drop these **one at a time**, out loud, to the whole room. Give each one 5 minutes.
Nobody has to finish the change; the point is to see where it lands.

1. **"Ladders go out for 2 days, not 7."**
   Where did the loan period live? A constant in the service is the common answer, and it's
   now wrong in a way that spreads.

2. **"Stewards can override the 3-tool limit for someone doing community work."**
   Is the limit a hard-coded `if`, or a policy that can vary? Watch for a boolean parameter
   creeping into the command — ask what the *third* override will do to that design.

3. **"We want to email the member when a tool they reserved comes back."**
   Does `ReturnTool` now know about email? Should it? Where does the event go, and what
   happens if the send fails after the commit?

4. **"The committee wants a report of who's had a tool out the longest."**
   Does a read model force them to break the write model? Watch for someone adding a getter
   to a domain entity purely so a report can see inside it.

5. *(spare, if a pair is flying)* **"A member wants to transfer a loan to their neighbour,
   who is also a member."**
   Is that one interaction or two? Is it `ReturnTool` + `BorrowTool`, or something the
   business would name differently?

6. *(spare)* **"Fines are going up to £1.50 a day in April, but only at the Southside
   branch."**
   Now the rule varies by time *and* by place. Does that become a constructor dependency, a
   registration-time decision, or something the domain asks for? Whatever they choose,
   make them say where it gets configured — this one lands squarely on the composition root.

---

## Debrief

Run it as a group, code on a screen. Questions in `README.md`, in that order. A few things
worth steering towards:

- **"It depends" is the honest answer to most of it** — but make them say *what* it depends
  on. That's the skill.
- **Compare two pairs' `Execute` methods side by side.** Different shapes, both defensible,
  is the most useful thing they'll see all day.
- **Name the trade-off out loud.** A rich domain costs indirection. Someone should say so.
- **Land it at home.** Finish with: "where in our codebase do we do the opposite of this?"
  Write the answers down. That list is worth more than the kata.

---

## Running it again

This is a *kata*, not a workshop. The value is in repetition.

- Same problem, blank page, 2–4 weeks later.
- Second run: add a round 4 constraint from the README.
- Third run: swap facilitators, so someone else has to play the librarian.

Don't let anyone keep their solution between runs.
