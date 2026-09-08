# Changelog

## [2.0.0] - 2026-09-07

- FastSpriteRenderer 正式完成 GPU Indirect、增量更新、批处理与大规模排序优化。
- 清理历史实验路径和诊断代码，Runtime 进入 Release Freeze。
- 50k 真机场景 MainCPU 相比 Native SpriteRenderer 约 **4.05x ~ 7.79x**。
- 62k VisualFPS：Fast **42.2 / 42.7 FPS**，Native **8.4 FPS**。
- Quick `14/14`、ProductionScaling `18/18`，OrderCheck 全部通过。

## [1.4.0] - 2026-08-28

### Runtime

- 固化当前正式 FastGUI 渲染路径，删除开发期实验 Toggle、A/B 对照入口和历史版本号痕迹。
- Runtime 手写计时收敛为 `FastCanvas` 整帧 CPU 统计，不再在生产热循环中保留阶段级 Stopwatch Breakdown。
- Profiler 仅保留 Canvas 总循环、Dirty Vertex、Transform Position、Transform Range、Clip、Deferred Structure、Batch Range 与 Vertex / Index Upload 等关键 Marker。
- 保留高频 SimpleQuad Matrix Rect Basis 计算路径，在保持数值几何等价的前提下降低矩形 Matrix Geometry 成本。
- 保留稳定 Geometry / Index、增量 Dirty Stream、Position Translation、父 Transform 复用、Text Batch / Cache、Clip 增量更新和 Direct GPU Buffer 等正式生产路径。
- CloneUtility 保持标准 GameObject + RectTransform 克隆模型；移除 Prefab 静态虚拟化旁路和对应 Runtime / Editor 元数据组件。
- TransformRange、Batch、DrawOrder、VertexStream 等核心系统删除仅用于开发归因的细粒度计时与兼容实验代码。

### Editor

- FastImage Inspector 按接近 UGUI Image 的使用顺序组织 Source Image、Color、Material、Image Type 与类型相关参数。
- FastGUI 创建入口统一到 `GameObject > UI > FastGUI`，常规 Image / Raw Image / Text / Input Field 可自动使用或创建 FastCanvas。
- 新建 Image 采用常见 UI 默认尺寸、Anchor / Pivot、命名和 Layer 继承规则。
- FastGUI 内部运行状态下沉到高级区域，不把 Vertex Slot、Render Index、Dirty Flag 等内部概念暴露为日常配置项。
- `Set Native Size` 等 Editor 操作支持正常 Undo / Prefab Override 记录。

### Benchmark

- 公共 Benchmark 与 Runtime 内部优化实现解耦，不再读取历史阶段计时、实验 Toggle 或版本化统计字段。
- Publication Profile 保留 31 个 CPU Case 和 13 个 Render Case，使用 Fast / UGUI / UGUI / Fast 交叉采样。
- README 汇总改为逐行输出，避免 Android Logcat 超长单条消息截断。
- 正式 Android 实测环境：HUAWEI ALP-AL00、Mali-G72、Vulkan、Unity 6000.3.21f1、IL2CPP Release。
- 2400 Item Total Ready：FastGUI `6529.426 ms`，UGUI `10016.053 ms`，约 `1.53x`。
- 31 个 CPU Case 中 20 个通过稳定性判定，20 个稳定 Case 均为 FastGUI 至少快 10%；稳定 Case 中位优势约 `19.9x`。
- 34 个 Weakness Probe 中未发现 UGUI CPU 刷新反超场景；最弱的长文本换行 Case 仍约 `7.91x`。
- 持续滚动 End-to-End Wall Median：FastGUI `33.325 ms`，UGUI `175.614 ms`，约 `5.27x`。
- README 明确披露 FastGUI 在持续滚动时 submitted vertices / triangles 高于 UGUI；测试设备 GPU Timing 不可用，因此不宣称 GPU execution time 一定更低。

### Documentation

- README 重写为 1.4.0 发布文档，覆盖安装、组件选择、Editor 工作流、使用示例、Profiler、Benchmark 方法、真机数据和已知 trade-off。
- 明确 FastGUI 不要求对象池，也不依赖运行时 UI 虚拟化；生命周期复用由业务框架自行管理。

## [1.3.0] - 2026-08-23

### Added

- 完整整理 Unity UPM 发布结构，Package 包含 `Runtime`、`Editor`、`Samples~`、`README.md`、`CHANGELOG.md`、`LICENSE.md` 与 `package.json`。
- `package.json` 声明 Benchmark Sample。
- README 增加新项目安装流程和 FastGUI vs UGUI Benchmark 总览。

### Changed

- Package 正式版本更新为 `1.3.0`。
- Editor 工作流开始向 UGUI 的组件创建和 Inspector 体验靠拢。
- Benchmark 建立正式 Player CPU / Render / End-to-End 对比口径。

## [1.2.0] - 2026-08-23

### Added

- 增加静态视觉节点 Bake 实验、自动 GPU Index Format 和相关验证能力。
- GPU Index 根据 VertexSpan 在 UInt16 / UInt32 间选择，并覆盖 Direct GPU / CPU fallback 路径。

### Changed

- Sparse SOA Hierarchy Role 支持更复杂的 RenderElement 结构。
- GPU Index Format 依据 VertexSpan 决策。

> 1.2.0 的静态节点 Bake / 虚拟化实验未作为 1.4.0 正式 Runtime 能力继续保留。

## [1.1.0] - 2026-08-20

### Added

- 建立集中式增量渲染体系，Position、Color、UV、Visibility、BatchKey、Geometry 与 Index 按 Dirty Range 局部处理。
- 引入 EasyECS SoA / Direct Column 热路径。
- 新增 FastSOARenderGroup、FastRectMask2D、FastMask、FastText、FastInputField。
- 新增 Direct GPU Vertex / Index Buffer、局部 Dirty Upload、Position Delta 和批量 Position 处理。
- 建立 CPU、Render、Mask、Text、InputField、ECS 和创建成本 Benchmark。

### Fixed

- 修复长期回归中发现的 GPU Buffer、SOA、Mask、Text、Visibility 与 Index Patch 正确性问题。

## [1.0.0] - 2026-08-19

### Added

- FastGUI 首个 Unity UPM 正式版本。
- 提供 FastCanvas、FastRawImage、基础 Geometry / Batch / Dirty 更新和 MeshRenderer 渲染框架。
