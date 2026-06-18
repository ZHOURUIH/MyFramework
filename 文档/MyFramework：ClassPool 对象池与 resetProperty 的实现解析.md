在 Unity 项目里，很多人一提到对象池，第一反应就是减少 `new`，降低 GC。

这个说法没错，但只说了一半。

对象池真正麻烦的地方，不是“怎么把对象存起来复用”，而是：

**一个对象被复用以后，如何保证它不会带着上一次使用留下的状态。**

如果这个问题解决不好，对象池反而会制造更隐蔽的 Bug。

比如一个对象上一次保存了角色 ID、计时器、回调、列表数据、状态标记，回收到对象池以后没有清理干净。下一次再从池里取出来时，这些旧数据仍然存在，就会出现非常难查的问题。

所以在 MyFramework 中，ClassPool 的核心并不只是复用对象，而是围绕对象生命周期设计了一套规则：

```bash
创建 / 取出
    ↓
onCreate
    ↓
使用
    ↓
destroy
    ↓
resetProperty
    ↓
回收到对象池
```

项目地址：

[https://github.com/ZHOURUIH/MyFramework](https://github.com/ZHOURUIH/MyFramework "https://github.com/ZHOURUIH/MyFramework")

---

## **一、为什么需要 ClassPool**

MyFramework 中很多数据对象并不是 Unity 的 GameObject，而是普通 C# 对象。

例如：

-   临时计算对象
    
-   事件参数
    
-   网络数据
    
-   命令对象
    
-   路径数据
    
-   中间列表包装
    
-   各种框架内部临时结构
    

这些对象如果频繁创建和销毁，就会带来 GC 压力。

尤其是在战斗、UI 刷新、网络消息、路径计算、事件分发这些高频场景中，大量短生命周期对象会反复产生。

所以框架里需要一个通用对象池，专门管理这类普通 C# 对象。

这就是 `ClassPool` 的作用。

每一种对象类型都有自己的未使用队列，需要时取出来，不需要时放回去。

---

## **二、ClassObject 是所有池化对象的基础**

MyFramework 中可以进入 ClassPool 的对象，都需要继承 `ClassObject`。

它里面有几个关键状态：

```bash
protected long mObjectInstanceID;
protected long mAssignID;
protected bool mHasDestroy;
protected bool mPendingDestroy;
```

这几个字段分别解决不同问题。

`mObjectInstanceID` 是对象实例 ID。

它在对象构造时生成，并且不会在 reset 时重置。

也就是说，一个对象即使被重复分配多次，它仍然是同一个实例。

`mAssignID` 是分配 ID。

每次对象从池里取出来时，都会重新设置一个新的分配 ID。

它表示“这一次使用周期”。

`mHasDestroy` 表示对象是否已经被销毁，也就是是否已经不应该再被使用。

`mPendingDestroy` 表示对象是否正在回收过程中。

这些状态的存在，是为了让对象池不只是一个简单的队列，而是能描述对象当前处于什么生命周期阶段。

---

## **三、newClass 的流程**

从对象池取对象时，调用的是 `newClass`。

它的流程大致是：

```bash
检查对象池是否可用
    ↓
检查是否在主线程
    ↓
根据 Type 找未使用队列
    ↓
队列里有对象就取出来
    ↓
没有对象就创建新对象
    ↓
设置新的 AssignID
    ↓
设置为未销毁
    ↓
调用 onCreate
    ↓
编辑器下加入使用中列表
```

关键点在这里：

```bash
obj.setAssignID(++mAssignIDSeed);
obj.setDestroy(false);
obj.onCreate();
```

对象每次被取出来，不管是第一次创建，还是从池里复用，都会走这一套流程。

也就是说，`onCreate` 不是构造函数。

构造函数只在对象真正 new 出来时执行一次。

而 `onCreate` 是每次被分配出去时调用,无论是第一次创建还是从池中获取都会调用,与destroy形成完成的生命周期.

这点很重要。

如果某些初始化逻辑需要每次从池里取出时都执行，就不应该只写在构造函数里，而应该放到 `onCreate` 中。

---

## **四、destroyClass 的流程**

对象使用完以后，通过 `destroyClass` 回收到池中。

它的大致流程是：

```bash
外部引用置空
    ↓
设置 PendingDestroy
    ↓
调用 destroy
    ↓
从使用中列表移除
    ↓
调用 resetProperty
    ↓
加入未使用队列
```

代码里有一个细节：

```bash
T temp = classObject;
classObject = null;
```

也就是说，回收对象时，会先把外部传进来的引用置空。

这样可以减少一种常见错误：

对象已经回收到池里了，但外部还继续拿着旧引用使用。

当然，这不能解决所有错误引用问题，但至少能让正常使用 `ref` 回收的地方，把引用立即清掉。

---

## **五、destroy 和 resetProperty 的区别**

这里很容易混淆。

`destroy` 和 `resetProperty` 不是一回事。

`destroy` 表示当前使用周期结束。

默认实现里，它只是把对象标记为已销毁：

```bash
public virtual void destroy()
{
    mHasDestroy = true;
}
```

而 `resetProperty` 是把对象状态重置到刚构造完的状态。也就是只要调了resetProperty,对象就一定跟new出来的是一样的.

基础类里会重置这些字段：

```bash
public virtual void resetProperty()
{
    mAssignID = 0;
    mHasDestroy = true;
    mPendingDestroy = false;
}
```

注意，`mObjectInstanceID` 不会被重置。

因为它表示对象实例本身，而不是某一次分配。

对于子类来说，`resetProperty` 才是最关键的地方。

如果子类里有自己的字段，就应该在 `resetProperty` 中清理。

例如：

```bash
public override void resetProperty()
{
    base.resetProperty();

    mID = 0;
    mTime = 0.0f;
    mName = null;
    mCallback = null;
    mList.Clear();
}
```

对象池最容易出问题的地方，就是这里漏清理。

---

## **六、对象池真正怕的是状态残留**

如果对象不用池，每次 `new` 出来都是新实例。

字段默认值比较干净。

但对象池不同。

对象从池里取出来时，本质上是旧对象再次使用。

所以它可能残留上一次的数据。

比如：

```bash
上一次使用：
mCharacterID = 10001
mTime = 5.0f
mCallback = 某个回调
mDataList = 10 条数据

回收到池中，但 resetProperty 没清干净

下一次使用：
对象又被取出来
旧字段还在
逻辑开始异常
```

这种 Bug 很难查。

因为你看到的是一个“刚从对象池取出来”的对象，但它并不是一个真正全新的对象。

所以在 MyFramework 里，`resetProperty` 是池化对象必须认真实现的函数。

对象池的稳定性，很大程度上取决于每个类有没有正确清理自己的字段。

所以就必须想办法去保证一定重置完了,所以我写了一个roslyn自定义编译规则,通过如果检测到某

个成员变量没有在resetProperty中被重置,就会编译报错,从而保证一定不会漏掉.

```cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Linq;

public class AnalyzerResetProperty
{
    public static readonly DiagnosticDescriptor ResetPropertyRule = new DiagnosticDescriptor(
        "RESET001",
        "Field not reset",
        "{0}",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor MissingResetPropertyRule = new DiagnosticDescriptor(
        "RESET002",
        "Missing resetProperty",
        "{0}",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    // 检测已经写了 resetProperty 的类,是否重置了全部字段
    public static void analyzeResetAllFields(SyntaxNodeAnalysisContext context)
    {
        MethodDeclarationSyntax method = context.Node as MethodDeclarationSyntax;
        if (method == null)
        {
            return;
        }

        if (method.Identifier.Text != "resetProperty")
        {
            return;
        }

        IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        INamedTypeSymbol classSymbol = methodSymbol.ContainingType;
        if (classSymbol == null)
        {
            return;
        }

        // 只检测 ClassObject 子类
        if (!inheritsFrom(classSymbol, "ClassObject"))
        {
            return;
        }

        // 排除 myUGUIObject 子类
        if (inheritsFrom(classSymbol, "myUGUIObject"))
        {
            return;
        }

        List<IFieldSymbol> allFields = getOwnInstanceFields(classSymbol);
        if (allFields.Count == 0)
        {
            return;
        }

        BlockSyntax body = method.Body;
        if (body == null)
        {
            return;
        }

        string bodyText = body.ToString();

        foreach (IFieldSymbol field in allFields)
        {
            string name = field.Name;

            if (!bodyText.Contains(name + " = ") &&
                !bodyText.Contains(name + ".") &&
                !bodyText.Contains(name + "?.") &&
                !bodyText.Contains(name + ")") &&
                !bodyText.Contains(name + ", "))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ResetPropertyRule,
                    method.Identifier.GetLocation(),
                    "Field '" + name + "' is not reset in resetProperty()"
                ));
            }
        }
    }

    // 检测 ClassObject 子类是否缺失 resetProperty
    public static void analyzeMissingResetProperty(SyntaxNodeAnalysisContext context)
    {
        ClassDeclarationSyntax classDeclaration = context.Node as ClassDeclarationSyntax;
        if (classDeclaration == null)
        {
            return;
        }

        INamedTypeSymbol classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (classSymbol == null)
        {
            return;
        }

        // 只检测普通 class
        if (classSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        // 只检测 ClassObject 子类
        if (!inheritsFrom(classSymbol, "ClassObject"))
        {
            return;
        }

        // 排除 myUGUIObject 子类
        if (inheritsFrom(classSymbol, "myUGUIObject") || 
            inheritsFrom(classSymbol, "FrameSystem") || 
            inheritsFrom(classSymbol, "LayoutScript") ||
            inheritsFrom(classSymbol, "NetPacketBit"))
        {
            return;
        }

        // 抽象类不检测
        if (classSymbol.IsAbstract)
        {
            return;
        }

        List<IFieldSymbol> allFields = getOwnInstanceFields(classSymbol);
        if (allFields.Count == 0)
        {
            return;
        }

        // 只判断当前类自己有没有声明 resetProperty
        // 父类继承来的 resetProperty 不算
        bool hasOwnResetProperty = false;
        foreach (IMethodSymbol method in classSymbol.GetMembers("resetProperty").OfType<IMethodSymbol>())
        {
            if (method.Parameters.Length != 0)
            {
                continue;
            }

            if (!method.ReturnsVoid)
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, classSymbol))
            {
                continue;
            }

            hasOwnResetProperty = true;
            break;
        }

        if (hasOwnResetProperty)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MissingResetPropertyRule,
            classDeclaration.Identifier.GetLocation(),
            "Class '" + classSymbol.Name + "' inherits from ClassObject and has instance fields, but does not implement resetProperty()"
        ));
    }

    private static List<IFieldSymbol> getOwnInstanceFields(INamedTypeSymbol classSymbol)
    {
        return classSymbol
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => !field.IsStatic)
            .Where(field => !field.IsConst)
            .Where(field => SymbolEqualityComparer.Default.Equals(field.ContainingType, classSymbol))
            .ToList();
    }

    private static bool inheritsFrom(INamedTypeSymbol symbol, string baseTypeName)
    {
        INamedTypeSymbol cur = symbol;
        while (cur != null)
        {
            if (cur.Name == baseTypeName)
            {
                return true;
            }

            cur = cur.BaseType;
        }

        return false;
    }
}
```
```cs
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static AnalyzerCallBase;
using static AnalyzerResetProperty;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AnalyzerUnityAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(CallBaseRule, ResetPropertyRule, MissingResetPropertyRule);
	public override void Initialize(AnalysisContext analysisContext)
	{
		analysisContext.EnableConcurrentExecution();
		// 检测是否有调基类函数
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "init"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "lateInit"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "update"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "fixedUpdate"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "lateUpdate"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "exit"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "reset"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "destroy"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "recycle"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "resetProperty"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "onGameState"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "onHide"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "assignWindow"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "assignWindowInternal"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "canEnter"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "enter"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "leave"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "setActive"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "initComponents"); }, SyntaxKind.MethodDeclaration);
        // 检测resetProperty中是否完全重置成员变量
        analysisContext.RegisterSyntaxNodeAction(analyzeResetAllFields, SyntaxKind.MethodDeclaration);
        // 检测 ClassObject 子类有没有缺失 resetProperty
        analysisContext.RegisterSyntaxNodeAction(analyzeMissingResetProperty, SyntaxKind.ClassDeclaration);
    }
}
```

---

## **七、临时对象和持久对象分开管理**

ClassPool 中有两类使用中列表：

```bash
mInusedList
mPersistentInuseList
```

它们主要在编辑器下用于调试。

`mInusedList` 表示临时对象。

这类对象一般只应该在当前作用域或当前帧内使用完，并及时回收。

`mPersistentInuseList` 表示持久使用对象。

这类对象允许跨帧存在。

`newClass` 里有一个参数：

```bash
bool onlyOnce
```

当 `onlyOnce` 为 true 时，对象会被放到临时使用列表。

如果到了下一帧还没有回收，ClassPool 在编辑器下会报错：

```bash
有临时对象正在使用中，是否在申请后忘记回收到池中
```

这个设计很实用。

因为临时对象最容易忘记回收。

忘记回收以后，表面上程序还能跑，但对象池会不断创建新对象，最后失去池化意义。

所以 MyFramework 在编辑器环境下会主动检查这类问题。

---

## **八、ClassScope 用 using 自动回收**

为了减少手动回收错误，MyFramework 里还有一组 Scope 工具。

例如：

```bash
using(new ClassScope<T>(out var value))
{
    // 使用 value
}
```

`ClassScope<T>` 的构造函数中，会从 `mClassPool` 里申请对象：

```bash
value = mClassPool?.newClass<T>(true);
```

`Dispose` 时，会自动回收：

```bash
mClassPool?.destroyClass(ref mValue);
```

这就是一种类似 RAII 的写法。

对象的申请和释放绑定在 `using` 作用域里。

只要离开作用域，就会自动回收到池中。

这种方式特别适合临时对象。

比如某个函数里临时申请一个对象，只在当前函数内使用，不希望它逃逸到外部。

用 Scope 可以明显减少忘记回收的问题。

---

## **九、主线程对象池和线程安全对象池**

MyFramework 中并不是只有一个 ClassPool。

主线程使用的是 `ClassPool`。

子线程使用的是 `ClassPoolThread`。

原因很简单：

主线程对象池不加锁，效率更高，但不能在子线程里使用。

所以代码中有检查：

```bash
只能在主线程中使用此对象池，子线程中请使用ClassPoolThread代替
```

而 `ClassPoolThread` 内部使用 `ThreadLock`，并且按类型维护 `ClassPoolSingle`。

它是线程安全版本，但效率会低一些。

所以两者的定位很清楚：

```bash
ClassPool
    主线程使用，效率优先

ClassPoolThread
    子线程使用，线程安全优先
```

这也是框架中常见的取舍。

不是所有地方都用线程安全结构，而是根据使用场景拆开。

---

## **十、这套方案解决的核心问题**

MyFramework 的 ClassPool 解决的不是单纯的“减少 new”。

它真正解决的是普通 C# 对象在长期项目中的生命周期管理问题。

核心价值包括：

-   减少短生命周期对象频繁创建
    
-   统一池化对象的创建和回收流程
    
-   用 `onCreate` 表示每次分配时的初始化
    
-   用 `destroy` 表示当前使用周期结束
    
-   用 `resetProperty` 清理状态，避免复用时带旧数据
    
-   用 `AssignID` 区分对象的不同分配周期
    
-   用编辑器下的使用中列表检查临时对象是否忘记回收
    
-   用 Scope 把临时对象生命周期绑定到 using 作用域
    
-   用 ClassPoolThread 支持子线程场景
    

这套设计看起来比直接 `new` 对象麻烦。

但它的目的不是让代码更短，而是让对象生命周期更可控。

在小项目里，很多对象直接 new 没什么问题。

但在长期项目里，如果某些对象频繁创建、频繁销毁，或者框架内部大量使用临时数据结构，对象池就有明显价值。

---

## **结语**

对象池并不是简单地把对象放进一个队列里。

真正难的是复用对象时，如何保证它是干净的。

MyFramework 的 ClassPool 围绕这个问题做了几件事：

```bash
ClassObject 定义基础生命周期
ClassPool 负责创建、分配、回收
resetProperty 负责清理状态
Scope 负责临时对象自动回收
编辑器检查负责发现未回收和漏清理问题
ClassPoolThread 负责子线程对象池场景
```

其中最重要的仍然是 `resetProperty`。

因为对象池带来的很多问题，本质上都不是“对象有没有回收”，而是“对象回收以后有没有清干净”。

所以在 MyFramework 里，一个池化对象是否可靠，不只是看它能不能被复用，更要看它每次复用前是否能回到明确的初始状态。

这就是 ClassPool 和 resetProperty 设计的核心。