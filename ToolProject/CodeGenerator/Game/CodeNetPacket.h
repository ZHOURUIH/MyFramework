#ifndef _CODE_NET_PACKET_H_
#define _CODE_NET_PACKET_H_

#include "CodeUtility.h"

class CodeNetPacket : public CodeUtility
{
public:
	static void generate();
	static void generateVirtualClient();
protected:
	//c++
	static void generateCppGamePacketDefineFile(const myVector<PacketInfo>& packetList, const string& filePath);
	static void generateCppGamePacketRegisteFile(const myVector<PacketInfo>& packetList, const myVector<PacketStruct>& structInfoList, const string& filePath);
	static void generateCppCSPacketFileHeader(const PacketInfo& packetInfo, const string& filePath);
	static void generateCppCSPacketFileSource(const PacketInfo& packetInfo, const string& filePath);
	static void generateCppSCPacketFileHeader(const PacketInfo& packetInfo, const string& filePath);
	static void generateCppSCPacketFileSource(const PacketInfo& packetInfo, const string& filePath);
	static void generateCppPacketMemberDeclare(const myVector<PacketMember>& memberList, myVector<string>& generateCodes);
	static void generateCppPacketReadWrite(const PacketInfo& packetInfo, myVector<string>& generateCodes);
	static void generateCppStruct(const PacketStruct& structInfo, const string& filePath);
	//c#
	static void generateCSharpPacketDefineFile(const myVector<PacketInfo>& packetList, const string& filePath);
	static void generateCSharpPacketRegisteFile(const myVector<PacketInfo>& packetList, const myVector<PacketStruct>& structInfoList, const string& filePath);
	static void generateCSharpPacketFile(const PacketInfo& packetInfo, const string& csFileHotfixPath, const string& scFileHotfixPath);
	static void generateCSharpStruct(const PacketStruct& structInfo, const string& hotFixPath);
protected:
	static string singleMemberReadLine(const string& memberName, const string& memberType, bool supportCustom);
	static string singleMemberWriteLine(const string& memberName, const string& memberType, bool supportCustom);
	static myVector<string> multiMemberReadLine(const myVector<string>& memberNameList, const string& memberType, bool supportCustom);
	static myVector<string> multiMemberWriteLine(const myVector<string>& memberNameList, const string& memberType, bool supportCustom);
	static string singleMemberReadLineCSharp(const string& memberName, const string& memberType);
	static string singleMemberWriteLineCSharp(const string& memberName, const string& memberType);
	static myVector<string> multiMemberReadLineCSharp(const myVector<string>& memberNameList, const string& memberType, bool supportCustom);
	static myVector<string> multiMemberWriteLineCSharp(const myVector<string>& memberNameList, const string& memberType, bool supportCustom);
	static bool isSameType(const string& sourceType, const string& curType);
	static string toPODType(const string& type);
	static bool isCustomStructType(const string& type);
	static void generateMemberGroup(const myVector<PacketMember>& memberList, myVector<myVector<PacketMember>>& memberNameList);
	static string expandMembersInGroup(const myVector<PacketMember>& memberList, myVector<string>& memberNameList);
	static string expandMembersInGroupCSharp(const myVector<PacketMember>& memberList, myVector<string>& memberNameList, bool supportSimplify);
	static void generateCpp(const myVector<PacketStruct>& structInfoList, const myVector<PacketInfo>& packetInfoList);
	static void generateCSharp(const myVector<PacketStruct>& structInfoList, const myVector<PacketInfo>& packetInfoList);
	static void generateCSharpVirtualClient(const myVector<PacketStruct>& structInfoList, const myVector<PacketInfo>& packetInfoList);
	static void parsePacketConfig(myVector<PacketStruct>& structInfoList, myVector<PacketInfo>& packetInfoList);
	static string generatePacketVersion(const myVector<PacketInfo>& packetList, const myVector<PacketStruct>& structInfoList);
};

#endif