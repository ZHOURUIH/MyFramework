using System.Collections.Generic;
using static UnityUtility;
using static StringUtility;

// 物品的总分类
public enum OBJECT_ITEM : byte
{
	NONE = 0,           // 无效类型
	BOX = 1,            // 宝箱
	CONSUMABLE = 2,     // 消耗品
	EQUIP = 3,          // 装备
	SKILL_BOOK = 4,     // 技能书
	MATERIAL = 5,       // 材料
	CARD = 6,           // 卡片
}

// 玩家职业类型
public enum PLAYER_JOB : byte
{
	NONE,           // 表示全职业
	FIGHTER,        // 战士
	MAGE,           // 法师
	TAOIST,         // 道士
	MAX,            // 用于获取职业数量
}

// 装备类型
public enum EQUIP_TYPE : byte
{
	CLOTH,          // 衣服
	WEAPON,         // 武器
	HELMET,         // 头盔
	NECKLACE,       // 项链
	MEDAL,          // 勋章
	BRACELET,       // 手镯
	RING,           // 戒指
	BELT,           // 腰带
	SHOE,           // 鞋子
	DIAMOND,        // 宝石
	SHOULDER,       // 护肩
	KNEE,           // 护膝
	FACE_MASK,      // 面具
	PENDANT,        // 吊坠
	BLOOD_RUNE,     // 血符
	SHIELD,         // 护盾
	SOUL_PEARL,     // 魂珠
	MAX,
}

// auto generate classname start
public class ScrollViewPanel : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected UGUITreeList mFilterTree;
	protected myUGUIObject[] mNode = new myUGUIObject[3];
	protected UGUIDropList mDropList;
	protected myUGUIDragView mNormalContent;
	protected myUGUIDragView mContent;
	protected UGUICheckbox mCheckBox;
	protected myUGUIImageSimple mSimpleImageButton;
	protected myUGUITextTMP mTestText;
	protected myUGUIImageButton mImageButton;
	protected myUGUIImageNumber mImageNumber;
	protected myUGUINumber mNumber;
	protected myUGUIObject mTextButton;
	protected myUGUIObject mEmptyButton;
	protected UGUISlider mSlider;
	protected UGUIProgress mProgress;
	protected myUGUIImage mImage;
	protected myUGUIImageAnim mImageAnim;
	protected myUGUIRawImage mRawImage;
	protected myUGUIRawImageAnim mRawImageAnim;
	protected myUGUIObject[] mElement = new myUGUIObject[5];
	protected myUGUIInputFieldTMP mInputField;
	protected UGUIDragViewLoop<DragItem, DragItem.Data> mDragItemList;
	protected WindowStructPool<NormalItem> mNormalItemPool;
	// auto generate member end
	protected List<WindowStructPool<CustomFilterTreeNode>> mFilterNodePoolList = new();
	protected float mTimer;
	public ScrollViewPanel(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		mFilterTree = new(this);
		mDropList = new(this);
		mCheckBox = new(this);
		mSlider = new(this);
		mProgress = new(this);
		mDragItemList = new(this);
		mNormalItemPool = new(this);
		// auto generate constructor end
		for (int i = 0; i < 3; ++i)
		{
			mFilterNodePoolList.Add(new(this));
		}
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		mFilterTree.assignWindow(mRoot, "FilterTree");
		newObject(out myUGUIObject myViewport, "MyViewport", false);
		newObject(out mContent, myViewport, "Content");
		newObject(out myUGUIObject viewport, mFilterTree.getRoot(), "Viewport", false);
		newObject(out myUGUIObject content, viewport, "Content", false);
		for (int i = 0; i < mNode.Length; ++i)
		{
			newObject(out mNode[i], content, "Node" + IToS(i));
		}
		mDropList.assignWindow(mRoot, "DropList");
		newObject(out myUGUIObject normalList, "NormalList", false);
		newObject(out myUGUIObject normalViewport, normalList, "NormalViewport", false);
		newObject(out mNormalContent, normalViewport, "NormalContent");
		mCheckBox.assignWindow(mContent, "CheckBox");
		newObject(out mSimpleImageButton, mContent, "SimpleImageButton");
		newObject(out mTestText, mContent, "TestText");
		newObject(out mImageButton, mContent, "ImageButton");
		newObject(out mImageNumber, mContent, "ImageNumber");
		newObject(out mNumber, mContent, "Number");
		newObject(out mTextButton, mContent, "TextButton");
		newObject(out mEmptyButton, mContent, "EmptyButton");
		mSlider.assignWindow(mContent, "Slider");
		mProgress.assignWindow(mContent, "Progress");
		newObject(out mImage, mContent, "Image");
		newObject(out mImageAnim, mContent, "ImageAnim");
		newObject(out mRawImage, mContent, "RawImage");
		newObject(out mRawImageAnim, mContent, "RawImageAnim");
		newObject(out myUGUIObject layoutGridVertical, mContent, "LayoutGridVertical", false);
		for (int i = 0; i < mElement.Length; ++i)
		{
			newObject(out mElement[i], layoutGridVertical, "Element" + IToS(i));
		}
		newObject(out mInputField, "InputField");
		newObject(out myUGUIObject dragViewLoop, "DragViewLoop", false);
		mDragItemList.assignWindow(dragViewLoop, "Viewport");
		mDragItemList.assignTemplate("DragItem");
		mNormalItemPool.assignTemplate(mNormalContent, "NormalItem");
		// auto generate assignWindowInternal end
		for (int i = 0; i < mFilterNodePoolList.Count; ++i)
		{
			mFilterNodePoolList[i].assignTemplate(mNode[i]);
		}
	}
	public override void init()
	{
		base.init();
		mScript.registeInputField(mInputField);
		mSlider.setSliderCallback(() => { log("slider变化:" + FToS(mSlider.getValue())); });
		mCheckBox.setCheckCallback((UGUICheckbox checkbox) => { log("checkbox变化:" + checkbox.isChecked()); });
		mSimpleImageButton.registeCollider(() => { log("mSimpleImageButton被点击"); });
		mImageButton.registeCollider(() => { log("mImageButton被点击"); mImageButton.setSelected(!mImageButton.isSelected()); });
		mImageButton.setSelectedSprite("Button1");
		mImageNumber.setNumber(0);
		mNumber.setNumber(0);
		mTextButton.registeCollider(() => { log("mTextButton被点击"); });
		mEmptyButton.registeCollider(() => { log("mEmptyButton被点击"); });

		using var a = new ListScope<string>(out var options);
		options.Add("选项一");
		options.Add("选项二");
		options.Add("选项三");
		options.Add("选项四");
		mDropList.setOptions(options);

		// 构建出物品类型过滤的树形列表
		createFilterNode("全部", OBJECT_ITEM.NONE, 0, PLAYER_JOB.NONE);
		CustomFilterTreeNode nodeWeapon = createFilterNode("武器", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.WEAPON, PLAYER_JOB.NONE);
		createFilterNode("战士", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.WEAPON, PLAYER_JOB.FIGHTER, nodeWeapon);
		createFilterNode("法师", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.WEAPON, PLAYER_JOB.MAGE, nodeWeapon);
		createFilterNode("道士", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.WEAPON, PLAYER_JOB.TAOIST, nodeWeapon);
		ushort allClothMask = 1 << (byte)EQUIP_TYPE.CLOTH |
							  1 << (byte)EQUIP_TYPE.HELMET |
							  1 << (byte)EQUIP_TYPE.BELT |
							  1 << (byte)EQUIP_TYPE.SHOE;
		CustomFilterTreeNode nodeCloth = createFilterNode("服装", OBJECT_ITEM.EQUIP, allClothMask, PLAYER_JOB.NONE);
		createFilterNode("衣服", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.CLOTH, PLAYER_JOB.NONE, nodeCloth);
		createFilterNode("头盔", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.HELMET, PLAYER_JOB.NONE, nodeCloth);
		createFilterNode("腰带", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.BELT, PLAYER_JOB.NONE, nodeCloth);
		createFilterNode("鞋", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.SHOE, PLAYER_JOB.NONE, nodeCloth);
		ushort allJewelyMask = 1 << (byte)EQUIP_TYPE.RING |
							   1 << (byte)EQUIP_TYPE.BRACELET |
							   1 << (byte)EQUIP_TYPE.NECKLACE |
							   1 << (byte)EQUIP_TYPE.MEDAL |
							   1 << (byte)EQUIP_TYPE.DIAMOND;
		CustomFilterTreeNode nodeJewelry = createFilterNode("首饰", OBJECT_ITEM.EQUIP, allJewelyMask, PLAYER_JOB.NONE);
		createFilterNode("戒指", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.RING, PLAYER_JOB.NONE, nodeJewelry);
		createFilterNode("手镯", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.BRACELET, PLAYER_JOB.NONE, nodeJewelry);
		createFilterNode("项链", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.NECKLACE, PLAYER_JOB.NONE, nodeJewelry);
		createFilterNode("勋章", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.MEDAL, PLAYER_JOB.NONE, nodeJewelry);
		createFilterNode("宝石", OBJECT_ITEM.EQUIP, 1 << (byte)EQUIP_TYPE.DIAMOND, PLAYER_JOB.NONE, nodeJewelry);
		createFilterNode("材料", OBJECT_ITEM.MATERIAL, 0, PLAYER_JOB.NONE);
		createFilterNode("消耗品", OBJECT_ITEM.CONSUMABLE, 0, PLAYER_JOB.NONE);
		createFilterNode("卡片", OBJECT_ITEM.CARD, 0, PLAYER_JOB.NONE);
		createFilterNode("宝箱", OBJECT_ITEM.BOX, 0, PLAYER_JOB.NONE);
		CustomFilterTreeNode nodeSkillbook = createFilterNode("技能书", OBJECT_ITEM.SKILL_BOOK, 0, PLAYER_JOB.NONE);
		createFilterNode("战士", OBJECT_ITEM.SKILL_BOOK, 0, PLAYER_JOB.FIGHTER, nodeSkillbook);
		createFilterNode("法师", OBJECT_ITEM.SKILL_BOOK, 0, PLAYER_JOB.MAGE, nodeSkillbook);
		createFilterNode("道士", OBJECT_ITEM.SKILL_BOOK, 0, PLAYER_JOB.TAOIST, nodeSkillbook);
		// 节点创建完毕后要重新刷新树形列表区域的大小
		mFilterTree.collapseAll(true);
		mScript.getLayout().refreshUIDepth(mFilterTree.getContent(), true);
		mFilterTree.setNodeClickCallback(onTreeItemClick);

		mImage.setSpriteName("SelectedItem");
		mRawImage.setTextureName("Texture/1/1_1.png");
		mImageAnim.setNeedUpdate(true);

		using var b = new MyStringBuilderScope(out var builder);
		builder.append("123");
		builder.append(456);
		builder.colorString("FF0000", 123);
		builder.colorStringComma("00FF00", 111222333);
		mTestText.setText(builder.ToString());
	}
	public override void onShow()
	{
		base.onShow();
		mTimer = 0.0f;
		mImageAnim.setLoop(LOOP_MODE.LOOP);
		mRawImageAnim.setLoop(LOOP_MODE.LOOP);
		mImageAnim.play();
		mRawImageAnim.play();
		var list = mDragItemList.startSetDataList();
		for (int i = 0; i < 15;++i)
		{
			list.addClass().mText = IToS(i);
		}
		mDragItemList.setDataList(list);
		mSlider.setValue(0.5f);
		refreshNormalList();
	}
	public void update(float elapsedTime)
	{
		mTimer += elapsedTime;
		if (mTimer > 20.0f)
		{
			mTimer = 0.0f;
		}
		mImageNumber.setNumber((int)mTimer);
		mNumber.setNumber((int)mTimer);
		mProgress.setValue(mTimer / 20.0f);
	}
	//------------------------------------------------------------------------------------------------------------------------------------------------
	protected void onTreeItemClick()
	{
		if (mFilterTree.getSelectedNode() is CustomFilterTreeNode node)
		{
			CustomFilterTreeNodeParam filterParam = node.getPram();
			log("mObjectType:" + filterParam.mObjectType);
			log("mEquipType:" + filterParam.mEquipType);
			log("mJob:" + filterParam.mJob);
		}
	}
	protected void refreshNormalList()
	{
		mNormalItemPool.unuseAll();
		for (int i = 0; i < 10; ++i)
		{
			mNormalItemPool.newItem().setData("Normal" + IToS(i));
		}
		mNormalItemPool.autoGridVertical();
	}
	protected CustomFilterTreeNode createFilterNode(string text, OBJECT_ITEM objectType, ushort equipType, PLAYER_JOB job, CustomFilterTreeNode parent = null)
	{
		CustomFilterTreeNode node = mFilterNodePoolList[parent?.getChildDepth() ?? 0].newItem();
		node.setText(text);
		node.setParam(objectType, equipType, job);
		mFilterTree.addNode(parent, node);
		return node;
	}
}
