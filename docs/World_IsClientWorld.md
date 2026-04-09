# World.IsClientWorld

## Inner Workings Diagram

```
 World.IsClientWorld() / World.IsServerWorld()
 ══════════════════════════════════════════════════════════════════
 Simplifies checking Netcode client/server/thin-client world states

 PURPOSE:
 ═════════
 Unity Netcode creates separate World instances for client, server,
 and thin-client. Checking WorldFlags directly is verbose and
 error-prone. These extensions provide clean, single-call checks.


 ┌──────────────────────────────────────────────────────────────────┐
 │  EXTENSION METHODS                                               │
 │                                                                  │
 │  World (managed):                                                │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  bool IsThinClientWorld(this World world)                   │  │
 │  │  bool IsClientWorld(this World world)                       │  │
 │  │  bool IsServerWorld(this World world)                       │  │
 │  │  bool IsEditorWorld(this World world)                       │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  WorldUnmanaged:                                                 │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  bool IsThinClientWorld(this WorldUnmanaged world)          │  │
 │  │  bool IsClientWorld(this WorldUnmanaged world)              │  │
 │  │  bool IsServerWorld(this WorldUnmanaged world)              │  │
 │  │  bool IsEditorWorld(this WorldUnmanaged world)              │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 WORLD FLAGS BITFIELD
 ┌──────────────────────────────────────────────────────────────────┐
 │  WorldFlags (bitmask enum)                                       │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Bit  Flag              Meaning                            │  │
 │  │  ───  ────────────────  ──────────────────────────────     │  │
 │  │        Game             Base game world flag                │  │
 │  │        GameClient       Client world (has Game set too)    │  │
 │  │        GameServer       Server world (has Game set too)    │  │
 │  │        GameThinClient   Thin client (subset of GameClient) │  │
 │  │        Editor           Editor world                       │  │
 │  │        Live             Live world (active simulation)     │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  BovineLabs custom flags (from Worlds.cs):                      │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  ServiceWorld = (1 << 16) | Live                            │  │
 │  │  MenuWorld    = (1 << 17) | Live                            │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 IMPLEMENTATION — FLAG CHECK LOGIC
 ┌──────────────────────────────────────────────────────────────────┐
 │                                                                  │
 │  IsThinClientWorld:                                              │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  return (world.Flags & WorldFlags.GameThinClient)           │  │
 │  │       == WorldFlags.GameThinClient;                         │  │
 │  │                                                              │  │
 │  │  Flags:  ....1...  & GameThinClient → true                  │  │
 │  │  Flags:  ....0...  & GameThinClient → false                 │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  IsClientWorld (includes thin client!):                         │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  return (world.Flags & WorldFlags.GameClient)               │  │
 │  │       == WorldFlags.GameClient                              │  │
 │  │       || world.IsThinClientWorld();                         │  │
 │  │                                                              │  │
 │  │  Two separate checks OR'd together:                         │  │
 │  │  1. GameClient flag is set        → true (full client)      │  │
 │  │  2. GameThinClient flag is set    → true (thin client)      │  │
 │  │  Otherwise                        → false                   │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  IsServerWorld:                                                  │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  return (world.Flags & WorldFlags.GameServer)               │  │
 │  │       == WorldFlags.GameServer;                             │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 WORLD TYPES IN MULTIPLAYER SCENARIOS
 ┌──────────────────────────────────────────────────────────────────┐
 │                                                                  │
 │  Dedicated Server:                                               │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  World.Flags = Game | GameServer | Live                     │  │
 │  │                                                              │  │
 │  │  IsClientWorld()     → false                                 │  │
 │  │  IsServerWorld()     → true                                  │  │
 │  │  IsThinClientWorld() → false                                 │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  Full Client (Host or Client):                                   │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  World.Flags = Game | GameClient | Live                     │  │
 │  │                                                              │  │
 │  │  IsClientWorld()     → true                                  │  │
 │  │  IsServerWorld()     → false                                 │  │
 │  │  IsThinClientWorld() → false                                 │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  Thin Client (spectator / headless client):                     │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  World.Flags = Game | GameThinClient | Live                 │  │
 │  │                                                              │  │
 │  │  IsClientWorld()     → true  (thin client IS a client!)     │  │
 │  │  IsServerWorld()     → false                                 │  │
 │  │  IsThinClientWorld() → true                                  │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  BovineLabs Service World:                                       │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  World.Flags = (1<<16) | Live                               │  │
 │  │                                                              │  │
 │  │  IsClientWorld()     → false                                 │  │
 │  │  IsServerWorld()     → false                                 │  │
 │  │  IsServiceWorld()    → true  (from Worlds.cs)               │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 USAGE PATTERN
 ┌──────────────────────────────────────────────────────────────────┐
 │  void OnUpdate(ref SystemState state)                            │
 │  {                                                               │
 │      if (!state.WorldUnmanaged.IsClientWorld())                  │
 │      {                                                           │
 │          // Server-only logic                                    │
 │      }                                                           │
 │                                                                  │
 │      if (state.WorldUnmanaged.IsServerWorld())                   │
 │      {                                                           │
 │          // Server-specific processing                           │
 │      }                                                           │
 │  }                                                               │
 └──────────────────────────────────────────────────────────────────┘

 KEY DETAIL:
 ══════════
 IsClientWorld returns TRUE for thin clients. This is intentional —
 thin clients are clients. Use IsThinClientWorld() when you need
 to distinguish full clients from thin clients.

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Worlds.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Worlds.cs)
- [BovineLabs.Core/Extensions/WorldExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/WorldExtensions.cs)
- [BovineLabs.Core/Utility/WorldUtility.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/WorldUtility.cs)
