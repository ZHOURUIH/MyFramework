# 🎮 MyFramework

一个专注于 **Unity 商业项目开发** 的游戏框架。

从 0 开始设计并持续迭代超过 5 年。

目前已经应用于：

- 个人商业项目（MMORPG / 传奇类游戏）
- 公司手游项目

支持：

✅ Windows  
✅ Android  
✅ iOS  
✅ WebGL  

---

# 💬 社区交流

欢迎交流 Unity 游戏开发、框架设计、客户端开发、服务器开发等相关内容。

| 渠道 | 地址 |
|--------|--------|
| QQ交流群 | 805116283 |
| GitHub Issues | https://github.com/ZHOURUIH/MyFramework/issues |
| GitHub Discussions | （如果以后开启的话） |

群内主要讨论：

- Unity开发
- MyFramework使用
- MyServerFramework使用
- HybridCLR
- 游戏架构设计
- MMO开发
- 网络同步
- 配置表工具链

# ✨ 项目特色

## 🚀 高性能

- 极少依赖 MonoBehaviour
- 大量对象池设计
- 减少 GC Alloc
- 自定义网络序列化
- 按位序列化支持
- Scope(RAII)设计
- 大量工具函数优化

---

## 🔥 HybridCLR 热更新

集成：

- HybridCLR

支持：

- Frame层热更新
- Game层热更新
- 几乎整个业务逻辑热更新

除了热更新下载逻辑外，绝大部分代码都支持热更新。

---

## 🔒 Obfuz 混淆

集成：

- Obfuz

支持：

- 符号混淆
- 字符串加密
- 代码保护

---

## 🖥 完整 UI 框架

支持：

- 无限滚动列表
- CheckBox
- Slider
- Progress
- DropList
- 长按
- 双击
- 拖拽
- 点击穿透

并提供：

- UI脚本自动生成
- 自动注册
- UI对象池

---

## 📦 完整资源管理系统

统一资源接口：

- AssetDatabase
- AssetBundle

业务层无需关心资源来源。

支持：

- 同步加载
- 异步加载

---

## 🌐 完整网络框架

支持：

- TCP
- UDP
- WebSocket
- HTTP

提供：

- 消息自动注册
- 消息代码自动生成
- 按位序列化
- 按字节序列化
- Json序列化

---

## 📊 配置表自动化

使用：

- CSV

配套：

- 自研表格编辑器

支持：

- 字段检查
- 引用检查
- 路径检查
- 客户端代码生成
- 服务端代码生成

在编辑阶段即可发现大量配置错误。

---

## 🧩 组件化设计

部分采用组件思想：

```text
Character
 ├─ ComponentA
 ├─ ComponentB
 └─ ComponentC
```

通过组合代替复杂继承。

---

## ⚙ 命令系统

命令系统负责：

- 逻辑封装
- 模块通信
- UI通信
- 延迟执行
- 主线程调度
- 日志跟踪

示例：

```text
数据改变
    ↓
发送命令
    ↓
数据更新
    ↓
界面刷新
    ↓
网络同步
```

调用方无需关心具体实现。

---

## 🎬 特效系统

支持：

- Prefab对象池
- Effect对象池
- 快速特效播放
- Trail管理
- 生命周期管理

---

## 📚 大量基础设施

包含：

- BinaryUtility
- StringUtility
- MathUtility
- FileUtility
- TimeUtility
- UnityUtility

等大量工具模块。

---

# 📋 功能概览

| 模块 | 支持 |
|--------|--------|
| HybridCLR | ✅ |
| Obfuz | ✅ |
| UI框架 | ✅ |
| UI代码生成 | ✅ |
| AssetBundle | ✅ |
| 对象池 | ✅ |
| 特效池 | ✅ |
| SQLite | ✅ |
| TCP | ✅ |
| UDP | ✅ |
| WebSocket | ✅ |
| HTTP | ✅ |
| CSV配置 | ✅ |
| 配置代码生成 | ✅ |
| 多平台 | ✅ |
| 单元测试 | ✅ |

---

# 📈 与常见框架对比

> 不同框架定位不同，下表仅展示设计方向差异。

| 特性 | MyFramework | GameFramework | ET |
|--------|--------|--------|--------|
| Unity版本依赖 | 低 | 中 | 中 |
| HybridCLR支持 | ✅ | 需自行集成 | 部分项目集成 |
| UI自动生成 | ✅ | ❌ | ❌ |
| 命令系统 | ✅ | 事件系统 | Actor消息 |
| 自定义输入系统 | ✅ | ❌ | ❌ |
| 自定义序列化 | ✅ | ❌ | Protobuf |
| 按位序列化 | ✅ | ❌ | ❌ |
| CSV代码生成 | ✅ | ❌ | 部分支持 |
| 资源加载封装 | ✅ | ✅ | ✅ |
| SQLite支持 | ✅ | 部分 | 部分 |
| 对象池体系 | 完整 | 完整 | 完整 |
| 学习成本 | 中 | 中 | 高 |
| MMO项目适配 | ✅ | ✅ | ✅ |

---

# 🎯 设计理念

MyFramework 并不是为了追求最新潮的架构。

目标只有三个：

### 1. 可维护

代码定位简单。

出了问题能快速找到。

---

### 2. 可控

避免过度依赖：

- Unity自动行为
- 第三方插件

尽可能掌控代码执行过程。

---

### 3. 商业项目可落地

优先解决：

- 性能
- 热更新
- 工具链
- 资源管理

而不是追求架构炫技。

---

# 🚀 快速开始

## 环境

```text
Unity 2022.3 LTS-Unity以上均可
```

---

## 启动

打开项目后：

```text
F5
无需做任何配置或者修改,直接按F5启动游戏
```

---

# 📖 文档

详细说明：

📚 文档目录

https://github.com/ZHOURUIH/MyFramework/tree/master/文档

---

# 🖥 配套服务器框架

服务器项目：

https://github.com/ZHOURUIH/MyServerFramework

支持：

- TCP
- UDP
- HTTP
- SQLite
- MySQL
- 消息自动生成

与客户端框架配套使用。

---

# 📌 当前状态

项目仍在持续维护中。

已经历：

✅ 多年迭代  
✅ 商业项目验证  
✅ 多平台发布验证  

---

# ⭐ Star History

如果这个项目对你有帮助，欢迎点一个 Star ⭐

你的支持会让我持续完善框架。

---

# License

MIT
