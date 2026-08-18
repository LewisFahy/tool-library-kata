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

One 3-hour block. Print this and keep it next to you.

| Clock | Slot | What you're doing |
|---|---|---|
| 0:00 | Framing (15) | Read the domain aloud. Take questions. Check everyone's `dotnet test` runs. |
| 0:15 | Session 1 — interactions (20) | Circulate. Say almost nothing. |
| 0:35 | Debrief 1 (10) | Collect lists on a whiteboard. Agree a shared set. |
| 0:45 | Break (10) | Re-pair during this. |
| 0:55 | Session 2 — build `BorrowTool` (50) | Answer as the librarian. Watch for constraint 3 breaking. |
| 1:45 | Debrief 2 (15) | Two pairs' `Execute` methods on the screen, side by side. |
| 2:00 | Break (10) | |
| 2:10 | Session 3 — curveballs (30) | Drop 3 of the requirements below, ~10 min each. |
| 2:40 | Final debrief (20) | The actual point of the session. |

**Where to take the time from if you overrun:** session 3, then framing. Never the final
debrief — a kata with no debrief is just typing.

**Three hours only gets you through this once**, so don't try to squeeze in a blank-page
re-run. Book the second run for a few weeks later instead; that's where the repetition
value lives.

---

## Session 1 checkpoints

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

Resist the urge to teach here. Let them build `LoanManager` and feel it in session 2.

---

## Session 2 checkpoints

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

If a pair gets there early, don't let them idle — give them a session 3 curveball ahead of
schedule.

---

## Session 3 — the curveballs

Drop these **one at a time**, out loud, to the whole room. In a 30-minute session you'll get
through **three** — pick 1 and 2, then either 3 or 4 depending on where the room is
struggling. Give each one about 10 minutes and cut it off whether or not they're done.

Nobody has to finish the change; the point is to see where it lands. Ask each pair
**"how many files did you have to open?"** rather than whether it works.

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

## The two debriefs

**Debrief 1 (10 min, after session 1)** — purely convergent. Collect the interaction lists
on a whiteboard, merge them, and agree a shared set so everyone starts session 2 from the
same place. Don't philosophise; you need the time later.

**Debrief 2 (15 min, after session 2)** — code on a screen. Put two pairs' `Execute` methods
side by side. Different shapes, both defensible, is the most useful thing they'll see all
day. Ask the authors to read them aloud rather than explain them.

**Final debrief (20 min)** — questions in `README.md`, in that order. Steer towards:

- **"It depends" is the honest answer to most of it** — but make them say *what* it depends
  on. That's the skill.
- **Name the trade-off out loud.** A rich domain costs indirection. Someone should say so.
- **Land it at home.** Finish with: "where in our codebase do we do the opposite of this?"
  Write the answers down. That list is worth more than the kata.

---

## Running it again

This is a *kata*, not a workshop, and three hours only gets you through it once. The value
is in repetition.

- Same problem, blank page, 2–4 weeks later.
- Second run: add a constraint from the README's "If you want more".
- Third run: swap facilitators, so someone else has to play the librarian.

Don't let anyone keep their solution between runs.
