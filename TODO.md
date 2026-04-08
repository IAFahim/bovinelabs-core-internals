# TODO.md — Fact-Check Progress Tracker

## Category Progress

| # | Category | Total | Done | Status |
|---|----------|-------|------|--------|
| 1 | Core Collections | 27 | 27 | ✅ Complete (700 assertions, 0 failures) |
| 2 | Blob System | 23 | 0 | ⬜ Not started |
| 3 | Memory & Allocators | 12 | 0 | ⬜ Not started |
| 4 | Dynamic Buffers | 10 | 0 | ⬜ Not started |
| 5 | ECS Extensions | 34 | 0 | ⬜ Not started |
| 6 | Jobs & Threading | 6 | 0 | ⬜ Not started |
| 7 | State & Model | 13 | 0 | ⬜ Not started |
| 8 | Spatial & Physics | 20 | 0 | ⬜ Not started |
| 9 | Utility | 24 | 1 | 🟡 Started (ButtonEvent) |
| 10 | Extension Methods | 15 | 0 | ⬜ Not started |
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

**Total: 357 topics, 28 completed (27 core-collections + 1 utility)**

## Doc Inaccuracies Found

1. **NativeThreadStream.md**: UnsafeThreadStreamRange = 48 bytes, not 40
2. **NativeLinearCongruentialGenerator.md**: Example table values wrong for seed 42
3. **NativeCounter.md**: Struct = 32 bytes not ~10; constructor takes AllocatorHandle not Allocator
4. **UnsafeArray.md**: No 2-param (int, Allocator) constructor, only 3-param
5. **NativeParallelMultiHashMapExtensions_GetUniqueKeyArray.md**: Method throws NotImplementedException at runtime

## Session Log

- Session 1: Project setup, minimal branch, Unity launched, 27 core-collections + ButtonEvent snippets, all docs updated with verified data, 700 assertions passing
