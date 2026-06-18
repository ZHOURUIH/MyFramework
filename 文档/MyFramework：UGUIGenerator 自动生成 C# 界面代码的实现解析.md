在 Unity 项目里，UI 代码最容易出现大量重复劳动。

所以在 MyFramework 中，我做了一个 UI 代码自动生成工具：**UGUIGenerator**。

它的目标不是“少写几行代码”，而是把 **UI Prefab 到 C# 界面类之间的绑定关系** 变成一个可重复、可检查、可重新生成的工程流程。

项目地址：

[https://github.com/ZHOURUIH/MyFramework](https://github.com/ZHOURUIH/MyFramework "https://github.com/ZHOURUIH/MyFramework")

***

## **一、整体流程**

UGUIGenerator 的整体流程可以概括为：

```bash
Canvas 根节点挂 UGUIGenerator
    ↓
在 Inspector 中选择需要生成的节点
    ↓
设置类型、数组、子窗口、点击事件等信息
    ↓
点击生成代码
```

也就是说，生成器不是简单地遍历 Prefab 下所有节点。

它只生成开发者明确配置过、业务代码需要访问的节点。

这样可以避免生成大量无意义字段，也能保持界面代码相对干净。

***

## **二、UGUIGenerator 挂在 UI Prefab 根节点上**

![](https://p0-xtjj-private.juejin.cn/tos-cn-i-73owjymdk6/47a15709e1d7441db1000e42d0724db3~tplv-73owjymdk6-jj-mark-v1:0:0:0:0:5o6Y6YeR5oqA5pyv56S-5Yy6IEAgX3pob3VydWlfaF8=:q75.awebp?policy=eyJ2bSI6MywidWlkIjoiMzc4MDU4MzQ0NTUwNzUyMCJ9&rk3s=f64ab15b&x-orig-authkey=f32326d3454f2ac7e96d3d06cdbb035152127018&x-orig-expires=1782380530&x-orig-sign=yh5WEZFRcMuknmbTdeFZTE5NRas%3D)

每个需要生成代码的 UI Prefab，根节点上会挂一个 `UGUIGenerator`。

它记录这个界面的基础生成信息，例如：

*   注释

*   是否为常驻界面

*   基类

*   需要生成的成员节点列表

***

## **三、生成出来的代码**

```cs
using Obfuz;

// auto generate member start
// generate from:Assets/GameResources/UI/UIPrefab/UILogin.prefab
// 登录界面
[ObfuzIgnore(ObfuzScope.TypeName)]
public class UILogin : LayoutScript
{
	protected myUGUIObject mLogin;
	protected ScrollViewPanel mScrollViewPanel;
	// auto generate member end
	public UILogin()
	{
		// auto generate constructor start
		mScrollViewPanel = new(this);
		// auto generate constructor end
		mNeedUpdate = true;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out mLogin, "Login");
		mScrollViewPanel.assignWindow(mRoot, "ScrollViewPanel");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		mLogin.registeCollider(onLoginClick);
		// auto generate init end
	}
}
```

## 四\*\*、子页面的UGUISubGenerator\*\*

![](https://p0-xtjj-private.juejin.cn/tos-cn-i-73owjymdk6/7910f1bb463b493795d762a15877c9ef~tplv-73owjymdk6-jj-mark-v1:0:0:0:0:5o6Y6YeR5oqA5pyv56S-5Yy6IEAgX3pob3VydWlfaF8=:q75.awebp?policy=eyJ2bSI6MywidWlkIjoiMzc4MDU4MzQ0NTUwNzUyMCJ9&rk3s=f64ab15b&x-orig-authkey=f32326d3454f2ac7e96d3d06cdbb035152127018&x-orig-expires=1782380530&x-orig-sign=MkIWXw57Do2SopKHom5g531m67c%3D)

大部分与UGUIGenerator相同,差异点在于基类不一样,还有就是类名可以自己定义.

UGUIGenerator对应的类名就固定是prefab的名字,因为肯定不会重复.

而UGUISubGenerator是可能挂在任意的节点上,如果直接取节点名字,可能就会重复,所以可以选择自己输入名字.

还有一个区别就是这里可以选择是否仅标记为一个类型,因为这个子界面可能已经由其他节点的UGUISubGenerator生成了,其他节点其实也是这个子界面类,但是不需要再重复生成,只需要标记一下就行了,这样在Hierarchy中也能清楚看到节点对应的子界面.

## **五、快捷操作**

通常直观操作肯定是先点击添加节点,成员列表中会出现一个空的位置,然后将节点拖拽上去,在节点多的时候可能会很慢.

所以就有一个快捷操作,可以选中一个节点,按Ctrl+W,就会自动添加到成员列表中,并且根据节点名字尝试匹配出合适的变量类型.

比如以0结尾就自动设置为数组,

此节点上有UGUISubGenerator组件就设置为子页面,

节点名字以Checkbox结尾,就自动设置为UGUICheckbox,

以Tab结尾,就自动设置为UGUITab,

以Progress结尾,就自动设置为UGUIProgress

等等......

## **六、对象池,窗口池,无限滚动列表**

这三个类型需要一些额外数据

对象池需要设置模板节点,并且模板节点上需要有一个UGUISubGenerator,也就是说模板节点需要是一个子界面

窗口池是一个单独窗口的对象池,不需要是子界面,比如就是一个最基本的myUGUIObject

无限滚动列表,需要一个Viewport节点,和模板节点,因为这个类型包含了列表的显示,所以会多出一个Viewport节点,而对象池只是负责创建子界面对象,不负责列表逻辑.

## **七、MemberData 记录节点如何变成代码**

UGUIGenerator 中最核心的数据是成员列表。

每一个需要生成的 UI 节点，都会对应一份类似 `MemberData` 的数据。

它记录的不是单纯的 GameObject 引用，而是“这个节点应该如何生成代码”。

比如：

*   对应的 GameObject

*   生成出来的变量类型

*   是否是数组

*   是否是子窗口

*   是否是滚动列表

*   是否是对象池模板

*   是否需要注册点击事件

*   是否需要生成事件函数

这一步很重要。

因为 UI 自动生成真正要解决的不是“找到节点”，而是要判断：

**这个节点在代码中应该以什么形式存在。**

普通节点可以生成 `myUGUIObject`、`myUGUIText` 等成员变量。

子窗口则需要生成对应子窗口类型，并在构造函数中创建。

列表、对象池、数组节点，还需要额外生成对应的初始化和绑定逻辑。

***

## **八、生成前先排序和检查**

点击生成代码后，并不是马上写文件。

生成器会先做两件事：

**第一，排序。**

UI 节点有父子关系，生成绑定代码时，父节点必须先被绑定，子节点才能基于父节点继续查找。

所以生成器会根据节点层级对成员列表排序，保证生成出来的 `assignWindow` 代码顺序正确。

**第二，检查。**

生成器会检查成员是否为空、类型是否正确、节点是否合法、数组配置是否匹配、子窗口配置是否完整。

这些检查的目的，是尽量在生成阶段发现问题，而不是等运行时空引用才发现。

***

## **九、只替换自动生成区域，不覆盖手写逻辑**

代码生成最怕一件事：

**重新生成时把手写逻辑覆盖掉。**

所以 MyFramework 的 UI 脚本中会保留明确的自动生成区域，例如：

```bash
// auto generate member start
// auto generate member end

// auto generate constructor start
// auto generate constructor end

// auto generate assignWindow start
// auto generate assignWindow end

// auto generate init start
// auto generate init end
```

UGUIGenerator 每次重新生成时，只替换这些区域里的内容。

业务代码、界面刷新逻辑、网络回调、按钮响应等，都写在自动生成区域之外。

这样 Prefab 改了以后，可以安全重新生成代码，而不用担心业务逻辑被覆盖。

***

## **十、生成出来的界面代码示例**

例如一个登录界面，生成后的代码大致是这样的：

```bash
// auto generate member start
// generate from:Assets/GameResources/UI/UIPrefab/UILogin.prefab
// 登录界面
[ObfuzIgnore(ObfuzScope.TypeName)]
public class UILogin : LayoutScript
{
    protected myUGUIObject mLogin;
    protected ScrollViewPanel mScrollViewPanel;
    // auto generate member end

    public UILogin()
    {
        // auto generate constructor start
        mScrollViewPanel = new(this);
        // auto generate constructor end
    }

    public override void assignWindow()
    {
        // auto generate assignWindow start
        newObject(out mLogin, "Login");
        mScrollViewPanel.assignWindow(mRoot, "ScrollViewPanel");
        // auto generate assignWindow end
    }
}
```

这里可以看到，生成器并不是简单生成 `transform.Find`。

它生成的是 MyFramework 自己的 UI 对象绑定逻辑。

例如：

```bash
newObject(out mLogin, "Login");
```

表示把 Prefab 中的 `Login` 节点绑定成框架中的 `myUGUIObject`。

而：

```bash
mScrollViewPanel.assignWindow(mRoot, "ScrollViewPanel");
```

表示这个节点不是普通控件，而是一个独立子窗口，需要交给 `ScrollViewPanel` 自己完成绑定。

这也是 UGUIGenerator 和普通节点查找代码最大的区别。

它不仅绑定节点，还维护 UI 结构。

***

## **十一、为什么要生成构造函数**

普通 UI 节点只需要在 `assignWindow` 中绑定。

但子窗口、通用控件、列表项等对象，本身是 C# 对象，需要先创建，再绑定 Prefab 节点。

所以生成器会在构造函数区域生成：

```bash
mScrollViewPanel = new(this);
```

这样做的好处是，复杂 UI 可以拆成多个子窗口类，而不是把几百个节点全部塞进一个界面脚本里。

大型 UI 维护时，这一点非常重要。

***

## **十二、为什么要生成 assignWindow**

`assignWindow` 是 MyFramework UI 体系中的绑定入口。

界面加载完成后，框架会调用 `assignWindow`，把 Prefab 中的节点绑定到 C# 成员变量上。

UGUIGenerator 生成的主要内容，其实就是这部分绑定代码。

它会根据节点类型生成不同绑定方式：

*   普通节点生成 `newObject`

*   子窗口生成 `assignWindow`

*   数组节点生成数组绑定逻辑

*   对象池节点生成模板绑定逻辑

*   滚动列表生成列表相关绑定逻辑

这样业务代码里就不需要关心节点路径怎么查找，只需要直接使用成员变量。

***

## **十三、注册代码也可以自动生成**

除了界面类本身，UGUIGenerator 还可以生成界面注册相关代码。

例如把界面注册到 `LayoutRegisterHotFix`，并在 `GBH` 中生成静态引用。

这样业务层可以直接通过统一入口访问界面，而不是到处写字符串或手动注册。

这一步虽然不是 UI 绑定本身，但它能让界面从 Prefab、脚本、注册到全局访问形成完整流程。

***

## **十四、这套方案解决的核心问题**

UGUIGenerator 解决的不是“少写几行代码”。

它真正解决的是：

**UI Prefab 结构和 C# 访问代码之间的同步问题。**

Prefab 改了，可以重新生成。

节点类型变了，可以重新生成。

新增成员，可以重新生成。

删除节点，生成前可以检查。

子窗口、数组、对象池、列表，都可以按统一规则生成。

这比手写绑定更适合长期项目。

***

## **结语**

MyFramework 的 UGUIGenerator，本质上是一个 UI 工程化工具。

它不是为了替代业务代码，也不是为了把所有东西都自动化。

它只负责一件事：

**把 UI Prefab 中需要被代码访问的结构，稳定地生成成 C# 界面绑定代码。**

业务逻辑仍然由程序员手写。

结构性、重复性、容易出错的绑定代码，则交给工具生成。

这也是我设计 UGUIGenerator 的核心思路。

在小项目里，手写 UI 绑定问题不大。

但在长期 Unity 项目里，当界面数量、节点数量、子窗口数量不断增加时，UI 自动生成能明显降低维护成本。

这也是 MyFramework 工具链中，我认为非常重要的一部分。
