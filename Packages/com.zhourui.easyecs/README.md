# EasyECS

EasyECS 是一个面向 Unity/C# 的高性能数据容器方案。

它保留接近 `List<T>` / `Dictionary<TKey,TValue>` 的使用体验，同时通过 Source Generator 将 `[ECS] struct` 中适合 ECS 的字段拆分成 SoA（Structure of Arrays）连续存储，并提供 Direct Column、字段级 `ByXXX`、DenseIndex 等高性能访问方式。

当前版本提供：

- `[ECS] struct` 自动生成 SoA 容器。
- 接近 `List<T>` / `Dictionary<TKey,TValue>` 的使用体验。
- Unsafe、SafeSpan、SafeRegistry 多后端。
- Direct Column、字段级 `ByXXX`、DenseIndex 等热点访问方式。
- Managed Hybrid，可同时存储 unmanaged ECS Column 与 `string` / `object` 等 managed 字段。
- BuiltIn ECSList / ECSDictionary，可直接使用 `Int_ECSList`、`Vector2_ECSList`、`Color32_ECSList` 等容器。
- Unity 复合值类型自动拆分为标量 SoA Column，例如 `Vector2 -> x/y`、`Color32 -> r/g/b/a`。
- 可选 Burst Integration，直接在 EasyECS 原生 SoA Column 上执行 Burst Job，不复制数据。
- `BurstView`、`ScheduleBurst`、Job 依赖管理。
- 通用 Burst Chunk：`GetChunkCount`、`GetChunkRange`、`ScheduleBurstChunk`。
- 默认 ChunkSize 为 `8192`。
- Burst 自动负责 SIMD 向量化，EasyECS 专注 SoA 数据布局、连续访问与 Chunk 多核并行。
- `EasyECS.Runtime` 不强制依赖 Unity Burst；没有 Burst 的项目仍可使用其他 EasyECS 能力。

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
- 提供常用基础类型 BuiltIn ECSList / ECSDictionary，无需额外声明包装 struct。
- 复合 BuiltIn 类型自动拆分为标量 SoA，例如 `Vector2 -> x/y`、`Color32 -> r/g/b/a`。
- 支持可选 Burst `BurstView` / `IJobParallelFor`。
- Burst 直接操作原有 native Column，不复制 ECS 数据。
- 支持 `GetChunkCount` / `GetChunkRange` / `ScheduleBurstChunk`。
- Chunk Job 在一个 `Execute(chunkIndex)` 内连续处理一段 SoA 数据，降低逐元素 Job 调度开销。
- Burst 自动负责 SIMD 向量化，不提供独立显式 SIMD API。

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
RoleData_ECSList
RoleData_ECSDictionary<TKey>
```

---

# 2. ECSList 基础使用

```csharp
var list = new RoleData_ECSList();
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


# 3. BuiltIn 基础类型容器

常用基础类型可以直接使用 EasyECS 容器，不需要为了获得 SoA 存储再手工声明：

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
var dict = new RoleData_ECSDictionary<int>();

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

# 10. Burst Integration

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

Source Generator 会根据当前编译环境选择可用能力。Unsafe + Jobs 环境下可以生成 `BurstView` / Job / Chunk 相关接口；`EasyECS.Runtime` 本身不强制引用 Unity Burst。

项目安装 Burst 后，由实际业务 Job 使用 `[BurstCompile]` 即可；没有 Burst 时，EasyECS 原有 List / Dictionary / SoA / Direct 等能力不受影响。

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
RoleData_ECSList.BurstView view = list.GetBurstView();
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
	public RoleData_ECSList.BurstView mData;
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
RoleData_ECSList.BurstView view = list.GetBurstView();

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

# 13. Burst Chunk

普通 `ScheduleBurst` 仍然保持逐元素语义：

```text
EntityCount = N
Execute(0)
Execute(1)
Execute(2)
...
Execute(N - 1)
```

其中 `innerloopBatchCount` 只是 Unity Job Scheduler 的批量调度参数，**不会**把 `Execute(index)` 变成逻辑 Chunk。

Burst Chunk 的逻辑是：

```text
ChunkSize = 8192

Execute(Chunk 0) -> [0, 8192)
Execute(Chunk 1) -> [8192, 16384)
Execute(Chunk 2) -> [16384, 24576)
...
```

一个 Worker 拿到 Chunk 后，在 Chunk 内连续处理一整段 SoA 数据。

## Chunk API

默认 ChunkSize：

```csharp
int chunkSize = RoleData_ECSList.DefaultBurstChunkSize; // 8192
```

获取 Chunk 数量：

```csharp
RoleData_ECSList.BurstView view = list.GetBurstView();
int chunkCount = view.GetChunkCount(chunkSize);
```

获取当前 Chunk 范围：

```csharp
view.GetChunkRange(chunkIndex, chunkSize, out int start, out int count);
```

Chunk Job：

```csharp
using Unity.Burst;
using Unity.Jobs;

[BurstCompile]
public unsafe struct RoleMoveChunkJob : IJobParallelFor
{
	public RoleData_ECSList.BurstView mData;
	public int mChunkSize;
	public float mDeltaTime;
	public void Execute(int chunkIndex)
	{
		mData.GetChunkRange(chunkIndex, mChunkSize, out int start, out int count);
		int end = start + count;
		for (int i = start; i < end; ++i)
		{
			if (mData.mHP[i] <= 0)
			{
				continue;
			}
			mData.mPositionX[i] += mData.mSpeed[i] * mDeltaTime;
		}
	}
}
```

调度：

```csharp
int chunkSize = RoleData_ECSList.DefaultBurstChunkSize;
list.ScheduleBurstChunk(new RoleMoveChunkJob
{
	mData = list.GetBurstView(),
	mChunkSize = chunkSize,
	mDeltaTime = deltaTime,
}, chunkSize);

list.CompleteBurstJobs();
```

`ScheduleBurstChunk` 会使用当前 ECSList 已登记的 Burst Job 作为依赖，并把新 Job Handle 继续登记回容器。

## SIMD 策略

EasyECS 当前不提供额外的显式 SIMD API。

对于连续 SoA 数值循环，推荐直接编写简单、连续、Burst-friendly 的循环，由 Burst 根据目标平台自动完成向量化。EasyECS 本身负责提供适合向量化的数据布局与访问方式：

```text
数据布局      -> EasyECS SoA
连续热点访问  -> Direct Column / BurstView
向量化        -> Burst Auto Vectorization
多核批处理    -> Burst Chunk
```

因此当前公开 API 中没有 `SIMDAdd`、`SIMDMad`、`ParallelSIMD*`、`ScheduleSIMD*`、`ScheduleBurstSIMD`、`GetSIMDChunk*` 等独立 SIMD 接口。

---

# 14. 自定义 Job 调度

如果不是直接通过 EasyECS 的 `ScheduleBurst` 调度，可以手动串依赖：

```csharp
RoleData_ECSList.BurstView view = list.GetBurstView();
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

# 15. Burst 与 Managed Hybrid

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

# 16. Burst 使用限制

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

# 17. 哪些场景应该使用 Burst

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

# 18. 代表性性能实测

EasyECS 的 Benchmark 不只测试单一 API，而是分别验证 List/Dictionary 数据布局、Direct 热点、BuiltIn 生成质量、Burst 和 Chunk。

下面列出当前版本中具有代表性的 Benchmark。不同测试的规模和负载不同，**不应该把不同表之间的绝对毫秒值直接横向比较**；每组结果用于说明对应 API 的性能特征。

## 18.1 ECSList：连续热点为什么推荐 Direct Column

测试条件：

```text
EntityCount: 500000
Backend: Unsafe
```

| 场景 | List<RoleData> | RoleData[] | ECS list[i] | ECS Ref | ECS Direct |
|---|---:|---:|---:|---:|---:|
| 修改 1 个字段 | 3.834 ms | 0.286 ms | 0.245 ms | 0.267 ms | **0.162 ms** |
| 访问 2 个字段 | 3.964 ms | 0.280 ms | 0.306 ms | 0.287 ms | **0.125 ms** |
| 访问 4 个字段 | 2.339 ms | 0.829 ms | 0.926 ms | 0.658 ms | **0.331 ms** |

这组测试最重要的不是 `List<T>` 与 EasyECS 的倍率，而是 EasyECS 内部不同访问层级的差异：热点连续循环中，Direct Column 避免逐元素重新组合 struct，通常是最合适的路径。

因此推荐：

```text
普通业务访问 -> Indexer / Ref
连续热点循环 -> Direct Column
```

## 18.2 ECSDictionary：随机 Key + Dense 数据并存

EasyECS Dictionary 的结构是：

```text
Key -> DenseIndex -> SoA Columns
```

在“Dense 全量更新 + 10% 随机 Key 修改”的混合测试中：

| 实现 | Median |
|---|---:|
| Dictionary<int, RoleData> | 13.203 ms |
| IndexMap + RoleData[] | 1.348 ms |
| EasyECS Direct + LocalRef | **0.897 ms** |

对应：

```text
Standard / ECS : 14.72x
Manual / ECS   : 1.50x
```

同时，在 Key + Value 全量 foreach 的代表性测试中：

```text
ECS foreach        : 0.284 ms
Dictionary foreach : 4.027 ms
Dictionary / ECS   : 14.17x
```

这说明 ECSDictionary 的价值不只是“做一个更快的 Dictionary”，而是让随机 Key 查询与 Dense SoA 批处理共存。

## 18.3 BuiltIn Containers：避免包装 struct，同时保持接近手写 ECS 的性能

BuiltIn Benchmark 的目标不是让 `Int_ECSList` 神奇地快过等价手写 ECS，而是验证 Source Generator 为基础类型生成的容器没有明显额外成本。

代表性 Burst `SharedJob + ContainerSchedule`：

| 类型 | 手写 ECS | BuiltIn | Median Ratio |
|---|---:|---:|---:|
| Int | 0.433 ms | 0.434 ms | 1.003x |
| Vector2 | 0.802 ms | 0.799 ms | 0.995x |
| Vector2Int | 0.798 ms | 0.814 ms | 1.020x |

结果基本处于同一性能等级。微基准中 1%~2% 的差异会受到执行顺序、线程调度、缓存状态影响，因此 BuiltIn 的判断重点是“与等价手写 ECS 接近”，而不是追求每一项都严格 `< 1.00x`。

## 18.4 Burst ParallelFor

代表性 Burst Benchmark：

```text
EntityCount: 500000

EasyECS Direct C#           0.264 ms
EasyECS Burst IJob          0.363 ms
EasyECS Burst ParallelFor   0.066 ms
```

对应：

```text
Direct C# / Burst ParallelFor ≈ 4.01x
```

这组结果建立了两个重要原则：

- EasyECS Direct 本身已经很快。
- 单线程 `IJob` 不保证比 Direct C# 更快。
- Burst 真正有价值的是大规模、可并行、纯数据计算。

## 18.5 Burst Chunk

Burst Chunk Benchmark：

```text
Unity:       6000.3.21f1
Platform:    Windows Player
EntityCount: 2000000
SampleCount: 21
WarmupCount: 5
ElementBatch:256
ChunkSize:   8192
```

| 测试 | Burst Single IJob | Element ParallelFor | Chunk Direct | Chunk Container | Container / Single | Container / Element |
|---|---:|---:|---:|---:|---:|---:|
| Float MAD | 0.205 ms | 0.079 ms | 0.072 ms | **0.075 ms** | 2.740x | 1.055x |
| Int Add | 0.204 ms | 0.068 ms | 0.053 ms | **0.043 ms** | 4.766x | 1.585x |
| Vector2 MAD | 0.413 ms | 0.097 ms | 0.073 ms | **0.076 ms** | 5.454x | 1.280x |
| Color32 Add | 0.204 ms | 0.074 ms | 0.048 ms | **0.044 ms** | 4.597x | 1.655x |

几何平均：

```text
Chunk Direct / Single Geomean Speedup    : 4.029x
Chunk Container / Single Geomean Speedup : 4.254x
Chunk Container / ElementParallel        : 1.372x
```

这组结果说明：对于大规模连续 SoA 批处理，Chunk 能在逐元素 ParallelFor 的基础上继续降低调度粒度并提高连续处理效率。

需要注意：`Chunk Direct` 与 `Chunk Container` 本质上使用同类 Job 调度模型，个别测试里 Container 比 Direct 更快并不代表封装本身产生了额外加速，微小差异应视为调度/缓存噪声。真正有意义的结论是：

```text
Chunk Direct ≈ Chunk Container
并且 Chunk 对大规模连续批处理有明确收益
```

性能倍率会随 CPU、Entity 数量、字段数量、每 Entity 工作量、ChunkSize 和实际算法变化，应以目标项目与目标设备 Benchmark 为准。

---

# 19. 推荐性能层级

当前版本建议按下面的层级选择 API：

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

大规模连续批处理
    ↓
BurstView + ScheduleBurstChunk
```

不是所有代码都应该强行 Burst。

EasyECS 的目标是：

> 能使用 Burst 的项目和数据尽可能利用 Burst；不能使用 Burst 的项目和数据仍然享受 EasyECS 原有的 SoA / Direct / ByXXX / DenseIndex 性能优势。

---

# 20. 后端说明

EasyECS 当前支持：

```text
Unsafe
SafeSpan
SafeRegistry
```

Burst Integration 只针对 Unsafe native Column 原地加速。

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

# 21. Source Generator 更新注意事项

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

# 22. Dispose

EasyECS 管理 native memory 的容器应正确释放：

```csharp
var list = new RoleData_ECSList();
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

# 23. 总结

EasyECS 的核心并不是要求业务代码全面改写成传统 ECS，而是让普通 C# struct 和常用基础类型在保留自然使用方式的同时，按热点程度逐步使用更高性能的访问方式：

```text
普通 API / BuiltIn List 风格 API
→ ByXXX / DenseIndex
→ Direct Column
→ Burst ParallelFor
→ Burst Chunk
```

BuiltIn 容器减少了为了性能而声明简单包装 struct 的样板代码，并让 `Vector2`、`Color32`、`Matrix4x4` 等复合基础类型直接获得标量 SoA 布局。

Burst Chunk 负责大规模连续数据的逻辑分块；EasyECS 负责 SoA 数据布局与连续 Chunk，多核调度交给 Job System，SIMD 向量化交给 Burst 自动完成。

没有 Burst 的项目仍然可以使用普通 API、ByXXX、DenseIndex 和 Direct Column。

支持 Burst 的项目则可以继续直接利用同一份 native SoA 数据进行多线程高性能计算，并在大规模连续批处理中使用 `ScheduleBurstChunk`。
