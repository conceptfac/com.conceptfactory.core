# com.conceptfactory.core — Agent Guide

Foundation package for all Concept Factory systems. No external dependencies. Every other package depends on this one; this one depends on nothing.

## Package layout

```
Runtime/Scripts/
  Addressables/     AddressablesManager — async load/unload on top of Unity Addressables
  Core/             EventBus, CallbackHub, ScreenMonitor
  Condition/        ObjectCondition, IHasHealth, HealthThresholds
  Drawers/          PathPicker, ReadOnly, VectorRange (Inspector attributes)
  Fluids/           Fluid, FluidMixture, FluidRegistry, FluidContainer, FluidReservoir, FluidSettings
  Helpers/          TSingleton, ActionManager, ProgressTask, WebTexture, NetworkHelper,
                    JsonExtensions, DiscordUtils, IOExtensions, ColorExtensions, ...
  Input/            IInputCallBacks, InputMonitor  ← touch/mobile gestures, NOT gameplay movement
  Localization/     LocalizationProvider
  UI/               AttributeDrawers
Editor/
  GameViewResolutionMonitor
  VectorRangeDrawer
```

## Key design rules

- **Input/ is touch/gesture input** (OnTouch, OnPinching, OnDragging). It is NOT movement input — do not confuse with `IMovementInputProvider` in the Movement package.
- **Fluids and Condition are shared primitives** — both `com.conceptfactory.interactions` (PortableFluidContainer) and `com.conceptfactory.vehicles` (fuel, oil, coolant) build on top of these.
- **TSingleton<T>** is the canonical singleton base. Use it instead of manual static Instance patterns.
- **EventBus** is decoupled pub/sub; **CallbackHub** is for registered callbacks with cleanup. Choose based on whether the subscriber cares about the source.

## Current state (v1.0.2)

`Condition/` and `Fluids/` folders were recently added (untracked in git at last check). Everything else is stable and used in production by all other packages.

## What NOT to add here

Do not add anything that references Unity animation, locomotion, cameras, or specific game systems. Core must remain dependency-free and reusable by any Unity project.
