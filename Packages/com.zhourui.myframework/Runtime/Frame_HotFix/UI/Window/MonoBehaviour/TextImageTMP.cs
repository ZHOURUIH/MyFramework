using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

// TMP图文混排组件，图片由外部对象池创建。
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextImageTMP : MonoBehaviour
{
	protected class InlineImage
	{
		public int mIndex;
		public string mSprite;
		public float mFPS;
		public myUGUIImage mImage;
		public myUGUIImageAnim mImageAnim;
	}
	protected const string HIDE_START = "<color=#00000000>";
	protected const string HIDE_END = "</color>";
	protected static readonly Regex mImageTag = new(@"<quad\s+width=(\d*\.?\d+)\s+sprite=([^\s/>]+)(?:\s+fps=(\d*\.?\d+))?\s*/>");
	protected List<InlineImage> mImages = new();
	protected TextMeshProUGUI mText;
	protected ImageFunction mCreateImage;
	protected ImageAnimFunction mCreateImageAnim;
	protected ImageCallback mDestroyImage;
	protected ImageAnimCallback mDestroyImageAnim;
	protected Action mLinkLayoutChanged;
	protected string mSourceText;
	protected void OnEnable()
	{
		mText = GetComponent<TextMeshProUGUI>();
		mText.OnPreRenderText += onPreRenderText;
	}
	protected void OnDisable()
	{
		if (mText != null)
		{
			mText.OnPreRenderText -= onPreRenderText;
		}
	}
	public void setCreateImage(ImageFunction callback) { mCreateImage = callback; createImages(); }
	public void setCreateImageAnim(ImageAnimFunction callback) { mCreateImageAnim = callback; createImages(); }
	public void setDestroyImage(ImageCallback callback) { mDestroyImage = callback; createImages(); }
	public void setDestroyImageAnim(ImageAnimCallback callback) { mDestroyImageAnim = callback; createImages(); }
	public void setLinkLayoutChanged(Action callback) { mLinkLayoutChanged = callback; }
	public string getText() { return mSourceText ?? GetComponent<TextMeshProUGUI>().text; }
	public void setText(string text)
	{
		clearImages();
		mText ??= GetComponent<TextMeshProUGUI>();
		mSourceText = text ?? string.Empty;
		StringBuilder output = new();
		int start = 0;
		foreach (Match match in mImageTag.Matches(mSourceText))
		{
			output.Append(mSourceText, start, match.Index - start);
			float width = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
			output.Append("<size=").Append((width * 100.0f).ToString(CultureInfo.InvariantCulture)).Append("%>").Append(HIDE_START);
			InlineImage image = new()
			{
				mIndex = output.Length,
				mSprite = match.Groups[2].Value,
				mFPS = match.Groups[3].Success ? float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture) : 0.0f,
			};
			mImages.Add(image);
			output.Append('　').Append(HIDE_END).Append("</size>");
			start = match.Index + match.Length;
		}
		output.Append(mSourceText, start, mSourceText.Length - start);
		if (mImages.Count > 0)
		{
			mText.richText = true;
		}
		mText.text = output.ToString();
		createImages();
	}
	protected void createImages()
	{
		foreach (InlineImage image in mImages)
		{
			if (image.mFPS > 0.0f)
			{
				if (image.mImageAnim != null || mCreateImageAnim == null || mDestroyImageAnim == null)
				{
					continue;
				}
				image.mImageAnim = mCreateImageAnim();
				image.mImageAnim.setTextureSet(image.mSprite);
				image.mImageAnim.setLoop(LOOP_MODE.LOOP);
				image.mImageAnim.setInterval(1.0f / image.mFPS);
				image.mImageAnim.play();
			}
			else
			{
				if (image.mImage != null || mCreateImage == null || mDestroyImage == null)
				{
					continue;
				}
				image.mImage = mCreateImage();
				image.mImage.setSpriteName(image.mSprite);
			}
		}
	}
	public void refreshImages()
	{
		if (mImages.Count == 0)
		{
			return;
		}
		mText.ForceMeshUpdate(true, true);
		layoutImages(mText.textInfo);
	}
	protected void onPreRenderText(TMP_TextInfo info)
	{
		layoutImages(info);
		mLinkLayoutChanged?.Invoke();
	}
	protected void layoutImages(TMP_TextInfo info)
	{
		foreach (InlineImage image in mImages)
		{
			myUGUIImage window = image.mImage ?? image.mImageAnim;
			if (window == null)
			{
				continue;
			}
			bool found = false;
			for (int i = 0; i < info.characterCount; ++i)
			{
				TMP_CharacterInfo character = info.characterInfo[i];
				if (character.index != image.mIndex)
				{
					continue;
				}
				float size = Mathf.Min(character.xAdvance - character.origin, character.ascender - character.descender);
				Vector2 spriteSize = window.getSpriteSize();
				float height = spriteSize.x > 0.0f ? spriteSize.y / spriteSize.x : 1.0f;
				found = true;
				window.setSize(new(size, size * height));
				window.setPosition(new Vector2((character.origin + character.xAdvance) * 0.5f, (character.ascender + character.descender) * 0.5f));
				break;
			}
			window.setActive(found);
		}
	}
	public void clearImages()
	{
		foreach (InlineImage image in mImages)
		{
			if (image.mImage != null && mDestroyImage != null)
			{
				mDestroyImage(image.mImage);
			}
			if (image.mImageAnim != null && mDestroyImageAnim != null)
			{
				mDestroyImageAnim(image.mImageAnim);
			}
		}
		mImages.Clear();
	}
	public void refreshImageAtlas()
	{
		foreach (InlineImage image in mImages)
		{
			image.mImage?.setSpriteName(image.mSprite);
		}
		refreshImages();
	}
	public void getLinkAreas(List<TextImageLinkArea> areas)
	{
		TMP_TextInfo info = mText.textInfo;
		for (int i = 0; i < info.linkCount; ++i)
		{
			TMP_LinkInfo link = info.linkInfo[i];
			int end = Mathf.Min(link.linkTextfirstCharacterIndex + link.linkTextLength, info.characterCount);
			int currentLine = -1;
			Rect line = default;
			for (int j = link.linkTextfirstCharacterIndex; j < end; ++j)
			{
				TMP_CharacterInfo character = info.characterInfo[j];
				if (character.character == '\n' || character.character == '\r' || character.origin == character.xAdvance)
				{
					continue;
				}
				Rect rect = Rect.MinMaxRect(Mathf.Min(character.origin, character.xAdvance), character.descender,
					Mathf.Max(character.origin, character.xAdvance), character.ascender);
				if (currentLine == character.lineNumber)
				{
					line = Rect.MinMaxRect(Mathf.Min(line.xMin, rect.xMin), Mathf.Min(line.yMin, rect.yMin),
						Mathf.Max(line.xMax, rect.xMax), Mathf.Max(line.yMax, rect.yMax));
					continue;
				}
				if (currentLine >= 0)
				{
					areas.Add(new(link.GetLinkID(), line));
				}
				line = rect;
				currentLine = character.lineNumber;
			}
			if (currentLine >= 0)
				{
				areas.Add(new(link.GetLinkID(), line));
				}
		}
	}
}
