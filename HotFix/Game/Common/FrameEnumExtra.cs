using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Frame需要的枚举值,每个项目的值都不同,但是Frame中需要使用
// 界面布局定义
public class LAYOUT_ILR
{
	public static int NONE = 1000;
	public static int LOGIN = 1001;
	public static int GAMING = 1002;
};

// 关键帧ID定义,对应关键帧预设中的曲线ID
public class KEY_CURVE_ILR
{
	// 自定义曲线,从101开始
	public static int ONE_ZERO_ONE_CURVE = 101;
	public static int QUADRATIC_CURVE = 102;
	public static int ZERO_ONE_ZERO_CURV = 103;
	public static int SIN_CURVE = 014;
	public static int YE_MAN_CHONG_ZHUANG = 105;
	public static int STRICK_BACK = 106;
	public static int BUFF_TIP = 107;
}

// 音效定义
public class SOUND_DEFINE_ILR
{
	public static int BAN_YUE_WAN_DAO = 1;				// 半月弯刀音效
	public static int BING_PAO_XIAO_FIRE = 2;			// 冰咆哮发动的音效
	public static int BING_PAO_XIAO_END = 3;			// 冰咆哮持续和消失的音效
	public static int DI_YU_LEI_GUANG = 4;				// 地狱雷光音效
	public static int GONG_SHA_JIAN_SHU_MALE = 5;		// 男角色攻杀剑术音效
	public static int GONG_SHA_JIAN_SHU_FEMALE = 6;     // 女角色攻杀剑术音效
	public static int HUO_QIANG_FIRE = 7;               // 火墙发动的音效
	public static int HUO_QIANG_NORMAL = 8;				// 火墙持续的音效
	public static int LEI_DIAN_SHU = 9;                 // 雷电术音效
	public static int LIE_HUO_JIAN_FA = 10;             // 烈火剑法音效
	public static int MO_FA_DUN_FIRE = 11;              // 魔法盾发动的音效
	public static int OPEN_GATE = 12;                   // 账号登录成功开门音效
	public static int SELECT_ROLE = 13;					// 角色选择界面音效
	public static int GAME_LOGIN = 14;                  // 账号登录界面音效
	public static int CI_SHA_JIAN_SHU = 15;             // 刺杀剑术音效
	public static int JI_BEN_JIAN_SHU = 16;             // 基本剑术音效(实际上基本剑术是被动,没有音效)
	public static int BASE_ATTACK_NO_WEAPON_0 = 17;		// 无武器的普通攻击音效0
	public static int BASE_ATTACK_NO_WEAPON_1 = 18;		// 无武器的普通攻击音效1
	public static int BASE_ATTACK_0 = 19;               // 带武器的普通攻击音效0
	public static int BASE_ATTACK_1 = 20;               // 带武器的普通攻击音效1
	public static int DIE_FEMALE = 21;                  // 女性角色死亡
	public static int DIE_MALE = 22;					// 男性角色死亡
}

// 游戏配置参数,浮点数
public class GAME_FLOAT
{
	public static int NONE = 0;
}

// 游戏配置参数,字符串
public class GAME_STRING
{
	public static int NONE = 0;
}