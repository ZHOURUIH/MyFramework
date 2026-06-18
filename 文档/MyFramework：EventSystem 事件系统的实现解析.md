结合上一篇,说一下我对CommandSystem和EventSystem的理解

**CommandSystem**:逻辑即是对象,明确知道一个事件发生时会触发什么逻辑,所以将这些逻辑集中到一个对象中.适合大部分情况下的事件逻辑封装.

**EventSystem**:完全事件分发,不知道一个事件发生时会触发什么逻辑.所以只管发,不管逻辑处理.需要的地方自己订阅事件去处理.适合逻辑非常分散而且无法收集的情况,比如任务条件的监听,成就进度的监听等.

好了,以下是正文.

在游戏项目里，系统之间经常需要互相通知。

比如背包数据变化后，可能需要刷新背包界面、刷新红点、推进任务、触发引导判断。

最直接的写法是让背包系统直接调用这些系统：

```bash
mBagUI.refresh();
mRedPointSystem.refresh();
mTaskSystem.checkTask();
mGuideSystem.checkGuide();
```

这种写法在小项目里没什么问题。

但项目越做越大以后，问题就会越来越明显。

背包系统本来只应该关心背包数据，但它开始知道 UI、红点、任务、引导这些系统的存在。

以后每增加一个需要响应背包变化的模块，背包系统就要继续改。

这会让模块之间的依赖越来越乱。

所以在 MyFramework 中，我把这类“某件事发生了，谁关心谁来处理”的逻辑，放到了统一的事件系统里：

**EventSystem**。

项目地址：

[GitHub - ZHOURUIH/MyFramework: Unity 商用级别开发框架,经过了多年经验沉淀.一个在unity上使用的网络游戏客户端开发框架,为unity所有使用方式提供完善的封装和管理,只需要专注于游戏逻辑的编写 · GitHub](https://github.com/ZHOURUIH/MyFramework "GitHub - ZHOURUIH/MyFramework: Unity 商用级别开发框架,经过了多年经验沉淀.一个在unity上使用的网络游戏客户端开发框架,为unity所有使用方式提供完善的封装和管理,只需要专注于游戏逻辑的编写 · GitHub")

---

## **一、事件系统解决的不是操作请求**

在前一篇 CommandSystem 中，命令解决的是：

```bash
我要让某个对象做一件事
```

比如关闭窗口、移动对象、延迟执行某个命令。

而 EventSystem 解决的是：

```bash
某件事已经发生了，谁关心谁来处理
```

这两者不能混在一起。

命令更像主动操作：

```bash
打开背包窗口
移动角色
关闭弹窗
延迟执行某个动作
```

事件更像状态通知：

```bash
背包数据变化了
角色属性变化了
任务完成了
场景切换了
某个角色触发了状态变化
```

所以 EventSystem 的核心作用不是替代函数调用，也不是替代 CommandSystem。

它解决的是模块之间的通知关系。

发送事件的系统不需要知道谁会处理事件。

监听事件的系统只需要注册自己关心的事件。

---

## **二、GameEvent 是事件参数对象**

MyFramework 中所有事件参数都继承自 `GameEvent`。

基础结构很简单：

```bash
public class GameEvent : ClassObject
{
    public long mCharacterGUID;

    public override void resetProperty()
    {
        base.resetProperty();
        mCharacterGUID = 0;
    }
}
```

这里有两个点。

第一，`GameEvent` 继承自 `ClassObject`。

这意味着事件参数对象也可以走对象池。

事件触发时，可以临时申请一个事件对象，用完以后自动回收到池中。

第二，基础事件参数里带了 `mCharacterGUID`。

因为游戏里很多事件不是纯全局事件，而是和某个角色相关。

例如某个角色属性变化、某个角色进入战斗、某个角色状态改变。

这种情况下，事件参数本身可以携带角色 ID。

---

## **三、事件注册信息 GameEventRegisteInfo**

事件系统中真正保存监听关系的，不是一个简单的委托列表，而是 `GameEventRegisteInfo`。

它记录了这一次注册的完整信息：

```bash
public class GameEventRegisteInfo : ClassObject
{
    public int mEventTypeID;
    public long mCharacterID;
    public IEventListener mListener;
    public Action mBaseCallback;
}
```

其中：

```bash
mEventTypeID：事件类型 ID
mCharacterID：指定角色 ID，0 表示全局事件
mListener：监听者
mBaseCallback：无参数回调
```

如果事件需要带参数，就使用泛型版本：

```bash
public class GameEventRegisteInfoT<T> : GameEventRegisteInfo where T : GameEvent
{
    public Action<T> mCallback;

    public override void call(GameEvent param)
    {
        base.call(param);
        mCallback?.Invoke(param as T);
    }
}
```

这样事件系统既支持无参数事件，也支持带事件参数的回调。

例如：

```bash
listenEvent<TestEvent>(() => { ... }, listener);
listenEvent<TestEvent>(e => { ... }, listener);
```

这两种都可以使用。

---

## **四、为什么需要 IEventListener**

事件监听者需要实现一个空接口：

```bash
public interface IEventListener
{ }
```

它本身不定义函数。

它的作用是给事件系统一个统一的监听者标识。

也就是说，事件系统可以知道：

```bash
某个监听者注册了哪些事件
某个监听者销毁时应该取消哪些事件
某个监听者是否还在监听列表中
```

这点很重要。

事件系统最容易出问题的地方，不是事件发不出去，而是监听者销毁以后没有取消监听。

如果一个窗口关闭了，但还挂在事件列表里，后面事件再次触发，就可能访问已经失效的窗口。

所以 MyFramework 中不是只保存“事件类型 -> 回调”，还会保存“监听者 -> 注册信息列表”。

---

## **五、EventSystem 里维护了三组核心数据**

EventSystem 中主要维护三组数据：

```bash
protected Dictionary<long, Dictionary<int, SafeList0<GameEventRegisteInfo>>> mCharacterEventList;
protected Dictionary<IEventListener, List<GameEventRegisteInfo>> mListenerList;
protected Dictionary<int, SafeList0<GameEventRegisteInfo>> mGlobalListenerEventList;
```

它们分别解决不同问题。

### **1\. 全局事件列表**

```bash
mGlobalListenerEventList
```

结构大致是：

```bash
事件类型 ID -> 监听列表
```

用于处理普通全局事件。

例如背包变化、配置变化、UI 状态变化这类事件，不需要绑定到某个角色。

### **2\. 指定角色事件列表**

```bash
mCharacterEventList
```

结构大致是：

```bash
角色 ID -> 事件类型 ID -> 监听列表
```

用于处理只关心某个角色的事件。

比如只监听某个角色的属性变化，而不是监听所有角色的属性变化。

这在多人、怪物、伙伴、宠物等对象都存在时比较有用。

### **3\. 监听者反查列表**

```bash
mListenerList
```

结构大致是：

```bash
监听者 -> 这个监听者注册过的所有事件
```

它的作用是取消监听。

当某个监听者销毁或不再需要事件时，可以通过监听者一次性找到它注册过的所有事件，并从全局事件列表、角色事件列表中移除。

这就是 `unlistenEvent(listener)` 的基础。

---

## **六、listenEvent 的注册流程**

注册事件时，大致流程是：

```bash
创建 GameEventRegisteInfo
    ↓
记录事件类型 ID
    ↓
记录角色 ID
    ↓
记录监听者
    ↓
记录回调函数
    ↓
加入 mListenerList
    ↓
加入全局事件列表或角色事件列表
```

比如全局事件注册：

```bash
public void listenEvent<T>(Action<T> callback, IEventListener listener) where T : GameEvent
{
    GameEventRegisteInfo info = createEventAddToListenList(0, callback, listener);
    mGlobalListenerEventList.getOrAddClass(info.mEventTypeID).add(info);
}
```

指定角色事件注册：

```bash
public void listenEvent<T>(long characterID, Action<T> callback, IEventListener listener) where T : GameEvent
{
    GameEventRegisteInfo info = createEventAddToListenList(characterID, callback, listener);
    var characterEventList = mCharacterEventList.getOrAddListPersist(characterID);
    characterEventList.getOrAddClass(info.mEventTypeID).add(info);
}
```

这里有一个关键点：

同一份注册信息，会同时放进两个方向的结构中。

一边用于事件触发时快速找到回调。

一边用于监听者取消时快速找到自己注册过的事件。

---

## **七、pushEvent 的分发流程**

发送全局事件时，流程比较直接：

```bash
根据事件类型 ID 找到监听列表
    ↓
遍历监听列表
    ↓
调用每个 GameEventRegisteInfo 的 call
    ↓
异常单独捕获，避免影响其他监听者
```

代码里事件类型不是用字符串，而是使用：

```bash
TypeID<T>.ID
```

**因为我觉得直接使用Type作为Key可能会比较慢,所以将Type转成int类型的ID来处理,这种做法也适用于其他任何地方.**

这样每种事件类型都有自己的类型 ID，不需要到处写字符串事件名。

发送指定角色事件时，还有一个细节：

```bash
// 即使只是指定角色的事件,也会先广播全局监听
pushEvent(param);
```

也就是说，如果触发一个指定角色事件，会先发送全局事件，再发送指定角色事件。

这样全局监听者仍然可以收到这类事件。

比如某个系统想监听所有角色的某类事件，就监听全局事件。

另一个系统只关心某个角色，就监听指定角色事件。

这两个需求可以同时存在。

---

## **八、为什么遍历时要固定 count**

事件分发时，代码里没有直接写：

```bash
for (int i = 0; i < infoList.count(); ++i)
```

而是先记录当前数量：

```bash
int count = infoList.count();
for (int i = 0; i < count; ++i)
{
    infoList.get(i)?.call(param);
}
```

原因是事件回调过程中，可能再次注册事件、取消事件，甚至再次触发事件。

如果遍历时直接使用动态变化的列表长度，就可能出现遍历结果不稳定。

所以这里固定当前 count。

本次分发只处理进入分发前已经存在的监听者。

新加入的监听者不会立刻插入到本次遍历流程中。

这可以减少很多边界问题。

---

## **九、为什么需要 SafeList0**

EventSystem 的监听列表使用的是 `SafeList0<GameEventRegisteInfo>`，而不是普通 `List<GameEventRegisteInfo>`。

原因也和上面一样：

事件分发过程中可能修改监听列表。

比如某个回调里取消了自己的监听。

如果用普通 List，正在遍历时直接删除元素，很容易出问题。

SafeList 的作用，就是让遍历过程中的删除更安全。

当列表正在遍历时，删除不一定立刻物理移除，而是延后处理。

这也是为什么 EventSystem 中会有 `mNeedCheckEmptyEvent`。

当遍历过程中不能立即清干净空列表时，就先标记一下。

后续在 update 中统一检查并清理空事件列表，避免字典和列表一直膨胀。

---

## **十、unlistenEvent 如何取消监听**

取消监听时，EventSystem 会先通过监听者找到它注册过的所有事件：

```bash
mListenerList[listener]
```

然后逐个从全局事件列表和角色事件列表中移除：

```bash
removeFromCharacterListenList
removeFromGlobalListenList
```

最后再从 `mListenerList` 中移除这个监听者。

这就是为什么注册时要保存反查表。

如果没有 `mListenerList`，取消监听时就只能遍历所有事件类型、所有角色事件、所有监听列表去查找这个监听者。

那样逻辑会更复杂，也更容易漏。

所以 MyFramework 的事件系统不是单向索引，而是双向维护：

```bash
事件类型 -> 监听者列表
监听者 -> 注册信息列表
```

这样注册和取消都比较明确。

---

## **十一、removeCharacterEvent 的作用**

除了取消某个监听者的所有事件，EventSystem 还支持移除某个角色相关的所有事件：

```bash
removeCharacterEvent(long characterID)
```

这个函数适用于角色销毁、离开场景、数据释放等场景。

比如某个角色离开以后，和这个角色绑定的事件监听就不应该继续存在。

`removeCharacterEvent` 会找到这个角色对应的事件列表，然后从监听者反查列表里同步移除相关注册信息。

这个过程很重要。

否则角色相关事件已经清掉了，但监听者反查表里还保留旧注册信息，就会造成两边数据不一致。

---

## **十二、防止事件递归过深**

事件系统里还有一个保护：

```bash
protected int mDispatchDepth;
protected const int MAX_DEPTH = 20;
```

每次发送事件时，都会增加分发深度。

如果递归超过上限，就会报错：

```bash
事件递归栈深度超过上限
```

这是为了防止事件之间互相触发，导致递归链条失控。

例如：

```bash
A 事件触发 B 事件
B 事件又触发 C 事件
C 事件又触发 A 事件
```

这种问题如果没有限制，可能会造成无限递归。

所以事件系统需要有一个最大深度保护。

这不是为了正常流程服务，而是为了在错误事件链出现时尽早暴露问题。

---

## **十三、事件对象和注册信息也会被池化**

`GameEvent` 和 `GameEventRegisteInfo` 都继承自 `ClassObject`。

这意味着事件参数和事件注册信息也可以使用对象池。

比如发送无参数事件时：

```bash
public void pushEvent<T>() where T : GameEvent, new()
{
    using var a = new ClassScope<T>(out var param);
    pushEvent(param);
}
```

这里使用了 `ClassScope<T>`。

也就是说，事件参数对象在作用域内申请，用完自动回收到对象池。

这和之前 ClassPool 文章中的设计是一致的。

事件系统并不是孤立存在的。

它也复用了框架中的对象池和 Scope 生命周期管理。

---

## **十四、EventSystem 和 CommandSystem 的区别**

CommandSystem 和 EventSystem 都能让模块之间减少直接调用，但它们解决的问题不同。

CommandSystem 更适合表达：

```bash
我要执行一个操作
```

EventSystem 更适合表达：

```bash
我通知一个状态变化
```

比如：

```bash
打开背包窗口
```

更像命令。

```bash
背包数据变化
```

更像事件。

如果把所有东西都做成事件，代码会变得很绕。

如果把所有东西都做成命令，状态通知又会变得很重。

所以两者应该分工明确。

在 MyFramework 中，这两个系统可以形成互补：

```bash
CommandSystem
    管理操作请求的执行生命周期

EventSystem
    管理状态变化后的通知关系
```

---

## **十五、这套方案解决的具体问题**

EventSystem 解决的不是“怎么调用一个回调函数”。

它主要解决的是模块之间的通知关系如何维护。

具体包括：

-   事件发送者不需要知道谁在监听
    
-   监听者可以只关心自己需要的事件
    
-   支持无参数事件和带参数事件
    
-   支持全局事件和指定角色事件
    
-   支持通过监听者统一取消注册
    
-   支持角色销毁时移除角色相关事件
    
-   支持事件分发过程中的安全删除
    
-   支持事件递归深度保护
    
-   事件参数对象可以通过 ClassScope 自动回收
    
-   注册信息可以通过 ClassObject 生命周期统一管理
    

这些能力看起来都不复杂，但它们解决的是长期项目中的真实问题。

项目越大，模块之间的通知关系越多。

如果没有统一事件系统，很多模块最终都会互相引用、互相调用、互相影响。

---

## **结语**

EventSystem 的价值，不是为了把简单回调包装得更复杂。

它真正解决的是游戏项目中模块之间的通知关系。

一个系统只负责把“发生了什么”发出去。

谁关心这个事件，谁自己注册监听。

监听者销毁时，通过统一接口取消监听。

角色销毁时，通过角色 ID 清理相关事件。

事件分发过程中，即使有人新增或删除监听，也由 SafeList 和固定 count 遍历来保证流程稳定。

所以 MyFramework 中的 EventSystem，本质上是一套事件生命周期管理系统。

它管理的不只是事件触发，还包括事件注册、事件取消、角色事件清理、监听者反查、事件对象回收和递归深度保护。

这就是 EventSystem 在 MyFramework 中的核心作用。