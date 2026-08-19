# EasyECS

EasyECS 是一个面向 Unity/C# 的高性能数据容器方案。

它保留接近 `List<T>` / `Dictionary<TKey,TValue>` 的使用体验，同时通过 Source Generator 将 `[ECS] struct` 中适合 ECS 的字段拆分成 SoA（Structure of Arrays）连续存储，并提供 Direct Column、字段级 `ByXXX`、DenseIndex 等高性能访问方式。

从 **1.3.0** 开始，EasyECS 增加了**可选 Burst Integration**：

- 项目可以使用 Burst 时，大规模纯数据计算可以直接在 EasyECS 原生 SoA Column 上运行 Burst Job。
- 项目没有 Burst 时，不影响 EasyECS 原有功能和性能。
- Managed Hybrid 数据可以只让可 Burst 的数值 Column 使用 Burst，`string` / `object` 等字段仍继续使用普通 EasyECS。
- Burst 不会成为 EasyECS 的强制依赖。

从 **1.4.0** 开始，EasyECS 增加了**内置基础类型容器（BuiltIn Containers）**：

- 常用基础类型不再需要为了 EasyECS 手工声明一层包装 struct。
- 可以直接使用 `Int_ECSList`、`Vector2_ECSList`、`Color32_ECSList` 等容器。
- 同时提供 `Int_ECSDictionary<TKey>`、`Vector2_ECSDictionary<TKey>` 等 Dictionary 版本。
- Unity 复合值类型会在底层自动拆成最细粒度的标量 SoA Column，例如 `Vector2 -> float x[] + float y[]`。
- 普通使用保持接近 `List<T>` / `Dictionary<TKey,TValue>`，热点代码仍可以直接访问拆分后的 Column。

---

## 主要特性

- `[ECS] struct` 自动生成 SoA 容器。
- 支持 Unsafe、SafeSpan、SafeRegistry 多后端。
- 支持 `[NotECS]` 字段，与 ECS Column 保持同一逻辑行。
- 提供接近 `List<T>` 的增删改查、Range、Sort、BinarySearch、CopyTo、ToArray 等 API。
- 提供 `ECSDictionary<TKey>`，支持 Key → DenseIndex → SoA 数据访问。
- 为 ECS 字段自动生成 `ByXXX` 高速 API。
- 支持 Direct Column 连续访问。
- 支持 DenseIndex，一次 Key 查找后连续访问多个字段。
- Managed Hybrid：同一 struct 可同时包含 unmanaged ECS Column 和 managed Column。
- 1.4.0 起提供常用基础类型 BuiltIn ECSList / ECSDictionary，无需额外声明包装 struct。
- 复合 BuiltIn 类型自动拆分为标量 SoA，例如 `Vector2 -> x/y`、`Color32 -> r/g/b/a`。
- 1.3.0 起支持可选 Burst `BurstView` / `IJobParallelFor`。
- Burst 直接操作原有 native Column，不复制 ECS 数据。

---

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

# 1. 基础定义

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

`mHP`、`mSpeed`、`mPositionX`、`mPositionY` 会按 ECS Column 存储。

`[NotECS]` 字段仍属于同一条 `RoleData`，但不会作为独立 ECS Column 参与字段级 Direct API。

Source Generator 会生成对应的：

```text
RoleDataECSList
RoleDataECSDictionary<TKey>
```

---

# 2. ECSList 基础使用

```csharp
var list = new RoleDataECSList();
list.Add(new RoleData
{
	mHP = 100,
	mSpeed = 5.0f,
	mPositionX = 10.0f,
	mPositionY = 20.0f,
	mID = 1,
	mModelID = 1001,
	mCamp = 1,
});

RoleData value = list[0];
value.mHP = 80;
list[0] = value;

list.Dispose();
```

普通业务逻辑可以继续使用接近 `List<T>` 的写法。

EasyECS 还支持常用的：

```text
Add
AddRange
Insert
InsertRange
Remove
RemoveAt
RemoveRange
RemoveAll
Contains
IndexOf
LastIndexOf
Reverse
Sort
BinarySearch
EnsureCapacity
TrimExcess
CopyTo
ToArray
Exists
Find
FindIndex
FindLast
FindLastIndex
TrueForAll
```

---


# 3. BuiltIn 基础类型容器（1.4.0）

1.4.0 开始，常用基础类型可以直接使用 EasyECS 容器，不需要为了获得 SoA 存储再手工声明：

```csharp
[ECS]
public struct IntData
{
	public int mValue;
}
```

对于 `int`，可以直接写：

```csharp
Int_ECSList list = new Int_ECSList();
list.Add(10);
list.Add(20);

int value = list[0];
list[1] = 30;
```

Dictionary 同样直接使用：

```csharp
Int_ECSDictionary<string> dict = new Int_ECSDictionary<string>();
dict.Add("HP", 100);
dict["HP"] = 120;

if (dict.TryGetValue("HP", out int hp))
{
	// use hp
}
```

## 命名规则

```text
<TypeName>_ECSList
<TypeName>_ECSDictionary<TKey>
```

例如：

```text
List<int>                 -> Int_ECSList
List<Vector2>             -> Vector2_ECSList
List<Vector2Int>          -> Vector2Int_ECSList
Dictionary<int, Vector2>  -> Vector2_ECSDictionary<int>
```

当前内置类型包括：

```text
Byte / SByte
Short / UShort
Int / UInt
Long / ULong
Float / Double
Bool / Char / Decimal

Vector2 / Vector2Int
Vector3 / Vector3Int
Vector4
Quaternion
Color / Color32
Rect / RectInt
Bounds / BoundsInt
Matrix4x4
```

## 普通使用保持接近 List<T>

```csharp
Vector2_ECSList positions = new Vector2_ECSList();
positions.Add(new Vector2(10.0f, 20.0f));
positions.Add(new Vector2(30.0f, 40.0f));

Vector2 value = positions[0];
positions[1] = new Vector2(50.0f, 60.0f);

positions.Insert(1, new Vector2(15.0f, 25.0f));
positions.RemoveAt(0);

Vector2[] array = positions.ToArray();
```

BuiltIn ECSList 同样支持 EasyECS 已有的 List 风格 API，包括 AddRange、InsertRange、RemoveRange、Sort、BinarySearch、CopyTo、ToArray、EnsureCapacity、TrimExcess 等。

## 复合基础类型自动拆成标量 SoA

BuiltIn 的目标不是简单把 Unity struct 整体放进一个 Column，而是尽可能拆成最细粒度的标量 Column。

```text
Vector2
-> float x[]
-> float y[]

Vector2Int
-> int x[]
-> int y[]

Vector3
-> float x[]
-> float y[]
-> float z[]

Vector4 / Quaternion
-> float x[]
-> float y[]
-> float z[]
-> float w[]

Color
-> float r[]
-> float g[]
-> float b[]
-> float a[]

Color32
-> byte r[]
-> byte g[]
-> byte b[]
-> byte a[]

Rect
-> float x[] / y[] / width[] / height[]

RectInt
-> int x[] / y[] / width[] / height[]

Bounds
-> float centerX[] / centerY[] / centerZ[]
-> float sizeX[] / sizeY[] / sizeZ[]

BoundsInt
-> int positionX[] / positionY[] / positionZ[]
-> int sizeX[] / sizeY[] / sizeZ[]

Matrix4x4
-> 16 个 float Column：m00[] ... m33[]
```

因此只修改 `Vector2.x` 时，可以只访问 x Column：

```csharp
var x = positions.getXColumn();
for (int i = 0; i < positions.Count; ++i)
{
	x[i] += 1.0f;
}
```

不会为了修改 x 同时读取 y。

需要同时访问 x/y 时：

```csharp
var x = positions.getXColumn();
var y = positions.getYColumn();
for (int i = 0; i < positions.Count; ++i)
{
	x[i] += 1.0f;
	y[i] += 2.0f;
}
```

## BuiltIn 与 Burst

Unsafe Backend 下，BuiltIn 的 `BurstView` 直接暴露拆分后的 SoA 指针。

`Vector2_ECSList.BurstView` 的数据形态类似：

```csharp
public readonly unsafe struct BurstView
{
	public readonly float* x;
	public readonly float* y;
	public readonly int Count;
}
```

Burst Job 可以只处理需要的 Column，不需要先把 `Vector2` 数组转换成 NativeArray，也不需要维护第二份数据。

## BuiltIn Dictionary

BuiltIn Dictionary 使用 Key -> DenseIndex -> BuiltIn ECSList 的结构：

```text
Dictionary<TKey, int> IndexMap
        ↓
DenseIndex
        ↓
BuiltIn ECSList SoA Columns
```

因此同样支持：

```csharp
Vector2_ECSDictionary<int> dict = new Vector2_ECSDictionary<int>();
dict.Add(1001, new Vector2(10.0f, 20.0f));

int index = dict.GetIndex(1001);
var x = dict.getXColumn();
var y = dict.getYColumn();
x[index] += 1.0f;
y[index] += 2.0f;
```

## Unsafe 不会传染业务程序集

EasyECS Package 自己的 `EasyECS.Runtime` 开启：

```json
{
	"name": "EasyECS.Runtime",
	"allowUnsafeCode": true
}
```

这是为了让 Package 内部生成的 BuiltIn 默认使用 Unsafe native SoA Backend，并获得 Direct pointer / BurstView 等最高性能能力。

业务程序集不需要因此开启 `allowUnsafeCode`，仍然可以正常使用：

```csharp
Vector2_ECSList list = new Vector2_ECSList();
list.Add(Vector2.zero);
```

用户自己声明的 `[ECS] struct` 仍按照**声明它的程序集环境**自动选择 Backend：

```text
允许 Unsafe
-> Unsafe

不允许 Unsafe + Span 可用
-> SafeSpan

Unsafe / Span 都不可用
-> SafeRegistry
```

EasyECS 的原则始终是：环境能力决定生成哪一种实现，而不是决定能不能使用 EasyECS。

## 关于 ref 与结构修改

单标量 BuiltIn（例如 `Int_ECSList`）可以直接引用底层连续 Column。高级代码可以使用 ref，但不要跨越 Resize、Insert、Remove 等结构修改长期保存底层引用。

普通 List 风格使用不需要关心这一点。

---

# 4. Direct Column

连续遍历热点代码优先使用 Direct Column，而不是逐元素还原完整 struct。

例如：

```csharp
var hp = list.getHPColumn();
var speed = list.getSpeedColumn();
var positionX = list.getPositionXColumn();

for (int i = 0; i < list.Count; ++i)
{
	if (hp[i] <= 0)
	{
		continue;
	}
	positionX[i] += speed[i] * deltaTime;
}
```

这种写法可以直接连续访问 SoA Column，避免每次重新组合完整 `RoleData`。

### 推荐原则

```text
普通、偶发访问       -> Indexer / StructRef / 普通 API
连续热点循环         -> Direct Column
```

---

# 5. 字段级 ByXXX API

对于真实 ECS 字段，Source Generator 会自动生成字段级高速 API。

例如 `mHP` 会生成类似：

```csharp
list.ContainsByHP(value);
list.IndexOfByHP(value);
list.LastIndexOfByHP(value);
list.ExistsByHP(match);
list.FindIndexByHP(match);
list.RemoveAllByHP(match);
list.SortByHP();
list.BinarySearchByHP(value);
```

Dictionary 会生成：

```csharp
dict.GetValueByHP(key);
dict.TryGetValueByHP(key, out int hp);
dict.SetValueByHP(key, hp);
dict.TrySetValueByHP(key, hp);
```

`ByHP` 中的 `HP` 来自字段 `mHP`，不是 EasyECS 写死的特殊字段。

例如：

```csharp
public float mSpeed;
public float mPositionX;
```

会对应生成：

```text
BySpeed
ByPositionX
```

### 适合场景

只需要随机访问一个 ECS 字段时，优先使用 `ByXXX`，避免读取/写回完整 struct。

---

# 6. ECSDictionary

```csharp
var dict = new RoleDataECSDictionary<int>();

dict.Add(10001, new RoleData
{
	mHP = 100,
	mSpeed = 5.0f,
});

RoleData value = dict[10001];
```

ECSDictionary 内部将 Key 映射到连续 DenseIndex，再由 DenseIndex 访问 EasyECS 的 SoA 数据。

支持：

```text
Add
TryAdd
Remove
Remove(key, out value)
ContainsKey
TryGetValue
SetValue
TrySetValue
SetOrAdd
GetOrAdd
ContainsValue
EnsureCapacity
TrimExcess
Keys
Values
foreach
```

---

# 7. Dictionary 单字段热点

只修改一个字段时：

```csharp
dict.SetValueByHP(roleID, newHP);
```

优于先读取完整 `RoleData`、修改 `mHP`、再把完整 struct 写回。

读取同理：

```csharp
int hp = dict.GetValueByHP(roleID);
```

---

# 8. Dictionary 多字段热点：DenseIndex + Direct

如果同一个 Key 一次需要访问多个字段，不要连续调用多个 `ByXXX`，因为每次都会重新查 Key。

不推荐：

```csharp
dict.SetValueByHP(roleID, hp);
dict.SetValueBySpeed(roleID, speed);
dict.SetValueByPositionX(roleID, x);
dict.SetValueByPositionY(roleID, y);
```

推荐：

```csharp
int index = dict.GetIndex(roleID);
var hpColumn = dict.getHPColumn();
var speedColumn = dict.getSpeedColumn();
var xColumn = dict.getPositionXColumn();
var yColumn = dict.getPositionYColumn();

hpColumn[index] = hp;
speedColumn[index] = speed;
xColumn[index] = x;
yColumn[index] = y;
```

这样 Key 只查找一次。

还可以使用：

```csharp
if (dict.TryGetIndex(roleID, out int index))
{
	// Direct Column access
}
```

以及：

```csharp
int index = dict.GetOrAddIndex(roleID, defaultValue);
```

如果需要知道是否本次创建：

```csharp
int index = dict.GetOrAddIndex(roleID, defaultValue, out bool added);
```

> DenseIndex 不应长期缓存。Dictionary 发生 SwapBack 删除后，其他元素的 DenseIndex 可能变化。

---

# 9. Managed Hybrid

EasyECS 支持 ECS struct 中存在 managed 字段：

```csharp
[ECS]
public struct MonsterData
{
	public int mHP;
	public float mSpeed;
	public string mName;
	public object mController;
	[NotECS] public int mID;
}
```

EasyECS 会根据字段类型选择对应存储方式：

```text
mHP / mSpeed        -> ECS SoA Column
mName / mController -> Managed Column
[NotECS]            -> 非 ECS 数据
```

这样可以保留一个业务 struct 的使用体验，同时让适合连续计算的数据仍然获得 SoA 优势。

---

# 10. Burst Integration（1.3.0）

Burst 是**可选加速层**。

EasyECS 不要求所有项目安装 Burst：

```text
没有 Burst
    -> 使用原 EasyECS

有 Burst + Unsafe 后端
    -> 原 EasyECS
       + BurstView
       + Burst Job
```

Source Generator 会检测当前编译环境是否存在 Burst / Jobs 所需类型。

不存在时不会生成 Burst API，因此不会给普通项目增加 Burst 编译依赖。

---

# 11. BurstView

对于 Burst-compatible ECS 字段，会生成 `BurstView`。

例如 `RoleData` 会生成类似：

```csharp
public readonly unsafe struct BurstView
{
	public readonly int* mHP;
	public readonly float* mSpeed;
	public readonly float* mPositionX;
	public readonly float* mPositionY;
	public readonly int Count;
}
```

获取：

```csharp
RoleDataECSList.BurstView view = list.GetBurstView();
```

`BurstView` 直接引用 EasyECS 原有 Column：

```text
EasyECS native SoA data
        │
        ├── Direct C#
        │
        └── BurstView -> Burst Job
```

没有中间数据复制。

---

# 12. Burst IJobParallelFor

推荐将大量、纯数据、可并行的连续计算放进 `IJobParallelFor`。

```csharp
using Unity.Burst;
using Unity.Jobs;

[BurstCompile]
public unsafe struct RoleMoveJob : IJobParallelFor
{
	public RoleDataECSList.BurstView mData;
	public float mDeltaTime;
	public void Execute(int index)
	{
		if (mData.mHP[index] <= 0)
		{
			return;
		}
		mData.mPositionX[index] += mData.mSpeed[index] * mDeltaTime;
	}
}
```

调度：

```csharp
RoleDataECSList.BurstView view = list.GetBurstView();

JobHandle handle = list.ScheduleBurst(new RoleMoveJob
{
	mData = view,
	mDeltaTime = deltaTime,
}, 256);

// 主线程可以继续执行不冲突的逻辑

list.CompleteBurstJobs();
```

`ScheduleBurst` 会自动串联该 ECSList 之前已经登记的 Burst Job。

---

# 13. 自定义 Job 调度

如果不是直接通过 EasyECS 的 `ScheduleBurst` 调度，可以手动串依赖：

```csharp
RoleDataECSList.BurstView view = list.GetBurstView();
JobHandle dependency = list.GetBurstDependency();

JobHandle handle = new RoleMoveJob
{
	mData = view,
	mDeltaTime = deltaTime,
}.Schedule(view.Count, 256, dependency);

list.RegisterBurstJob(handle);
```

重要：所有访问该 `BurstView` 的外部 Job 都必须登记回来。

否则 EasyECS 无法知道还有 Job 正在使用 native Column。

---

# 14. Burst 与 Managed Hybrid

有 managed 字段不代表整个 ECS struct 都不能使用 Burst。

例如：

```csharp
[ECS]
public struct MonsterData
{
	public int mHP;
	public float mSpeed;
	public UnityEngine.Vector3 mPosition;
	public string mName;
	public object mController;
}
```

BurstView 只暴露 Burst-compatible 字段：

```text
mHP       -> Burst
mSpeed    -> Burst
mPosition -> Burst

mName       -> 普通 EasyECS
mController -> 普通 EasyECS
```

因此可以把一帧逻辑拆为：

```text
Burst Job
位置 / 速度 / 距离 / 数值计算
        ↓
Complete
        ↓
普通 C#
string / object / UnityEngine.Object / UI / 业务对象
```

---

# 15. Burst 使用限制

BurstView 直接持有 native pointers，因此必须遵守生命周期规则。

## Job 运行期间不要进行冲突访问

错误示例：

```csharp
list.ScheduleBurst(job);

// Job 可能仍在运行
list.RemoveAt(0);       // 不推荐
list.SortByHP();        // 不推荐
list.getHPColumn()[0] = 100; // 与Job冲突写入
```

正确方式：

```csharp
list.ScheduleBurst(job);
list.CompleteBurstJobs();

list.RemoveAt(0);
```

## Resize / Dispose

EasyECS 会跟踪已经登记的 Job。

在需要迁移或释放 native memory 时，会先 Complete 已登记 Job，避免旧指针继续被后台线程使用。

## 外部 Job 必须 Register

使用 `GetBurstView()` 后自行 Schedule 的 Job，必须：

```text
GetBurstDependency
        ↓
Schedule
        ↓
RegisterBurstJob
```

否则 EasyECS 无法保护其生命周期。

---

# 16. 哪些场景应该使用 Burst

推荐 Burst：

```text
大量角色/怪物位置更新
速度积分
距离计算
属性批处理
Buff 数值计算
AOI 粗筛
碰撞粗筛
状态检查
大量实体数学运算
其他可并行纯数据循环
```

继续使用普通 EasyECS：

```text
数据量很小
每个 Entity 工作量很轻且 Job 调度成本更高
string / object
UnityEngine.Object
GameObject / MonoBehaviour 业务
UI
资源管理
普通 managed callback
无法安全并行的逻辑
```

---

# 17. Burst 性能实测

1.3.0 Burst Benchmark：

```text
EntityCount: 500000

EasyECS Direct C#          0.264 ms
EasyECS Burst IJob         0.363 ms
EasyECS Burst ParallelFor  0.066 ms
```

对应：

```text
Direct C# / Burst ParallelFor ≈ 4.01x
```

该结果说明：

- EasyECS 原有 Direct 已经非常快。
- 单线程 `IJob` 不一定比 Direct 更快。
- 大规模、可并行的纯数据处理是 Burst Integration 最有价值的场景。
- 性能倍率会随 CPU、Entity 数量、每 Entity 工作量、BatchCount 等因素变化，应以实际项目 Benchmark 为准。

---

# 18. 推荐性能层级

EasyECS 1.4.0 建议按下面的层级选择 API：

```text
普通业务代码
    ↓
List / Dictionary 风格 API

单字段随机热点
    ↓
ByXXX

多字段随机热点
    ↓
GetIndex / TryGetIndex / GetOrAddIndex
+ Direct Column

连续热点循环
    ↓
Direct Column

大规模可并行纯数据计算
    ↓
BurstView + IJobParallelFor
```

不是所有代码都应该强行 Burst。

EasyECS 的目标是：

> 能使用 Burst 的项目和数据尽可能利用 Burst；不能使用 Burst 的项目和数据仍然享受 EasyECS 原有的 SoA / Direct / ByXXX / DenseIndex 性能优势。

---

# 19. 后端说明

EasyECS 当前支持：

```text
Unsafe
SafeSpan
SafeRegistry
```

Burst Integration 1.3.0 只针对 Unsafe native Column 原地加速。

SafeSpan / SafeRegistry 不进行 NativeArray 中转，也不会为了 Burst 复制数据。

这样可以避免：

```text
Managed Data
    ↓ Copy
NativeArray
    ↓ Burst
NativeArray
    ↓ Copy Back
Managed Data
```

在轻量逻辑中反而抵消 Burst 收益。

---

# 20. Source Generator 更新注意事项

EasyECS 的实际 Source Generator 由：

```text
Analyzers/ECSGenerator.dll
```

提供给 Unity。

仅修改：

```text
SourceGenerator~/ECSGenerator/ECSGenerator.cs
```

不会自动让 Unity 使用新 Generator。

修改 Generator 后需要：

```text
修改 ECSGenerator.cs
        ↓
重新编译 ECSGenerator 项目
        ↓
得到新的 ECSGenerator.dll
        ↓
覆盖 Analyzers/ECSGenerator.dll
        ↓
让 Unity 重新导入
```

---

# 21. Dispose

EasyECS 管理 native memory 的容器应正确释放：

```csharp
var list = new RoleDataECSList();
try
{
	// use list
}
finally
{
	list.Dispose();
}
```

Burst 场景中，正常 `Dispose()` 会先完成 EasyECS 已登记的 Job，再释放 native memory。

不要依赖 Finalizer 代替正常 Dispose。

---

# 22. 总结

EasyECS 的核心并不是要求业务代码全面改写成传统 ECS，而是让普通 C# struct 和常用基础类型在保留自然使用方式的同时，按热点程度逐步使用更高性能的访问方式：

```text
普通 API / BuiltIn List 风格 API
→ ByXXX / DenseIndex
→ Direct Column
→ Burst ParallelFor
```

1.4.0 的 BuiltIn 容器进一步减少了为了性能而声明简单包装 struct 的样板代码，并让 `Vector2`、`Color32`、`Matrix4x4` 等复合基础类型直接获得标量 SoA 布局。

没有 Burst 的项目仍然可以使用普通 API、ByXXX、DenseIndex 和 Direct Column。

支持 Burst 的项目则可以继续直接利用同一份 native SoA 数据进行多线程高性能计算。
