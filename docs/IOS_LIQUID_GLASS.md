# iOS Liquid Glass — production pass (1.12.4)

Built: 2026-08-03  
Client: `MauverseIOS` (SwiftUI), deployment iOS 18+, glass API gated iOS 26+

## Intent

Ship a coherent Liquid Glass campus app: translucent materials everywhere cards live, intentional motion, no matte-on-glass bugs, Reduce Transparency / Reduce Motion respected.

## Contract

| API | Meaning |
| --- | --- |
| `mauGlass(radius:style:)` | Primary glass. iOS 26 → `glassEffect(.regular)`; older → materials; Reduce Transparency → solid card |
| `mauSurface` | Alias of **thin glass** (not opaque fill) |
| `MauGlassStack` | `GlassEffectContainer` on iOS 26 for sibling morph |
| `mauPressable()` | Scale/opacity press spring |
| `MauMotion` | `snappy` / `soft` / `press` / `orb` / `pulse` |
| `MauBackground` | Canvas + drifting cyan/navy orbs (orb motion off if Reduce Motion) |

Styles: `.regular` (cards), `.thin` (secondary / fields), `.interactive` (tappable tiles).

## What changed in 1.12.4

- Redesigned `DesignSystem.swift` (materials, a11y, motion tokens, press, glass stack, pulsing glass skeletons).
- Home: glass quick actions in `MauGlassStack`, press, skeleton, soft tab switches.
- Schedule: glass date/filter chips + lesson cards, skeleton loaders, list transitions.
- News: fixed double-layer card (opaque under glass); hero glass; animated filter chips.
- Services: interactive glass tiles.
- Login: glass form + fields + error appear.
- Campus search glass; events HTML cleaned; contacts toast stroked.
- RootTab: selection haptic + soft animation.
- Brand gradient: less purple, more Arctic cyan/navy.

## Smoke (device, before production)

1. iOS 26: glass on Home / Schedule / News / Services; tab bar system glass.
2. Dark mode + Reduce Transparency (solids, no broken strokes).
3. Reduce Motion (no orb drift / press scale still subtle).
4. Login → Home → each tab; news filters; schedule day chips.
5. Dynamic Type large; VoiceOver on quick cards.

## Follow-ups (post-ship if needed)

- Custom glass Form for certificate / profile editor.
- Departments off system `List`.
- Matched-geometry date chip pill.
