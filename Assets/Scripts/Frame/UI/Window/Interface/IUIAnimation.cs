using System.Collections.Generic;
using UnityEngine;

public interface IUIAnimation
{
	string getTextureSet();
	int getTextureFrameCount();
	void setTexturePosList(List<Vector2> posList);
	List<Vector2> getTexturePosList();
	void setUseTextureSize(bool useSize);
	LOOP_MODE getLoop();
	float getInterval();
	float getSpeed();
	int getStartIndex();
	PLAY_STATE getPlayState();
	bool getPlayDirection();
	int getEndIndex();
	bool isAutoHide();
	int getRealEndIndex();
	void setLoop(LOOP_MODE loop);
	void setInterval(float interval);
	void setPlayDirection(bool direction);
	void setAutoHide(bool autoHide);
	void setStartIndex(int startIndex);
	void setEndIndex(int endIndex);
	void setSpeed(float speed);
	void stop(bool resetStartIndex = true, bool callback = true, bool isBreak = true);
	void play();
	void pause();
	void addPlayEndCallback(TextureAnimCallback callback, bool clear = true);
	void addPlayingCallback(TextureAnimCallback callback, bool clear = true);
	int getCurFrameIndex();
	void setCurFrameIndex(int index);
}