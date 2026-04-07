# HSV — Inner Workings

## Overview

HSV is a Burst-compatible struct that converts HSV (Hue, Saturation, Value) color
values to RGB Color using pure mathematics — no Unity Color API calls, making it
suitable for use inside Burst-compiled jobs.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        HSV (readonly struct)                                │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │  float H   (Hue:        0–360°)                                       │  │
│  │  float S   (Saturation: 0–1)                                          │  │
│  │  float V   (Value:      0–1)                                          │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  ToColor() → converts HSV → RGB using sector-based math                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

## HSV to RGB Conversion Algorithm

```
  Input: H=240, S=0.8, V=0.9
         │
         ▼
  ┌─────────────────────────────────────────────────────────────────────┐
  │  Step 1: Chroma calculation                                          │
  │    c = V * S = 0.9 * 0.8 = 0.72                                    │
  │                                                                     │
  │  Step 2: Sector mapping                                             │
  │    hh = H / 60 = 240 / 60 = 4.0                                    │
  │                                                                     │
  │  Step 3: Intermediate value                                         │
  │    x = c * (1 - |hh % 2 - 1|)                                      │
  │      = 0.72 * (1 - |4.0 % 2 - 1|)                                  │
  │      = 0.72 * (1 - |0.0 - 1|)                                      │
  │      = 0.72 * (1 - 1) = 0.0                                        │
  │                                                                     │
  │  Step 4: Match value                                                │
  │    m = V - c = 0.9 - 0.72 = 0.18                                   │
  │                                                                     │
  │  Step 5: Sector selection (H < 240)                                 │
  │    R = m + m = 0.18   (m)                                          │
  │    G = x + m = 0.0 + 0.18 = 0.18  (x+m)                           │
  │    B = c + m = 0.72 + 0.18 = 0.90  (c+m)                          │
  └─────────────────────────────────────────────────────────────────────┘
```

## Color Sector Table

```
  The hue wheel is divided into six 60° sectors:
  ┌────────────────────────────────────────────────────────────────────┐
  │                                                                    │
  │  H range    Sector      R       G       B                         │
  │  ─────────  ──────────  ──────  ──────  ──────                    │
  │   0 - 60    Red-Yellow  c+m     x+m      m                        │
  │  60 - 120   Yellow-Grn  x+m     c+m      m                        │
  │ 120 - 180   Green-Cyan   m      c+m     x+m                       │
  │ 180 - 240   Cyan-Blue    m      x+m     c+m                       │
  │ 240 - 300   Blue-Mag.   x+m      m      c+m  ← default (else)    │
  │ 300 - 360   Mag.-Red    c+m      m      x+m                       │
  │                                                                    │
  │  Note: The switch in code uses < 60, < 120, < 180, < 240, else    │
  │  which covers 240-360 as the default case.                         │
  └────────────────────────────────────────────────────────────────────┘

  Visual hue wheel:
         0° Red
          │
  300°  \ │ /  60°
  Mag.───●───Yellow
  240°  / │ \  120°
         │
        180° Cyan
```

## Construction Validation

```
  new HSV(h, s, v)
         │
         ▼
  ┌───────────────────────────────────────────────────────────────┐
  │  H = clamp(h, 0, 360)                                       │
  │  if |H - 360| < EPSILON: H = 0  // normalize 360° → 0°      │
  │                                                               │
  │  S = clamp(s, 0, 1)                                          │
  │  V = clamp(v, 0, 1)                                          │
  │                                                               │
  │  All values are clamped to valid ranges.                      │
  │  Note: readonly struct — values can't change after construct. │
  └───────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Pure math**: Uses only `math.clamp`, `math.abs`, basic arithmetic — fully
  Burst-compatible with no managed dependencies.
- **Switch-based sectors**: Simple range check via `switch` expression is more
  Burst-friendly than lookup tables or conditional chains.
- **360 normalization**: Edge case where H=360 (same as H=0) is explicitly handled.
- **No alpha channel**: Returns `Color` (which includes alpha=1 by default from
  the 3-argument Color constructor).

## Source File

- `BovineLabs.Core/Utility/HSV.cs`

## Source

- [BovineLabs.Core/Utility/HSV.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/HSV.cs)
