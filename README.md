```
╔══════════════════════════════════════════════════════════════════════════════╗
║     mathex.GenerateGaussianNoise — Box-Muller Transform in Burst            ║
║              Source: BovineLabs.Core/Utility/mathex.cs                       ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  Generates pairs of normally-distributed (Gaussian) random numbers
  using the Box-Muller transform. Fully Burst-compiled.

  Returns TWO independent samples per call — useful for generating
  noise fields, particle velocities, etc.

SIGNATURE
═════════

  (float z0, float z1) GenerateGaussianNoise(ref Random random, float mu, float sigma)

  ● mu    = mean (center of distribution)
  ● sigma = standard deviation (spread)
  ● z0, z1 = two independent N(mu, sigma²) samples

BOX-MULLER TRANSFORM — ALGORITHM
════════════════════════════════

  The Box-Muller transform converts two uniform random numbers U(0,1)
  into two standard normal random numbers N(0,1).

  ┌──────────────────────────────────────────────────────────────────┐
  │  Step 1: Generate two uniform samples                           │
  │                                                                  │
  │    u1 = random.NextFloat()    ← U(0, 1), reject if exactly 0    │
  │    u2 = random.NextFloat()    ← U(0, 1)                         │
  │                                                                  │
  │    ┌───────┐     ┌───────┐                                      │
  │    │  u1   │     │  u2   │     both uniform on (0, 1]           │
  │    └───┬───┘     └───┬───┘                                      │
  │        │             │                                          │
  │  Step 2: Compute polar coordinates                              │
  │        │             │                                          │
  │        ▼             │                                          │
  │    R = sigma × √(-2 × ln(u1))    ← Rayleigh distribution       │
  │        │             │                                          │
  │        │             ▼                                          │
  │        │         theta = 2π × u2    ← uniform angle             │
  │        │             │                                          │
  │        ▼             ▼                                          │
  │  Step 3: Convert to Cartesian                                   │
  │                                                                  │
  │    z0 = R × cos(theta) + mu                                     │
  │    z1 = R × sin(theta) + mu                                     │
  └──────────────────────────────────────────────────────────────────┘

GEOMETRIC INTERPRETATION
════════════════════════

  The transform maps a point in polar coordinates (R, theta) —
  derived from uniform samples — to Cartesian coordinates that
  follow a Gaussian distribution:

                    │ z1
                    │
             z1 ──● │          The point (z0, z1) is Gaussian-
                 ╱  │          distributed because R follows a
               ╱    │          Rayleigh distribution and theta
             ╱      │          is uniform on [0, 2π).
           ╱   ·    │
         ╱ ·     ·  │
       ──·─────────·─┼─────────·── z0
         ╲ ·     ·  │
           ╲   ·    │
             ╲      │
               ╲    │
                 ╲  │
                    │

  Each point (z0, z1) is an independent draw from N(mu, sigma²).

NUMBER LINE TRACE
══════════════════

  Input:  u1 ∈ (0,1]  (never 0 — would give ln(0) = -∞)
          u2 ∈ (0,1]

        u1 ──▶ ln(u1) ──▶ ×(-2) ──▶ √() ──▶ × sigma ──▶ R
                                                         │
        u2 ──▶ × 2π ──▶ theta                          │
                                    │                    │
                                    ▼                    ▼
                              cos(theta) × R + mu ──▶ z0
                              sin(theta) × R + mu ──▶ z1

ZERO-GUARD DETAIL
═════════════════

  ┌──────────────────────────────────────────────────────────────────┐
  │  do { u1 = random.NextFloat(); }                                │
  │  while (Hint.Unlikely(u1 == 0));                                │
  │                                                                  │
  │  ● Hint.Unlikely tells Burst this branch is rarely taken         │
  │  ● Prevents ln(0) → -∞ which would produce NaN                  │
  │  ● u1 = 0 has probability ~1/2^23 (float32 mantissa)            │
  │    so this loop almost never repeats                             │
  └──────────────────────────────────────────────────────────────────┘

RELATED: POLAR METHOD (NormalDistribution)
═══════════════════════════════════════════

  The class also implements a POLAR rejection method for single samples:

  ┌──────────────────────────────────────────────────────────────────┐
  │  PolarTransform(float a, float b):                               │
  │                                                                  │
  │    v1 = 2a - 1     ← map (0,1) to (-1,1)                        │
  │    v2 = 2b - 1     ← map (0,1) to (-1,1)                        │
  │    r  = v1² + v2²                                              │
  │                                                                  │
  │    if r >= 1 or r == 0: REJECT → try again                      │
  │                                                                  │
  │    fac = √(-2 × ln(r) / r)                                      │
  │    x = v1 × fac                                                  │
  │    y = v2 × fac                                                  │
  │                                                                  │
  │  ● Rejects points outside the unit circle                        │
  │  ● Acceptance rate = π/4 ≈ 78.5%                                │
  │  ● Produces 2 samples per acceptance                             │
  └──────────────────────────────────────────────────────────────────┘

  Unit Circle Rejection:
          ┌───────────┐
          │  ╱ ╲      │
          │ ╱ ● ╲     │  ← accept if inside circle
          │╱     ╲    │     reject if in corners
          │╲     ╱    │
          │ ╲   ╱     │
          │  ╲ ╱      │
          └───────────┘
            square [-1,1]²

USAGE IN BURST JOBS
════════════════════

  [BurstCompile]
  struct NoiseJob : IJobParallelFor
  {
      public NativeArray<float> output;
      public Unity.Mathematics.Random random;

      void IJobParallelFor.Execute(int index)
      {
          var (z0, z1) = mathex.GenerateGaussianNoise(
              ref random, mu: 0f, sigma: 1f);
          output[index * 2]     = z0;
          output[index * 2 + 1] = z1;
      }
  }

DISTRIBUTION OUTPUT
═══════════════════

  For mu=0, sigma=1 (standard normal):

       ▲ density
       │ ██
       │ ████
       │ ██████
       │ ████████
       │ ██████████
       │ ████████████
       │ ██████████████
  ─────┼─────────────────────▶ value
      -3  -2  -1   0   1   2   3
       │   σ   σ   │   σ   σ
       └───68%─────┘
       └────── 95% ──────┘
       └───────── 99.7% ─────────┘
```
