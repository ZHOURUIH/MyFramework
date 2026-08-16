# EasyECS

**Version 1.1.0**

EasyECS 是 `MyFramework` 仓库中的独立 Unity Package：

```text
Packages/com.zhourui.easyecs
```

定位：

> **OOP-compatible SoA data layout optimizer for Unity**

它不是完整 ECS Framework。业务层仍然使用普通 Struct、List 风格和 Dictionary 风格 API，Source Generator 负责生成 SoA / AoS Hybrid Storage、Ref、ECSList、ECSDictionary 与 Direct Column。

# 安装

GitHub：

```text
https://github.com/ZHOURUIH/MyFramework.git?path=/Packages/com.zhourui.easyecs
```

Gitee：

```text
https://gitee.com/inothingtodo/MyFramework.git?path=/Packages/com.zhourui.easyecs
```

Package Name：

```text
com.zhourui.easyecs
```

# 快速开始

```csharp
using EasyECS;

[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	public float mPositionX;
	public float mPositionY;

	[NotECS] public int mID;
	[NotECS] public int mModelID;
	[NotECS] public int mCamp;
}
```

```csharp
RoleDataECSList roles = new RoleDataECSList(1024);

roles.Add(new RoleData
{
	mHP = 100,
	mSpeed = 5.0f,
	mID = 1001,
});

RoleDataRef role = roles[0];
role.mHP -= 10;
role.mPositionX += role.mSpeed;
```

# 布局规则

```text
Struct [ECS]
→ Field 默认 SoA
→ [NotECS] 切回 AoS

Struct [NotECS]
→ Field 默认 AoS
→ [ECS] 切到 SoA
```

# Hybrid Storage

```text
Unmanaged ECS Field  → Native SoA
Managed ECS Field    → Managed SoA Array
AoS 全 unmanaged      → Native AoS
AoS 含 managed        → Managed AoS Array
```

managed 字段不会强迫整个 Struct 放弃 Unsafe。

# Backend

```text
ECS_FORCE_SAFE_REGISTRY
→ SafeRegistry

Allow Unsafe Code=true 且存在 Native Storage
→ Unsafe

否则支持 Span<T>
→ SafeSpan

否则
→ SafeRegistry
```

# ECSList

```text
Add
Insert
RemoveAt
RemoveAtSwapBack
Clear
Indexer
Direct Column
Dispose
```

```text
Add                 O(1) amortized
Insert              O(n), ordered
RemoveAt            O(n), ordered
RemoveAtSwapBack    O(1), unordered
```

# ECSDictionary

```text
Dictionary<TKey,int>
        ↓
    dense index
        ↓
<Type>ECSList
```

支持：

```text
Add
TryAdd
ContainsKey
Indexer
TryGetValue
TryGetIndex
Remove
Clear
Count
Capacity
Comparer
getKeyAt
getValueAt
Keys
Values
foreach
Direct Column
Dispose
```

`Remove` 使用 dense swap-back，不保证顺序。

# 访问建议

```text
单字段
→ list[i] / dict[key]

多字段
→ Local Ref

重复读取同字段
→ Local Ref + 局部变量缓存

极端热点
→ Direct Column
```

# 生命周期

Ref 是位置引用，不是永久实体身份句柄。

结构变化后需要遵守：

```text
Resize
→ Ref 可保持有效

Insert / RemoveAt
→ 受移动区间影响的旧位置引用需要重新获取

SwapBack / Dictionary Remove
→ 被删除 / 被搬移位置需要重新获取

Clear / Dispose
→ 所有旧 Ref 失效
```

Direct Column 在 Add / Insert / Remove / Clear / Resize / Dispose 后重新获取。

# Diagnostics

| Code | 说明 |
|---|---|
| `ECS001` | ECS / NotECS 冲突 |
| `ECS002` | 不支持的 ECS 类型 |
| `ECS003` | 不支持的字段 |
| `ECS004` | Column 方法名冲突 |

# Benchmark Sample

```text
EasyECS/Import Benchmark Sample
```

包含：

```text
RoleDataBenchmark
RoleDataDictionaryBenchmark
RoleDataDictionaryEnumeratorBenchmark
RoleDataListStructuralBenchmark
EasyECSRuntimeUnitTest
```

# 📈 完整 Benchmark

以下数据全部来自 **1.1.0 封版前最后两轮 Windows x64 IL2CPP Release Player 实测日志**，分别验证 Unsafe 与 SafeSpan。

测试环境：

```text
Unity            : 6000.3.21f1
Platform         : Windows x64 Player
Scripting Backend: IL2CPP
Build            : Release
Graphics API     : Direct3D 12
GPU              : NVIDIA GeForce RTX 2060
VRAM             : 5955 MB
CPU Threads      : 32

EntityCount      : 500000
SampleCount      : 15
WarmupCount      : 3
RandomWriteCount : 50000
```

表格统一显示：

```text
Median 总耗时 / 单 entity(or op) 耗时
```

`Min / Max` 保留在原始日志中用于判断系统波动，README 以 Median 作为横向比较指标。

> 微基准用于比较同一机器、同一构建、同一循环下的访问路径，不应直接外推成所有项目中的绝对性能。

### ECSList：修改 1 个字段

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `List<RoleData>` | 3.973 ms / 7.946 ns/entity | 3.805 ms / 7.610 ns/entity |
| `RoleData[]` | 0.294 ms / 0.588 ns/entity | 0.285 ms / 0.570 ns/entity |
| `ECS list[i]` | 0.243 ms / 0.486 ns/entity | 0.357 ms / 0.713 ns/entity |
| `ECS Ref` | 0.244 ms / 0.488 ns/entity | 0.356 ms / 0.713 ns/entity |
| `ECS Direct` | 0.160 ms / 0.320 ns/entity | 0.183 ms / 0.366 ns/entity |
### ECSList：访问 2 个字段

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `List<RoleData>` | 3.824 ms / 7.648 ns/entity | 3.760 ms / 7.520 ns/entity |
| `RoleData[]` | 0.280 ms / 0.559 ns/entity | 0.278 ms / 0.556 ns/entity |
| `ECS list[i]` | 0.354 ms / 0.708 ns/entity | 0.619 ms / 1.237 ns/entity |
| `ECS Ref` | 0.267 ms / 0.534 ns/entity | 0.444 ms / 0.888 ns/entity |
| `ECS Direct` | 0.118 ms / 0.236 ns/entity | 0.220 ms / 0.440 ns/entity |
### ECSList：访问 4 个字段

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `List<RoleData>` | 2.144 ms / 4.288 ns/entity | 2.139 ms / 4.278 ns/entity |
| `RoleData[]` | 0.543 ms / 1.087 ns/entity | 0.458 ms / 0.917 ns/entity |
| `ECS list[i]` | 0.859 ms / 1.718 ns/entity | 1.416 ms / 2.832 ns/entity |
| `ECS Ref` | 0.575 ms / 1.150 ns/entity | 0.881 ms / 1.762 ns/entity |
| `ECS Direct` | 0.309 ms / 0.617 ns/entity | 0.485 ms / 0.969 ns/entity |
### 4 字段路径拆解：仅读

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `RoleData[] AoS` | 0.704 ms / 1.408 ns/entity | 0.704 ms / 1.408 ns/entity |
| `Raw SoA arrays` | 0.704 ms / 1.407 ns/entity | 0.703 ms / 1.407 ns/entity |
| `ECS repeated list[i]` | 0.705 ms / 1.411 ns/entity | 1.145 ms / 2.290 ns/entity |
| `ECS Local Ref` | 0.715 ms / 1.430 ns/entity | 0.744 ms / 1.488 ns/entity |
| `ECS Direct` | 0.716 ms / 1.431 ns/entity | 0.705 ms / 1.409 ns/entity |
### 4 字段路径拆解：读写

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `RoleData[] AoS` | 0.546 ms / 1.092 ns/entity | 0.456 ms / 0.912 ns/entity |
| `Raw SoA arrays` | 0.473 ms / 0.946 ns/entity | 0.449 ms / 0.899 ns/entity |
| `ECS repeated list[i]` | 0.860 ms / 1.720 ns/entity | 1.417 ms / 2.833 ns/entity |
| `ECS Local Ref` | 0.575 ms / 1.150 ns/entity | 0.882 ms / 1.764 ns/entity |
| `ECS Ref cache speed` | 0.498 ms / 0.997 ns/entity | 0.762 ms / 1.525 ns/entity |
| `ECS Direct` | 0.308 ms / 0.617 ns/entity | 0.484 ms / 0.969 ns/entity |
### ECSDictionary：随机 Key 读取

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `随机读取 Dictionary<int,RoleData>` | 9.690 ms / 19.380 ns/op | 7.880 ms / 15.760 ns/op |
| `随机读取 IndexMap + RoleData[]` | 22.459 ms / 44.918 ns/op | 11.818 ms / 23.636 ns/op |
| `随机读取 IndexMap + int[]` | 7.040 ms / 14.080 ns/op | 7.361 ms / 14.722 ns/op |
| `随机读取 ECS Inline Indexer` | 8.031 ms / 16.062 ns/op | 8.213 ms / 16.425 ns/op |
| `随机读取 ECS Local Ref` | 7.929 ms / 15.858 ns/op | 8.083 ms / 16.166 ns/op |
| `随机读取 ECS TryGetValue` | 7.960 ms / 15.920 ns/op | 8.355 ms / 16.709 ns/op |
### ECSDictionary：随机 Key 修改

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `随机修改 Dictionary<int,RoleData>` | 1.238 ms / 24.754 ns/op | 1.236 ms / 24.722 ns/op |
| `随机修改 IndexMap + RoleData[]` | 0.792 ms / 15.832 ns/op | 0.802 ms / 16.044 ns/op |
| `随机修改 IndexMap + int[]` | 0.680 ms / 13.596 ns/op | 0.696 ms / 13.926 ns/op |
| `随机修改 ECS Inline Indexer` | 0.770 ms / 15.404 ns/op | 0.789 ms / 15.772 ns/op |
| `随机修改 ECS Local Ref` | 0.791 ms / 15.826 ns/op | 0.785 ms / 15.708 ns/op |
| `随机修改 ECS TryGetValue` | 0.749 ms / 14.980 ns/op | 0.763 ms / 15.254 ns/op |
### TryGetValue / TryGetIndex 写入路径拆解

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `TryGetValue only` | 0.547 ms / 10.930 ns/op | 0.577 ms / 11.548 ns/op |
| `TryGetValue + read` | 0.731 ms / 14.612 ns/op | 0.714 ms / 14.274 ns/op |
| `TryGetValue + write` | 0.759 ms / 15.186 ns/op | 0.788 ms / 15.768 ns/op |
| `TryGetIndex only` | 0.601 ms / 12.024 ns/op | 0.573 ms / 11.462 ns/op |
| `TryGetIndex + Ref write` | 0.789 ms / 15.780 ns/op | 1.273 ms / 25.454 ns/op |
| `TryGetIndex + Direct write` | 0.742 ms / 14.848 ns/op | 0.744 ms / 14.874 ns/op |
| `Indexer write` | 0.728 ms / 14.564 ns/op | 0.811 ms / 16.218 ns/op |
| `Local Ref write` | 0.725 ms / 14.500 ns/op | 0.814 ms / 16.276 ns/op |
### ECSDictionary：Dense 全量修改 1 个字段

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `Dictionary Key全量更新` | 10.529 ms / 21.057 ns/op | 10.224 ms / 20.449 ns/op |
| `Dense RoleData[]` | 0.296 ms / 0.592 ns/op | 0.278 ms / 0.556 ns/op |
| `Dense int[]` | 0.143 ms / 0.286 ns/op | 0.143 ms / 0.286 ns/op |
| `ECS Dense Ref` | 0.360 ms / 0.721 ns/op | 0.447 ms / 0.894 ns/op |
| `ECS Direct` | 0.176 ms / 0.352 ns/op | 0.237 ms / 0.474 ns/op |
### ECSDictionary：Dense 全量访问 4 个字段

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `Dictionary Key全量更新` | 6.811 ms / 13.623 ns/op | 6.720 ms / 13.440 ns/op |
| `Dense RoleData[]` | 0.414 ms / 0.828 ns/op | 0.402 ms / 0.803 ns/op |
| `ECS Dense Ref` | 0.712 ms / 1.424 ns/op | 1.097 ms / 2.194 ns/op |
| `ECS Direct` | 0.310 ms / 0.620 ns/op | 0.603 ms / 1.205 ns/op |
### 混合场景：Dense 全量更新 + 10% 随机 Key 修改

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `Dictionary<int,RoleData>` | 13.003 ms / 23.642 ns/op | 12.405 ms / 22.555 ns/op |
| `IndexMap + RoleData[]` | 1.348 ms / 2.451 ns/op | 1.250 ms / 2.273 ns/op |
| `ECS Direct+LocalRef` | 0.893 ms / 1.624 ns/op | 1.021 ms / 1.857 ns/op |
### Dictionary 遍历：仅 Key

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `for + getKeyAt` | 0.176 ms / 0.352 ns/op | 0.177 ms / 0.353 ns/op |
| `foreach dict + item.Key` | 0.180 ms / 0.360 ns/op | 0.309 ms / 0.618 ns/op |
| `foreach dict.Keys` | 0.188 ms / 0.377 ns/op | 0.180 ms / 0.361 ns/op |
### Dictionary 遍历：仅 Value 并修改 1 个字段

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `for + getValueAt` | 0.361 ms / 0.723 ns/op | 0.449 ms / 0.897 ns/op |
| `foreach dict + item.Value` | 0.206 ms / 0.411 ns/op | 0.319 ms / 0.638 ns/op |
| `foreach dict.Values` | 0.198 ms / 0.395 ns/op | 0.275 ms / 0.549 ns/op |
| `Direct Column` | 0.182 ms / 0.364 ns/op | 0.243 ms / 0.486 ns/op |
### Dictionary 遍历：Key + Value

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `for getKeyAt+getValueAt` | 0.305 ms / 0.609 ns/op | 0.529 ms / 1.059 ns/op |
| `foreach dict` | 0.271 ms / 0.541 ns/op | 0.410 ms / 0.820 ns/op |
### Enumerator 细测：仅读取 Key

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `int[] for` | 0.117 ms / 0.235 ns/op | 0.117 ms / 0.235 ns/op |
| `int[] foreach` | 0.118 ms / 0.235 ns/op | 0.118 ms / 0.235 ns/op |
| `ReadOnlySpan<int> foreach` | 0.092 ms / 0.183 ns/op | 0.090 ms / 0.180 ns/op |
| `ReadOnlySpan<int> manual` | 0.189 ms / 0.377 ns/op | 0.182 ms / 0.364 ns/op |
| `GenericArray<T> foreach` | 1.843 ms / 3.685 ns/op | 1.837 ms / 3.675 ns/op |
| `GenericArray<T> manual` | 0.170 ms / 0.339 ns/op | 0.170 ms / 0.339 ns/op |
| `GenericArray<T> MoveNextOnly` | 0.106 ms / 0.212 ns/op | 0.106 ms / 0.212 ns/op |
| `ECS for + getKeyAt` | 0.179 ms / 0.357 ns/op | 0.176 ms / 0.353 ns/op |
| `ECS foreach dict + item.Key` | 0.181 ms / 0.362 ns/op | 0.308 ms / 0.616 ns/op |
| `ECS foreach dict.Keys` | 0.180 ms / 0.360 ns/op | 0.179 ms / 0.358 ns/op |
| `ECS Keys手动Enumerator` | 0.179 ms / 0.359 ns/op | 0.179 ms / 0.358 ns/op |
| `ECS Keys MoveNextOnly` | 0.123 ms / 0.246 ns/op | 0.123 ms / 0.247 ns/op |
| `Dictionary foreach` | 3.800 ms / 7.600 ns/op | 3.979 ms / 7.959 ns/op |
| `Dictionary foreach Keys` | 0.478 ms / 0.956 ns/op | 0.604 ms / 1.208 ns/op |
| `Dictionary Keys手动Enumerator` | 0.511 ms / 1.023 ns/op | 0.457 ms / 0.915 ns/op |
### Enumerator 细测：仅读取 Value.mHP

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `RoleData[] for` | 0.195 ms / 0.389 ns/op | 0.201 ms / 0.402 ns/op |
| `RoleData[] foreach` | 0.196 ms / 0.391 ns/op | 0.198 ms / 0.397 ns/op |
| `ECS for + getValueAt` | 0.192 ms / 0.384 ns/op | 0.448 ms / 0.895 ns/op |
| `ECS foreach dict + Value` | 0.198 ms / 0.397 ns/op | 0.308 ms / 0.617 ns/op |
| `ECS foreach dict.Values` | 0.205 ms / 0.410 ns/op | 0.274 ms / 0.547 ns/op |
| `ECS Values手动Enumerator` | 0.204 ms / 0.407 ns/op | 0.268 ms / 0.535 ns/op |
| `Dictionary foreach` | 3.802 ms / 7.604 ns/op | 3.955 ms / 7.910 ns/op |
| `Dictionary foreach Values` | 0.889 ms / 1.778 ns/op | 1.079 ms / 2.158 ns/op |
| `Dictionary Values手动Enumerator` | 0.526 ms / 1.053 ns/op | 0.563 ms / 1.126 ns/op |
### Enumerator 细测：修改 Value.mHP

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `ECS for + getValueAt` | 0.355 ms / 0.709 ns/op | 0.457 ms / 0.915 ns/op |
| `ECS foreach dict + Value` | 0.217 ms / 0.433 ns/op | 0.325 ms / 0.650 ns/op |
| `ECS foreach dict.Values` | 0.204 ms / 0.409 ns/op | 0.275 ms / 0.549 ns/op |
| `ECS Values手动Enumerator` | 0.203 ms / 0.406 ns/op | 0.275 ms / 0.549 ns/op |
| `ECS Direct Column` | 0.179 ms / 0.357 ns/op | 0.244 ms / 0.487 ns/op |
### Enumerator 细测：同时读取 Key + Value

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `ECS for getKeyAt+getValueAt` | 0.308 ms / 0.616 ns/op | 0.547 ms / 1.095 ns/op |
| `ECS foreach dict` | 0.264 ms / 0.528 ns/op | 0.396 ms / 0.792 ns/op |
| `ECS 手动Enumerator` | 0.274 ms / 0.547 ns/op | 0.416 ms / 0.832 ns/op |
| `Dictionary foreach` | 3.878 ms / 7.755 ns/op | 3.971 ms / 7.941 ns/op |
| `Dictionary 手动Enumerator` | 0.545 ms / 1.090 ns/op | 0.793 ms / 1.586 ns/op |


### ECSList 结构操作

| Case | Unsafe List | Unsafe ECS | Unsafe ECS/List | Gate | SafeSpan List | SafeSpan ECS | SafeSpan ECS/List | Gate |
|---|---:|---:|---:|---|---:|---:|---:|---|
| Insert Head | 1.554 ms / 6.069 us/op | 1.536 ms / 6.002 us/op | 0.989x | PASS | 1.512 ms / 5.904 us/op | 1.533 ms / 5.987 us/op | 1.014x | PASS |
| Insert Middle | 0.779 ms / 3.043 us/op | 0.765 ms / 2.988 us/op | 0.982x | PASS | 0.756 ms / 2.954 us/op | 0.770 ms / 3.007 us/op | 1.018x | PASS |
| Insert Tail | 0.001 ms / 0.005 us/op | 0.002 ms / 0.009 us/op | 1.917x | SKIP(TinyOperation) | 0.001 ms / 0.004 us/op | 0.011 ms / 0.043 us/op | 10.091x | SKIP(TinyOperation) |
| RemoveAt Head | 26.147 ms / 102.137 us/op | 8.170 ms / 31.914 us/op | 0.312x | PASS | 26.090 ms / 101.912 us/op | 25.873 ms / 101.066 us/op | 0.992x | PASS |
| RemoveAt Middle | 12.944 ms / 50.561 us/op | 4.074 ms / 15.914 us/op | 0.315x | PASS | 12.977 ms / 50.689 us/op | 12.883 ms / 50.324 us/op | 0.993x | PASS |
| RemoveAt Tail | 0.000 ms / 0.001 us/op | 0.000 ms / 0.001 us/op | 1.500x | SKIP(TinyOperation) | 0.000 ms / 0.001 us/op | 0.000 ms / 0.002 us/op | 2.000x | SKIP(TinyOperation) |
| Hybrid Insert Middle | 1.089 ms / 4.255 us/op | 1.005 ms / 3.924 us/op | 0.922x | PASS | 1.081 ms / 4.222 us/op | 1.002 ms / 3.912 us/op | 0.927x | PASS |
| Hybrid RemoveAt Middle | 18.349 ms / 71.676 us/op | 15.448 ms / 60.343 us/op | 0.842x | PASS | 18.528 ms / 72.373 us/op | 16.649 ms / 65.034 us/op | 0.899x | PASS |

Tail 级操作只有数纳秒，百分比会被固定调用开销放大，因此封版 Gate 对这类 Case 使用 `SKIP(TinyOperation)`，不把百分比当作硬门槛。


### RemoveAt 与 RemoveAtSwapBack

| 测试项 | Unsafe | SafeSpan |
|---|---:|---:|
| `ECS RemoveAt` | 4.093 ms / 15.989 us/op | 12.959 ms / 50.622 us/op |
| `ECS RemoveAtSwapBack` | 0.001 ms / 0.002 us/op | 0.001 ms / 0.003 us/op |


### Capacity / Resize：纯 RoleData

| Capacity | Unsafe List | Unsafe ECS | Unsafe ECS/List | SafeSpan List | SafeSpan ECS | SafeSpan ECS/List |
|---:|---:|---:|---:|---:|---:|---:|
| 1024 | 3.600 us | 0.800 us | 0.22x | 2.900 us | 2.800 us | 0.97x |
| 8192 | 17.700 us | 4.700 us | 0.27x | 12.800 us | 12.000 us | 0.94x |
| 32768 | 62.800 us | 183.500 us | 2.92x | 62.000 us | 67.200 us | 1.08x |
| 49152 | 93.300 us | 299.600 us | 3.21x | 105.300 us | 91.400 us | 0.87x |
| 57344 | 178.400 us | 331.700 us | 1.86x | 183.100 us | 95.800 us | 0.52x |
| 61440 | 197.800 us | 361.900 us | 1.83x | 176.800 us | 124.700 us | 0.71x |
| 65536 | 204.800 us | 372.400 us | 1.82x | 187.400 us | 121.000 us | 0.65x |
| 69632 | 211.300 us | 391.100 us | 1.85x | 203.600 us | 131.000 us | 0.64x |
| 73728 | 233.900 us | 412.500 us | 1.76x | 233.200 us | 142.400 us | 0.61x |
| 81920 | 250.100 us | 500.000 us | 2.00x | 231.800 us | 157.300 us | 0.68x |
| 98304 | 364.600 us | 547.400 us | 1.50x | 322.800 us | 186.300 us | 0.58x |
| 131072 | 452.900 us | 697.300 us | 1.54x | 419.400 us | 846.300 us | 2.02x |
| 262144 | 1207.500 us | 1261.900 us | 1.05x | 1605.600 us | 1724.900 us | 1.07x |

Resize 属于低频结构操作。已知大致规模时，推荐在构造时直接预留 Capacity。


### Capacity / Resize：Managed Hybrid

| Capacity | Unsafe List | Unsafe ECS | Unsafe ECS/List | SafeSpan List | SafeSpan ECS | SafeSpan ECS/List |
|---:|---:|---:|---:|---:|---:|---:|
| 1024 | 2.800 us | 3.300 us | 1.18x | 3.300 us | 2.800 us | 0.85x |
| 8192 | 15.400 us | 13.900 us | 0.90x | 16.700 us | 19.600 us | 1.17x |
| 32768 | 92.000 us | 80.900 us | 0.88x | 153.200 us | 113.200 us | 0.74x |
| 49152 | 160.600 us | 101.700 us | 0.63x | 279.800 us | 208.000 us | 0.74x |
| 65536 | 257.200 us | 168.800 us | 0.66x | 327.300 us | 322.200 us | 0.98x |
| 81920 | 370.000 us | 272.100 us | 0.74x | 443.900 us | 402.600 us | 0.91x |
| 98304 | 528.900 us | 1485.300 us | 2.81x | 479.700 us | 394.000 us | 0.82x |
| 131072 | 716.300 us | 1615.900 us | 2.26x | 680.400 us | 692.600 us | 1.02x |

Resize 属于低频结构操作。已知大致规模时，推荐在构造时直接预留 Capacity。


## Benchmark 结论

最终数据支持当前推荐的访问层级：

```text
简单单字段
→ list[i] / dictionary[key]

多字段
→ Local Ref

同一字段在一次逻辑里反复读取
→ Local Ref + 局部变量缓存

Profiler 确认的极端热点循环
→ Direct Column
```

Dictionary 的核心价值不是让单次 Hash Lookup 永远快于 BCL `Dictionary<TKey,TValue>`，而是：

```text
Dictionary<TKey,int> 随机定位
+
Dense ECS Value Storage
+
后续连续批处理
```

混合场景最终数据：

```text
Unsafe:
Dictionary<int,RoleData>  13.003 ms
ECS Direct+LocalRef        0.893 ms
Standard / ECS            14.56x

SafeSpan:
Dictionary<int,RoleData>  12.405 ms
ECS Direct+LocalRef        1.021 ms
Standard / ECS            12.15x
```

最终 `dict.Keys` / `dict.Values` 已经不存在早期自定义 Enumerator 的数量级退化；Unsafe 和 SafeSpan 都完成了封版回归。

## Runtime Correctness

最终 Runtime Unit Test：

```text
Unsafe   : 59 / 59 PASS
SafeSpan : 59 / 59 PASS
```

## GC 说明

当前 Release Player 中：

```text
ProfilerRecorder("GC.Alloc")
SelfCheck = INVALID
```

因此 README **不把无效测试得到的 0 事件宣传为“实测 0 GC”**。

需要检查精确 managed allocation 时，应使用：

```text
Development Build
+
Unity Profiler
+
CPU Usage / GC.Alloc
```

# Source Generator

```text
SourceGenerator~
```

发布 Analyzer：

```text
Analyzers/ECSGenerator.dll
```

# 独立展示仓库

```text
https://github.com/ZHOURUIH/EasyECS
```

# License

MIT
