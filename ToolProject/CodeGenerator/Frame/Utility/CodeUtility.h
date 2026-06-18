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
	static string ExcelPath;
	static string START_FALG;
	static bool mGenerateVirtualClient;
	static myMap<string, myVector<string>> mCachedFileLines;	// 缓存的所有已打开文件的内容,避免重复的IO
public:
	static bool initPath();
	static bool isGenerateVirtualClient() { return mGenerateVirtualClient; }
	static myVector<string> openFile(const string& file);
	static void writeFile(const string& file, const myVector<string>& content);
	static void writeFile(const string& file, const string& ansiContent);
	static void deleteFile(const string& file);
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
	static string getElementTypeCpp(const string& type);
	static string getElementTypeCS(const string& type);
	static MySQLMember parseMySQLMemberLine(string line);
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
	static myVector<string> findClass(const string& path, const LineMatchCallback& fileNameMatch, const LineMatchCallback& lineMatch);
	static myVector<string> findClass(const myVector<string>& files, const LineMatchCallback& fileNameMatch, const LineMatchCallback& lineMatch);
	static myVector<string> findPoolClass(const myVector<string>& files);
	static string getPoolName(const string& line, const string& preKey);
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
	static void parseCSVLine(const string& fullContent, myVector<myVector<string>>& result);
	static OWNER getOwner(const string& owner);
	static void parseCSV(const string& result, CSVHeader& header, myVector<myVector<string>>& dataList);
};

#endif
