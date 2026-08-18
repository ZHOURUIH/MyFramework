# Changelog

## 1.2.0 - 2026-08-18

### Added
- 为 `ECSList` 补齐常用 List 风格扩展 API，包括容量管理、批量添加/插入/删除、查找、排序、二分查找、复制与数组转换等。
- 为真实 ECS 字段生成字段级高速 API：`ContainsByXXX`、`IndexOfByXXX`、`LastIndexOfByXXX`、`ExistsByXXX`、`FindIndexByXXX`、`RemoveAllByXXX`、`SortByXXX`、`BinarySearchByXXX`。
- 为 `ECSDictionary<TKey>` 增加字段级直接访问 API：`GetValueByXXX`、`TryGetValueByXXX`、`SetValueByXXX`、`TrySetValueByXXX`。
- 为 `ECSDictionary<TKey>` 增加 DenseIndex API：`GetIndex`、`TryGetIndex`、`GetOrAddIndex`，用于一次 Key 查找后通过 Direct Column 连续访问多个字段。
- 为 `ECSDictionary<TKey>` 补充 `SetValue`、`TrySetValue`、`SetOrAdd`、`GetOrAdd`、`ContainsValue`、`Remove(key,out value)`、`EnsureCapacity`、`TrimExcess` 等接口。
- 增加 Dictionary Keys/Values/KeyValue 枚举支持及对应运行时回归测试。
- 增加 Managed Hybrid、数组批量转换、字段级 Dictionary 快路径、DenseIndex + Direct Column 等回归测试与 Benchmark。

### Changed
- `SortByXXX` 根据数据布局自适应选择排序实现：纯 unmanaged 数据使用 DirectSwap；包含 managed ECS/NotECS 字段时使用 permutation 排序，减少整行反复搬移。
- `BinarySearchByXXX`、字段级查找 API 直接读取目标 Column，避免构造完整结构体。
- `RemoveAllByXXX` 使用字段 Column 判断并同步压缩整行数据。
- `InsertRange`、`RemoveRange` 等结构移动改为 overlap-safe 批量移动路径。
- `T[] -> ECSList`：Managed Hybrid 使用 RowMajor 导入，减少重复读取源结构体。
- `ECSList -> T[]`：Managed Hybrid 使用 CachedDirectDestination，缓存 Column 后直接写目标数组元素，避免先组装局部完整 struct 再复制。
- `ECSDictionary.TryAdd` 改为单次 `Dictionary.TryAdd` 路径，消除 `ContainsKey + Add` 的重复哈希。
- `ECSDictionary.Remove` / `Remove(key,out value)` 使用 `Dictionary.Remove(key,out index)`，消除删除前后的重复 Key 查找。
- Dictionary 单字段热点访问优先走字段级 Direct Column API；多字段热点访问可先取得 DenseIndex，再连续访问多个 Column。
- 保留 Unsafe、SafeSpan、SafeRegistry 三套后端的对应实现，并保持 Editor 生命周期/引用失效检查。

### Performance
- Managed Hybrid `CopyTo` 恢复并略优于优化前基线；局部和全量导出保持稳定线性成本。
- 单独导出 unmanaged/managed Column 已接近或优于普通 `List<T>` 对应字段读取，剩余完整 `CopyTo/ToArray` 差距主要来自 SoA -> AoS 重组成本。
- `ECSDictionary.SetValueByXXX` / `TrySetValueByXXX` 在单字段随机写入场景显著快于普通 `Dictionary<TKey,T>` 的完整 struct 读改写。
- `GetIndex + Direct Column` 在一次 Key 查找后修改多个字段的场景明显优于连续多次 `ByXXX` 调用，并可快于普通 Dictionary 的多字段修改。
- `GetOrAddIndex` 已有 Key 路径接近普通 Dictionary 查询成本；新增 Key 的剩余差距主要属于 IndexMap + Keys + SoA 行创建的结构性成本，不再继续增加复杂实现换取小幅收益。

### Fixed
- 修复 Unsafe 后端 Dictionary 字段级 Direct Column API 缺少 `unsafe` 上下文导致的 `CS0214` 编译错误。
- 修复 Managed Hybrid 数组转换 Generator Test 对 C# 基元类型名称格式的错误断言。
- 修复批量移动在源/目标区间重叠时可能出现的数据覆盖问题。
- 保证 Sort、RemoveAll、Range 操作、Dictionary SwapBack 删除后各 ECS/NotECS 字段始终保持行同步。

### Cleanup
- 移除优化阶段仅用于识别具体实现策略的公开 `*Strategy` 字符串常量，避免把 Benchmark/诊断信息暴露为正式生成 API。
- Generator Test 改为直接验证生成代码结构与行为路径，而不是依赖诊断字符串。
- Runtime Test 保留行为正确性验证，移除对内部策略名称的耦合。
- Benchmark 移除阶段性策略名称日志，保留最终性能回归用例。

### Final usage guidance
- 普通业务代码：使用 List/Dictionary 风格兼容 API。
- 单字段热点访问：优先使用 `ByXXX` 字段级 API。
- 多字段随机热点访问：优先 `GetIndex` / `TryGetIndex` / `GetOrAddIndex` + Direct Column。
- 连续遍历热点：优先 Direct Column。
- `CopyTo` / `ToArray` 作为兼容 API 使用；完整 SoA -> AoS 转换存在不可避免的重组成本。
- DenseIndex 只应短期使用；Dictionary 发生 SwapBack 删除后，其他元素的 DenseIndex 可能变化。

## [1.1.1] - 2026-08-17
### Added
生成的ECSList类名上方添加记录原结构体类型,方便跳转

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
