# DRG Utils

Unity utilities: debounced executor, main-thread dispatcher, app review dialog.

## Assemblies

| Assembly | Contains |
|---|---|
| `DRG.Utils` | `IDebouncedExecutor`, `IAppReviewDialog` |
| `DRG.Utils.Runtime` | `MainThreadDispatcher`, `MainThreadDispatcherAdapter`, `AppReviewDialog`, `AppReviewDialogProxy`, `StaticMonoBehaviour` |

## Key types

- **`IDebouncedExecutor`** — runs an action with a frame cooldown. Prevents the same operation from firing multiple times per burst. Returns an `ICommand` handle to cancel.
- **`MainThreadDispatcher`** — static queue for dispatching work onto Unity's main thread from any thread. Backed by a persistent MonoBehaviour.
- **`MainThreadDispatcherAdapter`** — `IMainThreadDispatcher` implementation wrapping `MainThreadDispatcher`. Register via `IServiceLocator`.
- **`AppReviewDialog`** — cross-platform in-app review (iOS `RequestStoreReview`, Android Play Review API).
- **`AppReviewDialogProxy`** — routes to the real dialog on device, and to `EditorAppReviewDialog` in the Editor.

## Dependencies

- `com.drg.core`

## Install

```
https://github.com/yanmasharski/drg.utils.git#1.0.0
```
