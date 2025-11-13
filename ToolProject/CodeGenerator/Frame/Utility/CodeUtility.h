#ifndef _CODE_UTILITY_H_
#define _CODE_UTILITY_H_

#include "SystemUtility.h"
#include "GameDefine.h"

typedef std::function<bool(const string& line)> LineMatchCallback;

class CodeUtility : public SystemUtility
{
protected:
	static string ServerGameProjectPath;
	static string ServerFrameProjectPath;
	static string ClientProjectPath;
	static string ClientHotFixPath;
	static string VirtualClientProjectPath;
	static myVector<string> ServerExcludeIncludePath;
	static string cppGamePath;
	static string cppFramePath;
	static string cppGameStringDefineHeaderFile;
	static string VirtualClientSocketPath;
	static string SQLitePath;
	static string START_FALG;
	static bool mGenerateVirtualClient;
public:
	static bool initPath();
	static bool isGenerateVirtualClient() { return mGenerateVirtualClient; }
	static bool isPod(const string& type)
	{
		return type == "bool" ||
			   type == "char" ||
			   type == "sbyte" ||
			   type == "byte" ||
			   type == "short" ||
			   type == "ushort" ||
			   type == "int" ||
			   type == "uint" ||
			   type == "long" ||
			   type == "ulong" ||
			   type == "llong" ||
			   type == "ullong" ||
			   type == "float" ||
			   type == "double";
	}
	static bool isPodInteger(const string& type)
	{
		return type == "bool" ||
			   type == "char" ||
			   type == "byte" ||
			   type == "short" ||
			   type == "ushort" ||
			   type == "int" ||
			   type == "uint" ||
			   type == "long" ||
			   type == "ulong" ||
			   type == "llong" ||
			   type == "ullong";
	}
	static MySQLMember parseMySQLMemberLine(string line);
	// ignoreClientServer表示是否忽略客户端服务器标签,true则表示会忽略,将全部字段定义都导出
	static SQLiteMember parseSQLiteMemberLine(string line, bool ignoreClientServer);
	static PacketMember parseMemberLine(const string& line);
	static string packetNameToUpper(const string& packetName);
	static string nameToUpper(const string& sqliteName, bool preUnderLine = true);
	static string cSharpPushParamString(const PacketMember& memberInfo);
	static string cppTypeToCSharpType(const string& cppType);
	static string cSharpTypeToWrapType(const string& csharpType);
	static string cSharpMemberDeclareString(const PacketMember& memberInfo);
	static myVector<string> parseTagList(const string& line, string& newLine);
	static void parseStructName(const string& line, PacketStruct& structInfo);
	static void parsePacketName(const string& line, PacketInfo& packetInfo);
	static string convertToCSharpType(const string& cppType);
	static bool findCustomCode(const string& fullPath, myVector<string>& codeList, int& lineStart, const LineMatchCallback& startLineMatch, const LineMatchCallback& endLineMatch, bool showError = true);
	static string codeListToString(const myVector<string>& codeList);
	static myVector<string> findTargetHeaderFile(const string& path, const LineMatchCallback& fileNameMatch, const LineMatchCallback& lineMatch, myMap<string, myVector<string>>* fileContentList = nullptr);
	static string findClassName(const string& line);
	static string findClassBaseName(const string& line);
	static void line(string& str, const string& line, bool returnLine = true) 
	{
		str += line;
		if (returnLine)
		{
			str += "\r\n";
		}
	}
	static void generateStringDefine(const myVector<string>& defineList, int startID, const string& key, const string& stringDefineHeaderFile);
	static string replaceVariable(const myMap<string, string>& variableDefine, const string& value);
};

#endif
