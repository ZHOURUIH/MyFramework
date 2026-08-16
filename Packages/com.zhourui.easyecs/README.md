# EasyECS

EasyECS 是一个面向 Unity 的 **OOP 兼容 SoA 数据布局优化器**。

它不是一套新的 ECS 游戏框架，也不会要求业务层改写成 Entity / Component / System 模式。业务代码仍然可以使用普通结构体、List 风格访问和 Dictionary 风格访问，EasyECS 通过 Source Generator 在编译期生成 SoA / AoS 混合存储以及对应访问代码。

核心目标是：**尽量不改变现有 OOP 业务写法，只优化热点数据的物理布局和访问路径。**

## 安装

Unity Package Manager -> Add package from git URL：

GitHub：

```text
https://github.com/ZHOURUIH/MyFramework.git?path=/Packages/com.zhourui.easyecs
```

Gitee：

```text
https://gitee.com/inothingtodo/MyFramework.git?path=/Packages/com.zhourui.easyecs
```

## 快速开始

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

Source Generator 会生成：

```text
RoleDataECSList
RoleDataECSDictionary<TKey>
RoleDataRef
RoleData各SoA字段的Direct Column访问器
```

普通 List 风格：

```csharp
RoleDataECSList roles = new RoleDataECSList();
roles.Add(new RoleData { mHP = 100, mSpeed = 5.0f });
roles[0].mHP -= 10;
RoleDataRef role = roles[0];
role.mPositionX += role.mSpeed;
```

Dictionary 风格：

```csharp
RoleDataECSDictionary<int> roles = new RoleDataECSDictionary<int>();
roles.Add(1001, new RoleData { mHP = 100, mSpeed = 5.0f, mID = 1001 });
roles[1001].mHP -= 10;
RoleDataRef role = roles[1001];
```

## ECS / NotECS 规则

结构体标记 `[ECS]` 时，字段默认使用 SoA，字段上的 `[NotECS]` 可以切回 AoS。

结构体标记 `[NotECS]` 时，字段默认使用 AoS，字段上的 `[ECS]` 可以切到 SoA。

字段标记优先于结构体默认设置。

例如上面的 `RoleData` 最终布局近似为：

```text
mHP[]
mSpeed[]
mPositionX[]
mPositionY[]

mAoS[]
  ├ mID
  ├ mModelID
  └ mCamp
```

## Direct Column

普通业务逻辑优先使用 Ref / indexer。Profiler 确认的极端热点循环可以直接获取字段列：

```csharp
var hp = roles.getHPColumn();
var speed = roles.getSpeedColumn();
var positionX = roles.getPositionXColumn();
for (int i = 0; i < roles.Count; ++i)
{
	hp[i] -= 1;
	positionX[i] += speed[i];
}
```

建议的使用层级：

```text
普通业务逻辑
→ Ref

简单单字段访问
→ list[i] / dictionary[key]

Profiler确认的极端热点批处理
→ Direct Column
```

Direct Column 是临时字段视图。获取 Column 后如果发生 Add、Remove、Clear、Resize 或 Dispose，不应继续使用旧 Column。

## ECSDictionary

生成的 `RoleDataECSDictionary<TKey>` 内部采用：

```text
Dictionary<TKey,int>
        ↓
    dense index
        ↓
RoleDataECSList
```

因此随机 Key 查询仍由 BCL `Dictionary<TKey,int>` 负责，而 value 使用 EasyECS 的连续存储。

支持的常用接口包括：

```text
Add
TryAdd
ContainsKey
TryGetValue
TryGetIndex
Remove
Clear
Count
Capacity
Comparer
getKeyAt
getValueAt
foreach
Keys
Values
Direct Column
Dispose
```

`Remove` 使用 dense swap-back，因此遍历顺序不保证稳定。

## Backend

EasyECS 会在 Source Generator 阶段自动选择存储 Backend：

```text
ECS_FORCE_SAFE_REGISTRY
→ SafeRegistry

否则 Allow Unsafe Code=true 且结构体为 unmanaged
→ Unsafe

否则当前编译环境存在 Span<T>
→ SafeSpan

否则
→ SafeRegistry
```

### Unsafe

用于 unmanaged 数据的最高性能路径。底层使用 native memory 和指针访问。

### SafeSpan

无法或不希望使用 Unsafe 时的安全高性能路径。底层使用托管数组以及 Span 访问。

### SafeRegistry

用于旧运行环境或缺少 Span 的兼容路径，也可通过 `ECS_FORCE_SAFE_REGISTRY` 强制启用。

如果需要测试或强制兼容模式，在当前 Build Target 的 Scripting Define Symbols 中加入：

```text
ECS_FORCE_SAFE_REGISTRY
```

只要当前 Compilation 中存在该宏，无论 Editor、Development Build 还是 Release Build，都会强制生成 SafeRegistry。

## Managed 字段

结构体包含 `string`、`object` 或其他 managed 字段时不会使用 Unsafe Backend。

例如：

```csharp
[ECS]
public struct RoleRuntimeData
{
	public int mHP;
	public string mName;
	public object mPayload;
	[NotECS] public int mID;
}
```

在支持 Span 的环境中会自动选择 SafeSpan；缺少 Span 时回退 SafeRegistry。

## Ref 与生命周期

Ref 可以跨 Add / Resize 保持有效，因为底层 Storage 地址或 Storage 入口保持稳定。

但 EasyECS 的 Ref 是 **位置引用，不是实体身份句柄**：

- `RemoveAtSwapBack` 后，被删除位置以及被移动元素原位置的旧 Ref 不再有效。
- `Clear` 和 `Dispose` 会使已有 Ref 失效。
- Dictionary Remove 同样遵循 dense swap-back 语义。
- 结构变化期间不支持并发访问。

Editor 下生成代码会增加额外的边界、生命周期、Ref、Column 和 Enumerator 安全检查；Player 不承担这些 Editor 检查开销。

## Dispose

EasyECS 对三个 Backend 提供统一的 `Dispose()` API。

容器生命周期结束时建议调用 `Dispose()`。Unsafe Backend 会释放 native memory；SafeSpan / SafeRegistry 也通过同一接口保持统一的资源与失效语义。

## 线程安全

EasyECS 容器不是线程安全容器。

不要并发执行：

```text
Add
Remove
Clear
Resize
Dispose
```

也不要在结构变化的同时从其他线程持有 Ref / Column 继续访问。

## Benchmark Sample

安装 Package 后可以通过：

```text
EasyECS -> Import Benchmark Sample
```

导入测试代码。

Sample 包含：

```text
RoleDataBenchmark
RoleDataDictionaryBenchmark
RoleDataDictionaryEnumeratorBenchmark
EasyECSRuntimeUnitTest
```

其中 Runtime Unit Test 同时覆盖 List、Dictionary、managed 字段和 Editor 生命周期检查。

## Benchmark 结果

以下结果全部来自实际运行日志。为了让 README 可读且便于横向比较，当前结果表记录 **每一个 Benchmark 测试项的 Median 耗时与单位操作耗时**，格式统一为：

```text
Median ms / ns per entity(or op)
```

数值越低越好。每轮当前 Benchmark 使用：

```text
EntityCount      = 500000
SampleCount      = 15
WarmupCount      = 3
RandomWriteCount = 50000
```

`RandomWriteCount` 仅用于 Dictionary 随机修改测试。原始日志中的 Min / Max 用于观察采样抖动，README 不把它们作为最终性能排序指标，因此下面统一使用 Median。

### PC 测试环境

```text
Unity       : 6000.3.21f1
Platform    : Windows x64 Player
Graphics    : Direct3D 12
GPU         : NVIDIA GeForce RTX 2060
CPU threads : 32
```

#### PC 当前 backend-agnostic Benchmark：SafeSpan / SafeRegistry

SafeSpan 与 SafeRegistry 均使用当前 backend-agnostic Benchmark，同一套业务测试代码不包含 backend-specific pointer 访问。

#### List：修改 1 个字段

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `List<RoleData>` | 4.035 / 8.069 | 3.837 / 7.675 |
| `RoleData[]` | 0.323 / 0.646 | 0.297 / 0.595 |
| `ECS list[i]` | 0.358 / 0.716 | 0.815 / 1.630 |
| `ECS Ref` | 0.370 / 0.741 | 0.810 / 1.619 |
| `ECS Direct` | 0.176 / 0.352 | 0.183 / 0.366 |

#### List：访问 2 个字段

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `List<RoleData>` | 3.858 / 7.715 | 4.011 / 8.022 |
| `RoleData[]` | 0.313 / 0.626 | 0.306 / 0.612 |
| `ECS list[i]` | 0.637 / 1.274 | 1.468 / 2.935 |
| `ECS Ref` | 0.449 / 0.898 | 1.410 / 2.821 |
| `ECS Direct` | 0.220 / 0.441 | 0.232 / 0.465 |

#### List：访问 4 个字段

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `List<RoleData>` | 2.141 / 4.282 | 2.224 / 4.448 |
| `RoleData[]` | 0.503 / 1.007 | 0.472 / 0.944 |
| `ECS list[i]` | 1.478 / 2.955 | 3.808 / 7.615 |
| `ECS Ref` | 0.982 / 1.964 | 3.504 / 7.007 |
| `ECS Direct` | 0.582 / 1.164 | 0.502 / 1.003 |

#### Dictionary：随机 Key 读取

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `Dictionary<int,RoleData>` | 14.235 / 28.470 | 13.822 / 27.644 |
| `IndexMap + RoleData[]` | 21.080 / 42.160 | 20.982 / 41.963 |
| `IndexMap + int[]` | 7.639 / 15.279 | 7.741 / 15.483 |
| `ECS Inline Indexer` | 8.736 / 17.472 | 13.122 / 26.245 |
| `ECS Local Ref` | 8.898 / 17.795 | 14.050 / 28.100 |
| `ECS TryGetValue` | 8.585 / 17.170 | 11.511 / 23.023 |

#### Dictionary：随机 Key 修改

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `Dictionary<int,RoleData>` | 1.162 / 23.240 | 1.364 / 27.286 |
| `IndexMap + RoleData[]` | 0.805 / 16.102 | 0.962 / 19.242 |
| `IndexMap + int[]` | 0.701 / 14.014 | 0.730 / 14.598 |
| `ECS Inline Indexer` | 0.781 / 15.614 | 1.047 / 20.936 |
| `ECS Local Ref` | 0.783 / 15.654 | 1.044 / 20.888 |
| `ECS TryGetValue` | 0.791 / 15.816 | 0.826 / 16.510 |

#### Dictionary：连续存储全量修改 1 个字段

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `Dictionary Key全量更新` | 10.515 / 21.030 | 10.419 / 20.839 |
| `Dense RoleData[]` | 0.278 / 0.556 | 0.284 / 0.568 |
| `Dense int[]` | 0.143 / 0.285 | 0.151 / 0.301 |
| `ECS Dense Ref` | 0.447 / 0.894 | 0.859 / 1.718 |
| `ECS Direct` | 0.235 / 0.470 | 0.243 / 0.485 |

#### Dictionary：连续存储全量访问 4 个字段

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `Dictionary Key全量更新` | 6.679 / 13.359 | 6.843 / 13.687 |
| `Dense RoleData[]` | 0.528 / 1.056 | 0.459 / 0.917 |
| `ECS Dense Ref` | 1.087 / 2.173 | 3.734 / 7.468 |
| `ECS Direct` | 0.581 / 1.161 | 0.590 / 1.180 |

#### Dictionary：Dense 全量更新 + 10% 随机 Key 修改

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `Dictionary<int,RoleData>` | 14.203 / 25.824 | 14.242 / 25.894 |
| `IndexMap + RoleData[]` | 1.733 / 3.151 | 1.277 / 2.322 |
| `ECS Direct+LocalRef` | 1.094 / 1.989 | 1.147 / 2.085 |

#### Dictionary：仅遍历 Key

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `for + getKeyAt` | 0.189 / 0.378 | 0.177 / 0.354 |
| `foreach dict + item.Key` | 0.230 / 0.460 | 0.212 / 0.423 |
| `foreach dict.Keys` | 1.909 / 3.818 | 1.880 / 3.760 |

#### Dictionary：仅遍历 Value 并修改 1 个字段

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `for + getValueAt` | 0.470 / 0.939 | 0.857 / 1.714 |
| `foreach dict + item.Value` | 0.421 / 0.841 | 0.812 / 1.623 |
| `foreach dict.Values` | 1.825 / 3.649 | 1.841 / 3.681 |
| `Direct Column` | 0.235 / 0.470 | 0.227 / 0.455 |

#### Dictionary：同时遍历 Key + Value

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `for getKeyAt+getValueAt` | 0.538 / 1.075 | 0.954 / 1.907 |
| `foreach dict` | 0.400 / 0.800 | 0.855 / 1.709 |

#### Enumerator：仅读取 Key

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `int[] for` | 0.123 / 0.246 | 0.117 / 0.235 |
| `int[] foreach` | 0.123 / 0.247 | 0.131 / 0.263 |
| `ECS for + getKeyAt` | 0.196 / 0.391 | 0.187 / 0.373 |
| `ECS foreach dict + item.Key` | 0.215 / 0.430 | 0.220 / 0.440 |
| `ECS foreach dict.Keys` | 1.848 / 3.696 | 1.876 / 3.752 |
| `ECS Keys手动Enumerator` | 1.849 / 3.698 | 1.853 / 3.706 |
| `Dictionary foreach` | 3.802 / 7.604 | 4.044 / 8.088 |
| `Dictionary foreach Keys` | 0.547 / 1.094 | 0.688 / 1.377 |
| `Dictionary Keys手动Enumerator` | 0.495 / 0.989 | 0.448 / 0.896 |

#### Enumerator：仅读取 Value.mHP

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `RoleData[] for` | 0.205 / 0.410 | 0.216 / 0.432 |
| `RoleData[] foreach` | 0.216 / 0.432 | 0.211 / 0.421 |
| `ECS for + getValueAt` | 0.450 / 0.900 | 0.871 / 1.742 |
| `ECS foreach dict + Value` | 0.403 / 0.806 | 0.846 / 1.691 |
| `ECS foreach dict.Values` | 1.801 / 3.603 | 1.817 / 3.634 |
| `ECS Values手动Enumerator` | 1.804 / 3.609 | 1.816 / 3.631 |
| `Dictionary foreach` | 3.776 / 7.551 | 3.793 / 7.586 |
| `Dictionary foreach Values` | 0.827 / 1.655 | 0.781 / 1.562 |
| `Dictionary Values手动Enumerator` | 0.431 / 0.861 | 0.399 / 0.799 |

#### Enumerator：修改 Value.mHP

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `ECS for + getValueAt` | 0.449 / 0.898 | 0.852 / 1.703 |
| `ECS foreach dict + Value` | 0.398 / 0.795 | 0.839 / 1.678 |
| `ECS foreach dict.Values` | 1.790 / 3.580 | 1.843 / 3.686 |
| `ECS Values手动Enumerator` | 1.792 / 3.584 | 1.840 / 3.680 |
| `ECS Direct Column` | 0.235 / 0.469 | 0.235 / 0.469 |

#### Enumerator：同时读取 Key + Value

| 测试项 | SafeSpan | SafeRegistry |
|---|---:|---:|
| `ECS for getKeyAt+getValueAt` | 0.539 / 1.078 | 0.973 / 1.947 |
| `ECS foreach dict` | 0.415 / 0.830 | 0.825 / 1.650 |
| `ECS 手动Enumerator` | 0.401 / 0.801 | 0.805 / 1.610 |
| `Dictionary foreach` | 3.835 / 7.671 | 3.828 / 7.656 |
| `Dictionary 手动Enumerator` | 0.531 / 1.062 | 0.575 / 1.151 |


#### PC Unsafe 历史 Benchmark

PC Unsafe 是在较早一次 Generator / Benchmark 修订上测得的完整结果。它已经覆盖 List、Dictionary、Dense、Mixed 与 Enumerator，但当时随机 Dictionary Benchmark 仍额外包含 `IndexMap + int*` 研究基线，而且部分 Dictionary 微基准实现随后又做过调整。

因此下面结果保留作为 **Unsafe 在 PC 上的历史实测数据**，但不应拿它与上面的当前 SafeSpan / SafeRegistry 做严格微秒级横向排名。当前正式 Sample 已移除业务代码中的 `int*` backend-specific 基线。

旧版随机修改微基准的 `ns/op` 归一化口径也与当前版本不同，因此本节只保留最可靠的 **Median ms**，不展示旧版 `ns/op`。

#### List：修改 1 个字段

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `List<RoleData>` | 3.995 |
| `RoleData[]` | 0.292 |
| `ECS list[i]` | 0.274 |
| `ECS Ref` | 0.271 |
| `ECS Direct` | 0.078 |

#### List：访问 2 个字段

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `List<RoleData>` | 3.773 |
| `RoleData[]` | 0.317 |
| `ECS list[i]` | 0.207 |
| `ECS Ref` | 0.219 |
| `ECS Direct` | 0.098 |

#### List：访问 4 个字段

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `List<RoleData>` | 2.124 |
| `RoleData[]` | 0.461 |
| `ECS list[i]` | 0.819 |
| `ECS Ref` | 0.535 |
| `ECS Direct` | 0.326 |

#### Dictionary：随机 Key 读取

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `Dictionary<int,RoleData>` | 11.993 |
| `IndexMap + RoleData[]` | 16.519 |
| `IndexMap + int[]` | 6.731 |
| `IndexMap + int*` | 6.758 |
| `ECS Inline Indexer` | 7.470 |
| `ECS Local Ref` | 7.642 |
| `ECS TryGetValue` | 7.511 |

#### Dictionary：随机 Key 修改

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `Dictionary<int,RoleData>` | 24.653 |
| `IndexMap + RoleData[]` | 17.320 |
| `IndexMap + int[]` | 7.226 |
| `IndexMap + int*` | 7.156 |
| `ECS Inline Indexer` | 7.792 |
| `ECS Local Ref` | 8.051 |
| `ECS TryGetValue` | 10.206 |

#### Dictionary：连续存储全量修改 1 个字段

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `Dictionary Key全量更新` | 10.338 |
| `Dense RoleData[]` | 0.313 |
| `Dense int[]` | 0.156 |
| `ECS Dense Ref` | 0.188 |
| `ECS Direct` | 0.091 |

#### Dictionary：连续存储全量访问 4 个字段

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `Dictionary Key全量更新` | 6.888 |
| `Dense RoleData[]` | 0.450 |
| `ECS Dense Ref` | 0.500 |
| `ECS Direct` | 0.321 |

#### Dictionary：Dense 全量更新 + 10% 随机 Key 修改

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `Dictionary<int,RoleData>` | 9.780 |
| `IndexMap + RoleData[]` | 1.949 |
| `ECS Direct+LocalRef` | 1.101 |

#### Dictionary：仅遍历 Key

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `for + getKeyAt` | 0.156 |
| `foreach dict + item.Key` | 0.238 |
| `foreach dict.Keys` | 1.869 |

#### Dictionary：仅遍历 Value 并修改 1 个字段

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `for + getValueAt` | 0.176 |
| `foreach dict + item.Value` | 0.245 |
| `foreach dict.Values` | 1.811 |
| `Direct Column` | 0.078 |

#### Dictionary：同时遍历 Key + Value

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `for getKeyAt+getValueAt` | 0.224 |
| `foreach dict` | 0.275 |

#### Enumerator：仅读取 Key

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `int[] for` | 0.118 |
| `int[] foreach` | 0.121 |
| `ECS for + getKeyAt` | 0.133 |
| `ECS foreach dict + item.Key` | 0.220 |
| `ECS foreach dict.Keys` | 1.855 |
| `ECS Keys手动Enumerator` | 1.834 |
| `Dictionary foreach` | 3.788 |
| `Dictionary foreach Keys` | 0.945 |
| `Dictionary Keys手动Enumerator` | 0.500 |

#### Enumerator：仅读取 Value.mHP

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `RoleData[] for` | 0.229 |
| `RoleData[] foreach` | 0.230 |
| `ECS for + getValueAt` | 0.156 |
| `ECS foreach dict + Value` | 0.243 |
| `ECS foreach dict.Values` | 1.864 |
| `ECS Values手动Enumerator` | 1.829 |
| `Dictionary foreach` | 3.782 |
| `Dictionary foreach Values` | 0.859 |
| `Dictionary Values手动Enumerator` | 0.424 |

#### Enumerator：修改 Value.mHP

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `ECS for + getValueAt` | 0.173 |
| `ECS foreach dict + Value` | 0.239 |
| `ECS foreach dict.Values` | 1.810 |
| `ECS Values手动Enumerator` | 1.793 |
| `ECS Direct Column` | 0.078 |

#### Enumerator：同时读取 Key + Value

| 测试项 | Unsafe（历史 Median ms） |
|---|---:|
| `ECS for getKeyAt+getValueAt` | 0.220 |
| `ECS foreach dict` | 0.268 |
| `ECS 手动Enumerator` | 0.266 |
| `Dictionary foreach` | 3.828 |
| `Dictionary 手动Enumerator` | 0.556 |


### Android 真机测试环境

```text
Device            : HUAWEI ALP-AL00
OS                : Android 10 / API 29
CPU               : ARM64, 8 Cores
big.LITTLE        : 4 big + 4 little
Memory            : 3648 MB
Unity             : 6000.3.21f1
Build Type        : Release
Scripting Backend : IL2CPP
CPU Target        : arm64-v8a
Code Stripping    : Enabled
```

Android 的 Unsafe、SafeSpan、SafeRegistry 均在同一台真机、同一套当前 backend-agnostic Benchmark 上运行。

三轮测试为连续执行，手机温度、动态调频与大小核调度会影响绝对耗时。因此这些数据适合观察数量级、访问模式和 Backend 的结构性差异，不应把非常小的毫秒差当成跨设备固定结论。

### Android Benchmark

#### List：修改 1 个字段

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `List<RoleData>` | 12.610 / 25.221 | 13.157 / 26.315 | 12.602 / 25.204 |
| `RoleData[]` | 3.263 / 6.526 | 3.524 / 7.048 | 3.285 / 6.571 |
| `ECS list[i]` | 1.063 / 2.126 | 2.305 / 4.609 | 6.713 / 13.425 |
| `ECS Ref` | 1.063 / 2.126 | 2.343 / 4.685 | 6.563 / 13.126 |
| `ECS Direct` | 0.750 / 1.500 | 1.068 / 2.135 | 1.166 / 2.332 |

#### List：访问 2 个字段

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `List<RoleData>` | 12.436 / 24.871 | 12.532 / 25.065 | 12.612 / 25.224 |
| `RoleData[]` | 3.257 / 6.513 | 3.408 / 6.817 | 3.301 / 6.602 |
| `ECS list[i]` | 1.278 / 2.555 | 3.785 / 7.570 | 13.137 / 26.274 |
| `ECS Ref` | 1.317 / 2.633 | 2.905 / 5.810 | 12.673 / 25.346 |
| `ECS Direct` | 0.940 / 1.879 | 1.390 / 2.780 | 1.595 / 3.190 |

#### List：访问 4 个字段

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `List<RoleData>` | 12.189 / 24.377 | 12.243 / 24.485 | 12.298 / 24.597 |
| `RoleData[]` | 4.256 / 8.513 | 2.934 / 5.869 | 4.321 / 8.642 |
| `ECS list[i]` | 2.758 / 5.516 | 8.879 / 17.757 | 32.704 / 65.408 |
| `ECS Ref` | 2.743 / 5.486 | 4.802 / 9.603 | 30.067 / 60.133 |
| `ECS Direct` | 2.683 / 5.367 | 3.380 / 6.759 | 3.381 / 6.761 |

#### Dictionary：随机 Key 读取

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `Dictionary<int,RoleData>` | 210.057 / 420.115 | 210.311 / 420.622 | 211.278 / 422.556 |
| `IndexMap + RoleData[]` | 211.423 / 422.846 | 235.051 / 470.102 | 216.365 / 432.730 |
| `IndexMap + int[]` | 204.765 / 409.529 | 209.595 / 419.190 | 206.285 / 412.571 |
| `ECS Inline Indexer` | 210.568 / 421.135 | 208.101 / 416.201 | 218.437 / 436.874 |
| `ECS Local Ref` | 211.314 / 422.628 | 205.736 / 411.471 | 214.262 / 428.523 |
| `ECS TryGetValue` | 209.584 / 419.168 | 210.138 / 420.276 | 214.062 / 428.124 |

#### Dictionary：随机 Key 修改

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `Dictionary<int,RoleData>` | 24.967 / 499.344 | 24.878 / 497.562 | 25.123 / 502.468 |
| `IndexMap + RoleData[]` | 31.220 / 624.396 | 31.136 / 622.728 | 30.819 / 616.386 |
| `IndexMap + int[]` | 27.283 / 545.656 | 27.256 / 545.126 | 27.388 / 547.750 |
| `ECS Inline Indexer` | 27.467 / 549.334 | 27.583 / 551.656 | 28.051 / 561.010 |
| `ECS Local Ref` | 27.359 / 547.188 | 27.402 / 548.042 | 28.213 / 564.260 |
| `ECS TryGetValue` | 27.295 / 545.906 | 27.661 / 553.228 | 28.098 / 561.958 |

#### Dictionary：连续存储全量修改 1 个字段

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `Dictionary Key全量更新` | 51.985 / 103.970 | 52.605 / 105.209 | 51.838 / 103.675 |
| `Dense RoleData[]` | 3.236 / 6.472 | 3.232 / 6.464 | 3.622 / 7.244 |
| `Dense int[]` | 0.743 / 1.485 | 0.743 / 1.486 | 0.742 / 1.483 |
| `ECS Dense Ref` | 1.062 / 2.124 | 2.486 / 4.972 | 6.725 / 13.449 |
| `ECS Direct` | 0.748 / 1.496 | 0.955 / 1.909 | 1.167 / 2.334 |

#### Dictionary：连续存储全量访问 4 个字段

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `Dictionary Key全量更新` | 52.557 / 105.114 | 52.583 / 105.166 | 52.134 / 104.268 |
| `Dense RoleData[]` | 3.321 / 6.642 | 3.315 / 6.629 | 3.808 / 7.617 |
| `ECS Dense Ref` | 2.420 / 4.841 | 5.150 / 10.300 | 30.271 / 60.543 |
| `ECS Direct` | 2.663 / 5.325 | 3.225 / 6.450 | 3.385 / 6.771 |

#### Dictionary：Dense 全量更新 + 10% 随机 Key 修改

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `Dictionary<int,RoleData>` | 77.504 / 140.917 | 77.764 / 141.388 | 81.010 / 147.291 |
| `IndexMap + RoleData[]` | 34.900 / 63.455 | 34.873 / 63.405 | 34.503 / 62.732 |
| `ECS Direct+LocalRef` | 28.815 / 52.390 | 29.384 / 53.426 | 30.138 / 54.795 |

#### Dictionary：仅遍历 Key

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `for + getKeyAt` | 0.954 / 1.908 | 0.954 / 1.908 | 0.955 / 1.910 |
| `foreach dict + item.Key` | 1.591 / 3.181 | 1.591 / 3.181 | 1.591 / 3.181 |
| `foreach dict.Keys` | 1.270 / 2.541 | 1.269 / 2.539 | 1.239 / 2.478 |

#### Dictionary：仅遍历 Value 并修改 1 个字段

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `for + getValueAt` | 1.061 / 2.122 | 2.478 / 4.955 | 6.761 / 13.522 |
| `foreach dict + item.Value` | 1.803 / 3.606 | 2.904 / 5.807 | 8.849 / 17.699 |
| `foreach dict.Values` | 1.107 / 2.214 | 2.468 / 4.936 | 6.678 / 13.356 |
| `Direct Column` | 0.766 / 1.531 | 0.955 / 1.909 | 1.167 / 2.334 |

#### Dictionary：同时遍历 Key + Value

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `for getKeyAt+getValueAt` | 1.617 / 3.233 | 3.111 / 6.223 | 7.521 / 15.042 |
| `foreach dict` | 2.067 / 4.133 | 3.133 / 6.266 | 8.078 / 16.156 |

#### Enumerator：仅读取 Key

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `int[] for` | 0.639 / 1.277 | 0.638 / 1.275 | 0.640 / 1.279 |
| `int[] foreach` | 0.638 / 1.276 | 0.639 / 1.278 | 0.639 / 1.278 |
| `ECS for + getKeyAt` | 0.956 / 1.912 | 0.957 / 1.915 | 0.956 / 1.913 |
| `ECS foreach dict + item.Key` | 1.591 / 3.182 | 1.591 / 3.182 | 1.592 / 3.184 |
| `ECS foreach dict.Keys` | 1.269 / 2.539 | 1.269 / 2.538 | 1.258 / 2.516 |
| `ECS Keys手动Enumerator` | 1.270 / 2.540 | 1.269 / 2.538 | 1.270 / 2.540 |
| `Dictionary foreach` | 13.611 / 27.222 | 13.647 / 27.294 | 13.795 / 27.591 |
| `Dictionary foreach Keys` | 4.978 / 9.956 | 5.000 / 10.000 | 4.983 / 9.966 |
| `Dictionary Keys手动Enumerator` | 4.945 / 9.891 | 4.950 / 9.899 | 4.922 / 9.845 |

#### Enumerator：仅读取 Value.mHP

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `RoleData[] for` | 2.573 / 5.146 | 2.639 / 5.277 | 2.763 / 5.526 |
| `RoleData[] foreach` | 2.580 / 5.159 | 2.640 / 5.279 | 2.758 / 5.517 |
| `ECS for + getValueAt` | 0.743 / 1.486 | 2.432 / 4.864 | 6.645 / 13.290 |
| `ECS foreach dict + Value` | 1.709 / 3.418 | 2.801 / 5.601 | 8.454 / 16.907 |
| `ECS foreach dict.Values` | 1.079 / 2.158 | 2.347 / 4.694 | 6.597 / 13.194 |
| `ECS Values手动Enumerator` | 1.063 / 2.125 | 2.344 / 4.687 | 6.584 / 13.168 |
| `Dictionary foreach` | 13.543 / 27.085 | 13.548 / 27.096 | 13.805 / 27.609 |
| `Dictionary foreach Values` | 5.713 / 11.425 | 5.623 / 11.246 | 5.548 / 11.097 |
| `Dictionary Values手动Enumerator` | 5.701 / 11.401 | 5.706 / 11.411 | 5.585 / 11.171 |

#### Enumerator：修改 Value.mHP

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `ECS for + getValueAt` | 1.061 / 2.122 | 2.487 / 4.974 | 6.748 / 13.496 |
| `ECS foreach dict + Value` | 1.805 / 3.609 | 2.906 / 5.811 | 8.628 / 17.256 |
| `ECS foreach dict.Values` | 1.103 / 2.205 | 2.469 / 4.939 | 6.679 / 13.357 |
| `ECS Values手动Enumerator` | 1.109 / 2.218 | 2.465 / 4.930 | 6.675 / 13.350 |
| `ECS Direct Column` | 0.781 / 1.563 | 0.954 / 1.908 | 1.168 / 2.335 |

#### Enumerator：同时读取 Key + Value

| 测试项 | Unsafe | SafeSpan | SafeRegistry |
|---|---:|---:|---:|
| `ECS for getKeyAt+getValueAt` | 1.606 / 3.211 | 3.125 / 6.250 | 7.478 / 14.956 |
| `ECS foreach dict` | 2.057 / 4.115 | 3.117 / 6.234 | 8.317 / 16.633 |
| `ECS 手动Enumerator` | 2.053 / 4.106 | 3.126 / 6.251 | 8.332 / 16.664 |
| `Dictionary foreach` | 14.000 / 27.999 | 14.039 / 28.078 | 13.888 / 27.776 |
| `Dictionary 手动Enumerator` | 13.589 / 27.178 | 13.735 / 27.470 | 13.669 / 27.338 |


### Benchmark 结论

从 PC 与 Android 真机结果可以得到几条比较稳定的结论：

- **Unsafe 是 unmanaged 数据的最高性能路径。** Android ARM64 + IL2CPP 下，List 单字段 `ECS Direct` 为 `0.750 ms`，`ECS Ref` 为 `1.063 ms`；Dictionary 连续存储单字段场景中 `Dense int[]` 为 `0.743 ms`，`ECS Direct` 为 `0.748 ms`，已经非常接近手写连续数组。
- **SafeSpan 是安全路径的性能默认选择。** 相比 SafeRegistry，Ref / indexer 在多字段访问时明显更快，同时 Direct Column 仍然保持较低成本。
- **SafeRegistry 的主要定位是旧运行环境与热更新兼容。** 它的 Ref 在多字段热点中成本明显更高，但 Direct Column 与 SafeSpan 很接近，因此兼容模式下仍然可以通过 Direct Column 获得良好的批处理性能。
- **随机 Dictionary 查询的主要成本来自 Hash 查找。** Android 真机随机读取时三个 Backend 的 EasyECS 路径差距远小于 Dense 批处理场景，说明数据布局优化最适合连续、高频访问。
- **热点循环优先使用 Direct Column。** 普通业务逻辑仍建议使用 `Ref` / indexer，只有 Profiler 确认的高频批处理才需要下沉到 Direct Column。

Benchmark 是特定设备、Unity 版本、编译器和运行状态下的测量结果，不代表所有项目都能得到完全相同的倍率。建议通过 `Samples~/Benchmark` 在目标项目和目标设备上重新运行。


## 已验证环境

当前版本实际验证过：

```text
Unity 6000.3.21f1
Windows x64
Android ARM64
IL2CPP
```

Android 真机已经覆盖 Unsafe、SafeSpan、SafeRegistry 三条 Backend 路径。

没有实际验证过的平台不在这里声明兼容性结论。

## Source Generator

Generator 源码位于：

```text
SourceGenerator~/ECSGenerator
```

Generator Test 位于：

```text
SourceGenerator~/ECSGeneratorTest
```

修改 Generator 后必须重新编译 `ECSGenerator.dll` 并替换：

```text
Analyzers/ECSGenerator.dll
```

`ECSGenerator.dll.meta` 中的 `RoslynAnalyzer` label 必须保留。

## License

MIT License。
