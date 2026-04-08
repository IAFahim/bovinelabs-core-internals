# TODO.md — Fact-Check Progress Tracker

## Category Progress

| # | Category | Total | Done | Status |
|---|----------|-------|------|--------|
| 1 | Core Collections | 28 | 23 | 🟡 In progress (23 snippets written, 5 remaining extension methods) |
| 2 | Blob System | 23 | 0 | ⬜ Not started |
| 3 | Memory & Allocators | 12 | 0 | ⬜ Not started |
| 4 | Dynamic Buffers | 10 | 0 | ⬜ Not started |
| 5 | ECS Extensions | 34 | 0 | ⬜ Not started |
| 6 | Jobs & Threading | 6 | 0 | ⬜ Not started |
| 7 | State & Model | 13 | 0 | ⬜ Not started |
| 8 | Spatial & Physics | 20 | 0 | ⬜ Not started |
| 9 | Utility | 24 | 1 | 🟡 Started (ButtonEvent) |
| 10 | Extension Methods | 15 | 2 | 🟡 Started (GetOrAddRef, ClearAndAddBatchUnsafe) |
| 11 | ConfigVars | 13 | 0 | ⬜ Not started |
| 12 | Authoring & Baking | 11 | 0 | ⬜ Not started |
| 13 | Editor Tools | 31 | 0 | ⬜ Not started |
| 14 | Source Generators | 20 | 0 | ⬜ Not started |
| 15 | SubScene System | 19 | 0 | ⬜ Not started |
| 16 | Pause & Time | 7 | 0 | ⬜ Not started |
| 17 | Relevancy & Netcode | 6 | 0 | ⬜ Not started |
| 18 | Singleton System | 8 | 0 | ⬜ Not started |
| 19 | Object Management | 13 | 0 | ⬜ Not started |
| 20 | Physics States | 3 | 0 | ⬜ Not started |
| 21 | Life Cycle | 11 | 0 | ⬜ Not started |
| 22 | Tests & Diagnostics | 5 | 0 | ⬜ Not started |
| 23 | Math Extensions | 11 | 0 | ⬜ Not started |
| 24 | Other | 34 | 0 | ⬜ Not started |

**Total: 357 topics, 26 snippets written, 647 assertions passing**

## Doc Inaccuracies Found

1. **NativeThreadStream.md**: UnsafeThreadStreamRange claimed as 40 bytes, actual is 48 bytes
2. **NativeLinearCongruentialGenerator.md**: Example table has wrong output values (seed 42 sequence is incorrect)
3. **NativeCounter.md**: Constructor param doc says `Allocator` enum, actual is `AllocatorManager.AllocatorHandle`
4. **NativeCounter.md**: Struct size doc says ~10 bytes, actual is 32 (includes safety handles)
5. **ThreadRandom.md**: Randoms struct layout — needs verification of StructLayout attribute presence

## Session Log

- Session 1: Project setup, minimal branch, Unity launched, 23 core-collections snippets + ButtonEvent = 647 assertions passing
