# Known Issues — drg.utils

> Code review: 2026-05-27. Ordered by severity.

## Compile blockers

- [ ] Fix `AppReviewDialog` Android build — add `using System.Collections;` for `IEnumerator` (`AppReviewDialog.cs:57`)

## Critical

- [ ] Fix `MainThreadDispatcher` — safe first access from background thread (no `GameObject` in static ctor off main thread) (`MainThreadDispatcher.cs:27-32`)

## Major

- [ ] Fix `DebouncedExecutorUnity.Command.Execute` — store and use `framesCooldown` (`DebouncedExecutorUnity.cs:84-90`)
