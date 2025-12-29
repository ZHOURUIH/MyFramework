
// auto generate classname start
public class ScrollView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIDragView mContent;
	protected UGUICheckbox mCheckBox;
	protected myUGUIImageSimple mSimpleImageButton;
	protected myUGUIImageButton mImageButton;
	protected myUGUIImageNumber mImageNumber;
	protected myUGUINumber mNumber;
	protected myUGUIObject mTextButton;
	protected myUGUIObject mEmptyButton;
	protected UGUISlider mSlider;
	protected UGUIProgress mProgress;
	protected UGUIDropList mDropList;
	protected UGUITreeList mFilterTree;
	protected myUGUIImage mImage;
	protected myUGUIImageAnim mImageAnim;
	protected myUGUIRawImage mRawImage;
	protected myUGUIRawImageAnim mRawImageAnim;
	protected UGUIDragViewLoop<DragItem, DragItem.Data> mDragItemList;
	// auto generate member end
	public ScrollView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		mCheckBox = new(this);
		mSlider = new(this);
		mProgress = new(this);
		mDropList = new(this);
		mFilterTree = new(this);
		mDragItemList = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out myUGUIObject viewport, "Viewport", false);
		newObject(out mContent, viewport, "Content");
		mCheckBox.assignWindow(mContent, "CheckBox");
		newObject(out mSimpleImageButton, mContent, "SimpleImageButton");
		newObject(out mImageButton, mContent, "ImageButton");
		newObject(out mImageNumber, mContent, "ImageNumber");
		newObject(out mNumber, mContent, "Number");
		newObject(out mTextButton, mContent, "TextButton");
		newObject(out mEmptyButton, mContent, "EmptyButton");
		mSlider.assignWindow(mContent, "Slider");
		mProgress.assignWindow(mContent, "Progress");
		mDropList.assignWindow(mContent, "DropList");
		mFilterTree.assignWindow(mContent, "FilterTree");
		newObject(out mImage, mContent, "Image");
		newObject(out mImageAnim, mContent, "ImageAnim");
		newObject(out mRawImage, mContent, "RawImage");
		newObject(out mRawImageAnim, mContent, "RawImageAnim");
		newObject(out myUGUIObject dragViewLoop, mContent, "DragViewLoop", false);
		mDragItemList.assignWindow(dragViewLoop, "Viewport");
		mDragItemList.assignTemplate("DragItem");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
	}
	public override void onShow()
	{
		base.onShow();
	}
}
