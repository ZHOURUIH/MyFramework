# Changelog

## [1.1.0] - 2026-08-16

Initial public release.

### Added

- `[ECS]` and `[NotECS]` data layout attributes.
- Struct-level default layout with field-level override support.
- Generated SoA / AoS hybrid storage.
- Generated `<Type>ECSList` with indexer, Ref and Direct Column access.
- Generated `<Type>ECSDictionary<TKey>` with dense value storage.
- Dictionary `Add`, `TryAdd`, `ContainsKey`, `TryGetValue`, `TryGetIndex`, `Remove`, `Clear`, `Keys`, `Values` and foreach support.
- Unsafe Backend for unmanaged structs when Allow Unsafe Code is enabled.
- SafeSpan Backend for safe high-performance managed storage.
- SafeRegistry compatibility Backend for environments without Span.
- `ECS_FORCE_SAFE_REGISTRY` compile symbol for forcing the compatibility Backend.
- Managed-field support with automatic non-Unsafe Backend selection.
- Editor-only Ref, Column, Enumerator, bounds and lifecycle validation.
- Unsafe native allocation leak tracking in Editor.
- Source Generator diagnostics for unsupported or conflicting declarations.
- Runtime List / Dictionary tests and performance Benchmark sample.
- Android ARM64 + IL2CPP runtime validation for Unsafe, SafeSpan and SafeRegistry paths.

### Notes

- Containers are not thread safe.
- Ref is a position reference, not a persistent entity identity handle.
- Direct Column becomes invalid after structural changes.
- Dictionary Remove uses dense swap-back and does not preserve order.
