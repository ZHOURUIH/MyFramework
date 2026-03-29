using System.Collections.Generic;
using Obfuz;
using UnityEngine;
using static StringUtility;
using static FrameUtility;
using static MathUtility;
using static FrameBaseHotFix;

// auto generate member start
// generate from:Assets/GameResources/UI/UIPrefab/UIGame.prefab
// 游戏界面
[ObfuzIgnore(ObfuzScope.TypeName)]
public class UIGame : LayoutScript
{
	protected myUGUIObject mAvatar;
	protected myUGUIText mSpeed;
	protected myUGUIDamageNumber mDamageNumber;
	protected myUGUITileImage mTile;
	// auto generate member end
	protected Dictionary<float, Vector3> mPositionKeyframes;
	protected Dictionary<float, Vector3> mScaleKeyframes;
	protected string mCriticalSpriteName;
	protected string mMissSpriteName;
	protected List<string> mPrefabList = new();
	public UIGame()
	{
		// auto generate constructor start
		// auto generate constructor end
		mPrefabList.add("Effect/Prefabs/CyanBlueBullet/CyanBlueMuzzle.prefab");
		mPrefabList.add("Effect/Prefabs/EnergyExplosion/EnergyExplosionCharge.prefab");
		mPrefabList.add("Effect/Prefabs/EnergyExplosion/EnergyExplosionHit.prefab");
		mPrefabList.add("Effect/Prefabs/EnergyExplosion/EnergyExplosionMuzzle.prefab");
		mPrefabList.add("Effect/Prefabs/Fireball/FireballMuzzle.prefab");
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out myUGUIObject background, "Background", false);
		newObject(out mAvatar, background, "Avatar");
		newObject(out mSpeed, background, "Speed");
		newObject(out mDamageNumber, background, "DamageNumber");
		newObject(out mTile, background, "Tile");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
		mDamageNumber.setInterval(-10);
		mPositionKeyframes = getTranslate("pg01");
		mScaleKeyframes = getScale("pg01");
		mCriticalSpriteName = mDamageNumber.getNumberStyle() + "_Critical";
		mMissSpriteName = "miss";
		// 需要去掉这句话才能看到伤害数字的显示
		mAvatar.getParent().setActive(false);
	}
	public override void onGameState()
	{
		base.onGameState();
		mAtlasManager.getAtlasAsyncSafe(this, "Atlas/NumberStyle/CommonNumber.png", (AtlasRef atlas)=>
		{
			Dictionary<object, TileData> list = new();
			list.add("1", CLASS(out TileData _)).init(atlas.getSprite("Number_Magic_0"), new(10, 10, 0), new(50, 100));
			list.add("2", CLASS(out TileData _)).init(atlas.getSprite("Number_Magic_1"), new(40, 10, 0), new(50, 100));
			list.add("3", CLASS(out TileData _)).init(atlas.getSprite("Number_Magic_2"), new(70, 10, 0), new(50, 100));
			list.add("4", CLASS(out TileData _)).init(atlas.getSprite("Number_Magic_3"), new(100, 10, 0), new(50, 100));
			mTile.setTileMap(list);
		});
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 测试特效播放
		for (int i = 0; i < 20; ++i)
		{
			mEffectManager.playEffectAsyncAtPositionQuick(mPrefabList.random(), new(randomFloat(-100, 100), randomFloat(-100, 100)), 0);
		}
		// 测试伤害数字显示
		for (int i = 0; i < 10; ++i)
		{
			showNumber(new Vector3(randomFloat(400, -400), randomFloat(400, -400)), randomInt(10, 10000));
			showMiss(new Vector3(randomFloat(400, -400), randomFloat(400, -400)));
			showCritical(new Vector3(randomFloat(400, -400), randomFloat(400, -400)), randomInt(10, 10000));
		}
	}
	public void setAvatarPosition(Vector3 pos)
	{
		mAvatar.setPosition(pos);
	}
	public void setSpeed(float speed)
	{
		mSpeed.setText("速度:" + FToS(speed, 0));
	}
	// 显示伤害数字,position是数字的世界坐标
	public void showNumber(Vector3 position, int number)
	{
		Vector3 randomOffset = new(randomFloat(-20.0f, 20.0f), randomFloat(-5.0f, 5.0f));
		mDamageNumber.addDamageNumber(position + randomOffset, 0.3f, number, 1.0f, mPositionKeyframes, mScaleKeyframes);
	}
	public void showCritical(Vector3 position, int number)
	{
		CLASS(out DamageNumberFlag criticalFlag);
		criticalFlag.setSprite(mDamageNumber.getSprite(mCriticalSpriteName));
		criticalFlag.setScale(1.0f);
		criticalFlag.setOffset(-mDamageNumber.getNumberTotalWidth(number) * 0.5f - criticalFlag.mSpriteWidth * 0.5f, 20.0f);
		Vector3 randomOffset = new(randomFloat(-20.0f, 20.0f), randomFloat(-5.0f, 5.0f));
		mDamageNumber.addDamageNumber(position + randomOffset, 0.3f, number, criticalFlag, 1.0f, mPositionKeyframes, mScaleKeyframes);
	}
	public void showMiss(Vector3 position)
	{
		CLASS(out DamageNumberFlag missFlag);
		missFlag.setSprite(mDamageNumber.getSprite(mMissSpriteName));
		missFlag.setScale(2.0f);
		Vector3 randomOffset = new(randomFloat(-20.0f, 20.0f), randomFloat(-5.0f, 5.0f));
		mDamageNumber.addDamageNumber(position + randomOffset, 0.3f, missFlag, 1.0f, mPositionKeyframes, mScaleKeyframes);
	}
	protected Dictionary<float, Vector3> getTranslate(string pathName) { return mPathKeyframeManager.getTranslatePath(pathName); }
	protected Dictionary<float, Vector3> getScale(string pathName) { return mPathKeyframeManager.getScalePath(pathName); }
}
