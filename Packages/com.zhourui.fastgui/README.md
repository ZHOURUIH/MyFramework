# FastGUI 2.0.0

FastGUI 是一个面向 Unity 游戏 UI 与大规模 2D Sprite 场景的高性能渲染框架。

2.0.0 主要包含两部分：

- **FastGUI UI**：使用 `FastCanvas / FastImage / FastText / FastRawImage` 等组件替代 UGUI 的高频重建路径。
- **FastSpriteRenderer**：面向大量 Sprite 的高性能渲染组件，用于替代原生 `SpriteRenderer` 的高 CPU 开销场景。

FastGUI 仍然保留常规的：

```text
GameObject
RectTransform / Transform
Component
Hierarchy
Inspector
Prefab
```

开发工作流。

UI 部分不依赖 UGUI 的 `Graphic / Canvas` 重建链路，而由 `FastCanvas` 集中维护：

```text
Geometry
Transform
Visibility
Batch
Clip / Stencil
Vertex Stream
Index Stream
GPU Buffer
```

FastSpriteRenderer 则针对大量 Sprite 的 Transform、Sprite、排序、动画和批量提交进行集中处理，并在支持的平台使用 GPU Driven 路径。

FastGUI 的目标很直接：

> **尽量保留 Unity 原有的开发体验，同时显著降低大型动态 UI 和大规模 2D Sprite 场景的 CPU 渲染成本。**

当前正式版本：**2.0.0**

UPM 包名：

```text
com.zhourui.fastgui
```

---
## 1. 安装

FastGUI 依赖：

- Unity Burst
- TextMesh Pro
- EasyECS Runtime

新项目需要先安装 EasyECS，再安装 FastGUI。

### 1.1 安装 EasyECS

在 Unity Package Manager 中选择：

```text
Add package from git URL...
```

输入：

```text
https://github.com/ZHOURUIH/MyFramework.git?path=/Packages/com.zhourui.easyecs
```

或者
```text
https://gitee.com/inothingtodo/MyFramework.git?path=/Packages/com.zhourui.easyecs
```

### 1.2 安装 FastGUI

再添加：

```text
https://github.com/ZHOURUIH/MyFramework.git?path=/Packages/com.zhourui.fastgui
```

或者
```text
https://gitee.com/inothingtodo/MyFramework.git?path=/Packages/com.zhourui.fastgui
```

---

## 2. 快速开始

### 2.1 在 Hierarchy 创建

正式版 Editor 入口：

```text
GameObject
└─ UI
   └─ FastGUI
      ├─ Canvas
      ├─ Image
      ├─ Raw Image
      ├─ Text
      ├─ Input Field
      ├─ Visibility Group
      └─ Rect Mask 2D
```

创建普通 FastGUI 元素时：

- 如果当前选择节点已经位于 `FastCanvas` 下，会直接创建为其子节点。
- 如果场景中没有可用的 `FastCanvas`，Editor 会自动创建。
- 新建 Image 默认采用常见的中心 Anchor / Pivot 与 `100×100` 尺寸。
- UI Layer 会遵循父节点 / Canvas 的 UI 工作流。

### 2.2 FastImage

普通 UI 图片优先使用 `FastImage + Sprite`：

```csharp
FastImage image = GetComponent<FastImage>();
image.setSprite(iconSprite);
image.setColor(Color.white);
image.setVisible(true);
```

适合：

- 图标
- 按钮背景
- 面板
- 九宫格
- Tiled
- Filled 进度条 / 冷却

Inspector 按接近 UGUI Image 的使用顺序组织：

```text
Source Image
Color
Material
Image Type
  Simple / Sliced / Tiled / Filled
对应类型参数
Set Native Size
```

FastGUI 自身的 Canvas 绑定和运行时状态放在高级区域，不干扰日常编辑。

### 2.3 FastRawImage

需要直接使用 `Texture` 时使用 `FastRawImage`：

```csharp
FastRawImage image = GetComponent<FastRawImage>();
image.setTexture(texture);
image.setColor(Color.white);
```

典型场景：

- RenderTexture
- 摄像机画面
- 视频纹理
- 运行时下载图片
- 不需要 Sprite Border / Filled 语义的动态 Texture

### 2.4 FastText

```csharp
FastText text = GetComponent<FastText>();
text.setText("HP 1024");
text.setFontSize(28.0f);
text.setColor(Color.white);
```

FastText 使用 TMP Font Asset / SDF 材质，支持常见文本布局、对齐、换行、颜色和多 Atlas / Fallback 场景。

### 2.5 显隐

如果只需要 FastGUI 渲染层面的轻量显隐，优先：

```csharp
fastImage.setVisible(false);
```

或对子树使用 `FastUIVisibility`。

如果业务语义要求完整 Unity 生命周期，例如需要触发：

- `OnEnable`
- `OnDisable`
- 其他组件的激活状态

则继续正常使用：

```csharp
gameObject.SetActive(false);
```

两者语义不同，不应只按 Benchmark 数字互换。

---

## 3. 核心组件

### 3.1 FastCanvas

FastGUI 的核心渲染入口，集中管理：

- RenderElement 注册
- Dirty 状态
- Geometry
- Position Stream
- Color Stream
- UV Stream
- Index Stream
- Batch
- Transform Range
- Clip
- Stencil
- Visibility
- Text Geometry
- GPU Buffer

业务 UI 不需要自己管理这些内部结构。

### 3.2 FastImage

Sprite UI 组件，支持：

- Simple
- Preserve Aspect
- Sliced
- Tiled
- Filled

### 3.3 FastRawImage

直接 Texture UI 组件。

### 3.4 FastText

TMP Font Asset 驱动的文本组件，针对大量文本更新和重复文本布局进行了集中批处理。

### 3.5 FastRectMask2D

矩形裁剪组件。FastGUI 会维护裁剪层级、可见状态以及增量裁剪更新。

### 3.6 FastMask

基于 Stencil 的 Mask。

### 3.7 FastSOARenderGroup

用于大量重复结构 UI 的可选组件。它不会改变 GameObject / RectTransform 节点模型，而是在 RenderOrder / Batch 热路径上提供连续数据访问。

### 3.8 FastUIVisibility

轻量 UI 子树显隐，不强制触发整个 GameObject 生命周期。

### 3.9 FastSpriteRenderer

面向大量 2D Sprite 的高性能渲染组件。

它继续使用标准 `GameObject / Transform / Component` 工作流，但把大量 Sprite 的运行时状态、排序和提交集中处理。

2.0.0 的正式真机 Benchmark 中，FastSpriteRenderer 在支持的平台使用 GPU Indirect 路径；不支持时保留 CPU fallback。

典型场景包括：

- MMO / ARPG 场景角色
- 大量怪物与 NPC
- 2D 特效
- 大量动态 Sprite
- 高频移动、动画和 SortingOrder 变化

---

## 4. 为什么 FastGUI 的 CPU 开销更低

FastGUI 的核心不是“换一个 Image 组件”，而是改变高频 UI 更新的组织方式。

```text
业务属性变化
    ↓
精确 Dirty 标记
    ↓
FastCanvas 集中处理
    ↓
Position / Geometry / Color / UV / Index / Batch 分别判断
    ↓
只更新真正发生变化的数据
    ↓
局部 Buffer Patch
```

主要设计包括：

### 4.1 Stable Geometry Slot

大量普通属性变化不需要重新创建完整 Mesh。

### 4.2 Independent Dirty Streams

FastGUI 不把所有变化都理解成“Mesh Dirty”，而是区分：

```text
Position
Geometry
Color
UV
Index
Batch
Clip
Visibility
```

例如只改变颜色时，不需要重新计算 Position Geometry。

### 4.3 FastCanvas 集中更新

单个组件不各自驱动 Canvas rebuild，而由 FastCanvas 在统一阶段处理：

```text
Collect
Merge
Process
Upload
Submit
```

这样更容易：

- 合并 Dirty Range
- 消除重复工作
- 批量更新
- 复用 Hierarchy 信息
- 复用 Transform 结果

### 4.4 Transform Translation Fast Path

大型 Content 滚动时，大量元素的几何形状本身没有变化，变化的是父节点 Translation。

FastGUI 会尽量复用已有 Geometry，只更新最终 Position。

### 4.5 EasyECS SoA / Direct Column

热点 Runtime 数据采用连续数据访问，减少：

```text
Managed Object Jump
Dictionary Lookup
Component Traversal
Random Memory Access
```

让大量连续 UI Element 更适合批处理。

### 4.6 文本缓存与批处理

FastText 对：

- 数字
- 短文本
- 高频内容变化
- 重复布局
- Primary Atlas

等常见场景减少重复 TMP 计算。

### 4.7 Direct GPU Buffer / 局部上传

在支持的平台路径中，FastGUI 尽量只更新真正发生变化的 Vertex / Index Stream 区域。

Index 使用 Compact Layout，并把逻辑 Index 与最终 Submit Index 分离，避免无意义的全量重建与上传。

### 4.8 FastSpriteRenderer GPU Driven

FastSpriteRenderer 会把大量 Sprite 的运行时状态集中维护，并针对不同变化类型分别处理：

```text
Transform
Sprite
Sorting
Active
Animation
Batch
GPU Upload
```

在支持 Compute Shader / GPU Indirect 的平台优先使用 GPU Driven 路径；其他平台保留 fallback。

这种设计的重点不是追求最少 DrawCall，而是降低大量 Sprite 场景中的 CPU 更新、排序和 Render Submission 成本。

---

## 5. Benchmark

本章集中放置 FastGUI 目前公开的完整 Benchmark 数据。

其中：

- **FastGUI UI vs UGUI**：数据来自正式 Android Publication Benchmark。
- **FastSpriteRenderer vs Unity SpriteRenderer**：数据来自 2.0.0 Final Release 真机 Benchmark。

FastGUI UI 使用正式 Android Player 进行了完整 Publication Benchmark。

测试覆盖：

```text
Creation
Prefab
Clone
31 CPU Cases
13 Render Cases
10 Sprite / Image Cases
34 Weakness Cases
Index Boundary
Runtime Correctness
```

## 5.1 测试环境

| 项目 | 配置 |
|---|---|
| Device | HUAWEI ALP-AL00 |
| OS | Android 10 / API 29 |
| CPU | ARM64 |
| CPU Cores | 8 |
| RAM | 3648 MB |
| GPU | Mali-G72 |
| Graphics API | Vulkan |
| Unity | 6000.3.21f1 |
| Scripting Backend | IL2CPP |
| Development Build | false |
| VSync | 0 |
| Target FPS | -1 |

主测试场景：

| 项目 | FastGUI | UGUI |
|---|---:|---:|
| Inventory Items | 2400 | 2400 |
| RectTransform | 28850 | 28848 |
| Render Elements / Graphics | 19233 | 19231 |
| Image | 14413 | 14413 |
| Text | 4818 | 4818 |
| Hierarchy Depth | 7 | 7 |
| Visual Elements / Item | 8 | 8 |
| Text / Item | 2 | 2 |

两边使用：

```text
相同业务结构
相同 SpriteAtlas
相同 Item 数量
相同视觉元素
相同文本数量
相同 Hierarchy Depth
```

CPU Benchmark 使用：

```text
FastGUI → UGUI → UGUI → FastGUI
```

交叉顺序。

稳定性判定：

```text
Spread <= 20%
```

---

## 5.2 FastGUI UI Benchmark 总结

| Benchmark | 结果 |
|---|---:|
| 2400 Item Create | **1.52x** |
| 2400 Item First Full Flush | **1.48x** |
| 2400 Item Total Ready | **1.52x** |
| Prefab Load → Ready | **1.63x** |
| Prefab First Full Flush | **3.35x** |
| Object.Instantiate → Ready | **1.69x** |
| FastUICloneUtility → Ready | **3.03x** |
| CPU Cases | **31** |
| CPU Stable Cases | **23 / 31** |
| Stable CPU FastGUI Wins | **23 / 23** |
| Stable CPU Median | **19.31x** |
| Stable CPU Minimum | **2.15x** |
| Stable CPU Maximum | **265.08x** |
| Render Cases | **13** |
| Wall Median FastGUI Wins | **13 / 13** |
| Wall P95 FastGUI Wins | **13 / 13** |
| Render Median Speedup | **3.86x** |
| Sprite / Image Cases | **10 / 10 FastGUI Wins** |
| Weakness Cases | **34 / 34 FastGUI Wins** |
| Weakness Worst Case | **7.10x** |
| UInt16 → UInt32 Boundary | **Pass** |
| Final Runtime Validation | **Pass** |

正式稳定 CPU Case：

```text
FastGUI >= 10% faster    23
UGUI >= 10% faster        0
Within 10%                0
```

---

## 5.3 Creation

2400 个 Inventory Item 一次性创建：

| Case | FastGUI | UGUI | UGUI / FastGUI |
|---|---:|---:|---:|
| Create | 4159.110 ms | 6339.066 ms | **1.52x** |
| First Full Flush | 266.090 ms | 394.331 ms | **1.48x** |
| Total Ready | 4425.200 ms | 6733.397 ms | **1.52x** |

FastGUI 仍然创建标准：

```text
GameObject
RectTransform
Component
```

即使没有替换 Unity 节点模型，从创建到真正可用仍然比 UGUI 快约 **1.52x**。

---

## 5.4 Prefab

大型 Prefab：

```text
Items   500
Rects   5502
Images  2500
Texts   1000
```

Hierarchy 验证：

```text
Same Rects       True
Same Images      True
Same Texts       True
Hierarchy Match  True
```

| Case | FastGUI | UGUI | UGUI / FastGUI |
|---|---:|---:|---:|
| Cold Asset Load | 223.055 ms | 357.935 ms | **1.60x** |
| Instantiate | 258.528 ms | 346.771 ms | **1.34x** |
| First Full Flush | 50.026 ms | 167.467 ms | **3.35x** |
| Instantiate → Ready | 308.554 ms | 508.771 ms | **1.65x** |
| Load → Ready | 531.609 ms | 866.706 ms | **1.63x** |

Prefab 阶段最明显的差异来自首次完整 UI 更新：

```text
FastGUI    50.026 ms
UGUI      167.467 ms

UGUI / FastGUI = 3.35x
```

---

## 5.5 Clone

测试规模：

```text
Instances = 2000
Samples   = 3
```

### 5.5.1 Object.Instantiate

| Case | FastGUI | UGUI | UGUI / FastGUI |
|---|---:|---:|---:|
| Instantiate | 670.474 ms | 892.366 ms | **1.33x** |
| First Flush | 146.548 ms | 496.900 ms | **3.39x** |
| Total Ready | 817.022 ms | 1384.524 ms | **1.69x** |

### 5.5.2 FastUICloneUtility

| Case | FastUICloneUtility | FastGUI Instantiate | UGUI Instantiate |
|---|---:|---:|---:|
| Instantiate | **331.266 ms** | 670.474 ms | 892.366 ms |
| First Flush | **125.472 ms** | 146.548 ms | 496.900 ms |
| Total Ready | **456.739 ms** | 817.022 ms | 1384.524 ms |

最终倍率：

| 对比 | 倍率 |
|---|---:|
| UGUI Ready / FastGUI Object Ready | **1.69x** |
| UGUI Ready / FastUICloneUtility Ready | **3.03x** |
| FastGUI Object Ready / FastUICloneUtility Ready | **1.79x** |

FastUICloneUtility：

```text
BulkRegister = 16000
BulkFlush    = 1
```

它仍然创建标准 GameObject / RectTransform，并不是对象池或 Runtime Virtualization。

---

## 5.6 CPU Benchmark

CPU 统计范围：

```text
业务 Mutation
+
Immediate UI Update
```

FastGUI：

```text
Mutation + FastCanvas
```

UGUI：

```text
Mutation + Canvas.ForceUpdateCanvases
```

GPU execution 不包含在这一阶段。

最终结果：

```text
Cases                    31
Stable Cases             23

FastGUI Wins             23 / 23
UGUI Wins                 0 / 23

Median UGUI / FastGUI    19.31x
Minimum                   2.15x
Maximum                 265.08x
```

### 5.6.1 完整 31 Case

| # | Case | FastGUI | UGUI | UGUI / FastGUI | Stable |
|---:|---|---:|---:|---:|:---:|
| 1 | 单个道具完整刷新 | 0.206 ms | 54.599 ms | **265.08x** | ✓ |
| 2 | 可见区 800 道具混合刷新 | 5.276 ms | 89.091 ms | **16.89x** | ✓ |
| 3 | 2400 道具全量重绑 | 10.843 ms | 186.288 ms | **17.18x** | — |
| 4 | 1400 数量文字变化 | 1.914 ms | 109.423 ms | **57.18x** | ✓ |
| 5 | 1400 同图集 Sprite 变化 | 1.373 ms | 50.732 ms | **36.95x** | ✓ |
| 6 | Content 持续滚动 + Clip | 0.585 ms | 53.738 ms | **91.86x** | — |
| 7 | 选中道具 + 详情刷新 | 0.638 ms | 31.314 ms | **49.07x** | ✓ |
| 8 | 1400 Item Visibility / SetActive | 11.366 ms | 447.160 ms | **39.34x** | — |
| 9 | 整窗口 setVisible / SetActive | 0.030 ms | 597.366 ms | **19852.63x*** | — |
| 10 | 整窗口 SetActive 兼容路径 | 131.862 ms | 284.128 ms | **2.15x** | ✓ |
| 11 | 真实混合操作 | 6.093 ms | 64.643 ms | **10.61x** | ✓ |
| 12 | 1400 图标 Position | 2.618 ms | 50.555 ms | **19.31x** | ✓ |
| 13 | 800 Item Root 整体移动 | 4.038 ms | 50.333 ms | **12.47x** | ✓ |
| 14 | 1400 图标 Size | 4.746 ms | 52.621 ms | **11.09x** | ✓ |
| 15 | 1400 Quality Color | 1.110 ms | 51.053 ms | **45.98x** | ✓ |
| 16 | 1400 离散 Sprite 变化 | 2.714 ms | 51.957 ms | **19.15x** | ✓ |
| 17 | 1400 Leaf Visibility | 1.731 ms | 46.379 ms | **26.79x** | ✓ |
| 18 | 1400 Text Color | 1.177 ms | 75.979 ms | **64.58x** | — |
| 19 | 1400 Text FontSize | 1.199 ms | 76.880 ms | **64.10x** | ✓ |
| 20 | 800 Scale + Rotation | 3.846 ms | 49.915 ms | **12.98x** | ✓ |
| 21 | RectMask Size | 1.225 ms | 49.734 ms | **40.60x** | ✓ |
| 22 | 2200 Heavy Mixed Refresh | 40.976 ms | 279.937 ms | **6.83x** | ✓ |
| 23 | 2400 双文本内容变化 | 2.749 ms | 137.928 ms | **50.18x** | ✓ |
| 24 | 2400 Color + Visibility | 4.817 ms | 68.613 ms | **14.24x** | ✓ |
| 25 | 2400 Leaf Visibility | 2.726 ms | 54.944 ms | **20.15x** | ✓ |
| 26 | 1400 离散子树 Visibility | 11.688 ms | 802.932 ms | **68.70x** | — |
| 27 | 2200 Size + Sprite + Position | 11.701 ms | 67.025 ms | **5.73x** | ✓ |
| 28 | 2200 Root 批量移动 | 10.287 ms | 60.888 ms | **5.92x** | — |
| 29 | 1400 Material 变化 | 3.179 ms | 54.682 ms | **17.20x** | ✓ |
| 30 | RectMask Padding | 1.121 ms | 148.948 ms | **132.88x** | ✓ |
| 31 | RectMask Move + Size | 9.198 ms | 62.757 ms | **6.82x** | — |

`—` 表示 Sandwich Spread 超过 20%，因此不计入正式 Stable Case 胜负统计。

`*` 极小操作中 Immediate Flush 的固定成本会放大倍率，该数字不代表游戏 FPS 可以提升 19852 倍。

### 5.6.2 CPU 场景汇总

| 类型 | 代表 Case | UGUI / FastGUI |
|---|---|---:|
| 单元素刷新 | 单个道具完整刷新 | **265.08x** |
| 批量混合更新 | 800 Item Mixed | **16.89x** |
| Text | 1400 数量文字 | **57.18x** |
| Text | 2400 双文本 | **50.18x** |
| Sprite | 1400 同图集 Sprite | **36.95x** |
| Position | 1400 Position | **19.31x** |
| Root Transform | 800 Root Move | **12.47x** |
| Geometry | 2200 Size+Sprite+Position | **5.73x** |
| Visibility | 1400 Leaf Visibility | **26.79x** |
| Mask | RectMask Size | **40.60x** |
| Mask | RectMask Padding | **132.88x** |
| Heavy Mixed | 2200 Heavy Refresh | **6.83x** |
| SetActive Compatible | 整窗口 SetActive | **2.15x** |

即使只看更接近真实游戏压力的批量 Case，FastGUI 仍然保持明显优势，而不是只在微基准中占优。

---

## 5.7 End-to-End Render Benchmark

Render Benchmark：

```text
Cases          13
Warmup Frames   3
Sample Frames  20
Order           Fast / UGUI / UGUI / Fast
```

汇总：

| 指标 | 结果 |
|---|---:|
| Wall Median FastGUI Wins | **13 / 13** |
| Wall P95 FastGUI Wins | **13 / 13** |
| Median Wall Speedup | **3.86x** |
| Minimum Wall Speedup | **1.40x** |
| Maximum Wall Speedup | **30.49x** |

### 5.7.1 Wall Time

| # | Case | FastGUI Median | UGUI Median | UGUI / FastGUI | FastGUI P95 | UGUI P95 |
|---:|---|---:|---:|---:|---:|---:|
| 1 | 静态背包 | 33.219 ms | 46.431 ms | **1.40x** | 34.885 ms | 50.241 ms |
| 2 | 持续滚动 | 33.518 ms | 117.080 ms | **3.49x** | 35.141 ms | 122.262 ms |
| 3 | 1400 Sprite | 32.905 ms | 58.370 ms | **1.77x** | 34.029 ms | 60.819 ms |
| 4 | 1400 Text | 32.654 ms | 170.137 ms | **5.21x** | 34.449 ms | 177.714 ms |
| 5 | 1400 Leaf Visibility | 33.611 ms | 74.848 ms | **2.23x** | 35.950 ms | 100.206 ms |
| 6 | RectMask Size | 32.460 ms | 61.977 ms | **1.91x** | 34.979 ms | 65.532 ms |
| 7 | 2400 Full Rebind | 33.574 ms | 334.844 ms | **9.97x** | 35.313 ms | 340.591 ms |
| 8 | 1400 连续子树显隐 | 33.123 ms | 1009.786 ms | **30.49x** | 35.059 ms | 1521.139 ms |
| 9 | 1400 离散子树显隐 | 32.819 ms | 292.459 ms | **8.91x** | 37.735 ms | 409.908 ms |
| 10 | 2200 Heavy Mixed | 54.006 ms | 470.175 ms | **8.71x** | 60.270 ms | 480.853 ms |
| 11 | 1400 Material | 33.640 ms | 57.299 ms | **1.70x** | 35.597 ms | 58.265 ms |
| 12 | RectMask Padding | 33.123 ms | 186.128 ms | **5.62x** | 34.127 ms | 191.113 ms |
| 13 | 整窗口 SetActive | 196.278 ms | 757.642 ms | **3.86x** | 293.607 ms | 1001.845 ms |

### 5.7.2 Render Submission / Upload

| Case | Draw Calls F/U | Vertices F/U | VB Upload F/U | IB Upload F/U |
|---|---:|---:|---:|---:|
| 静态背包 | 15 / 6 | 17096 / 11580 | 0 / 0 B | 0 / 0 B |
| 持续滚动 | 15 / 6 | 16936 / 11456 | 0 / 1.04 MB | 0 / 41 KB |
| 1400 Sprite | 15 / 6 | 17096 / 11580 | 0 / 1.05 MB | 0 / 42 KB |
| 1400 Text | 15 / 6 | 17096 / 11580 | 0 / 1.05 MB | 0 / 42 KB |
| Leaf Visibility | 15 / 8 | 18008 / 14583 | 0 / 1.30 MB | 1.85 MB / 51 KB |
| RectMask Size | 15 / 6 | 16760 / 11354 | 0 / 1.04 MB | 0 / 41 KB |
| Full Rebind | 15 / 8 | 19466 / 16227 | 0 / 1.28 MB | 1.85 MB / 50 KB |
| 连续子树显隐 | 15 / 4 | 9590 / 6272 | 0 / 2.64 MB | 1.85 MB / 104 KB |
| 离散子树显隐 | 14.5 / 6 | 10753 / 6230 | 0 / 1.28 MB | 1.85 MB / 50 KB |
| Heavy Mixed | 15 / 6 | 19331 / 11829 | 0 / 1.63 MB | 1.85 MB / 64 KB |
| Material | 15 / 10 | 17096 / 11836 | 0 / 1.88 MB | 1.85 MB / 74 KB |
| RectMask Padding | 15 / 6 | 16760 / 11610 | 0 / 1.86 MB | 0 / 74 KB |
| SetActive | 7.5 / 3 | 8548 / 5918 | 0 / 0.94 MB | 0 / 37 KB |

FastGUI 并不追求最少 DrawCall 或最少 submitted vertex，而是优先减少 CPU rebuild 与频繁 Buffer Upload。

即使不少 Case 中 FastGUI DrawCall 更多，13 个 Render Case 的 Wall Median 仍然全部低于 UGUI。

---

## 5.8 Sprite / Image Benchmark

结果：

```text
Cases        10
FastGUI Win  10
UGUI Win      0
Near Parity   0
```

| # | Case | Changed | FastGUI | UGUI | UGUI / FastGUI |
|---:|---|---:|---:|---:|---:|
| 1 | Sprite Swap | 1 / 2048 | 0.0017 ms | 44.8171 ms | **25980.94x*** |
| 2 | Sprite Swap | 128 / 2048 | 0.0652 ms | 47.4867 ms | **728.67x*** |
| 3 | Sprite Swap | 1024 / 2048 | 0.4207 ms | 63.1187 ms | **150.03x** |
| 4 | Sprite Swap | 2048 / 2048 | 0.8595 ms | 79.8352 ms | **92.88x** |
| 5 | Simple Size | 1 / 2048 | 0.0029 ms | 44.8840 ms | **15312.23x*** |
| 6 | Simple Size | 128 / 2048 | 0.1588 ms | 47.9898 ms | **302.17x** |
| 7 | Simple Size | 1024 / 2048 | 1.7324 ms | 66.0009 ms | **38.10x** |
| 8 | Simple Size | 2048 / 2048 | 4.0680 ms | 85.3068 ms | **20.97x** |
| 9 | Sliced Size | 512 / 512 | 0.8618 ms | 55.1810 ms | **64.03x** |
| 10 | Filled Amount | 512 / 512 | 0.4974 ms | 53.7868 ms | **108.14x** |

即使只看全量 2048 个 Simple Image Size Mutation：

```text
FastGUI   4.068 ms
UGUI     85.307 ms

UGUI / FastGUI = 20.97x
```

---

## 5.9 Weakness Probe

Weakness Probe 的目的不是寻找 FastGUI 最擅长的 Case，而是主动寻找：

```text
FastGUI slower
FastGUI near parity
FastGUI scaling weakness
```

最终：

```text
Cases           34
FastGUI Worse    0
Near Parity      0
FastGUI Win     34
```

### 5.9.1 完整 34 Case

| # | Case | 配置 | FastGUI | UGUI | UGUI / FastGUI |
|---:|---|---|---:|---:|---:|
| 1 | Sparse Position | 1 / 2048 | 0.0028 ms | 44.7930 ms | **16178.07x*** |
| 2 | Sparse Position | 16 / 2048 | 0.0120 ms | 44.6996 ms | **3711.44x*** |
| 3 | Sparse Position | 128 / 2048 | 0.0871 ms | 45.4393 ms | **521.65x** |
| 4 | Sparse Position | 512 / 2048 | 0.2775 ms | 45.3501 ms | **163.42x** |
| 5 | Sparse Position | 2048 / 2048 | 1.1599 ms | 45.6002 ms | **39.31x** |
| 6 | Size Mutation | 1 / 2048 | 0.0037 ms | 45.5802 ms | **12175.02x*** |
| 7 | Size Mutation | 16 / 2048 | 0.0249 ms | 45.8755 ms | **1839.62x*** |
| 8 | Size Mutation | 128 / 2048 | 0.1599 ms | 48.0848 ms | **300.78x** |
| 9 | Size Mutation | 512 / 2048 | 0.6200 ms | 56.0528 ms | **90.41x** |
| 10 | Size Mutation | 2048 / 2048 | 4.2825 ms | 84.6002 ms | **19.75x** |
| 11 | Sibling Reorder | 1 / 2048 | 4.5545 ms | 44.8685 ms | **9.85x** |
| 12 | Sibling Reorder | 16 / 2048 | 4.5001 ms | 44.6328 ms | **9.92x** |
| 13 | Sibling Reorder | 128 / 2048 | 5.1325 ms | 45.4916 ms | **8.86x** |
| 14 | Batch Fragmentation | Batch 1 | 1.0201 ms | 47.9040 ms | **46.96x** |
| 15 | Batch Fragmentation | Batch 32 | 0.9469 ms | 47.9913 ms | **50.68x** |
| 16 | Batch Fragmentation | Batch 256 | 1.0593 ms | 48.1796 ms | **45.48x** |
| 17 | Batch Fragmentation | Batch 2048 | 3.1846 ms | 47.9682 ms | **15.06x** |
| 18 | Deep Hierarchy Root Move | Depth 1 | 0.0597 ms | 44.9848 ms | **753.51x*** |
| 19 | Deep Hierarchy Root Move | Depth 4 | 0.0591 ms | 44.6918 ms | **756.05x*** |
| 20 | Deep Hierarchy Root Move | Depth 8 | 0.0547 ms | 44.5156 ms | **814.00x*** |
| 21 | Deep Hierarchy Root Move | Depth 16 | 0.0551 ms | 45.2927 ms | **822.76x*** |
| 22 | Nested RectMask Resize | Depth 1 | 0.0789 ms | 46.1405 ms | **584.98x*** |
| 23 | Nested RectMask Resize | Depth 2 | 0.0858 ms | 46.0365 ms | **536.32x*** |
| 24 | Nested RectMask Resize | Depth 4 | 0.1057 ms | 45.8340 ms | **433.65x*** |
| 25 | Nested RectMask Resize | Depth 8 | 0.4335 ms | 46.2505 ms | **106.68x** |
| 26 | Many Canvas Dirty One | 1 Canvas | 0.0017 ms | 44.9319 ms | **26047.50x*** |
| 27 | Many Canvas Dirty One | 4 Canvas | 0.0068 ms | 45.2223 ms | **6613.87x*** |
| 28 | Many Canvas Dirty One | 16 Canvas | 0.0406 ms | 44.7811 ms | **1102.30x*** |
| 29 | Many Canvas Dirty One | 64 Canvas | 0.1780 ms | 45.0954 ms | **253.31x** |
| 30 | Short Text Mutation | 1 / 512 | 0.0068 ms | 45.5417 ms | **6722.02x*** |
| 31 | Short Text Mutation | 32 / 512 | 0.0848 ms | 47.0566 ms | **554.91x** |
| 32 | Short Text Mutation | 128 / 512 | 0.3245 ms | 50.9523 ms | **157.01x** |
| 33 | Short Text Mutation | 512 / 512 | 2.2961 ms | 66.4445 ms | **28.94x** |
| 34 | Wrapped Long Text | 128 / 512 | 11.7410 ms | 83.3431 ms | **7.10x** |

Weakness Probe 中最接近 UGUI 的 Case：

```text
Wrapped Long Text

FastGUI   11.741 ms
UGUI      83.343 ms

UGUI / FastGUI = 7.10x
```

即使是专门用于寻找 FastGUI 弱点的测试，最弱的一项仍然保持 **7.10x**。

---

## 5.10 Index Boundary

专门测试 16-bit Index 边界：

| Element Count | Vertex Span | Index Format |
|---:|---:|---|
| 16000 | 64000 | UInt16 |
| 16512 | 66048 | UInt32 |

最终：

```text
UInt16ToUInt32 = True
```

说明大规模 UI 超过 65535 Vertex 后可以正确切换到 UInt32 Index。

---

## 5.11 Runtime Correctness Validation

Publication Benchmark 不只测试性能，同时检查输出正确性。

| Validation | Result |
|---|:---:|
| Visual Layout | ✓ |
| Vertex Stream Bounds | ✓ |
| Window Geometry | ✓ |
| Item Geometry | ✓ |
| Icon Geometry | ✓ |
| Quality Geometry | ✓ |
| Clip / Stencil | ✓ |
| Batch Slot | ✓ |
| Sparse SOA | ✓ |
| Nested SOA | ✓ |
| Invalid SOA Fallback | ✓ |
| UInt16 → UInt32 | ✓ |
| TMP Layout | ✓ |
| TMP Vertex | ✓ |
| Font Source | ✓ |

最终 Runtime：

```text
CompactMode = True
Indices     = 277650
TMPLayout   = True
TMPVertex   = True
FontSource  = True
Valid       = True
```

非法 Sparse SOA：

```text
Cycle
Type Mismatch
Empty Record
Stencil Barrier
Nested Conflict
```

全部正确执行：

```text
FallbackNormal = True
Valid          = True
```

无法安全应用 SOA 优化时，FastGUI 会回退普通绘制顺序，而不是以渲染正确性换性能。

---

## 5.12 FastGUI UI Benchmark 最终结论

FastGUI UI 在本次正式 Android Publication Benchmark 中：

| 类别 | FastGUI 结果 |
|---|---:|
| Creation | **3 / 3 更快** |
| Prefab | **5 / 5 更快** |
| Clone | **全部更快** |
| Stable CPU | **23 / 23 更快** |
| Render Wall Median | **13 / 13 更快** |
| Render Wall P95 | **13 / 13 更快** |
| Sprite / Image | **10 / 10 更快** |
| Weakness Probe | **34 / 34 更快** |
| Index Boundary | **Pass** |
| Correctness | **Pass** |

几个最具有代表性的综合数字：

| 指标 | UGUI / FastGUI |
|---|---:|
| 2400 Item Ready | **1.52x** |
| Prefab Ready | **1.65x** |
| FastUICloneUtility Ready | **3.03x** |
| Stable CPU Median | **19.31x** |
| Render Wall Median 中位优势 | **3.86x** |
| Heavy Mixed CPU | **6.83x** |
| Heavy Mixed Render Wall | **8.71x** |
| Full Rebind Render Wall | **9.97x** |
| Wrapped Long Text Weakness | **7.10x** |

所以这组 UI Benchmark 的结论不是：

```text
FastGUI 在某几个特殊微基准里比 UGUI 快
```

而是：

```text
Creation            Faster
Prefab              Faster
Clone               Faster
Runtime Mutation    Faster
Text                Faster
Sprite              Faster
Transform           Faster
Geometry            Faster
Visibility          Faster
Mask                Faster
Heavy Mixed UI      Faster
End-to-End Wall     Faster
```

同时保留：

```text
GameObject
RectTransform
Prefab
Hierarchy
Inspector
Component
```

这一套 Unity UI 工作流。

FastGUI 的核心目标就是：

> **不为了性能放弃 Unity UI 的开发体验，而是在尽量保留这种体验的前提下，大幅降低大型动态 UI 的更新成本。**

---

## 5.13 FastSpriteRenderer 2.0.0 测试环境

FastSpriteRenderer 最终 Release Benchmark 使用：

| 项目 | 配置 |
|---|---|
| Device | HUAWEI ALP-AL00 |
| OS | Android 10 / API 29 |
| CPU | ARM64 |
| CPU Cores | 8 |
| RAM | 3648 MB |
| Unity | 6000.3.21f1 |
| Scripting Backend | IL2CPP |
| Build | Release |
| Stripping | Enabled |
| Backend | GPU Indirect |
| Compare | FastSpriteRenderer / Unity SpriteRenderer |

测试使用 ABBA / Fast-Native-Fast 方式降低设备状态和顺序影响。

FastSpriteRenderer 的主要对比指标：

```text
MainThread Median
Wall Median / P95
RenderSubmit CPU
Mutation
DrawCalls
Order Correctness
```

---

## 5.14 FastSpriteRenderer Quick Benchmark

Quick Suite：

```text
Cases       14 / 14
OrderCheck  116 / 0
```

完整 14 Case：

| Count | Case | MainCPU Fast | Native | Native / Fast | Wall Median F/N | RenderSubmit F/N | Mutation F/N | DrawCalls F/N | OrderCheck |
|---:|---|---:|---:|---:|---:|---:|---:|---:|:---:|
| 1000 | Static | 1.8268 ms | 5.0250 ms | **2.75x** | 17.1087/16.5914 ms | 0.1844/3.8612 ms | 0.0010/0.0013 ms | 3.0/3.0 | 0/0 |
| 1000 | Move10Percent | 2.2237 ms | 6.9443 ms | **3.12x** | 16.4115/16.5998 ms | 0.2542/5.9886 ms | 0.2362/0.2987 ms | 5.0/5.0 | 0/0 |
| 1000 | SpriteSwap10Percent | 2.2102 ms | 6.1185 ms | **2.77x** | 16.4821/16.3798 ms | 0.3646/6.0685 ms | 0.1288/0.1351 ms | 5.0/5.0 | 0/0 |
| 1000 | SortingOrder10Percent | 4.2922 ms | 6.5734 ms | **1.53x** | 16.3615/16.4803 ms | 0.3628/6.7162 ms | 0.1199/0.1077 ms | 5.0/5.0 | 0/0 |
| 1000 | TiledResize10Percent | 5.2406 ms | 10.8557 ms | **2.07x** | 16.2764/27.9277 ms | 2.2386/8.3917 ms | 0.1234/0.2969 ms | 204.0/8.0 | 0/0 |
| 1000 | RealGameOrderBoundaryCross | 3.7372 ms | 7.9870 ms | **2.14x** | 15.9190/16.4365 ms | 0.5000/7.1490 ms | 0.0433/0.0331 ms | 19.0/39.1 | 15/0 |
| 1000 | RealGameOrderGapInsert | 3.7419 ms | 7.7227 ms | **2.06x** | 15.7578/16.4440 ms | 0.5175/7.1682 ms | 0.0295/0.0320 ms | 19.0/39.7 | 15/0 |
| 1000 | RealGameOrderPingPong | 3.7294 ms | 7.8089 ms | **2.09x** | 15.8617/16.4487 ms | 0.4990/7.1256 ms | 0.0432/0.0331 ms | 19.0/40.1 | 15/0 |
| 6000 | RealGameGameplayLoop | 5.7573 ms | 16.2979 ms | **2.83x** | 16.2390/16.3242 ms | 0.8536/19.6696 ms | 1.0258/0.7357 ms | 48.0/85.0 | 1/0 |
| 6000 | RealGameIdle | 3.2331 ms | 13.4414 ms | **4.16x** | 16.4800/16.5053 ms | 0.9131/19.5242 ms | 0.0016/0.0016 ms | 48.0/85.0 | 0/0 |
| 6000 | RealGameMoveAndSort | 5.3042 ms | 16.5948 ms | **3.13x** | 16.3601/16.9013 ms | 0.6932/19.5964 ms | 0.6104/0.4727 ms | 48.0/85.0 | 1/0 |
| 6000 | RealGameAnimationStorm | 5.5273 ms | 14.2448 ms | **2.58x** | 16.5073/16.3378 ms | 0.8766/19.9284 ms | 1.6261/1.1483 ms | 48.0/85.0 | 0/0 |
| 6000 | RealGameDepthSortChurn | 5.7602 ms | 17.7523 ms | **3.08x** | 16.8886/17.6104 ms | 0.8930/19.2919 ms | 0.7261/0.5146 ms | 48.0/85.0 | 1/0 |
| 6000 | RealGamePoolChurn | 5.6693 ms | 14.7221 ms | **2.60x** | 16.2940/16.2245 ms | 0.8378/19.1758 ms | 2.1472/1.5182 ms | 48.0/85.0 | 1/0 |

Quick Suite 同时覆盖：

- Static
- Transform Move
- Sprite Swap
- SortingOrder
- Tiled Resize
- Sorting boundary / gap / ping-pong
- Gameplay Loop
- Idle
- Move + Sort
- Animation Storm
- Depth Sort
- Pool Churn

所有 Correctness Case 均通过。

---

## 5.15 FastSpriteRenderer ProductionScaling Benchmark

ProductionScaling：

```text
Cases       18 / 18
OrderCheck   24 / 0
```

规模：

```text
10,000
30,000
50,000
```

每个规模均测试 6 个真实场景 Case：

```text
GameplayLoop
Idle
MoveAndSort
AnimationStorm
DepthSortChurn
PoolChurn
```

完整 18 Case：

| Count | Case | MainCPU Fast | Native | Native / Fast | Wall Median F/N | RenderSubmit F/N | Mutation F/N | DrawCalls F/N | OrderCheck |
|---:|---|---:|---:|---:|---:|---:|---:|---:|:---:|
| 10000 | RealGameGameplayLoop | 5.6674 ms | 21.0932 ms | **3.72x** | 16.7118/20.8951 ms | 0.6117/26.5016 ms | 1.5811/1.2151 ms | 72.0/130.0 | 1/0 |
| 10000 | RealGameIdle | 3.1401 ms | 17.5109 ms | **5.58x** | 16.4667/19.1739 ms | 1.1904/28.1506 ms | 0.0015/0.0016 ms | 74.0/132.0 | 0/0 |
| 10000 | RealGameMoveAndSort | 5.4393 ms | 21.5310 ms | **3.96x** | 16.4307/21.1568 ms | 0.6599/27.8446 ms | 0.9610/0.7825 ms | 74.0/132.0 | 1/0 |
| 10000 | RealGameAnimationStorm | 5.4036 ms | 18.8815 ms | **3.49x** | 16.3836/18.8850 ms | 1.1907/27.4328 ms | 2.1378/1.8100 ms | 74.0/132.0 | 0/0 |
| 10000 | RealGameDepthSortChurn | 5.6417 ms | 22.9625 ms | **4.07x** | 16.4581/22.5310 ms | 0.9317/27.6724 ms | 1.0937/0.8323 ms | 74.0/132.0 | 1/0 |
| 10000 | RealGamePoolChurn | 6.8195 ms | 19.5792 ms | **2.87x** | 16.5172/19.4659 ms | 0.6256/27.2959 ms | 3.3802/2.4103 ms | 74.0/132.0 | 1/0 |
| 30000 | RealGameGameplayLoop | 11.0466 ms | 54.7943 ms | **4.96x** | 16.5857/52.8943 ms | 1.4722/65.3480 ms | 3.8707/4.5560 ms | 204.0/231.0 | 1/0 |
| 30000 | RealGameIdle | 5.6878 ms | 41.7995 ms | **7.35x** | 16.9993/42.9063 ms | 2.8016/65.0461 ms | 0.0015/0.0021 ms | 204.0/231.0 | 0/0 |
| 30000 | RealGameMoveAndSort | 9.4581 ms | 52.6482 ms | **5.57x** | 16.3370/50.4763 ms | 1.4151/64.7472 ms | 2.3053/3.0878 ms | 204.0/231.0 | 1/0 |
| 30000 | RealGameAnimationStorm | 9.4388 ms | 48.1859 ms | **5.11x** | 16.1802/48.0482 ms | 1.6943/64.7901 ms | 4.9512/6.2941 ms | 204.0/231.0 | 0/0 |
| 30000 | RealGameDepthSortChurn | 9.8891 ms | 59.3302 ms | **6.00x** | 16.2427/58.9902 ms | 1.2581/64.9612 ms | 2.5739/3.3414 ms | 204.0/231.0 | 1/0 |
| 30000 | RealGamePoolChurn | 13.2263 ms | 50.1333 ms | **3.79x** | 16.0625/50.1138 ms | 1.3857/61.4524 ms | 7.8151/7.8362 ms | 204.0/231.0 | 1/0 |
| 50000 | RealGameGameplayLoop | 16.5773 ms | 89.5680 ms | **5.40x** | 17.0094/89.5128 ms | 1.9734/97.3010 ms | 6.3593/7.7872 ms | 328.0/232.0 | 1/0 |
| 50000 | RealGameIdle | 8.1229 ms | 63.3039 ms | **7.79x** | 18.7269/63.5242 ms | 4.4394/97.4797 ms | 0.0016/0.0019 ms | 328.0/232.0 | 0/0 |
| 50000 | RealGameMoveAndSort | 13.8479 ms | 87.9167 ms | **6.35x** | 16.7084/87.2320 ms | 2.0857/97.5143 ms | 4.1315/5.2548 ms | 328.0/232.0 | 1/0 |
| 50000 | RealGameAnimationStorm | 14.4492 ms | 73.7865 ms | **5.11x** | 18.0445/73.3224 ms | 2.5338/97.9231 ms | 8.2589/9.9201 ms | 328.0/232.0 | 0/0 |
| 50000 | RealGameDepthSortChurn | 14.2078 ms | 98.1323 ms | **6.91x** | 16.8021/98.0451 ms | 2.0534/97.1709 ms | 4.9172/5.7194 ms | 328.0/232.0 | 1/0 |
| 50000 | RealGamePoolChurn | 19.3414 ms | 78.4052 ms | **4.05x** | 18.2172/78.1613 ms | 2.0802/92.8505 ms | 11.9582/12.3049 ms | 328.0/232.0 | 1/0 |

### 5.15.1 50k 代表结果

50k 是最终版本最有代表性的压力规模：

| Case | Fast MainCPU | Native MainCPU | Native / Fast |
|---|---:|---:|---:|
| GameplayLoop | **16.5773 ms** | 89.5680 ms | **5.40x** |
| Idle | **8.1229 ms** | 63.3039 ms | **7.79x** |
| MoveAndSort | **13.8479 ms** | 87.9167 ms | **6.35x** |
| AnimationStorm | **14.4492 ms** | 73.7865 ms | **5.11x** |
| DepthSortChurn | **14.2078 ms** | 98.1323 ms | **6.91x** |
| PoolChurn | **19.3414 ms** | 78.4052 ms | **4.05x** |

这里最重要的不是单个极端倍率，而是 Gameplay、移动排序、动画、Depth Sort 和 Pool Churn 都保持稳定优势。

---

## 5.16 FastSpriteRenderer 62k VisualFPS

最终 62,000 Sprite 压力测试使用：

```text
Fast → Native → Fast
```

结果：

| Target | Stable FPS | Stable Frame |
|---|---:|---:|
| FastSpriteRenderer #1 | **42.2 FPS** | 23.70 ms |
| Unity SpriteRenderer | **8.4 FPS** | 118.83 ms |
| FastSpriteRenderer #2 | **42.7 FPS** | 23.39 ms |

前后两次 FastSpriteRenderer 非常接近，说明切换 Native 后能够稳定恢复，没有看到明显的状态泄漏或持续性性能衰减。

最终 correctness：

```text
Quick               14 / 14
Quick OrderCheck    116 / 0

Production          18 / 18
Production OrderCheck
                     24 / 0
```

---

## 5.17 Benchmark 数据边界

README 中部分极小 Mutation Case 会出现数百倍甚至上万倍的倍率。

这些数字来自：

```text
Mutation + Immediate Flush
```

的真实测量，但不能理解为：

```text
游戏 FPS 提升几千倍
```

真正更值得关注的是：

```text
800 / 1400 / 2200 / 2400 大规模 Case
Heavy Mixed
Full Rebind
Prefab
Clone
Render Wall Median
Render Wall P95
```

这些大规模 Case 仍然表现出稳定的明显优势。

另外本次 Mali-G72 / Vulkan 环境无法获得有效 Unity GPU Frame Timing，因此：

> **这组 UI Benchmark 不宣称 GPU execution time 一定比 UGUI 更低。**

能够确认的是：

```text
13 / 13 Render Wall Median 更低
13 / 13 Render Wall P95 更低
```

FastGUI 部分 Case 的 DrawCall 和 submitted geometry 高于 UGUI，这是稳定 Buffer + 增量 Patch 策略的明确 trade-off，需要在具体目标 GPU 上继续验证。

---

## 6. 性能使用建议

### 6.1 大型列表

推荐：

- 重复 Item 结构保持稳定。
- 尽量使用 `FastImage` / `FastText` 的增量 setter。
- 只改变需要改变的属性。
- 大量重复结构可以评估 `FastSOARenderGroup`。
- 对象生命周期和复用仍由业务框架管理。

不要为了减少一次 Dirty 而频繁销毁 / 重建整个节点树。

### 6.2 图片

普通图标优先使用同一 SpriteAtlas，可减少 Material / Texture Batch 断裂。

### 6.3 文本

- 数字和短文本是 FastText 的典型优势场景。
- 长文本换行成本仍明显高于简单文本，应避免每帧重排大段 RichText。
- TMP Font Asset 的字符覆盖、Atlas 大小与 Fallback 配置仍然遵循 TMP 自身规则。

### 6.4 Visibility

如果只是 UI 渲染隐藏：

```text
FastUIVisibility
setVisible()
```

通常成本远低于完整 `SetActive`。

如果需要 Unity 生命周期，仍然使用：

```csharp
gameObject.SetActive(false);
```

### 6.5 Mask

嵌套 Stencil / RectMask 会增加 Batch 和裁剪状态复杂度。应按实际 UI 语义使用，不要为了布局方便无意义叠加很多 Mask。

---

## 7. 对象池与 Virtualization

FastGUI：

```text
不要求对象池
不内置业务对象池
不要求 Runtime UI Virtualization
```

对象池属于：

```text
业务层
UI Framework
Game Framework
```

的生命周期策略。

FastGUI 负责：

```text
Render
Dirty
Geometry
Batch
Clip
Upload
```

如果项目已有成熟对象池，FastGUI 可以直接配合对象池使用。

---

## 8. 已知限制与边界

1. FastGUI 不是 UGUI 的逐 API 兼容层，组件使用体验接近，但底层架构不同。
2. GPU 性能必须在目标设备验证，尤其是低端 GPU、高 Overdraw 和超高分辨率场景。
3. FastGUI 追求稳定 Buffer + 增量 Patch，因此 submitted geometry 可能高于 UGUI。
4. 部分场景 FastGUI DrawCall 数可能高于 UGUI。
5. GameObject / RectTransform 的创建成本仍然存在，FastGUI 不尝试替换 Unity 节点模型。
6. 对象池属于业务层，不由 FastGUI Runtime 管理。
7. 如果业务依赖完整 GameObject 生命周期，请使用 `SetActive`，不要用轻量 Visibility 替代生命周期语义。
8. 使用 `FastSOARenderGroup` 时，应保证重复结构的 RenderOrder 关系可稳定表达；无法安全转换时 FastGUI 会回退普通顺序。
9. TMP Font Asset / Atlas / Fallback 仍需遵循 TMP 自身资源规则。
10. 长文本 Wrap / RichText 仍比短文本昂贵。
11. FastSpriteRenderer 的 GPU Driven 路径依赖目标平台图形能力；不支持时会使用 fallback。
12. FastSpriteRenderer 的最终性能仍应在目标设备、目标 Sprite 数量、材质数量与实际 Overdraw 下验证。

---

## 9. 版本说明

FastGUI **2.0.0** 是当前正式版本。

2.0.0 的重点：

- 保留 FastCanvas 增量渲染架构。
- 保留 Stable Geometry / Independent Dirty Stream / Compact Index 等正式 UI 路径。
- 保留 Transform / Geometry / Visibility / Clip / Text 的增量更新。
- 新增并完成 FastSpriteRenderer 正式 Runtime 收敛。
- FastSpriteRenderer 在支持的平台使用 GPU Indirect / GPU Driven 路径，并保留 fallback。
- 清理历史实验代码、废弃路径和无意义性能诊断，仅保留轻量 Runtime 埋点。
- UI Publication Benchmark 的完整历史 Case 数据继续保留在 README。
- FastSpriteRenderer 2.0.0 已完成 Quick 14 Case、ProductionScaling 18 Case、62k VisualFPS 与 Order Correctness 真机验证。

完整 Benchmark 数据统一见第 5 章。

