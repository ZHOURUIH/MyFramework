using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public struct TextImageLinkArea
{
	public string mID;
	public Rect mRect;
	public TextImageLinkArea(string id, Rect rect) { mID = id; mRect = rect; }
}

// 带图文混排的文字组件
public class TextImage : Text
{
	protected class InlineImage
	{
		public int mIndex;			// 图片在显示文本中的字符索引
		public string mSprite;
		public float mWidth;
		public float mFPS;
		public myUGUIImage mImage;
		public myUGUIImageAnim mImageAnim;
	}
	protected class InlineLink
	{
		public string mID;
		public int mStart;
		public int mEnd;
		public List<Rect> mRects = new();
		public List<int> mLines = new();
	}
	protected List<InlineImage> mImages = new();	// 图片列表
	protected List<InlineLink> mLinks = new();
	protected List<UIVertex> mVertices = new();		// 避免GC用
	protected List<int> mVisibleIndices = new();
	protected ImageFunction mCreateImage;			// 创建图片的委托,让外边决定怎么创建图片
	protected ImageAnimFunction mCreateImageAnim;
	protected ImageCallback mDestroyImage;			// 销毁图片的委托,让外边决定怎么销毁图片
	protected ImageAnimCallback mDestroyImageAnim;
	protected Action mLinkLayoutChanged;
	protected string mSourceText;
	protected int mParsedFontSize;
	// 正则取出所需要的属性
	protected static readonly Regex mTokens = new(
		@"<link=([^>]+)>|</link>|<quad\s+width=(\d*\.?\d+)\s+sprite=([^\s/>]+)(?:\s+fps=(\d*\.?\d+))?\s*/>|<[^>]+>|\n|.",
		RegexOptions.Singleline);
	public override string text
	{
		get => base.text;
		set
		{
			if (mSourceText == value)
			{
				return;
			}
			mSourceText = value;
			parseText();
		}
	}
	public string getSourceText() { return mSourceText ?? base.text; }
	public void refreshImageAtlas()
	{
		foreach (InlineImage image in mImages)
		{
			if (image.mImage == null)
			{
				continue;
			}
			image.mImage.setSpriteName(image.mSprite);
			Vector2 spriteSize = image.mImage.getSpriteSize();
			float height = spriteSize.x > 0.0f ? spriteSize.y / spriteSize.x : 1.0f;
			image.mImage.setSize(new(fontSize * image.mWidth, fontSize * image.mWidth * height));
		}
	}
	public void refreshImageAnimAtlas()
	{
		foreach (InlineImage image in mImages)
		{
			if (image.mImageAnim == null)
			{
				continue;
			}
			Vector2 spriteSize = image.mImageAnim.getSpriteSize();
			float height = spriteSize.x > 0.0f ? spriteSize.y / spriteSize.x : 1.0f;
			image.mImageAnim.setSize(new(fontSize * image.mWidth, fontSize * image.mWidth * height));
		}
	}
	public void getLinkAreas(List<TextImageLinkArea> areas)
	{
		foreach (InlineLink link in mLinks)
		{
			Rect line = default;
			int lineIndex = -1;
			for (int i = 0; i < link.mRects.Count; ++i)
			{
				Rect rect = link.mRects[i];
				if (rect.width <= 0.0f || rect.height <= 0.0f)
				{
					continue;
				}
				if (lineIndex == link.mLines[i])
				{
					line = Rect.MinMaxRect(Mathf.Min(line.xMin, rect.xMin), Mathf.Min(line.yMin, rect.yMin),
						Mathf.Max(line.xMax, rect.xMax), Mathf.Max(line.yMax, rect.yMax));
					continue;
				}
				if (lineIndex >= 0)
				{
					areas.Add(new(link.mID, line));
				}
				line = rect;
				lineIndex = link.mLines[i];
			}
			if (lineIndex >= 0)
			{
				areas.Add(new(link.mID, line));
			}
		}
	}
	public void setCreateImageAnim(ImageAnimFunction callback) { mCreateImageAnim = callback; refreshImages(); }
	public void setDestroyImageAnim(ImageAnimCallback callback) { mDestroyImageAnim = callback; refreshImages(); }
	public void setCreateImage(ImageFunction callback) { mCreateImage = callback; refreshImages(); }
	public void setDestroyImage(ImageCallback callback)
	{
		mDestroyImage = callback;
		if (mSourceText == null)
		{
			mSourceText = base.text;
			parseText();
		}
		else
		{
			refreshImages();
		}
	}
	public void setLinkLayoutChanged(Action callback) { mLinkLayoutChanged = callback; }
	// 此函数由UGUI自动调用
	public override void SetVerticesDirty()
	{
		base.SetVerticesDirty();
		if (mSourceText != null && mImages.Count > 0 && mParsedFontSize != fontSize)
		{
			parseText();
		}
	}
	protected void parseText()
	{
		mParsedFontSize = fontSize;
		clearImages();
		mLinks.Clear();
		mVisibleIndices.Clear();
		StringBuilder output = new();
		InlineLink link = null;
		int index = 0;
		foreach (Match token in mTokens.Matches(mSourceText ?? string.Empty))
		{
			string value = token.Value;
			if (token.Groups[1].Success)
			{
				link = new() { mID = token.Groups[1].Value.Trim('"'), mStart = index };
				mLinks.Add(link);
			}
			else if (value == "</link>")
			{
				if (link != null)
				{
					link.mEnd = index;
					link = null;
				}
			}
			else if (token.Groups[2].Success)
			{
				float width = float.Parse(token.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
				float fps = token.Groups[4].Success ? float.Parse(token.Groups[4].Value, System.Globalization.CultureInfo.InvariantCulture) : 0.0f;
				mVisibleIndices.Add(output.Length);
				mImages.Add(new() { mIndex = index++, mSprite = token.Groups[3].Value, mWidth = width, mFPS = fps });
				output.Append("<quad size=").Append(fontSize).Append(" width=").Append(token.Groups[2].Value).Append("/>");
			}
			else
			{
				if (value[0] != '<')
				{
					mVisibleIndices.Add(output.Length);
					++index;
				}
				output.Append(value);
			}
		}
		if (link != null)
		{
			link.mEnd = index;
		}
		if (mImages.Count > 0)
		{
			supportRichText = true;
		}
		base.text = output.ToString();
		refreshImages();
	}
	// 销毁现有的所有Image
	protected void clearImages()
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
	protected void refreshImages()
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
			myUGUIImage window = image.mImage ?? image.mImageAnim;
			Vector2 spriteSize = window.getSpriteSize();
			float height = spriteSize.x > 0.0f ? spriteSize.y / spriteSize.x : 1.0f;
			// 图片宽度等于fontSize * scale,scale是富文本标签中的width值
			window.setSize(new(fontSize * image.mWidth, fontSize * image.mWidth * height));
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 此函数由UGUI自动调用
	protected override void OnPopulateMesh(VertexHelper vertices)
	{
		base.OnPopulateMesh(vertices);
		mVertices.Clear();
		vertices.GetUIVertexStream(mVertices);
		foreach (InlineLink link in mLinks)
		{
			link.mRects.Clear();
			link.mLines.Clear();
			int lineIndex = 0;
			for (int index = link.mStart; index < link.mEnd && index * 6 + 2 < mVertices.Count; ++index)
			{
				while (lineIndex + 1 < cachedTextGenerator.lineCount && cachedTextGenerator.lines[lineIndex + 1].startCharIdx <= mVisibleIndices[index])
				{
					++lineIndex;
				}
				link.mLines.Add(lineIndex);
				Vector3 a = mVertices[index * 6].position;
				Vector3 b = mVertices[index * 6 + 2].position;
				link.mRects.Add(Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y)));
			}
		}
		foreach (InlineImage image in mImages)
		{
			// 在获得的mVertexStream中每个四边形有2个三角形,共6个顶点
			int vertexIndex = image.mIndex * 6;
			myUGUIImage window = image.mImage ?? image.mImageAnim;
			if (window == null)
			{
				continue;
			}
			bool visible = vertexIndex + 2 < mVertices.Count;
			window.setActive(visible);
			if (!visible)
			{
				continue;
			}
			Vector3 a = mVertices[vertexIndex].position;
			Vector3 b = mVertices[vertexIndex + 2].position;
			Vector3 position = (a + b) * 0.5f;
			// y坐标需要调整,不做调整会有一定的向上偏移量,不知道这个偏移量是怎么产生的
			position.y -= (int)(Mathf.Abs(b.x - a.x) * 0.16f);
			window.setPosition(position);
			// 抹掉左下角的不需要渲染的部分,也就是将其顶点合并,这里的下标仅仅是顶点的下标,不考虑索引
			for (int i = 1; i < 4; ++i)
			{
				vertices.SetUIVertex(mVertices[vertexIndex], image.mIndex * 4 + i);
			}
		}
		mLinkLayoutChanged?.Invoke();
	}
}
