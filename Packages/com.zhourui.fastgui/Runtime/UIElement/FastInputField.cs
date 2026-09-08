using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum FastInputFieldContentType
{
	Standard,
	Autocorrected,
	IntegerNumber,
	DecimalNumber,
	Alphanumeric,
	Name,
	EmailAddress,
	Password,
	Pin,
	Custom,
}
public enum FastInputFieldInputType
{
	Standard,
	AutoCorrect,
	Password,
}
public enum FastInputFieldCharacterValidation
{
	None,
	Digit,
	Integer,
	Decimal,
	Alphanumeric,
	Name,
	Regex,
	EmailAddress,
	CustomValidator,
}
public enum FastInputFieldLineType
{
	SingleLine,
	MultiLineSubmit,
	MultiLineNewline,
}
public delegate char FastInputFieldValidateInput(string text, int charIndex, char addedChar);
[Serializable]
public class FastInputFieldStringEvent : UnityEvent<string>
{
}
[Serializable]
public class FastInputFieldEvent : UnityEvent
{
}
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FastInputField : MonoBehaviour
{
	[SerializeField] protected TMP_FontAsset mFont;
	[SerializeField] protected string mText = string.Empty;
	[SerializeField] protected string mPlaceholder = string.Empty;
	[SerializeField] protected float mFontSize = 36.0f;
	[SerializeField] protected Color mTextColor = Color.white;
	[SerializeField] protected Color mPlaceholderColor = new(1.0f, 1.0f, 1.0f, 0.5f);
	[SerializeField] protected Color mCaretColor = Color.white;
	[SerializeField] protected Color mSelectionColor = new(0.239f, 0.502f, 0.875f, 0.5f);
	[SerializeField] protected char mAsteriskChar = '*';
	[SerializeField] protected Vector4 mTextPadding = new(4.0f, 2.0f, 4.0f, 2.0f);
	[SerializeField] protected float mCaretWidth = 2.0f;
	[SerializeField] protected float mCaretBlinkRate = 0.85f;
	[SerializeField] protected bool mInteractable = true;
	[SerializeField] protected bool mReadOnly;
	[SerializeField] protected bool mRestoreOriginalTextOnEscape = true;
	[SerializeField] protected bool mEnableTouchScreenKeyboard = true;
	[SerializeField] protected bool mOnFocusSelectAll = true;
	[SerializeField] protected Camera mEventCamera;
	[SerializeField] protected int mCharacterLimit;
	[SerializeField] protected int mLineLimit;
	[SerializeField] protected FastInputFieldContentType mContentType = FastInputFieldContentType.Standard;
	[SerializeField] protected FastInputFieldInputType mInputType = FastInputFieldInputType.Standard;
	[SerializeField] protected FastInputFieldCharacterValidation mCharacterValidation = FastInputFieldCharacterValidation.None;
	[SerializeField] protected FastInputFieldLineType mLineType = FastInputFieldLineType.SingleLine;
	[SerializeField] protected TouchScreenKeyboardType mKeyboardType = TouchScreenKeyboardType.Default;
	[SerializeField] protected TMP_InputValidator mInputValidator;
	[SerializeField] protected string mRegexValue = string.Empty;
	[SerializeField] protected FastText mTextComponent;
	[SerializeField] protected FastText mPlaceholderComponent;
	[SerializeField] protected FastRectMask2D mViewportClip;
	[SerializeField] protected FastInputFieldSelectionGraphic mSelectionGraphic;
	[SerializeField] protected FastInputFieldSelectionGraphic mCaretGraphic;
	[SerializeField] protected FastInputFieldStringEvent mOnValueChanged = new();
	[SerializeField] protected FastInputFieldStringEvent mOnSubmit = new();
	[SerializeField] protected FastInputFieldStringEvent mOnEndEdit = new();
	[SerializeField] protected FastInputFieldEvent mOnSelect = new();
	[SerializeField] protected FastInputFieldEvent mOnDeselect = new();
	protected RectTransform mRectTransform;
	protected RectTransform mViewportRectTransform;
	protected RectTransform mTextRectTransform;
	protected RectTransform mSelectionRectTransform;
	protected RectTransform mCaretRectTransform;
	protected int mCaretPosition;
	protected int mSelectionAnchorPosition;
	protected bool mFocused;
	protected bool mWasCanceled;
	protected bool mDragging;
	protected string mOriginalText = string.Empty;
	protected string mCompositionString = string.Empty;
	protected float mBlinkStartTime;
	protected bool mCaretVisible;
	protected Vector2 mScrollOffset;
	// 多行选择时数量可超过4；这是一次选择重建期间复用的完整Rect Scratch，按需扩容，不机械改为ECS。
	protected Rect[] mSelectionRects = new Rect[4];
	protected TouchScreenKeyboard mKeyboard;
	protected Event mProcessingEvent = new();
	protected bool mDefaultPointerPressed;
	protected int mDefaultTouchFingerId = -1;
	protected Vector2 mDefaultPointerPosition;
	protected static FastInputField sFocusedInputField;
	protected static IFastInputFieldInput sInput;
	protected static readonly HashSet<FastInputField> sActiveInputFields = new();
	public FastInputFieldValidateInput onValidateInput;
	public static IFastInputFieldInput getInput()
	{
		return sInput;
	}
	public static FastInputField getFocusedInputField()
	{
		return sFocusedInputField;
	}
	public static bool hasRegisteredInput()
	{
		return sInput != null;
	}
	public static void registerInput(IFastInputFieldInput input)
	{
		if (sInput == input)
		{
			return;
		}
		IFastInputFieldInput oldInput = sInput;
		sInput = null;
		if (oldInput != null)
		{
			FastInputField[] inputFields = new FastInputField[sActiveInputFields.Count];
			sActiveInputFields.CopyTo(inputFields);
			for (int i = 0; i < inputFields.Length; ++i)
			{
				if (inputFields[i] == null)
				{
					continue;
				}
				inputFields[i].resetDefaultPointerState();
				oldInput.unregisterInputField(inputFields[i]);
			}
		}
		sInput = input;
		if (sInput != null)
		{
			FastInputField[] inputFields = new FastInputField[sActiveInputFields.Count];
			sActiveInputFields.CopyTo(inputFields);
			for (int i = 0; i < inputFields.Length; ++i)
			{
				if (inputFields[i] == null)
				{
					continue;
				}
				inputFields[i].resetDefaultPointerState();
				sInput.registerInputField(inputFields[i]);
			}
		}
	}
	public static void unregisterInput(IFastInputFieldInput input)
	{
		if (sInput != input)
		{
			return;
		}
		unregisterInput();
	}
	public static void unregisterInput()
	{
		if (sInput == null)
		{
			return;
		}
		IFastInputFieldInput oldInput = sInput;
		sInput = null;
		FastInputField[] inputFields = new FastInputField[sActiveInputFields.Count];
		sActiveInputFields.CopyTo(inputFields);
		for (int i = 0; i < inputFields.Length; ++i)
		{
			if (inputFields[i] == null)
			{
				continue;
			}
			inputFields[i].resetDefaultPointerState();
			oldInput.unregisterInputField(inputFields[i]);
		}
	}
	public static void deactivateFocusedInputField(bool submit = false)
	{
		if (sFocusedInputField != null)
		{
			sFocusedInputField.deactivateInputField(submit);
		}
	}
	public TMP_FontAsset getFont()
	{
		return mFont;
	}
	public string getText()
	{
		return mText;
	}
	public string getPlaceholder()
	{
		return mPlaceholder;
	}
	public float getFontSize()
	{
		return mFontSize;
	}
	public char getAsteriskChar()
	{
		return mAsteriskChar;
	}
	public bool getInteractable()
	{
		return mInteractable;
	}
	public bool getReadOnly()
	{
		return mReadOnly;
	}
	public bool getOnFocusSelectAll()
	{
		return mOnFocusSelectAll;
	}
	public Camera getEventCamera()
	{
		return mEventCamera;
	}
	public bool isFocused()
	{
		return mFocused;
	}
	public bool wasCanceled()
	{
		return mWasCanceled;
	}
	public int getCaretPosition()
	{
		return mCaretPosition;
	}
	public int getSelectionAnchorPosition()
	{
		return mSelectionAnchorPosition;
	}
	public bool hasSelection()
	{
		return mCaretPosition != mSelectionAnchorPosition;
	}
	public int getSelectionStart()
	{
		return Mathf.Min(mCaretPosition, mSelectionAnchorPosition);
	}
	public int getSelectionEnd()
	{
		return Mathf.Max(mCaretPosition, mSelectionAnchorPosition);
	}
	public int getCharacterLimit()
	{
		return mCharacterLimit;
	}
	public int getLineLimit()
	{
		return mLineLimit;
	}
	public FastInputFieldContentType getContentType()
	{
		return mContentType;
	}
	public FastInputFieldInputType getInputType()
	{
		return mInputType;
	}
	public FastInputFieldCharacterValidation getCharacterValidation()
	{
		return mCharacterValidation;
	}
	public FastInputFieldLineType getLineType()
	{
		return mLineType;
	}
	public TouchScreenKeyboardType getKeyboardType()
	{
		return mKeyboardType;
	}
	public FastText getTextComponent()
	{
		ensureHierarchy();
		return mTextComponent;
	}
	public FastText getPlaceholderComponent()
	{
		ensureHierarchy();
		return mPlaceholderComponent;
	}
	public FastRectMask2D getViewportClip()
	{
		ensureHierarchy();
		return mViewportClip;
	}
	public FastInputFieldSelectionGraphic getSelectionGraphic()
	{
		ensureHierarchy();
		return mSelectionGraphic;
	}
	public FastInputFieldSelectionGraphic getCaretGraphic()
	{
		ensureHierarchy();
		return mCaretGraphic;
	}
	public FastInputFieldStringEvent getOnValueChanged()
	{
		return mOnValueChanged;
	}
	public FastInputFieldStringEvent getOnSubmit()
	{
		return mOnSubmit;
	}
	public FastInputFieldStringEvent getOnEndEdit()
	{
		return mOnEndEdit;
	}
	public FastInputFieldEvent getOnSelect()
	{
		return mOnSelect;
	}
	public FastInputFieldEvent getOnDeselect()
	{
		return mOnDeselect;
	}

#if UNITY_EDITOR
	// Inspector-only refresh entry point.
	// Do not create or repair the generated hierarchy here; only synchronize an already valid hierarchy.
	// Calling the protected helpers directly avoids Component.SendMessage(), which can assert
	// ShouldRunBehaviour() for normal MonoBehaviours while editing outside Play Mode.
	public void refreshEditorVisuals()
	{
		if (mViewportClip == null ||
			mTextComponent == null ||
			mPlaceholderComponent == null ||
			mSelectionGraphic == null ||
			mCaretGraphic == null)
		{
			return;
		}

		mViewportRectTransform = mViewportClip.getRectTransform();
		mTextRectTransform = mTextComponent.getRectTransform();
		mSelectionRectTransform = mSelectionGraphic.getRectTransform();
		mCaretRectTransform = mCaretGraphic.getRectTransform();

		applyViewportPadding();
		configureVisualComponents();
		refreshTextDisplay();
		refreshPlaceholder();
		refreshCaretAndSelection();
	}
#endif
	private void Awake()
	{
		ensureHierarchy();
		enforceContentType();
		clampPositions();
		refreshVisualState(true);
	}
	private void OnEnable()
	{
		ensureHierarchy();
		registerActiveInputField();
		refreshVisualState(true);
	}
	private void OnDisable()
	{
		unregisterActiveInputField();
		resetDefaultPointerState();
		if (mFocused)
		{
			deactivateInputField(false);
		}
		setCaretVisible(false);
		if (mSelectionGraphic != null)
		{
			mSelectionGraphic.clearRects();
		}
	}
	private void OnDestroy()
	{
		unregisterActiveInputField();
		if (sFocusedInputField == this)
		{
			sFocusedInputField = null;
		}
	}
	private void OnRectTransformDimensionsChange()
	{
		if (mRectTransform == null)
		{
			return;
		}
		applyViewportPadding();
		refreshVisualState(true);
	}
	private void Update()
	{
		if (sInput == null)
		{
			processDefaultPointerInput();
		}
		if (!mFocused || !isActiveAndEnabled)
		{
			return;
		}
		processTouchScreenKeyboard();
		if (mKeyboard == null)
		{
			processKeyboardEvents();
		}
		updateCompositionString();
		updateCaretBlink();
	}
	public void setFont(TMP_FontAsset font)
	{
		if (mFont == font)
		{
			return;
		}
		mFont = font;
		ensureHierarchy();
		mTextComponent.setFont(font);
		mPlaceholderComponent.setFont(font);
		refreshVisualState(true);
	}
	public void setText(string text)
	{
		setTextInternal(text, true);
	}
	public void setTextWithoutNotify(string text)
	{
		setTextInternal(text, false);
	}
	public void setPlaceholder(string placeholder)
	{
		placeholder ??= string.Empty;
		if (mPlaceholder == placeholder)
		{
			return;
		}
		mPlaceholder = placeholder;
		ensureHierarchy();
		mPlaceholderComponent.setText(placeholder);
		refreshPlaceholder();
	}
	public void setFontSize(float fontSize)
	{
		fontSize = Mathf.Max(fontSize, 0.01f);
		if (Mathf.Approximately(mFontSize, fontSize))
		{
			return;
		}
		mFontSize = fontSize;
		ensureHierarchy();
		mTextComponent.setFontSize(fontSize);
		mPlaceholderComponent.setFontSize(fontSize);
		refreshVisualState(true);
	}
	public void setTextColor(Color color)
	{
		mTextColor = color;
		ensureHierarchy();
		mTextComponent.setColor(color);
	}
	public void setPlaceholderColor(Color color)
	{
		mPlaceholderColor = color;
		ensureHierarchy();
		mPlaceholderComponent.setColor(color);
	}
	public void setCaretColor(Color color)
	{
		mCaretColor = color;
		ensureHierarchy();
		mCaretGraphic.setColor(color);
	}
	public void setSelectionColor(Color color)
	{
		mSelectionColor = color;
		ensureHierarchy();
		mSelectionGraphic.setColor(color);
	}
	public void setTextPadding(Vector4 padding)
	{
		if (mTextPadding == padding)
		{
			return;
		}
		mTextPadding = padding;
		ensureHierarchy();
		applyViewportPadding();
		refreshVisualState(true);
	}
	public void setAsteriskChar(char character)
	{
		if (mAsteriskChar == character)
		{
			return;
		}
		mAsteriskChar = character;
		if (mInputType == FastInputFieldInputType.Password)
		{
			refreshTextDisplay();
		}
	}
	public void setCaretWidth(float width)
	{
		mCaretWidth = Mathf.Max(width, 0.01f);
		refreshCaretAndSelection();
	}
	public void setCaretBlinkRate(float rate)
	{
		mCaretBlinkRate = Mathf.Max(rate, 0.0f);
		resetCaretBlink();
	}
	public void setInteractable(bool interactable)
	{
		if (mInteractable == interactable)
		{
			return;
		}
		mInteractable = interactable;
		if (!interactable && mFocused)
		{
			deactivateInputField(false);
		}
	}
	public void setReadOnly(bool readOnly)
	{
		mReadOnly = readOnly;
		refreshCaretAndSelection();
	}
	public void setOnFocusSelectAll(bool selectAllOnFocus)
	{
		mOnFocusSelectAll = selectAllOnFocus;
	}
	public void setEventCamera(Camera eventCamera)
	{
		mEventCamera = eventCamera;
	}
	public void setCharacterLimit(int limit)
	{
		mCharacterLimit = Mathf.Max(limit, 0);
		if (mKeyboard != null)
		{
			mKeyboard.characterLimit = mCharacterLimit;
		}
		if (mCharacterLimit > 0 && mText.Length > mCharacterLimit)
		{
			setTextInternal(mText[..mCharacterLimit], true);
		}
	}
	public void setLineLimit(int limit)
	{
		mLineLimit = Mathf.Max(limit, 0);
		string limited = enforceLineLimit(mText);
		if (limited != mText)
		{
			setTextInternal(limited, true);
		}
	}
	public void setContentType(FastInputFieldContentType contentType)
	{
		if (mContentType == contentType)
		{
			return;
		}
		mContentType = contentType;
		enforceContentType();
		refreshVisualState(true);
	}
	public void setInputType(FastInputFieldInputType inputType)
	{
		mInputType = inputType;
		mContentType = FastInputFieldContentType.Custom;
		refreshVisualState(true);
	}
	public void setCharacterValidation(FastInputFieldCharacterValidation validation)
	{
		mCharacterValidation = validation;
		mContentType = FastInputFieldContentType.Custom;
	}
	public void setLineType(FastInputFieldLineType lineType)
	{
		mLineType = lineType;
		if (mContentType != FastInputFieldContentType.Standard && mContentType != FastInputFieldContentType.Autocorrected)
		{
			mContentType = FastInputFieldContentType.Custom;
		}
		refreshTextLayoutMode();
		refreshVisualState(true);
	}
	public void setKeyboardType(TouchScreenKeyboardType keyboardType)
	{
		mKeyboardType = keyboardType;
		mContentType = FastInputFieldContentType.Custom;
	}
	public void setInputValidator(TMP_InputValidator inputValidator)
	{
		mInputValidator = inputValidator;
		mCharacterValidation = FastInputFieldCharacterValidation.CustomValidator;
		mContentType = FastInputFieldContentType.Custom;
	}
	public void setRegexValue(string regexValue)
	{
		mRegexValue = regexValue ?? string.Empty;
		mCharacterValidation = FastInputFieldCharacterValidation.Regex;
		mContentType = FastInputFieldContentType.Custom;
	}
	public void activateInputField()
	{
		if (!mInteractable || !isActiveAndEnabled)
		{
			return;
		}
		if (mFocused)
		{
			if (mKeyboard != null && !mKeyboard.active)
			{
				mKeyboard.active = true;
			}
			return;
		}
		if (sFocusedInputField != null && sFocusedInputField != this)
		{
			sFocusedInputField.deactivateInputField(false);
		}
		ensureHierarchy();
		sFocusedInputField = this;
		mFocused = true;
		mWasCanceled = false;
		mOriginalText = mText;
		resetCaretBlink();
		openTouchScreenKeyboard();
		refreshVisualState(true);
		if (mOnFocusSelectAll && !string.IsNullOrEmpty(mText))
		{
			selectAll();
		}
		mOnSelect?.Invoke();
	}
	public void deactivateInputField(bool submit)
	{
		if (!mFocused)
		{
			return;
		}
		commitCompositionString();
		mFocused = false;
		mDragging = false;
		mCompositionString = string.Empty;
		if (sFocusedInputField == this)
		{
			sFocusedInputField = null;
		}
		if (mKeyboard != null)
		{
			mKeyboard.active = false;
			mKeyboard = null;
		}
		setCaretVisible(false);
		if (mSelectionGraphic != null)
		{
			mSelectionGraphic.clearRects();
		}
		refreshTextDisplay();
		refreshPlaceholder();
		if (submit)
		{
			mOnSubmit?.Invoke(mText);
		}
		mOnEndEdit?.Invoke(mText);
		mOnDeselect?.Invoke();
	}
	public void cancelInput()
	{
		if (!mFocused)
		{
			return;
		}
		mWasCanceled = true;
		mCompositionString = string.Empty;
		if (mRestoreOriginalTextOnEscape)
		{
			setTextInternal(mOriginalText, true);
		}
		deactivateInputField(false);
	}
	public void setCaretPosition(int position, bool keepSelection = false)
	{
		position = normalizeTextIndex(position);
		mCaretPosition = position;
		if (!keepSelection)
		{
			mSelectionAnchorPosition = position;
		}
		resetCaretBlink();
		refreshCaretAndSelection();
		ensureCaretVisible();
		syncSelectionToKeyboard();
		updateIMECursorPosition();
	}
	public void setSelection(int anchorPosition, int focusPosition)
	{
		mSelectionAnchorPosition = normalizeTextIndex(anchorPosition);
		mCaretPosition = normalizeTextIndex(focusPosition);
		resetCaretBlink();
		refreshCaretAndSelection();
		ensureCaretVisible();
		syncSelectionToKeyboard();
		updateIMECursorPosition();
	}
	public void selectAll()
	{
		mSelectionAnchorPosition = 0;
		mCaretPosition = mText.Length;
		resetCaretBlink();
		refreshCaretAndSelection();
		ensureCaretVisible();
		syncSelectionToKeyboard();
		updateIMECursorPosition();
	}
	public bool insert(char character)
	{
		if (!canEdit())
		{
			return false;
		}
		if (character == '\r')
		{
			character = '\n';
		}
		if (character == '\n' && mLineType != FastInputFieldLineType.MultiLineNewline)
		{
			return false;
		}
		return insertValidatedCharacter(character, true);
	}
	public int insert(string value)
	{
		if (!canEdit() || string.IsNullOrEmpty(value))
		{
			return 0;
		}
		int inserted = 0;
		for (int i = 0; i < value.Length; ++i)
		{
			char character = value[i];
			if (character == '\r')
			{
				if (i + 1 < value.Length && value[i + 1] == '\n')
				{
					++i;
				}
				character = '\n';
			}
			if (character == '\n' && mLineType != FastInputFieldLineType.MultiLineNewline)
			{
				continue;
			}
			if (insertValidatedCharacter(character, false))
			{
				++inserted;
			}
			if (mCharacterLimit > 0 && mText.Length >= mCharacterLimit)
			{
				break;
			}
		}
		if (inserted > 0)
		{
			onTextEdited();
		}
		return inserted;
	}
	public bool backspace()
	{
		if (!canEdit())
		{
			return false;
		}
		if (deleteSelection())
		{
			return true;
		}
		if (mCaretPosition <= 0)
		{
			return false;
		}
		int start = previousTextIndex(mCaretPosition);
		mText = mText.Remove(start, mCaretPosition - start);
		mCaretPosition = start;
		mSelectionAnchorPosition = start;
		onTextEdited();
		return true;
	}
	public bool deleteForward()
	{
		if (!canEdit())
		{
			return false;
		}
		if (deleteSelection())
		{
			return true;
		}
		if (mCaretPosition >= mText.Length)
		{
			return false;
		}
		int end = nextTextIndex(mCaretPosition);
		mText = mText.Remove(mCaretPosition, end - mCaretPosition);
		mSelectionAnchorPosition = mCaretPosition;
		onTextEdited();
		return true;
	}
	public string getSelectedText()
	{
		if (!hasSelection())
		{
			return string.Empty;
		}
		return mText[getSelectionStart()..getSelectionEnd()];
	}
	public void copy()
	{
		if (!hasSelection())
		{
			return;
		}
		if (mInputType == FastInputFieldInputType.Password)
		{
			GUIUtility.systemCopyBuffer = string.Empty;
			return;
		}
		GUIUtility.systemCopyBuffer = getSelectedText();
	}
	public void cut()
	{
		if (!canEdit() || !hasSelection())
		{
			return;
		}
		copy();
		deleteSelection();
	}
	public void paste()
	{
		if (!canEdit())
		{
			return;
		}
		insert(GUIUtility.systemCopyBuffer);
	}
	public bool processKeyEvent(Event evt)
	{
		if (!mFocused || evt == null || evt.type != EventType.KeyDown)
		{
			return false;
		}
		bool shift = (evt.modifiers & EventModifiers.Shift) != 0;
		bool action = (evt.modifiers & (EventModifiers.Control | EventModifiers.Command)) != 0;
		switch (evt.keyCode)
		{
			case KeyCode.LeftArrow: moveHorizontal(-1, shift, action); return true;
			case KeyCode.RightArrow: moveHorizontal(1, shift, action); return true;
			case KeyCode.UpArrow: moveVertical(-1, shift); return true;
			case KeyCode.DownArrow: moveVertical(1, shift); return true;
			case KeyCode.PageUp: movePage(-1, shift); return true;
			case KeyCode.PageDown: movePage(1, shift); return true;
			case KeyCode.Home: moveLineBoundary(false, shift, action); return true;
			case KeyCode.End: moveLineBoundary(true, shift, action); return true;
			case KeyCode.Backspace:
				if (!mReadOnly)
				{
					backspace();
				}
				return true;
			case KeyCode.Delete:
				if (!mReadOnly)
				{
					deleteForward();
				}
				return true;
			case KeyCode.Return:
			case KeyCode.KeypadEnter:
				if (mLineType == FastInputFieldLineType.MultiLineNewline && !action)
				{
					if (!mReadOnly)
					{
						insert('\n');
					}
				}
				else
				{
					deactivateInputField(true);
				}
				return true;
			case KeyCode.Escape: cancelInput(); return true;
			case KeyCode.A:
				if (action)
				{
					selectAll();
					return true;
				}
				break;
			case KeyCode.C:
				if (action)
				{
					copy();
					return true;
				}
				break;
			case KeyCode.V:
				if (action)
				{
					paste();
					return true;
				}
				break;
			case KeyCode.X:
				if (action)
				{
					cut();
					return true;
				}
				break;
		}
		if (!action && evt.character != '\0' && !char.IsControl(evt.character))
		{
			if (!mReadOnly)
			{
				insert(evt.character);
			}
			return true;
		}
		return false;
	}
	public bool setCaretFromScreenPosition(Vector2 screenPosition, Camera camera, bool keepSelection)
	{
		return setCaretFromScreenPositionInternal(screenPosition, camera, keepSelection, true);
	}
	private bool setCaretFromScreenPositionInternal(Vector2 screenPosition, Camera camera, bool keepSelection, bool requireInsideViewport)
	{
		if (!mInteractable)
		{
			return false;
		}
		ensureHierarchy();
		if (requireInsideViewport && !RectTransformUtility.RectangleContainsScreenPoint(mViewportRectTransform, screenPosition, camera))
		{
			return false;
		}
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mTextRectTransform, screenPosition, camera, out Vector2 localPosition))
		{
			return false;
		}
		if (!mFocused)
		{
			activateInputField();
		}
		int index = displayIndexToTextIndex(mTextComponent.getStringIndexFromLocalPosition(localPosition));
		setCaretPosition(index, keepSelection);
		return true;
	}
	public bool beginPointerDrag(Vector2 screenPosition, Camera camera)
	{
		if (!setCaretFromScreenPosition(screenPosition, camera, false))
		{
			return false;
		}
		mDragging = true;
		mSelectionAnchorPosition = mCaretPosition;
		return true;
	}
	public bool dragPointer(Vector2 screenPosition, Camera camera)
	{
		if (!mDragging)
		{
			return false;
		}
		return setCaretFromScreenPositionInternal(screenPosition, camera, true, false);
	}
	public void endPointerDrag()
	{
		mDragging = false;
	}
	public bool onPointerDown(Vector2 screenPosition, Camera camera = null)
	{
		if (!mInteractable || !isActiveAndEnabled)
		{
			return false;
		}
		if (!setCaretFromScreenPositionInternal(screenPosition, resolvePointerCamera(camera), false, false))
		{
			return false;
		}
		mDragging = true;
		mSelectionAnchorPosition = mCaretPosition;
		return true;
	}
	public bool onPointerDrag(Vector2 screenPosition, Camera camera = null)
	{
		if (!mDragging || !mInteractable || !isActiveAndEnabled)
		{
			return false;
		}
		return setCaretFromScreenPositionInternal(screenPosition, resolvePointerCamera(camera), true, false);
	}
	public void onPointerUp(Vector2 screenPosition, Camera camera = null)
	{
		if (!mDragging)
		{
			return;
		}
		onPointerDrag(screenPosition, camera);
		mDragging = false;
	}
	public void onPointerCancel()
	{
		mDragging = false;
	}
	public bool onPointerDownLocal(Vector2 localPosition)
	{
		if (!mInteractable || !isActiveAndEnabled)
		{
			return false;
		}
		ensureHierarchy();
		if (!mFocused)
		{
			activateInputField();
		}
		int index = displayIndexToTextIndex(mTextComponent.getStringIndexFromLocalPosition(localPosition));
		setCaretPosition(index, false);
		mDragging = true;
		mSelectionAnchorPosition = mCaretPosition;
		return true;
	}
	public bool onPointerDragLocal(Vector2 localPosition)
	{
		if (!mDragging || !mInteractable || !isActiveAndEnabled)
		{
			return false;
		}
		int index = displayIndexToTextIndex(mTextComponent.getStringIndexFromLocalPosition(localPosition));
		setCaretPosition(index, true);
		return true;
	}
	public void onPointerUpLocal(Vector2 localPosition)
	{
		if (!mDragging)
		{
			return;
		}
		onPointerDragLocal(localPosition);
		mDragging = false;
	}
	private Camera resolvePointerCamera(Camera camera)
	{
		return camera != null ? camera : mEventCamera;
	}
	private void registerActiveInputField()
	{
		if (!sActiveInputFields.Add(this))
		{
			return;
		}
		sInput?.registerInputField(this);
	}
	private void unregisterActiveInputField()
	{
		if (!sActiveInputFields.Remove(this))
		{
			return;
		}
		sInput?.unregisterInputField(this);
	}
	private void resetDefaultPointerState()
	{
		mDefaultPointerPressed = false;
		mDefaultTouchFingerId = -1;
		mDefaultPointerPosition = Vector2.zero;
		mDragging = false;
	}
	private bool defaultPointerContains(Vector2 screenPosition)
	{
		ensureHierarchy();
		return RectTransformUtility.RectangleContainsScreenPoint(mRectTransform, screenPosition, mEventCamera);
	}
	private void processDefaultPointerInput()
	{
		if (!mInteractable || !isActiveAndEnabled)
		{
			resetDefaultPointerState();
			return;
		}
		try
		{
			if (mDefaultTouchFingerId >= 0)
			{
				if (Input.touchCount > 0)
				{
					processDefaultTouchInput();
				}
				else
				{
					resetDefaultPointerState();
				}
				return;
			}
			if (Input.touchCount > 0)
			{
				processDefaultTouchInput();
				return;
			}
			processDefaultMouseInput();
		}
		catch (InvalidOperationException)
		{
			resetDefaultPointerState();
		}
	}
	private void processDefaultMouseInput()
	{
		Vector2 screenPosition = Input.mousePosition;
		if (Input.GetMouseButtonDown(0))
		{
			if (defaultPointerContains(screenPosition))
			{
				mDefaultPointerPressed = onPointerDown(screenPosition, mEventCamera);
				mDefaultPointerPosition = screenPosition;
			}
			else
			{
				mDefaultPointerPressed = false;
				if (mFocused)
				{
					deactivateInputField(false);
				}
			}
		}
		if (mDefaultPointerPressed && Input.GetMouseButton(0) && screenPosition != mDefaultPointerPosition)
		{
			onPointerDrag(screenPosition, mEventCamera);
			mDefaultPointerPosition = screenPosition;
		}
		if (mDefaultPointerPressed && Input.GetMouseButtonUp(0))
		{
			onPointerUp(screenPosition, mEventCamera);
			mDefaultPointerPressed = false;
		}
	}
	private void processDefaultTouchInput()
	{
		if (mDefaultTouchFingerId < 0)
		{
			for (int i = 0; i < Input.touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.phase != TouchPhase.Began)
				{
					continue;
				}
				if (defaultPointerContains(touch.position))
				{
					if (onPointerDown(touch.position, mEventCamera))
					{
						mDefaultPointerPressed = true;
						mDefaultTouchFingerId = touch.fingerId;
						mDefaultPointerPosition = touch.position;
					}
				}
				else if (mFocused)
				{
					deactivateInputField(false);
				}
				break;
			}
			return;
		}
		for (int i = 0; i < Input.touchCount; ++i)
		{
			Touch touch = Input.GetTouch(i);
			if (touch.fingerId != mDefaultTouchFingerId)
			{
				continue;
			}
			if (touch.phase == TouchPhase.Moved)
			{
				onPointerDrag(touch.position, mEventCamera);
				mDefaultPointerPosition = touch.position;
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				onPointerUp(touch.position, mEventCamera);
				mDefaultPointerPressed = false;
				mDefaultTouchFingerId = -1;
			}
			else if (touch.phase == TouchPhase.Canceled)
			{
				onPointerCancel();
				mDefaultPointerPressed = false;
				mDefaultTouchFingerId = -1;
			}
			break;
		}
	}
	private void ensureHierarchy()
	{
		if (mRectTransform == null)
		{
			mRectTransform = (RectTransform)transform;
		}
		if (mViewportClip == null)
		{
			GameObject viewport = createChild("Viewport", mRectTransform);
			mViewportRectTransform = (RectTransform)viewport.transform;
			mViewportClip = viewport.AddComponent<FastRectMask2D>();
		}
		else
		{
			mViewportRectTransform = mViewportClip.getRectTransform();
		}
		if (mSelectionGraphic == null)
		{
			GameObject selection = createChild("Selection", mViewportRectTransform);
			mSelectionGraphic = selection.AddComponent<FastInputFieldSelectionGraphic>();
		}
		mSelectionRectTransform = mSelectionGraphic.getRectTransform();
		if (mPlaceholderComponent == null)
		{
			GameObject placeholder = createChild("Placeholder", mViewportRectTransform);
			mPlaceholderComponent = placeholder.AddComponent<FastText>();
		}
		if (mTextComponent == null)
		{
			GameObject text = createChild("Text", mViewportRectTransform);
			mTextComponent = text.AddComponent<FastText>();
		}
		mTextRectTransform = mTextComponent.getRectTransform();
		if (mCaretGraphic == null)
		{
			GameObject caret = createChild("Caret", mViewportRectTransform);
			mCaretGraphic = caret.AddComponent<FastInputFieldSelectionGraphic>();
		}
		mCaretRectTransform = mCaretGraphic.getRectTransform();
		mSelectionRectTransform.SetSiblingIndex(0);
		mPlaceholderComponent.getRectTransform().SetSiblingIndex(1);
		mTextRectTransform.SetSiblingIndex(2);
		mCaretRectTransform.SetSiblingIndex(3);
		applyViewportPadding();
		configureVisualComponents();
	}
	private GameObject createChild(string name, RectTransform parent)
	{
		GameObject child = new(name, typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)child.transform;
		rectTransform.SetParent(parent, false);
		setStretchRect(rectTransform);
		return child;
	}
	private void setStretchRect(RectTransform rectTransform)
	{
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		rectTransform.localScale = Vector3.one;
		rectTransform.localRotation = Quaternion.identity;
	}
	private void applyViewportPadding()
	{
		if (mViewportRectTransform == null)
		{
			return;
		}
		mViewportRectTransform.anchorMin = Vector2.zero;
		mViewportRectTransform.anchorMax = Vector2.one;
		mViewportRectTransform.offsetMin = new Vector2(mTextPadding.x, mTextPadding.y);
		mViewportRectTransform.offsetMax = new Vector2(-mTextPadding.z, -mTextPadding.w);
		setStretchRectPreserveOffsets(mTextRectTransform);
		setStretchRectPreserveOffsets(mPlaceholderComponent != null ? mPlaceholderComponent.getRectTransform() : null);
		setStretchRectPreserveOffsets(mSelectionRectTransform);
		setStretchRectPreserveOffsets(mCaretRectTransform);
	}
	private void setStretchRectPreserveOffsets(RectTransform rectTransform)
	{
		if (rectTransform == null)
		{
			return;
		}
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
	}
	private void configureVisualComponents()
	{
		mTextComponent.setFont(mFont);
		mTextComponent.setFontSize(mFontSize);
		mTextComponent.setColor(mTextColor);
		mTextComponent.setRichText(false);
		mPlaceholderComponent.setFont(mFont);
		mPlaceholderComponent.setFontSize(mFontSize);
		mPlaceholderComponent.setColor(mPlaceholderColor);
		mPlaceholderComponent.setRichText(false);
		mPlaceholderComponent.setText(mPlaceholder);
		mSelectionGraphic.setColor(mSelectionColor);
		mCaretGraphic.setColor(mCaretColor);
		refreshTextLayoutMode();
	}
	private void refreshTextLayoutMode()
	{
		if (mTextComponent == null || mPlaceholderComponent == null)
		{
			return;
		}
		bool multiline = mLineType != FastInputFieldLineType.SingleLine;
		mTextComponent.setWordWrap(multiline);
		mPlaceholderComponent.setWordWrap(multiline);
		mTextComponent.setVerticalAlignment(multiline ? FastUITextVerticalAlignment.Top : FastUITextVerticalAlignment.Middle);
		mPlaceholderComponent.setVerticalAlignment(multiline ? FastUITextVerticalAlignment.Top : FastUITextVerticalAlignment.Middle);
		mTextComponent.setHorizontalAlignment(FastUITextHorizontalAlignment.Left);
		mPlaceholderComponent.setHorizontalAlignment(FastUITextHorizontalAlignment.Left);
	}
	private void refreshVisualState(bool resetScroll)
	{
		if (mTextComponent == null)
		{
			return;
		}
		if (resetScroll)
		{
			mScrollOffset = Vector2.zero;
			applyScrollOffset();
		}
		refreshTextDisplay();
		refreshPlaceholder();
		refreshCaretAndSelection();
		ensureCaretVisible();
		updateIMECursorPosition();
	}
	private void refreshTextDisplay()
	{
		if (mTextComponent == null)
		{
			return;
		}
		string displayText = getDisplayText();
		mTextComponent.setText(displayText);
	}
	private string getDisplayText()
	{
		string display = mInputType == FastInputFieldInputType.Password ? new string(mAsteriskChar, mText.Length) : mText;
		if (!mFocused || string.IsNullOrEmpty(mCompositionString))
		{
			return display;
		}
		int compositionStart = getCompositionInsertionPosition();
		if (hasSelection())
		{
			display = display.Remove(getSelectionStart(), getSelectionEnd() - getSelectionStart());
		}
		compositionStart = Mathf.Clamp(compositionStart, 0, display.Length);
		string composition = mInputType == FastInputFieldInputType.Password ? new string(mAsteriskChar, mCompositionString.Length) : mCompositionString;
		return display.Insert(compositionStart, composition);
	}
	private void refreshPlaceholder()
	{
		if (mPlaceholderComponent == null)
		{
			return;
		}
		bool visible = string.IsNullOrEmpty(mText) && string.IsNullOrEmpty(mCompositionString);
		mPlaceholderComponent.setVisible(visible);
	}
	private void refreshCaretAndSelection()
	{
		if (mTextComponent == null || mCaretGraphic == null || mSelectionGraphic == null)
		{
			return;
		}
		if (!mFocused)
		{
			mCaretGraphic.clearRects();
			mSelectionGraphic.clearRects();
			return;
		}
		if (!string.IsNullOrEmpty(mCompositionString))
		{
			mSelectionGraphic.clearRects();
			if (mCaretVisible)
			{
				mCaretGraphic.setRect(mTextComponent.getCaretLocalRect(getVisualCaretIndex(), mCaretWidth));
			}
			return;
		}
		if (hasSelection())
		{
			mCaretGraphic.clearRects();
			buildSelectionGeometry();
		}
		else
		{
			mSelectionGraphic.clearRects();
			if (mReadOnly)
			{
				mCaretGraphic.clearRects();
				return;
			}
			if (mCaretVisible)
			{
				mCaretGraphic.setRect(mTextComponent.getCaretLocalRect(mCaretPosition, mCaretWidth));
			}
			else
			{
				mCaretGraphic.clearRects();
			}
		}
	}
	private void buildSelectionGeometry()
	{
		int start = getSelectionStart();
		int end = getSelectionEnd();
		int lineCount = Mathf.Max(mTextComponent.getLineCount(), 1);
		int rectCount = 0;
		ensureSelectionRectCapacity(lineCount);
		for (int line = 0; line < lineCount; ++line)
		{
			if (mTextComponent.getSelectionLocalRect(line, start, end, out Rect rect))
			{
				mSelectionRects[rectCount++] = rect;
			}
		}
		mSelectionGraphic.setRects(mSelectionRects, rectCount);
	}
	private void ensureSelectionRectCapacity(int required)
	{
		if (required <= mSelectionRects.Length)
		{
			return;
		}
		Array.Resize(ref mSelectionRects, Mathf.NextPowerOfTwo(Mathf.Max(required, 4)));
	}
	private void resetCaretBlink()
	{
		mBlinkStartTime = Time.unscaledTime;
		setCaretVisible(true);
	}
	private void updateCaretBlink()
	{
		if (mReadOnly || hasSelection())
		{
			setCaretVisible(false);
			return;
		}
		if (mCaretBlinkRate <= 0.0f)
		{
			setCaretVisible(true);
			return;
		}
		float period = 1.0f / mCaretBlinkRate;
		bool visible = Mathf.Repeat(Time.unscaledTime - mBlinkStartTime, period) < period * 0.5f;
		setCaretVisible(visible);
	}
	private void setCaretVisible(bool visible)
	{
		if (mCaretVisible == visible)
		{
			return;
		}
		mCaretVisible = visible;
		refreshCaretAndSelection();
	}
	private void ensureCaretVisible()
	{
		if (!mFocused || mTextComponent == null || mViewportRectTransform == null)
		{
			return;
		}
		Rect caret = mTextComponent.getCaretLocalRect(getVisualCaretIndex(), mCaretWidth);
		Rect viewport = mViewportRectTransform.rect;
		Vector2 newOffset = mScrollOffset;
		float caretMinX = caret.xMin + newOffset.x;
		float caretMaxX = caret.xMax + newOffset.x;
		if (caretMinX < viewport.xMin)
		{
			newOffset.x += viewport.xMin - caretMinX;
		}
		else if (caretMaxX > viewport.xMax)
		{
			newOffset.x -= caretMaxX - viewport.xMax;
		}
		if (mLineType == FastInputFieldLineType.SingleLine)
		{
			newOffset.y = 0.0f;
		}
		else
		{
			float caretMinY = caret.yMin + newOffset.y;
			float caretMaxY = caret.yMax + newOffset.y;
			if (caretMinY < viewport.yMin)
			{
				newOffset.y += viewport.yMin - caretMinY;
			}
			else if (caretMaxY > viewport.yMax)
			{
				newOffset.y -= caretMaxY - viewport.yMax;
			}
		}
		if (newOffset != mScrollOffset)
		{
			mScrollOffset = newOffset;
			applyScrollOffset();
		}
	}
	private void applyScrollOffset()
	{
		if (mTextRectTransform == null)
		{
			return;
		}
		mTextComponent.setAnchoredPosition(mScrollOffset);
		mSelectionGraphic.setAnchoredPosition(mScrollOffset);
		mCaretGraphic.setAnchoredPosition(mScrollOffset);
	}
	private void setTextInternal(string text, bool notify)
	{
		text ??= string.Empty;
		text = normalizeExternalText(text);
		if (mText == text)
		{
			clampPositions();
			refreshVisualState(false);
			return;
		}
		mText = text;
		clampPositions();
		refreshVisualState(false);
		if (notify)
		{
			mOnValueChanged?.Invoke(mText);
		}
	}
	private string normalizeExternalText(string text)
	{
		text ??= string.Empty;
		text = text.Replace("\0", string.Empty);
		if (mLineType != FastInputFieldLineType.MultiLineNewline)
		{
			text = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
		}
		else
		{
			text = text.Replace("\r\n", "\n").Replace('\r', '\n');
		}
		if (mCharacterLimit > 0 && text.Length > mCharacterLimit)
		{
			text = text[..mCharacterLimit];
		}
		return enforceLineLimit(text);
	}
	private string enforceLineLimit(string text)
	{
		if (mLineLimit <= 0 || string.IsNullOrEmpty(text))
		{
			return text;
		}
		int line = 1;
		for (int i = 0; i < text.Length; ++i)
		{
			if (text[i] != '\n')
			{
				continue;
			}
			++line;
			if (line > mLineLimit)
			{
				return text[..i];
			}
		}
		return text;
	}
	private bool insertValidatedCharacter(char character, bool notify)
	{
		int basePosition = hasSelection() ? getSelectionStart() : mCaretPosition;
		string baseText = hasSelection() ? mText.Remove(getSelectionStart(), getSelectionEnd() - getSelectionStart()) : mText;
		if (mCharacterLimit > 0 && baseText.Length >= mCharacterLimit)
		{
			return false;
		}
		if (onValidateInput != null)
		{
			char callbackCharacter = onValidateInput(baseText, basePosition, character);
			if (callbackCharacter == '\0')
			{
				return false;
			}
			return commitValidatedCharacter(baseText, basePosition, callbackCharacter, notify);
		}
		if (mCharacterValidation == FastInputFieldCharacterValidation.CustomValidator && mInputValidator != null)
		{
			string validatedText = baseText;
			int validatedPosition = basePosition;
			char validated = mInputValidator.Validate(ref validatedText, ref validatedPosition, character);
			if (validated == '\0')
			{
				return false;
			}
			if (validatedText == baseText)
			{
				validatedText = validatedText.Insert(Mathf.Clamp(validatedPosition, 0, validatedText.Length), validated.ToString());
				validatedPosition = Mathf.Clamp(validatedPosition + 1, 0, validatedText.Length);
			}
			if (mCharacterLimit > 0 && validatedText.Length > mCharacterLimit)
			{
				return false;
			}
			if (mLineLimit > 0 && countExplicitLines(validatedText) > mLineLimit)
			{
				return false;
			}
			mText = validatedText;
			mCaretPosition = normalizeTextIndex(validatedPosition);
			mSelectionAnchorPosition = mCaretPosition;
			if (notify)
			{
				onTextEdited();
			}
			return true;
		}
		char validatedCharacter = validateCharacter(baseText, basePosition, character);
		if (validatedCharacter == '\0')
		{
			return false;
		}
		return commitValidatedCharacter(baseText, basePosition, validatedCharacter, notify);
	}
	private bool commitValidatedCharacter(string baseText, int basePosition, char validatedCharacter, bool notify)
	{
		string candidate = baseText.Insert(basePosition, validatedCharacter.ToString());
		if (mCharacterLimit > 0 && candidate.Length > mCharacterLimit)
		{
			return false;
		}
		if (mLineLimit > 0 && countExplicitLines(candidate) > mLineLimit)
		{
			return false;
		}
		mText = candidate;
		mCaretPosition = basePosition + 1;
		mSelectionAnchorPosition = mCaretPosition;
		if (notify)
		{
			onTextEdited();
		}
		return true;
	}

	private char validateCharacter(string text, int position, char character)
	{
		switch (mCharacterValidation)
		{
			case FastInputFieldCharacterValidation.None:
				return character;
			case FastInputFieldCharacterValidation.Integer:
			case FastInputFieldCharacterValidation.Decimal:
				{
					bool cursorBeforeDash = position == 0 && text.Length > 0 && text[0] == '-';
					if (cursorBeforeDash)
					{
						return '\0';
					}
					if (character >= '0' && character <= '9')
					{
						return character;
					}
					if (character == '-' && position == 0 && !text.Contains("-"))
					{
						return character;
					}
					string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
					char decimalCharacter = !string.IsNullOrEmpty(separator) ? separator[0] : '.';
					if (mCharacterValidation == FastInputFieldCharacterValidation.Decimal && character == decimalCharacter && !text.Contains(decimalCharacter.ToString()))
					{
						return character;
					}
					if (mCharacterValidation == FastInputFieldCharacterValidation.Integer && character == '.' && position == 0 && !text.Contains("-"))
					{
						return '-';
					}
					return '\0';
				}
			case FastInputFieldCharacterValidation.Digit:
				return character >= '0' && character <= '9' ? character : '\0';
			case FastInputFieldCharacterValidation.Alphanumeric:
				return (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z') || (character >= '0' && character <= '9') ? character : '\0';
			case FastInputFieldCharacterValidation.Name:
				return validateNameCharacter(text, position, character);
			case FastInputFieldCharacterValidation.EmailAddress:
				return validateEmailCharacter(text, position, character);
			case FastInputFieldCharacterValidation.Regex:
				if (string.IsNullOrEmpty(mRegexValue))
				{
					return character;
				}
				try
				{
					return Regex.IsMatch(character.ToString(), mRegexValue) ? character : '\0';
				}
				catch (ArgumentException)
				{
					return '\0';
				}
			case FastInputFieldCharacterValidation.CustomValidator:
				return character;
			default:
				return '\0';
		}
	}
	private char validateNameCharacter(string text, int position, char character)
	{
		char prevChar = text.Length > 0 ? text[Mathf.Clamp(position - 1, 0, text.Length - 1)] : ' ';
		char currentChar = text.Length > 0 ? text[Mathf.Clamp(position, 0, text.Length - 1)] : ' ';
		char nextChar = text.Length > 0 ? text[Mathf.Clamp(position + 1, 0, text.Length - 1)] : '\n';
		if (char.IsLetter(character))
		{
			if (char.IsLower(character) && position == 0)
			{
				return char.ToUpper(character, CultureInfo.CurrentCulture);
			}
			if (char.IsLower(character) && (prevChar == ' ' || prevChar == '-'))
			{
				return char.ToUpper(character, CultureInfo.CurrentCulture);
			}
			if (char.IsUpper(character) && position > 0 && prevChar != ' ' && prevChar != '\'' && prevChar != '-' && !char.IsLower(prevChar))
			{
				return char.ToLower(character, CultureInfo.CurrentCulture);
			}
			if (char.IsUpper(character) && char.IsUpper(currentChar))
			{
				return '\0';
			}
			return character;
		}
		if (character == '\'' && currentChar != ' ' && currentChar != '\'' && nextChar != '\'' && !text.Contains("'"))
		{
			return character;
		}
		if (char.IsLetter(prevChar) && character == '-' && currentChar != '-')
		{
			return character;
		}
		if ((character == ' ' || character == '-') && position != 0 && prevChar != ' ' && prevChar != '\'' && prevChar != '-' &&
			currentChar != ' ' && currentChar != '\'' && currentChar != '-' && nextChar != ' ' && nextChar != '\'' && nextChar != '-')
		{
			return character;
		}
		return '\0';
	}
	private char validateEmailCharacter(string text, int position, char character)
	{
		if ((character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z') || (character >= '0' && character <= '9'))
		{
			return character;
		}
		if (character == '@' && text.IndexOf('@') == -1)
		{
			return character;
		}
		const string specialCharacters = "!#$%&'*+-/=?^_`{|}~";
		if (specialCharacters.IndexOf(character) != -1)
		{
			return character;
		}
		if (character == '.')
		{
			char currentChar = text.Length > 0 ? text[Mathf.Clamp(position, 0, text.Length - 1)] : ' ';
			char nextChar = text.Length > 0 ? text[Mathf.Clamp(position + 1, 0, text.Length - 1)] : '\n';
			if (currentChar != '.' && nextChar != '.')
			{
				return character;
			}
		}
		return '\0';
	}

	private void enforceContentType()
	{
		switch (mContentType)
		{
			case FastInputFieldContentType.Standard:
				mInputType = FastInputFieldInputType.Standard;
				mKeyboardType = TouchScreenKeyboardType.Default;
				mCharacterValidation = FastInputFieldCharacterValidation.None;
				break;
			case FastInputFieldContentType.Autocorrected:
				mInputType = FastInputFieldInputType.AutoCorrect;
				mKeyboardType = TouchScreenKeyboardType.Default;
				mCharacterValidation = FastInputFieldCharacterValidation.None;
				break;
			case FastInputFieldContentType.IntegerNumber:
				mLineType = FastInputFieldLineType.SingleLine;
				mInputType = FastInputFieldInputType.Standard;
				mKeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
				mCharacterValidation = FastInputFieldCharacterValidation.Integer;
				break;
			case FastInputFieldContentType.DecimalNumber:
				mLineType = FastInputFieldLineType.SingleLine;
				mInputType = FastInputFieldInputType.Standard;
				mKeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
				mCharacterValidation = FastInputFieldCharacterValidation.Decimal;
				break;
			case FastInputFieldContentType.Alphanumeric:
				mLineType = FastInputFieldLineType.SingleLine;
				mInputType = FastInputFieldInputType.Standard;
				mKeyboardType = TouchScreenKeyboardType.ASCIICapable;
				mCharacterValidation = FastInputFieldCharacterValidation.Alphanumeric;
				break;
			case FastInputFieldContentType.Name:
				mLineType = FastInputFieldLineType.SingleLine;
				mInputType = FastInputFieldInputType.Standard;
				mKeyboardType = TouchScreenKeyboardType.Default;
				mCharacterValidation = FastInputFieldCharacterValidation.Name;
				break;
			case FastInputFieldContentType.EmailAddress:
				mLineType = FastInputFieldLineType.SingleLine;
				mInputType = FastInputFieldInputType.Standard;
				mKeyboardType = TouchScreenKeyboardType.EmailAddress;
				mCharacterValidation = FastInputFieldCharacterValidation.EmailAddress;
				break;
			case FastInputFieldContentType.Password:
				mLineType = FastInputFieldLineType.SingleLine;
				mInputType = FastInputFieldInputType.Password;
				mKeyboardType = TouchScreenKeyboardType.Default;
				mCharacterValidation = FastInputFieldCharacterValidation.None;
				break;
			case FastInputFieldContentType.Pin:
				mLineType = FastInputFieldLineType.SingleLine;
				mInputType = FastInputFieldInputType.Password;
				mKeyboardType = TouchScreenKeyboardType.NumberPad;
				mCharacterValidation = FastInputFieldCharacterValidation.Digit;
				break;
		}
		refreshTextLayoutMode();
	}

	private bool deleteSelection()
	{
		return deleteSelection(true);
	}
	private bool deleteSelection(bool notify)
	{
		if (!hasSelection())
		{
			return false;
		}
		int start = getSelectionStart();
		int end = getSelectionEnd();
		mText = mText.Remove(start, end - start);
		mCaretPosition = start;
		mSelectionAnchorPosition = start;
		if (notify)
		{
			onTextEdited();
		}
		return true;
	}
	private void onTextEdited()
	{
		mText = normalizeExternalText(mText);
		clampPositions();
		resetCaretBlink();
		refreshVisualState(false);
		mOnValueChanged?.Invoke(mText);
		if (mKeyboard != null)
		{
			mKeyboard.text = mText;
		}
	}
	private bool canEdit()
	{
		return mInteractable && !mReadOnly;
	}
	private void clampPositions()
	{
		mCaretPosition = normalizeTextIndex(mCaretPosition);
		mSelectionAnchorPosition = normalizeTextIndex(mSelectionAnchorPosition);
	}
	private int normalizeTextIndex(int index)
	{
		index = Mathf.Clamp(index, 0, mText != null ? mText.Length : 0);
		if (mText != null && index > 0 && index < mText.Length && char.IsLowSurrogate(mText[index]) && char.IsHighSurrogate(mText[index - 1]))
		{
			--index;
		}
		return index;
	}
	private int previousTextIndex(int index)
	{
		index = normalizeTextIndex(index);
		if (index <= 0)
		{
			return 0;
		}
		--index;
		if (index > 0 && char.IsLowSurrogate(mText[index]) && char.IsHighSurrogate(mText[index - 1]))
		{
			--index;
		}
		return index;
	}
	private int nextTextIndex(int index)
	{
		index = normalizeTextIndex(index);
		if (index >= mText.Length)
		{
			return mText.Length;
		}
		if (char.IsHighSurrogate(mText[index]) && index + 1 < mText.Length && char.IsLowSurrogate(mText[index + 1]))
		{
			return index + 2;
		}
		return index + 1;
	}
	private void moveHorizontal(int direction, bool keepSelection, bool byWord)
	{
		if (!keepSelection && hasSelection())
		{
			setCaretPosition(direction < 0 ? getSelectionStart() : getSelectionEnd(), false);
			return;
		}
		int position = mCaretPosition;
		if (byWord)
		{
			position = direction < 0 ? findPreviousWordStart(position) : findNextWordEnd(position);
		}
		else
		{
			position = direction < 0 ? previousTextIndex(position) : nextTextIndex(position);
		}
		setCaretPosition(position, keepSelection);
	}
	private void moveVertical(int direction, bool keepSelection)
	{
		ensureHierarchy();
		int currentLine = mTextComponent.getLineIndexFromStringIndex(mCaretPosition);
		int lineCount = Mathf.Max(mTextComponent.getLineCount(), 1);
		int targetLine = Mathf.Clamp(currentLine + direction, 0, lineCount - 1);
		if (targetLine == currentLine)
		{
			setCaretPosition(direction < 0 ? 0 : mText.Length, keepSelection);
			return;
		}
		Rect caret = mTextComponent.getCaretLocalRect(mCaretPosition, mCaretWidth);
		float targetY = mTextComponent.getCaretLocalRect(mTextComponent.getLineStartStringIndex(targetLine), mCaretWidth).center.y;
		int target = mTextComponent.getStringIndexFromLocalPosition(new Vector2(caret.xMin, targetY));
		setCaretPosition(displayIndexToTextIndex(target), keepSelection);
	}
	private void movePage(int direction, bool keepSelection)
	{
		ensureHierarchy();
		float lineAdvance = Mathf.Max(mTextComponent.getLineAdvance(), 0.01f);
		float viewportHeight = mViewportRectTransform != null ? Mathf.Abs(mViewportRectTransform.rect.height) : lineAdvance;
		int linesPerPage = Mathf.Max(1, Mathf.FloorToInt(viewportHeight / lineAdvance));
		int currentLine = mTextComponent.getLineIndexFromStringIndex(mCaretPosition);
		int lineCount = Mathf.Max(mTextComponent.getLineCount(), 1);
		int targetLine = Mathf.Clamp(currentLine + direction * linesPerPage, 0, lineCount - 1);
		Rect caret = mTextComponent.getCaretLocalRect(mCaretPosition, mCaretWidth);
		float targetY = mTextComponent.getCaretLocalRect(mTextComponent.getLineStartStringIndex(targetLine), mCaretWidth).center.y;
		int displayIndex = mTextComponent.getStringIndexFromLocalPosition(new Vector2(caret.xMin, targetY));
		setCaretPosition(displayIndexToTextIndex(displayIndex), keepSelection);
	}
	private void moveLineBoundary(bool end, bool keepSelection, bool wholeText)
	{
		if (wholeText)
		{
			setCaretPosition(end ? mText.Length : 0, keepSelection);
			return;
		}
		int line = mTextComponent.getLineIndexFromStringIndex(mCaretPosition);
		int position = end ? mTextComponent.getLineEndStringIndex(line) : mTextComponent.getLineStartStringIndex(line);
		setCaretPosition(displayIndexToTextIndex(position), keepSelection);
	}
	private int findPreviousWordStart(int position)
	{
		position = normalizeTextIndex(position);
		while (position > 0 && char.IsWhiteSpace(mText[position - 1]))
		{
			--position;
		}
		while (position > 0 && !char.IsWhiteSpace(mText[position - 1]))
		{
			--position;
		}
		return normalizeTextIndex(position);
	}
	private int findNextWordEnd(int position)
	{
		position = normalizeTextIndex(position);
		while (position < mText.Length && char.IsWhiteSpace(mText[position]))
		{
			++position;
		}
		while (position < mText.Length && !char.IsWhiteSpace(mText[position]))
		{
			++position;
		}
		return normalizeTextIndex(position);
	}
	private int getCompositionInsertionPosition()
	{
		return hasSelection() ? getSelectionStart() : mCaretPosition;
	}
	private int getVisualCaretIndex()
	{
		return getCompositionInsertionPosition() + (!string.IsNullOrEmpty(mCompositionString) ? mCompositionString.Length : 0);
	}
	private int displayIndexToTextIndex(int displayIndex)
	{
		if (string.IsNullOrEmpty(mCompositionString))
		{
			return normalizeTextIndex(displayIndex);
		}
		int compositionStart = getCompositionInsertionPosition();
		int compositionEnd = compositionStart + mCompositionString.Length;
		int removedSelectionLength = hasSelection() ? getSelectionEnd() - getSelectionStart() : 0;
		if (displayIndex <= compositionStart)
		{
			return normalizeTextIndex(displayIndex);
		}
		if (displayIndex <= compositionEnd)
		{
			return compositionStart;
		}
		return normalizeTextIndex(displayIndex - mCompositionString.Length + removedSelectionLength);
	}
	private int countExplicitLines(string text)
	{
		int count = 1;
		for (int i = 0; i < text.Length; ++i)
		{
			if (text[i] == '\n')
			{
				++count;
			}
		}
		return count;
	}
	private void syncSelectionToKeyboard()
	{
		if (mKeyboard == null || !mKeyboard.canSetSelection)
		{
			return;
		}
		int start = getSelectionStart();
		int length = getSelectionEnd() - start;
		mKeyboard.selection = new RangeInt(start, length);
	}
	private void updateIMECursorPosition()
	{
		if (!mFocused || string.IsNullOrEmpty(mCompositionString) || mTextComponent == null || mTextRectTransform == null)
		{
			return;
		}
		Rect caret = mTextComponent.getCaretLocalRect(getVisualCaretIndex(), mCaretWidth);
		Vector3 worldPosition = mTextRectTransform.TransformPoint(new Vector3(caret.xMin, caret.yMin, 0.0f));
		Camera camera = mEventCamera != null ? mEventCamera : Camera.main;
		Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
		screenPosition.y = Screen.height - screenPosition.y;
		try
		{
			Input.compositionCursorPos = screenPosition;
		}
		catch (InvalidOperationException)
		{
		}
	}
	private void processKeyboardEvents()
	{
		while (Event.PopEvent(mProcessingEvent))
		{
			if (mProcessingEvent.type == EventType.KeyDown)
			{
				processKeyEvent(mProcessingEvent);
				if (!mFocused)
				{
					break;
				}
			}
		}
	}
	private void updateCompositionString()
	{
		string composition;
		try
		{
			composition = Input.compositionString ?? string.Empty;
		}
		catch (InvalidOperationException)
		{
			composition = string.Empty;
		}
		if (composition == mCompositionString)
		{
			return;
		}
		mCompositionString = composition;
		refreshTextDisplay();
		refreshPlaceholder();
		refreshCaretAndSelection();
		ensureCaretVisible();
	}
	private void commitCompositionString()
	{
		if (string.IsNullOrEmpty(mCompositionString) || mReadOnly)
		{
			mCompositionString = string.Empty;
			return;
		}
		string composition = mCompositionString;
		mCompositionString = string.Empty;
		insert(composition);
	}
	private void openTouchScreenKeyboard()
	{
		if (!mEnableTouchScreenKeyboard || !TouchScreenKeyboard.isSupported)
		{
			return;
		}
		bool autocorrection = mInputType == FastInputFieldInputType.AutoCorrect;
		bool multiline = mLineType != FastInputFieldLineType.SingleLine;
		bool secure = mInputType == FastInputFieldInputType.Password;
		mKeyboard = TouchScreenKeyboard.Open(mText, mKeyboardType, autocorrection, multiline, secure, false, mPlaceholder, mCharacterLimit);
	}
	private string validateKeyboardText(string value)
	{
		value ??= string.Empty;
		string result = string.Empty;
		int position = 0;
		for (int i = 0; i < value.Length; ++i)
		{
			char character = value[i];
			if (character == '\r' || character == (char)3)
			{
				character = '\n';
			}
			if (character == '\n' && mLineType != FastInputFieldLineType.MultiLineNewline)
			{
				continue;
			}
			if (mCharacterLimit > 0 && result.Length >= mCharacterLimit)
			{
				break;
			}
			if (onValidateInput != null)
			{
				character = onValidateInput(result, position, character);
				if (character == '\0')
				{
					continue;
				}
			}
			else if (mCharacterValidation == FastInputFieldCharacterValidation.CustomValidator && mInputValidator != null)
			{
				string before = result;
				string validatedText = result;
				int validatedPosition = position;
				char validated = mInputValidator.Validate(ref validatedText, ref validatedPosition, character);
				if (validated == '\0')
				{
					continue;
				}
				if (validatedText == before)
				{
					validatedText = validatedText.Insert(Mathf.Clamp(validatedPosition, 0, validatedText.Length), validated.ToString());
					validatedPosition = Mathf.Clamp(validatedPosition + 1, 0, validatedText.Length);
				}
				if (mCharacterLimit > 0 && validatedText.Length > mCharacterLimit)
				{
					validatedText = validatedText[..mCharacterLimit];
				}
				if (mLineLimit > 0 && countExplicitLines(validatedText) > mLineLimit)
				{
					continue;
				}
				result = validatedText;
				position = Mathf.Clamp(validatedPosition, 0, result.Length);
				continue;
			}
			else
			{
				character = validateCharacter(result, position, character);
				if (character == '\0')
				{
					continue;
				}
			}
			string candidate = result.Insert(position, character.ToString());
			if (mCharacterLimit > 0 && candidate.Length > mCharacterLimit)
			{
				break;
			}
			if (mLineLimit > 0 && countExplicitLines(candidate) > mLineLimit)
			{
				continue;
			}
			result = candidate;
			++position;
		}
		return result;
	}

	private void processTouchScreenKeyboard()
	{
		if (mKeyboard == null)
		{
			return;
		}
		bool textChanged = mKeyboard.text != mText && !mReadOnly;
		if (textChanged)
		{
			string keyboardText = validateKeyboardText(mKeyboard.text);
			setTextInternal(keyboardText, true);
			if (mKeyboard.text != keyboardText)
			{
				mKeyboard.text = keyboardText;
			}
		}
		if (mKeyboard.canGetSelection)
		{
			RangeInt range = mKeyboard.selection;
			int anchor = Mathf.Clamp(range.start, 0, mText.Length);
			int focus = Mathf.Clamp(range.start + range.length, 0, mText.Length);
			if (anchor != mSelectionAnchorPosition || focus != mCaretPosition)
			{
				setSelection(anchor, focus);
			}
		}
		else if (textChanged)
		{
			setCaretPosition(mText.Length, false);
		}
		switch (mKeyboard.status)
		{
			case TouchScreenKeyboard.Status.Done:
				deactivateInputField(true);
				break;
			case TouchScreenKeyboard.Status.Canceled:
				cancelInput();
				break;
			case TouchScreenKeyboard.Status.LostFocus:
				deactivateInputField(false);
				break;
		}
	}
}
