# Domain

The model. This is where the rules live.

Namespace: `ToolLibrary.Domain`

## What goes here

- **Entities** — things with identity and a lifecycle: `Member`, `Tool`, `Loan`.
- **Value objects** — things defined entirely by their values: `AssetTag`, `Money`,
  `LoanPeriod`, `DueDate`.
- **Domain services** — rules that don't belong to any single entity: `LendingPolicy`.
- **Ports** — the interfaces the domain needs the outside world to satisfy:
  `IMemberRepository`, `IClock`. Declared here, implemented elsewhere.

## What doesn't

- Anything that knows about a database, HTTP, email, or the container.
- DTOs and commands — those belong with the interaction, in `Application`.
- Anything registered in the container. A `Member` is *loaded*, not *resolved*.

## The test

Every rule from the README's rule list should end up in this folder somewhere. When a
requirement changes in session 3, the amount of code you touch outside `Domain` is your
score.

If your entities are just properties with getters and setters, the rules have leaked
upwards into `Application` — that's the anaemic domain model, and it's the single most
common outcome of this kata.
