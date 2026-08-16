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
