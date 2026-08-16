# Changelog

## [1.1.0] - 2026-08-17

首个正式公开版本。

### Added

- `[ECS]` / `[NotECS]` 数据布局标记。
- Struct 级默认布局与 Field 级覆盖。
- SoA / AoS Hybrid Storage。
- Managed / Unmanaged 字段混合存储。
- `<Type>Storage`。
- `<Type>Ref`。
- `<Type>ECSList`。
- `<Type>ECSDictionary<TKey>`。
- Direct Column。
- Unsafe Backend。
- SafeSpan Backend。
- SafeRegistry Backend。
- `ECS_FORCE_SAFE_REGISTRY`。
- Editor-only Bounds / Dispose / Ref / Column / Enumerator 生命周期检查。
- Native Allocation Leak Tracking。
- Source Generator Diagnostics：ECS001 ~ ECS004。
- Benchmark Sample。
- Runtime Unit Test。
- Source Generator Test。

### ECSList

- `Add`。
- Indexer。
- Local Ref。
- Direct Column。
- `Insert(index,value)`：O(n)，保持顺序。
- `RemoveAt(index)`：O(n)，保持顺序。
- `RemoveAtSwapBack(index)`：O(1)，不保持顺序。
- `Clear`。
- `Dispose`。
- Unsafe Insert Native 数据移动使用 `Buffer.MemoryCopy`。
- SafeSpan / Managed 数据移动使用 `Array.Copy`。
- Managed 字段在 Remove / Clear / Dispose 时清理尾部引用。

### ECSDictionary

- `Dictionary<TKey,int> + dense ECSList`。
- `Add` / `TryAdd` / `ContainsKey`。
- Indexer。
- `TryGetValue` / `TryGetIndex`。
- `Remove` / `Clear`。
- `Count` / `Capacity` / `Comparer`。
- `getKeyAt` / `getValueAt`。
- `Keys` / `Values`。
- Key + Value foreach。
- Direct Column。
- `Dispose`。
- Remove 使用 dense swap-back，不保证顺序。

### Performance

- Keys Player 路径使用 `ReadOnlySpan<TKey>.Enumerator`。
- Values 使用 Backend-specific fast path。
- SafeSpan `ValueEnumerator` 使用 `ref struct`。
- Dictionary 主 Enumerator 缓存 Storage Handle。
- Entry 延迟读取 Key。
- Unsafe Hybrid Enumerator 同时持有 Native / Managed Storage Handle。
- Ref 支持跨 Resize 保持稳定 Storage。
- Direct Column 针对 Native / Managed Storage 生成不同访问路径。
- Insert / RemoveAt 最终结构移动路径通过 5% Gate。

最终封版环境：

```text
Unity            : 6000.3.21f1
Windows x64 Player
IL2CPP Release
Direct3D 12
NVIDIA GeForce RTX 2060
5955 MB VRAM
32 CPU Threads
```

最终 Runtime Unit Test：

```text
Unsafe   : 59 / 59 PASS
SafeSpan : 59 / 59 PASS
```

结构操作：

```text
Unsafe
Insert Head        0.989x PASS
Insert Middle      0.982x PASS
RemoveAt Head      0.312x PASS
RemoveAt Middle    0.315x PASS
Hybrid Insert      0.922x PASS
Hybrid RemoveAt    0.842x PASS

SafeSpan
Insert Head        1.014x PASS
Insert Middle      1.018x PASS
RemoveAt Head      0.992x PASS
RemoveAt Middle    0.993x PASS
Hybrid Insert      0.927x PASS
Hybrid RemoveAt    0.899x PASS
```

最终 Dictionary Values：

```text
Unsafe
dict.Values        0.395 ns/op
Direct Column      0.364 ns/op

SafeSpan
dict.Values        0.549 ns/op
Direct Column      0.486 ns/op
```

最终混合场景：

```text
Dense full update + 10% random key writes

Unsafe
Dictionary<int,RoleData>  13.003 ms
ECS Direct+LocalRef        0.893 ms
Standard / ECS            14.56x

SafeSpan
Dictionary<int,RoleData>  12.405 ms
ECS Direct+LocalRef        1.021 ms
Standard / ECS            12.15x
```

### Fixed

- 修复早期 `dict.Keys` 自定义 generic foreach 在 IL2CPP 下的数量级性能退化。
- 修复 SafeSpan `dict.Values` Player 路径约 1.79 ms 的异常退化，最终约 0.275 ms / 500000 entities。
- 修复 Player Entry 可见性设计导致的生成代码访问级别问题。
- 修复 Hybrid Storage Resize 后 Ref 稳定性问题。
- 修复 Managed Storage Remove / Clear / Dispose 后引用清理问题。
- 修复早期 Insert / RemoveAt 多列逐元素移动带来的性能退化。
- 修复 Direct Column 方法重名缺少明确 Diagnostic。
- 修复多种 Editor 生命周期 / Enumerator 版本检查边界问题。

### Changed

- 项目定位明确为 **OOP-compatible SoA data layout optimizer**，不是完整 ECS Framework。
- 普通业务优先使用 Ref。
- 简单单字段可直接使用 indexer。
- 极端热点循环使用 Direct Column。
- 大容量容器建议预留 Capacity。
- ECSDictionary 不提供有序 Insert / RemoveAt。
- 最终公开 Benchmark 同时保留 Unsafe 与 SafeSpan 完整封版数据。

### Notes

- 容器不是线程安全的。
- Ref 是位置引用。
- Direct Column 在结构变化后重新获取。
- ECSDictionary Remove 使用 dense swap-back。
- Tail 数纳秒级结构操作不使用百分比作为硬 Gate。
- Release Player 中 `ProfilerRecorder("GC.Alloc")` SelfCheck 无效，因此不把其中的 0 数据宣传为“实测 0 GC”。
- 精确 managed allocation 使用 Development Build + Unity Profiler 验证。
