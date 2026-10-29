# Concept Factory — Core

`com.conceptfactory.core` is the foundation package for all Concept Factory systems, providing shared utilities, abstractions, and data management tools.

## Included

**Systems**
- `EventBus` — decoupled publish/subscribe messaging between systems
- `CallbackHub` — centralized callback management with automatic cleanup
- `ScreenMonitor` — tracks screen resolution and orientation changes
- `InputMonitor` — unified input polling abstraction

**Addressables**
- `AddressablesManager` — async asset loading and lifetime management on top of Unity Addressables

**Fluids**
- `FluidRegistry` — global registry of known fluid types
- `Fluid` / `FluidMixture` — data types for fluids and their combinations
- `FluidContainer` / `FluidReservoir` — physical containers with volume tracking
- `FluidSettings` — project-wide fluid configuration asset

**Condition / Health**
- `ObjectCondition` — generic health and damage state for any GameObject
- `IHasHealth` / `HealthThresholds` — interface and threshold events for health systems

**Helpers**
- `TSingleton<T>` — MonoBehaviour singleton base class
- `ActionManager` — safe multi-delegate event wrapper
- `ProgressTask` — async operation with progress reporting
- `WebTexture` — downloads and caches remote textures
- `NetworkHelper` — lightweight HTTP utilities
- `JsonExtensions` — convenience wrappers for JsonUtility
- `DiscordUtils` — Discord webhook integration for logging
- `IOExtensions`, `ColorExtensions`, `AsyncOperationExtensions` — general extension methods

**Localization**
- `LocalizationProvider` — thin wrapper over Unity Localization for string lookup

**Custom Drawers / Attributes**
- `PathPicker` — file/folder path picker in the Inspector
- `VectorRange` — range-constrained vector attribute
- `ReadOnly` — marks a serialized field as non-editable in the Inspector

## Requirements

- Unity 6000.0+
- No external dependencies
