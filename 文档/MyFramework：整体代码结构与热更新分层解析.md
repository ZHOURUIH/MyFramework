前面几篇文章分别讲了 MyFramework 中的一些具体模块，比如 UI 自动生成、配置表工具链、ClassPool、GlobalTouchSystem、CommandSystem、EventSystem。

这些模块单独看，都是一个个具体系统。

但如果要真正理解一个框架，只看单个模块还不够，还要看它的整体代码结构。

因为代码放在哪里，往往决定了这个框架的设计思路。

MyFramework 的代码结构不是简单按功能随便分目录，而是围绕几个核心目标组织的：

```bash
热更新分层
框架和业务分离
Editor 工具链独立
运行时系统统一注册
基础能力尽量下沉
```

项目地址：

[GitHub - ZHOURUIH/MyFramework: Unity 商用级别开发框架,经过了多年经验沉淀.一个在unity上使用的网络游戏客户端开发框架,为unity所有使用方式提供完善的封装和管理,只需要专注于游戏逻辑的编写 · GitHub](https://github.com/ZHOURUIH/MyFramework "GitHub - ZHOURUIH/MyFramework: Unity 商用级别开发框架,经过了多年经验沉淀.一个在unity上使用的网络游戏客户端开发框架,为unity所有使用方式提供完善的封装和管理,只需要专注于游戏逻辑的编写 · GitHub")

---

## **一、整体目录结构**

MyFramework 的主要代码都在 `Assets` 目录下。

整体结构大致是：

```bash
Assets
├── Editor
│   ├── Frame
│   └── Game
│
├── EditorRes
│
└── Scripts
    ├── Frame_Base
    ├── Frame_Game
    ├── Game
    ├── Frame_HotFix
    └── HotFix
```

这里最重要的不是目录名字，而是它们的职责。

可以简单理解为：

```bash
Editor
    编辑器工具

Frame_Base
    最底层基础代码，不可热更

Frame_Game
    非热更框架代码，不可热更

Game
    非热更项目启动代码，不可热更

Frame_HotFix
    热更框架代码，可热更

HotFix
    热更业务代码，可热更
```

也就是说，MyFramework 不是把所有代码都塞进一个 Scripts 目录里，而是先把代码分成 **Editor 工具层、非热更层、热更框架层、热更业务层**。

---

## **二、为什么要这样分层**

Unity 项目如果不做热更新，代码怎么放都可以。

但一旦项目需要 HybridCLR 热更新，就必须认真考虑代码边界。

哪些代码启动时必须存在？

哪些代码可以热更？

哪些代码是框架通用能力？

哪些代码是具体项目业务？

如果这些边界不清楚，后期很容易出现问题。

比如：

```bash
热更层引用了不该引用的非热更业务代码
框架层和业务层混在一起
Editor 工具和运行时代码混在一起
项目启动逻辑和热更逻辑互相纠缠
```

所以 MyFramework 的目录结构首先要解决的就是：

**代码属于哪一层。**

---

## **三、Frame\_Base：最底层基础层**

`Frame_Base` 是最底层的基础代码。

它不可热更。

这一层主要放一些非常基础、依赖很少、热更前后都可能会用到的内容。

比如：

```bash
基础 Attribute
基础工具函数
Native / DLL 接口
WebSocket 基础支持
一些最底层公共定义
```

这一层的特点是：

```bash
依赖最少
位置最低
不放业务逻辑
不依赖热更层
```

它更像整个框架的底座。

无论是非热更代码，还是热更代码，都可以基于这一层的基础能力工作。

---

## **四、Frame\_Game：非热更框架层**

`Frame_Game` 也是不可热更代码。

它主要负责那些必须在热更 DLL 加载之前就存在的框架能力。

例如：

```bash
Unity 启动流程
HybridCLR 加载
资源基础加载
版本检测基础逻辑
平台基础接口
非热更 UI 衔接
```

可以把它理解为：

```bash
Unity 原生工程启动
    ↓
初始化基础框架
    ↓
加载资源
    ↓
加载热更 DLL
    ↓
进入热更框架层
```

这一层不应该放大量业务逻辑。

它的职责是把 Unity 原生工程和热更世界连接起来。

---

## **五、Game：非热更项目启动层**

`Game` 目录是不可热更的项目层。

它不是框架主体，而是项目启动壳层。

它通常负责：

```bash
设置资源路径
设置启动场景
初始化项目入口
启动热更加载流程
进入热更主逻辑
```

这一层代码应该尽量少。

因为真正经常变化的业务逻辑，应该放在 `HotFix` 中。

如果大量业务写在 `Game` 里，那么热更新的意义就会下降。

所以 `Game` 更像一个启动壳。

它负责把项目带到热更入口，而不是承载主要业务。

---

## **六、Frame\_HotFix：热更框架主体**

`Frame_HotFix` 是 MyFramework 最核心的目录。

这里放的是可热更的框架层代码。

绝大多数框架系统都在这里，比如：

```bash
UI
ResourceManager
CommandSystem
EventSystem
GlobalTouchSystem
InputManager
ClassPool
Scope
SafeList
Net
Serialize
DataBase
AudioManager
AtlasManager
LocalizationManager
GameSceneManager
SceneSystem
StateManager
TweenerManager
KeyFrameManager
EffectManager
UnitTest
```

这说明 MyFramework 的大部分框架能力本身也是热更的。

这样做的好处是，框架里的很多逻辑也可以随着项目迭代调整，而不是全部被固定在非热更层。

但这也带来一个要求：

**Frame\_HotFix 必须尽量保持框架属性，不应该混入具体业务。**

例如 ClassPool、EventSystem、CommandSystem、GlobalTouchSystem 这些系统，都属于框架能力。

它们可以被不同项目使用。

而具体战斗逻辑、具体 UI 业务、具体 Demo 逻辑，就不应该放在这里。

---

## **七、HotFix：热更业务层**

`HotFix` 是具体项目的热更业务层。

这里放的是项目业务代码，比如：

```bash
BattleSystem
DemoSystem
项目 UI 脚本
项目场景逻辑
项目角色逻辑
业务协议包
配置表注册代码
业务工具函数
```

例如：

```bash
UILogin
UIGame
GameHotFix
PacketRegister
ExcelRegister
SQLiteRegister
```

这些都更接近具体项目，而不是框架通用能力。

可以简单理解为：

```bash
Frame_HotFix
    提供框架能力

HotFix
    使用框架能力实现游戏业务
```

这是框架层和业务层的边界。

如果这个边界混乱，框架就很难复用。

---

## **八、Editor：工程工具层**

MyFramework 中 `Editor` 目录非常重要。

它不是附属品，而是整个框架工程化能力的一部分。

这里包含很多编辑器工具，比如：

```bash
UGUIGenerator
配置表工具
协议生成工具
资源检查工具
代码检查工具
Prefab 节点定位工具
自定义 Inspector
自定义 EditorWindow
Unity 菜单项
```

这些工具的作用不是运行时表现，而是减少长期项目维护成本。

例如：

```bash
UI Prefab 改了，通过 UGUIGenerator 重新生成绑定代码
配置表改了，通过工具生成数据类和注册代码
协议改了，通过工具生成消息代码
资源出问题，通过检查工具提前发现
代码风格和热更引用问题，通过检查工具提前发现
```

所以 MyFramework 的结构里，Editor 工具和运行时框架是互相配合的。

这也是它和很多只提供运行时模块的框架不同的地方。

---

## **九、框架系统如何注册**

MyFramework 的热更框架系统会在统一入口中注册。

比如资源系统、时间系统、命令系统、输入系统、点击系统、事件系统、场景系统、UI 系统等，都会在框架启动时被集中注册。

这样做的意义是：

```bash
所有系统的生命周期由框架统一管理
系统初始化顺序更清楚
update / lateUpdate / fixedUpdate 更容易统一调度
exit / destroy 更容易统一处理
```

如果每个系统都自己挂 MonoBehaviour，自己决定什么时候初始化，项目后期会很难管理。

所以 MyFramework 更倾向于把系统收敛到统一框架生命周期中。

这也是它偏传统引擎风格的地方。

---

## **十、代码生成为什么贯穿多个目录**

MyFramework 中有很多自动生成代码。

例如：

```bash
UI 自动生成代码
配置表数据类
配置表注册代码
协议包代码
协议注册代码
GBH / GBR 静态引用代码
```

这些生成结果分布在不同目录中。

例如 UI 生成后的脚本可能在 `HotFix/UI`。

配置表生成后的注册代码可能在 `HotFix/DataBase`。

协议生成后的代码可能在 `HotFix/Socket`。

Editor 工具负责生成，HotFix 层负责使用。

这说明 MyFramework 不是只靠手写维护，而是把很多重复、结构化、容易出错的代码交给工具生成。

这个设计和目录结构是配套的。

---

## **十一、为什么要区分框架代码和业务代码**

很多项目到后期会遇到一个问题：

框架和业务混在一起。

一开始写得很快，后来越来越难改。

比如 UI 管理器里混了具体界面的特殊逻辑。

资源系统里混了某个业务资源规则。

事件系统里混了某个具体玩法判断。

这样做短期方便，长期会让框架越来越不可复用。

所以 MyFramework 里要尽量区分：

```bash
Frame_HotFix
    放通用框架系统

HotFix
    放项目业务逻辑
```

如果某个功能以后可能被多个项目使用，就应该尽量放在框架层。

如果某个功能只服务当前 Demo 或当前项目，就应该放在业务层。

这不是为了形式上分目录，而是为了让代码边界更清楚。

---

## **十二、这套结构解决的核心问题**

MyFramework 的整体代码结构主要解决几个问题：

-   热更新代码和非热更代码边界清楚
    
-   框架层和业务层分离
    
-   Editor 工具和运行时代码分离
    
-   启动壳层和热更主体分离
    
-   框架系统可以统一注册和管理生命周期
    
-   自动生成代码有明确落点
    
-   项目业务可以基于框架扩展，而不是污染框架层
    
-   后续维护时更容易判断代码应该放在哪里
    

这些问题在小项目里可能不明显。

但项目越大，代码越多，系统越多，目录结构的重要性就越明显。

一个框架的目录结构，其实就是它的设计边界。

---

## **结语**

MyFramework 的整体结构可以用一句话概括：

```bash
非热更层负责启动和热更加载，
热更框架层负责通用系统，
热更业务层负责项目逻辑，
Editor 层负责工程化工具链。
```

所以它不是一个简单的 Unity Scripts 目录划分。

它是围绕热更新分层、框架业务分离、工具链生成、系统生命周期管理组织起来的工程结构。

这种结构的好处是前期会显得更复杂，但长期项目里会更清楚。

当项目不断增加 UI、配置、协议、资源、事件、命令、场景和业务系统时，清晰的代码边界能减少很多后期维护成本。

这也是 MyFramework 整体代码结构设计的核心目的。