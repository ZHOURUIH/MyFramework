在 Unity 项目里，配置表是一个绕不开的问题。

道具、技能、怪物、任务、地图、掉落、商城、活动，几乎所有系统都会依赖配置表。

表格有很多种,比如常用的xlsx,sqlite,csv,甚至unity的ScriptableObject.

对于表格的需求就是首先能够配,然后能够正常读,能够在git中diff,能够自动被合并.

所以综合这些csv似乎就成了唯一的选择.

但是编辑又有点麻烦,没有好用的编辑工具,即使excel也会有一些单元格格式引起的一些小问题,所以最终还是决定我自己写一个表格编辑器.

所以在 MyFramework 中，配置表不是一个简单的读取模块，而是一套完整的工具链。

项目地址：

[GitHub - ZHOURUIH/MyFramework: Unity 商用级别开发框架,经过了多年经验沉淀.一个在unity上使用的网络游戏客户端开发框架,为unity所有使用方式提供完善的封装和管理,只需要专注于游戏逻辑的编写 · GitHub](https://github.com/ZHOURUIH/MyFramework "GitHub - ZHOURUIH/MyFramework: Unity 商用级别开发框架,经过了多年经验沉淀.一个在unity上使用的网络游戏客户端开发框架,为unity所有使用方式提供完善的封装和管理,只需要专注于游戏逻辑的编写 · GitHub")

***

## **一、为什么选择 CSV，而不是直接使用 xlsx**

上面也说了,唯一的目的就是为了能够diff并且能自动合并,xlsx可以diff,但是合并还是需要手动处理,在多人同时改表时很不方便,比如策划在改,程序也在改,这在实际的开发中会造成很大的麻烦.可能大多数团队只能通过严格流程来规避问题.比如谁要改什么表,必须在群里说一声,避免两个人同时改一个表的情况出现.

最早以前我使用sqlite来配表,一直都是我一个人的项目,所以也没觉得有多大的问题,可能唯一的麻烦就是改完了只能看到文件有修改,不知道改了什么.不过基本也没出过什么问题.而且sqlite的编辑器用起来还是很简单的,列排序,列筛选都非常方便.这也是我觉得比excel好用的一个方面.excel的优势在于可以很方便利用公式去批量填写大量单元格.当然查找,排序,筛选什么的也都能做.

***

## **二、为什么还要自己写配置表编辑器**

原因也很简单：

还是觉得excel不是那么好用,更是因为我需要在编辑器中扩展很多自己需要的功能.

游戏表格一般来说都是有表头的,excel没有表头的概念,所以基本都是约定而成.比如第一行是字段名,第二行是字段类型等等.

那我就想能不能自己写个软件,来实现最基础的表头功能,有表头了,那自然很多东西就很好处理了.

添加表头,于是整个表格的数据构成我就非常清晰了,然后就能够做很多约束,很多校验.

表头我现在是这样的结构:

![](https://p0-xtjj-private.juejin.cn/tos-cn-i-73owjymdk6/aaa289ff788d416590896286b9f35d1b~tplv-73owjymdk6-jj-mark-v1:0:0:0:0:5o6Y6YeR5oqA5pyv56S-5Yy6IEAgX3pob3VydWlfaF8=:q75.awebp?policy=eyJ2bSI6MywidWlkIjoiMzc4MDU4MzQ0NTUwNzUyMCJ9&rk3s=f64ab15b&x-orig-authkey=f32326d3454f2ac7e96d3d06cdbb035152127018&x-orig-expires=1782380499&x-orig-sign=lR2OMYu51NKXgYdL%2BlCkfACqVa8%3D)

第一行第一列:表格中文名,用于生成表格类的注释

第二行第一列:表格所属,是客户端,服务器还是双端都用

第三行:字段名,字段英文名,用于生成字段的变量.

第四行:字段类型,变量的类型,支持整数,浮点数,向量,列表,字符串,枚举等

第五行:字段所属,单独配置字段用于单端还是双端.

第六行:字段注释,用于生成变量的注释.

第七行:字段链接表,也就是这个字段必须是填写单个ID或者ID的列表,用于索引到其他表格.

第八行:长度链接,用于约束同一链接名的字段中列表的长度,比如两个字段都是列表类型,并且要求长度是一致的,用以一一配对,在这里就会去约束填写内容.

第九行:字段标签,也就是标记一下字段是填写的什么内容,比如是填写的路径,那就会生成对于路径检测的代码,比如填写的是物品的名字,就会生成对物品名字的检测.目前支持的标签有Path,ItemName,PropertyName,EquipTypeName,当前这个可能不是所有项目都能用得上,只能说也许有用的.

第十行:仅用于过滤数据行的显示,不会存储.

所以 MyFramework 中自研配置表编辑器的目的，不是为了替代 Excel 的表格能力，而是为了把游戏配置规则提前内置到编辑和生成流程里。

Excel 解决的是“表格好不好编辑”。

自研编辑器解决的是“配置数据能不能安全进入项目”。

核心区别如下：

| 对比点   | Excel / WPS    | 自研配置表编辑器       |
| ----- | -------------- | -------------- |
| 主要用途  | 通用表格编辑         | 游戏配置编辑和检查      |
| 字段类型  | 主要靠人为约定        | 可以按字段定义限制类型    |
| 枚举字段  | 容易手填错误值        | 可以做成固定选项       |
| ID 引用 | 需要人工检查         | 可以检查引用 ID 是否存在 |
| 资源引用  | 不知道 Unity 资源情况 | 可以结合项目资源规则检查   |
| 错误发现  | 很多问题运行时才暴露     | 生成前就能提示错误      |
| 输出结果  | 表格文件           | 稳定的 CSV 和生成代码  |

所以更合理的流程不是“完全不用 Excel”，而是：

```bash
Excel / WPS / 自研编辑器
    ↓
编辑和整理数据
    ↓
导出 CSV
    ↓
工具链检查
    ↓
生成客户端 / 服务器代码
```

也就是说，Excel 可以继续用于编辑。

但进入工程、进入版本库、进入代码生成流程的，应该是 CSV。

***

## **三、配置表工具链的整体流程**

MyFramework 中配置表工具链的大致流程是：

```bash
CSV 配置表
    ↓
读取表结构
    ↓
检查字段类型
    ↓
检查数据合法性
    ↓
生成 C# 数据类
    ↓
生成表格注册代码
    ↓
运行时由管理器统一加载和查询
```

这里最重要的一点是：

**业务代码不直接依赖字符串字段名。**

配置表字段会提前生成成 C# 成员变量。

业务代码访问的是强类型字段，而不是到处写字符串。

例如道具表里有 ID、名称、类型、价格、图标等字段，生成后可以变成类似这样的数据结构：

```cs
// auto generate start
using System;
using System.Collections.Generic;
using UnityEngine;

// 物品总表,不同的ID段表示不同的物品类型
public class EDItem : ExcelData
{
	public static int GOLDEN_BOX = 10037;			// 赤金宝箱
	public static int SEASON_GIFT_BOX = 10039;		// 赛季礼盒
	public static int HUI_CHENG_JUAN = 20018;		// 回城卷
	public static int LUCKY_OIL = 20041;			// 祝福油
	public static int WORLD_CHAT = 20043;			// 传音筒(小)
	public static int SPECIAL_CHAT = 20044;			// 传音筒(大)
	public static int CLEAR_PROFICIENCY = 20049;	// 熟练清洗剂
	public static int REPAIR_EQUIP = 20050;			// 便携式修复油

	public string mName;							// 物品名称
	public string mDescription;						// 物品描述,一般是物品的来源,故事等,不包含物品效果
	public ITEM_QUALITY mQuality;					// 物品品质
	public ushort mLevel;							// 物品使用等级
	public string mIcon;							// 物品在背包中的图标名
	public int mAnimationIcon;						// 图标动画所在图集中的动画ID,索引到ImagePositionAnimation,专用于衣服,武器,翅膀在角色面板的显示,优先使用AnimationIcon,AnimationIcon为0时才会使用Icon
	public int mPrice;								// 物品出售价格
	public float mCD;								// 物品使用CD
	public short mCDGroup;							// 物品所属CD组,同一CD组的所有物品共享一个CD,-1表示不属于任何CD组
	public int mMaxCount;							// 物品最大堆叠数量
	public int mMaxCarryCount;						// 物品最大携带数量
	public float mWeight;							// 物品重量,单位kg
	public TRADE_TYPE mTrade;						// 是否可交易
	public ITEM_FUNCTION mFunctionType;				// 物品的功能类型,
	public bool mCanMonsterDrop;					// 怪物是否可掉落此物品,用于检测怪物的掉落配置是否配置完全
	public int mUseSound;							// 使用物品时播放的音效
	public override bool read(SerializerRead reader)
	{
		bool result = base.read(reader);
		result = result && reader.readString(out mName);
		result = result && reader.readString(out mDescription);
		result = result && reader.readEnumByte(out mQuality);
		result = result && reader.read(out mLevel);
		result = result && reader.readString(out mIcon);
		result = result && reader.read(out mAnimationIcon);
		result = result && reader.read(out mPrice);
		result = result && reader.read(out mCD);
		result = result && reader.read(out mCDGroup);
		result = result && reader.read(out mMaxCount);
		result = result && reader.read(out mMaxCarryCount);
		result = result && reader.read(out mWeight);
		result = result && reader.readEnumByte(out mTrade);
		result = result && reader.readEnumByte(out mFunctionType);
		result = result && reader.read(out mCanMonsterDrop);
		result = result && reader.read(out mUseSound);
		return result;
	}
}
// auto generate end
```

```cs
using System;
using System.Collections.Generic;
using static GBR;

public class ExcelItem : ExcelTableT<EDItem>
{
	public override void checkAllData()
	{
		foreach (EDItem item in queryAll())
		{
			mSQLiteImagePositionAnimation.checkData(item.mAnimationIcon, item.mID, this);
		}
	}
	// auto generate start
	protected override void checkAllDataDefault()
	{
		foreach (EDItem item in queryAll())
		{
			checkEnum(item.mQuality, "mQuality", item.mID);
			checkEnum(item.mTrade, "mTrade", item.mID);
			checkEnum(item.mFunctionType, "mFunctionType", item.mID);
			mExcelSound.checkData(item.mUseSound, item.mID, this);
		}
	}
	// auto generate end
}
```

业务层使用时，可以直接写：

```cs
EDItem item = itemTable.getData(itemID);
string name = item.mName;
int price = item.mPrice;
```

而不是写：

```bash
row["Name"]
row["Price"]
```

字符串字段名的问题是，写错了编译器不知道。

字段改名了，编译器也不知道。

只有运行到那一行才可能发现。

配置表代码生成的意义，就是把这类问题尽量提前暴露。

***

## **四、生成前先检查，而不是运行时再报错**

配置表最怕的问题，不是读取失败，而是错误数据混进项目。

比如：

*   ID 重复

*   字段数量不对

*   字段类型不匹配

*   枚举值非法

*   引用 ID 不存在

*   必填字段为空

*   客户端和服务器字段定义不一致

这些问题如果运行时才发现，排查成本会很高。

所以配置表工具链必须在生成前做检查。

这也是我做配置表编辑器和生成工具的主要原因之一。

配置表不是简单数据文件，而是项目逻辑的一部分。

既然它会影响运行时逻辑，就应该在进入运行时之前先验证。

***

## **五、生成数据类和注册代码**

配置表工具链不只生成数据类。

如果只生成数据类，业务层仍然需要手动注册表格、手动管理加载流程。

所以 MyFramework 还会生成表格注册相关代码。

例如：

```bash
ExcelRegister
SQLiteRegister
GameBaseExcelILR
```

这些代码用于告诉框架：

*   当前项目有哪些表

*   每张表对应哪个数据类型

*   表格应该注册到哪里

*   热更层如何访问这些表

这样新增一张配置表时，不需要到处手动加注册代码。

只要表结构正确，工具链重新生成一次，就能把相关代码补齐。

这和 UGUIGenerator 的思路是一样的。

UGUIGenerator 生成 UI 绑定代码。

配置表工具链生成数据类和注册代码。

它们解决的都是同一类问题：

**把重复、结构化、容易手写出错的代码交给工具生成。**

***

## **六、运行时如何使用配置表**

生成完成后，运行时不应该让每个系统自己读取配置文件。

否则项目里很快会出现各种不同的读取方式。

有的系统读 CSV。

有的系统读 SQLite。

有的系统自己缓存。

有的系统直接解析字符串。

这会让配置表管理变得混乱。

所以 MyFramework 中配置表会交给统一管理器处理，例如：

```bash
ExcelManager
SQLiteManager
```

它们负责表格的加载、注册、查询和释放。

业务系统只需要通过统一入口查询数据。

这样底层数据来自哪里、什么时候加载、是否热更、是否缓存，都由框架统一管理。

业务层只关心配置数据本身。

***

## **七、客户端和服务器为什么都需要配置表工具链**

很多配置不是客户端单独使用的。

比如技能表。

客户端需要它显示技能名、图标、描述、冷却时间。

服务器也需要它判断释放条件、伤害参数、消耗和效果。

如果客户端和服务器各自维护一套结构，就很容易出现不一致。

客户端显示消耗 10 点蓝。

服务器实际扣 20 点蓝。

客户端认为某个道具是装备。

服务器认为它是材料。

这种问题很难查，因为双方代码看起来都没错，真正错的是配置结构没有统一。

所以配置表工具链还有一个重要目标：

**让客户端和服务器尽量基于同一份配置数据和同一套表结构生成代码。**

这样可以减少两端字段不一致、类型不一致、含义不一致的问题。

***

## **八、这套方案解决的核心问题**

MyFramework 的配置表工具链，解决的不是“CSV 怎么读”。

它真正解决的是长期项目里的配置维护问题。

核心价值包括：

*   CSV 作为可合并的工程数据源

*   自研编辑器负责规则约束

*   生成前检查配置错误

*   自动生成强类型数据类

*   自动生成表格注册代码

*   统一运行时加载和查询入口

*   降低客户端和服务器配置不一致风险

这些能力在小 Demo 里可能没那么明显。

但在长期项目里，配置表数量越来越多，字段越来越多，参与修改的人越来越多，工具链的价值就会越来越明显。

***

## **结语**

配置表系统最重要的不是“能不能读取表格”。

读取只是最基础的功能。

真正重要的是：

**配置表能不能被多人长期维护。**

**错误能不能提前发现。**

**字段能不能生成成代码。**

**客户端和服务器能不能保持一致。**

这也是 MyFramework 配置表工具链的设计目标。

CSV 负责成为稳定、可合并的工程数据源。

自研编辑器负责编辑约束和规则检查。

代码生成工具负责把表结构转换成客户端和服务器可使用的代码。

运行时管理器负责统一加载和查询。

这套流程看起来比直接读取 Excel 麻烦一些。

但对于长期 Unity 项目来说，它能减少很多后期维护成本。

配置表越多，项目越长，这套工具链的价值就越明显。
