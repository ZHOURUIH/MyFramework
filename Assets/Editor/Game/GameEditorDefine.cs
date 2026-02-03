using System.Collections.Generic;

public class GameEditorDefine : EditorDefine
{
	public const string GAME_NAME = "MicroLegend";
	protected override List<string> getNoMipmapsPath_Extension()
	{
		return new()
		{
		};
	}
	protected override List<string> getUnpackFolder_Extension() 
	{
		return new() 
		{
			
		}; 
	}
	// GameResources下的相对路径,以/结尾
	protected override List<string> getForceSingleFolder_Extension() 
	{
		return new()
		{
		};
	}
	protected override List<string> getIgnoreScriptCheck_Extension()
	{
		return new()
		{
			"Game/Bugly/",
			"DataBase/Excel/Table/",
		};
	}
	protected override List<string> getIgnoreLayoutScript_Extension() 
	{
		return new() 
		{
		}; 
	}
	protected override List<string> getIgnoreCodeWidth_Extension() 
	{
		return new()
		{
			"/PacketRegister.cs",
			"/StateRegister.cs",
		};
	}
	protected override List<string> getIgnoreSystemFunctionCheck_Extension() 
	{
		return new()
		{
			"GameUtilityHotFix.cs",
		};
	}
}