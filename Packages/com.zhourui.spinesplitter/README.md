# SpineSplitter

SpineSplitter 是一个面向 Unity / spine-unity 的 Spine 动画拆分与动态加载插件。

它将原本跟随 `SkeletonData` 一次性加载的动画数据拆分为独立资源，只保留基础 Skeleton 数据常驻，在需要播放某个动画时再加载对应的单动画文件，从而降低大量 Spine 动画同时常驻时的内存占用。

当前插件支持 **Spine 4.0 / 4.1 / 4.2 / 4.3**，并提供运行时动态动画管理、Zero Copy 解析、LRU 缓存和安全卸载能力。

## 主要特性

- 支持 Spine 4.0、4.1、4.2、4.3。
- 支持 Spine Binary Skeleton 拆分。
- Spine 4.1 / 4.2 / 4.3 支持 JSON Skeleton 拆分。
- 自动生成不包含普通动画的 `SkeletonOnly` Skeleton。
- 每个动画生成独立 `.bytes` 文件。
- 生成一份共享 `Common.bytes`，避免单动画重复保存公共数据。
- Binary 动画使用直接 `byte[] + offset/length` 解析，避免 `MemoryStream` 热点开销。
- 单动画文件支持 Zero Copy，解析阶段不再额外复制完整动画 payload。
- 动态动画解析完成后，SpineSplitter 不继续持有源动画 `byte[]`。
- 支持动态动画安全移除和强制移除。
- 支持按 Skeleton 共享检查 `AnimationState`，避免卸载正在播放、排队或 Mixing 的动画。
- 支持 LRU 动态动画缓存。
- 默认最小驻留时间为 60 秒，刚加载或刚播放的动画不会马上被 LRU 淘汰。
- 支持 Pin / Unpin 常驻动画。
- LRU 为事件驱动，不要求业务层每帧 Tick。
- 支持 `SkeletonAnimation` 和 `SkeletonGraphic`。
- 适配 Spine 4.3 分离后的 Animation / Renderer 组件结构。
- 提供拆分、验证、内存分析和 Benchmark Editor 工具。

## 支持范围

| Spine Runtime | Binary | JSON | 说明 |
| --- | --- | --- | --- |
| 4.0 | 支持 | 不支持 | 主要面向 `.skel.bytes` |
| 4.1 | 支持 | 支持 | JSON 动画拆分为独立 `.bytes` |
| 4.2 | 支持 | 支持 | 支持官方 JSON 导出结构 |
| 4.3 | 支持 | 支持 | 支持 4.3 JSON、Slider 依赖动画和 Split Component 架构 |

SpineSplitter **不包含 spine-csharp / spine-unity**。项目必须先安装与资源版本匹配的官方 Spine Runtime。

Spine Runtime 本身受 Esoteric Software 的 Spine Runtimes License Agreement 约束，SpineSplitter 不改变该授权要求。

---

# 安装

通过 Unity Package Manager 的 Git URL 安装：

```text
https://github.com/ZHOURUIH/MyFramework.git?path=/Packages/com.zhourui.spinesplitter
```

仓库：

```text
https://github.com/ZHOURUIH/MyFramework
```

Package Name：

```text
com.zhourui.spinesplitter
```

## 依赖

Package 直接依赖：

```text
com.unity.nuget.newtonsoft-json 3.2.1
```

`Newtonsoft.Json` 只用于 Editor 下的 Spine JSON Skeleton 拆分。

另外项目必须自行安装：

```text
spine-csharp
spine-unity
```

并保证 Spine Runtime 版本和导出的 Spine Skeleton 版本一致。

---

# 选择 Spine Runtime 版本

SpineSplitter 同时包含 4.0~4.3 的版本实现，但一个 Unity 工程中只应该启用当前正在使用的 Spine Runtime 版本。

在 `Scripting Define Symbols` 中添加且只添加以下一个宏：

```text
Spine 4.0 -> SPINE_RUNTIME_40
Spine 4.1 -> SPINE_RUNTIME_41
Spine 4.2 -> SPINE_RUNTIME_42
Spine 4.3 -> SPINE_RUNTIME_43
```

例如 Spine 4.3 工程：

```text
SPINE_RUNTIME_43
```

不要同时定义多个 `SPINE_RUNTIME_xx`。

如果业务代码位于自定义 asmdef 中，需要显式引用：

```text
SpineSplitter.Runtime
```

Editor 扩展由 `SpineSplitter.Editor` 提供。

---

# 基本使用流程

## 1. 导入正常 Spine 资源

先按照 Spine 官方方式导入 Skeleton、Atlas、Texture，并生成正常的：

```text
xxx_SkeletonData.asset
```

确认原始 Spine 资源本身可以正常显示和播放。

## 2. 打开拆分窗口

选择要拆分的 `SkeletonDataAsset`，然后打开：

```text
Tools
-> Spine
-> Spine动画拆分
```

窗口会根据当前 `SPINE_RUNTIME_xx` 自动识别当前版本支持的 JSON 或 Binary Skeleton 源文件。

先扫描动画，确认动画列表和版本正确，再执行生成。

## 3. 生成结果

Binary Skeleton 的结果大致为：

```text
xxx_SkeletonOnly.skel.bytes
xxx_SkeletonOnly_SkeletonData.asset
xxx_Animations/
    xxx_SkeletonOnly_SkeletonData_Common.bytes
    xxx_SkeletonOnly_SkeletonData_idle.bytes
    xxx_SkeletonOnly_SkeletonData_walk.bytes
    ...
```

JSON Skeleton 的 `SkeletonOnly` 会保存为普通 TextAsset 或对应版本使用的 JSON TextAsset，最终同样生成：

```text
xxx_SkeletonOnly_SkeletonData.asset
xxx_Animations/
    xxx_SkeletonOnly_SkeletonData_Common.bytes
    xxx_SkeletonOnly_SkeletonData_<AnimationName>.bytes
```

生成后的 `SkeletonDataAsset` 会保留源 `SkeletonDataAsset` 的 Atlas、Scale、Mix 等配置，只替换 Skeleton 数据源。

业务中应使用生成后的：

```text
xxx_SkeletonOnly_SkeletonData.asset
```

而不是继续使用原始包含全部动画的 `SkeletonDataAsset`。

---

# 运行时动态加载

运行时主要使用：

```csharp
SpineDynamicAnimation
```

## 设置 Common 数据

每个 Skeleton 只需要设置一次 Common 数据：

```csharp
SkeletonData skeletonData = skeletonAnimation.Skeleton.Data;
SpineDynamicAnimation.setCommonData(skeletonData, commonTextAsset.bytes);
```

可以查询：

```csharp
var commonData = SpineDynamicAnimation.getCommonData(skeletonData);
```

Skeleton 生命周期结束时可以清理：

```csharp
SpineDynamicAnimation.removeCommonData(skeletonData);
```

## 加载一个动画

资源系统加载对应的单动画 `.bytes` 后：

```csharp
Spine.Animation animation =
    SpineDynamicAnimation.addAnimation(skeletonAnimation, animationTextAsset.bytes);
```

成功后动画会加入当前 `SkeletonData.Animations`。

`addAnimation()` 完成解析后，SpineSplitter 不会继续持有传入动画文件的源 `byte[]`。

如果外部 ResourceManager / Addressables / AssetBundle 仍然持有该 `TextAsset`，源资源是否释放仍由外部资源系统负责。

推荐生命周期：

```text
加载单动画 TextAsset
    ↓
addAnimation()
    ↓
解析成 Spine.Animation
    ↓
释放外部资源系统对源 TextAsset 的引用
```

## 播放动画

拆分动画建议通过 SpineSplitter 的播放接口播放：

```csharp
SpineDynamicAnimation.playAnimation(
    skeletonAnimation,
    0,
    "walk",
    true);
```

也支持：

```csharp
SpineDynamicAnimation.playAnimation(
    skeletonAnimation,
    "idle",
    true);
```

`SkeletonGraphic` 提供对应重载。

---

# 动态动画缓存

## Count LRU

默认缓存数量限制关闭：

```text
Limit = -1
```

可以主动设置：

```csharp
SpineDynamicAnimation.setDynamicAnimationCacheLimit(
    skeletonAnimation,
    16);
```

关闭数量限制：

```csharp
SpineDynamicAnimation.disableDynamicAnimationCacheLimit(
    skeletonAnimation.Skeleton.Data);
```

## 最小驻留时间

默认：

```text
60 秒
```

动画从最后一次加载或使用开始，至少经过该时间后才有资格被 LRU 淘汰。

修改：

```csharp
SpineDynamicAnimation.setDynamicAnimationMinResidentTime(
    skeletonAnimation,
    120.0);
```

LRU 淘汰必须同时满足：

```text
属于动态加载动画
未 Pin
当前没有被 Track 使用
没有通过 Next 排队
没有被 MixingFrom 引用
已经超过最小驻留时间
缓存数量超过限制
```

如果所有动画当前都不能安全卸载，缓存允许暂时超过 Limit，后续动画状态变化时会再次尝试 Trim。

不需要业务代码每帧调用 Tick。

## Pin 动画

高频动作可以常驻：

```csharp
SpineDynamicAnimation.pinAnimation(
    skeletonAnimation,
    "idle");
```

解除：

```csharp
SpineDynamicAnimation.unpinAnimation(
    skeletonAnimation,
    "idle");
```

---

# 动画卸载

普通移除：

```csharp
SpineDynamicAnimation.removeAnimation(
    skeletonAnimation,
    "walk");
```

如果动画仍然被任意已注册 `AnimationState` 使用，普通移除会失败，避免出现正在播放的 Timeline 被删除。

可以查询：

```csharp
bool inUse =
    SpineDynamicAnimation.isAnimationInUse(
        skeletonAnimation,
        "walk");
```

只有明确知道业务状态安全时才使用：

```csharp
SpineDynamicAnimation.forceRemoveAnimation(
    skeletonAnimation,
    "walk");
```

`forceRemoveAnimation()` 会绕过播放状态保护，不建议作为普通资源回收接口。

---

# Spine 4.3

Spine 4.3 将动画组件和渲染组件拆分。

SpineSplitter 按 **原生 Spine 4.3 工程结构**工作，不负责把 4.2 项目迁移到 4.3。

对于 UI Spine：

```text
SkeletonGraphic
+
SkeletonAnimation
```

`SkeletonGraphic` 的动画状态来自它关联的 `SkeletonAnimation`。

如果 4.3 的 `SkeletonGraphic` 没有关联可用的 `SkeletonAnimation`，动态动画接口无法正常工作。

4.3 的 Slider Constraint 可能依赖特定 Animation。拆分器会识别这些结构依赖动画，并在 `SkeletonOnly` 中保留它们，避免 Skeleton 初始化失败。

---

# Editor 工具

## Spine动画拆分

```text
Tools/Spine/Spine动画拆分
```

扫描当前 `SkeletonDataAsset`，查看动画列表并生成 SkeletonOnly / Common / 单动画资源。

## 验证动态单动画文件

```text
Tools/Spine/验证动态单动画文件
```

验证 Common 和单动画文件是否能按照当前 Spine Runtime 正确重新解析。

建议每次切换 Spine Runtime 版本或首次适配一批新资源时执行。

## 动画内存分析

```text
Tools/Spine/动画内存分析
```

分析拆分动画的内存结构。

## Animation Benchmark

```text
Tools/Spine/Animation Benchmark
```

Editor 下测试单动画解析性能、内存以及动态动画缓存相关行为。

Benchmark 逻辑位于 Editor，不会把 Profiler / Benchmark Counter 注入正式 Runtime Reader。

## 重新拆分全部 Spine

```text
Tools/Spine/重新拆分全部Spine
```

用于批量重新生成当前版本可识别的 Spine 拆分资源。

---

# Zero Copy 与内存说明

单动画容器支持：

```text
byte[] source
+ binaryOffset
+ binaryLength
```

Reader 直接解析源数组范围，不再为了读取单动画 payload 额外复制一份完整二进制。

解析完成后的长期内存主要来自：

```text
Spine.Animation
Timeline
Deform float[]
其他 Timeline 数组
```

而不是单动画源文件副本。

需要注意：SpineSplitter 只能释放自己对源数组的引用，无法替代项目自己的资源管理系统。要真正释放源 `TextAsset` / AssetBundle / Addressables 内存，需要业务资源系统同步释放自己的引用。

---

# Runtime 生命周期清理

当一套 Skeleton Runtime 数据确定不再使用时，可以按项目生命周期调用：

```csharp
SpineDynamicAnimation.clearAnimationStates(skeletonData);
SpineDynamicAnimation.clearDynamicAnimationCache(skeletonData);
SpineDynamicAnimation.removeCommonData(skeletonData);
```

需要整体清理 SpineSplitter 运行时静态状态时：

```csharp
SpineDynamicAnimation.clearRuntimeData();
```

---

# 注意事项

- Spine Skeleton 导出版本必须和项目中使用的 Spine Runtime 版本匹配。
- 一个工程只能定义一个 `SPINE_RUNTIME_40 / 41 / 42 / 43`。
- 不要把不同 Skeleton 的 Common 文件和 Animation 文件混用，插件会校验 Spine Version 和 Skeleton Hash。
- JSON Skeleton 的 Hash 可能不是数字，插件会转换为稳定的 64 位 Hash 用于拆分文件一致性校验。
- 动画文件被 LRU 移除后，再次播放前需要由外部资源系统重新加载对应 `.bytes` 并调用 `addAnimation()`。
- 原始 Spine Atlas / Texture 不属于动画拆分范围，仍沿用原 SkeletonDataAsset 的资源引用。
- SpineSplitter 不是 Spine Runtime 的替代实现，也不包含 Spine Runtime。

---

# License

SpineSplitter 随 MyFramework 使用 MIT License：

```text
https://github.com/ZHOURUIH/MyFramework/blob/master/LICENSE
```

Spine Runtime、Spine Editor 及相关资源仍遵循 Esoteric Software 自己的授权协议。
