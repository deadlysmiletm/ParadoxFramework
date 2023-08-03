# ParadoxFramework
Simple modular Unity library with performance in mind (under construction).

# Content

## Abstract

- Singleton
  - Simple singleton implementation.
  - Automatic creation when is needed (optimized search instance first to avoid double creation).
  - Nullable avoid pattern.
  - Race condition protection.
  - Can be or not a persistant singleton.
  - Can be easily disposed.
  - **Dependencies:**
    - _OptionT_ in **Utilities** folder.

## General

### Pools
- Features shared by all pools implementations
  - Automatic creation when is needed.
  - Simple configuration using Scripbatle objects _(except GenericPool and GenericPoolManager)_
  - Support fill pool after creation.
  - Support indexing to access pools.
  - Support return object with a delay.
  - Support fill pool after creation.
  - Support onFactory and onReturn events for each pool.
  - Support easy pool and manager dispose using TimeSlicing for reduce performance impact.

- Addressables Pool Manager
  - Optimized pool implementation for addressable assets.
  - Support Get asset with a delegate, waiting, task and unsafe (the most quick, but don't create new instances).

- GameObject Pool Manager
  - Optimized pool implementation for game objects. _If using Addressables, use Addressables Pool instead._

- Generic Pool Manager
  - Most optimized pool implementation for advanced users, using _IPoolObject_ interface.
  - Every pool support IEnumerable interface.

- Generic Pool
  - Most optimized pool, using _IPoolObject_ interface for your custom classes.
  - No MonoBehaviour class.
  - Used by _Generic Pool Manager_.

### Managers
- Game Manager
  - Optimized update, fixed update and late update manager for game flow.
  - Lazy creation and dispose of events.
  - Support disposable for every subscription.
  - Simple and easy to use with _IUpdateManaged_, _IFixedUpdateManaged_ and _ILateUpdateManaged_ interfaces.
 
### Project Initializer
- Bootstrapper
  - Optimized autoloading on scene load objects initializer, perfect for managers.
  - Support persistent and not persistent managers.
  - Had _Game Object_ and _Addressables assets_ variations.
 
## Utilities

### Nullable pattern
- Simple Option implementation.
- Avoid multiple nullable comparation.
- Support map, get mapped and iterate value.
- Support create value without check.
- Support create value with a default value (with default value check option).

### Editor Utilities
- Addressables editor utilities
  - Simple utilities to make assets Addressables.
  - Create or find a group.
  - Check if a group exist.
  - Create a new entry with labels.
