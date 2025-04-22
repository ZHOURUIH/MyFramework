
namespace Flower.UnityObfuscator
{
	public enum NameType
    {
        Namespace,
        Class,
        Filed,
        Property,
        Method,
        Other,
    }
	public enum WhiteListType
    {
        Namespace,
        NameSpcaceNameOnly,
        Class,
        ClassNameOnly,
        Method,
        Member,
        BaseClassNameOnly
    }
	public enum ObfuscateType
    {
        ParticularRange,    // 只混淆指定范围的代码
        WhiteList,          // 只混淆不在白名单中的代码
        Both,               // 在指定的混淆范围内,但是不在白名单范围内
    }
	public class Const
    {
		// 白名单配置文件路径
		// 名单内命名空间不混
		public static readonly string WhiteList_NamespacePath = @"/3rdParty/UnityObfuscator/Editor/Res/WhiteList/WhiteList-NameObfuscate-Namespace.txt";
		// 名单内命名空间的类都混，命名空间的名字不混
		public static readonly string WhiteList_NamespaceNameOnlyPath = @"/3rdParty/UnityObfuscator/Editor/Res/WhiteList/WhiteList-NameObfuscate-NamespaceNameOnly.txt";
		// 名单内类不混
		public static readonly string WhiteList_ClassPath = @"/3rdParty/UnityObfuscator/Editor/Res/WhiteList/WhiteList-NameObfuscate-Class.txt";
		// 名单内的类的成员都混，类名不混
		public static readonly string WhiteList_ClassNameOnlyPath = @"/3rdParty/UnityObfuscator/Editor/Res/WhiteList/WhiteList-NameObfuscate-ClassNameOnly.txt";
		// 名单内的方法不混
		public static readonly string WhiteList_MethodPath = @"/3rdParty/UnityObfuscator/Editor/Res/WhiteList/WhiteList-NameObfuscate-Method.txt";
		// 名单内的类成员不混
		public static readonly string WhiteList_MemberPath = @"/3rdParty/UnityObfuscator/Editor/Res/WhiteList/WhiteList-NameObfuscate-ClassMember.txt";
        // 该类的所有子类的类名不混淆
		public static readonly string WhiteList_BaseClassNameOnlyPath = @"/3rdParty/UnityObfuscator/Editor/Res/WhiteList/WhiteList-NameObfuscate-BaseClassNameOnly.txt";

		// 混淆范围配置文件路径
		// 名单内的命名空间所有类和类成员都混
		public static readonly string ObfuscateList_NamespacePath = @"/3rdParty/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-NameObfuscate-Namespace.txt";
		// 名单内命名空间的类都混，命名空间的名字不混
		public static readonly string ObfuscateList_NamespaceExceptNamespaceNamePath = @"/3rdParty/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-NameObfuscate-NamespaceExceptNamespaceName.txt";
		// 名单内的类都混
		public static readonly string ObfuscateList_ClassPath = @"/3rdParty/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-NameObfuscate-Class.txt";
		// 名单内的类的类成员都混，类名不混
		public static readonly string ObfuscateList_ClassExceptClassNamePath = @"/3rdParty/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-NameObfuscate-ClassExceptClassName.txt";
		// 名单内的方法都混
		public static readonly string ObfuscateList_MethodPath = @"/3rdParty/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-NameObfuscate-Method.txt";
		// 名单内的类成员都混
		public static readonly string ObfuscateList_MemberPath = @"/3rdParty/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-NameObfuscate-ClassMember.txt";
        public static readonly char[] randomCharArray = { '0', 'o', 'O' };
        // 使用随机字符混淆时，命名长度
        public static readonly int RandomNameLen = 40;
    }
}