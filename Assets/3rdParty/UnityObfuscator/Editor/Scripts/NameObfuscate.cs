//using System.Collections.Generic;
//using Mono.Cecil;

//namespace Flower.UnityObfuscator
//{
//    internal class NameObfuscate
//    {
//		protected ObfuscateType obfuscateType = ObfuscateType.Both;
//		protected WhiteList whiteList;
//		protected WhiteList obfuscateList;
//		protected static NameObfuscate _instance = null;
//        public static NameObfuscate Instance
//        {
//            get
//            {
//                _instance ??= new NameObfuscate();
//                return _instance;
//            }
//        }
//		protected bool IsChange(bool inObfuscateList, bool inWhiteList)
//		{
//			bool change;
//			switch (obfuscateType)
//			{
//				case ObfuscateType.ParticularRange: change = inObfuscateList; break;
//				case ObfuscateType.WhiteList: change = !inWhiteList; break;
//				case ObfuscateType.Both: change = inObfuscateList && !inWhiteList; break;
//				default: change = false; break;
//			}
//			return change;
//		}
//		public bool IsChangeField(TypeDefinition t, string fieldName)
//		{
//			bool isWhiteListNamespace = obfuscateList.IsWhiteListNamespace(t.Namespace);
//			bool isWhiteListClassMember = obfuscateList.IsWhiteListClassMember(fieldName, t.Name, t.Namespace);
//			bool isWhiteListClass = obfuscateList.IsWhiteListClass(t.Name, t.Namespace);
//			bool isWhiteListClassNameOnly = obfuscateList.IsWhiteListClassNameOnly(t.Name, t.Namespace);
//			bool skipClass = ClassNeedSkip(t);
//			bool IsWhiteListNamespace = whiteList.IsWhiteListNamespace(t.Namespace);
//			bool IsWhiteListClassMember = whiteList.IsWhiteListClassMember(fieldName, t.Name, t.Namespace);
//			bool IsWhiteListClass = whiteList.IsWhiteListClass(t.Name, t.Namespace);

//			bool inObfuscateList = isWhiteListNamespace ||
//								   isWhiteListClassMember ||
//								   isWhiteListClass ||
//								   isWhiteListClassNameOnly &&
//								   !skipClass;
//			bool inWhiteList = IsWhiteListNamespace ||
//							   IsWhiteListClassMember ||
//							   IsWhiteListClass ||
//							   skipClass;
//			return IsChange(inObfuscateList, inWhiteList);
//		}
//		public bool IsChangeProperty(TypeDefinition t, string propertyName)
//		{
//			bool isWhiteListNamespace = obfuscateList.IsWhiteListNamespace(t.Namespace);
//			bool isWhiteListClassMember = obfuscateList.IsWhiteListClassMember(propertyName, t.Name, t.Namespace);
//			bool isWhiteListClass = obfuscateList.IsWhiteListClass(t.Name, t.Namespace);
//			bool isWhiteListClassNameOnly = obfuscateList.IsWhiteListClassNameOnly(t.Name, t.Namespace);
//			bool skipClass = ClassNeedSkip(t);
//			bool IsWhiteListNamespace = whiteList.IsWhiteListNamespace(t.Namespace);
//			bool IsWhiteListClassMember = whiteList.IsWhiteListClassMember(propertyName, t.Name, t.Namespace);
//			bool IsWhiteListClass = whiteList.IsWhiteListClass(t.Name, t.Namespace);

//			bool inObfuscateList = isWhiteListNamespace ||
//								   isWhiteListClassMember ||
//								   isWhiteListClass ||
//								   isWhiteListClassNameOnly &&
//								   !skipClass;
//			bool inWhiteList = IsWhiteListNamespace ||
//							   IsWhiteListClassMember ||
//							   IsWhiteListClass ||
//							   skipClass;
//			return IsChange(inObfuscateList, inWhiteList);
//		}
//		public bool IsChangeMethod(TypeDefinition t, MethodDefinition method)
//		{
//			bool skipMethod = MethodNeedSkip(method);
//			bool isWhiteListNamespace = obfuscateList.IsWhiteListNamespace(t.Namespace);
//			bool isWhiteListMethod = obfuscateList.IsWhiteListMethod(method.Name, t.Name, t.Namespace);
//			bool isWhiteListClass = obfuscateList.IsWhiteListClass(t.Name, t.Namespace);
//			bool isWhiteListClassNameOnly = obfuscateList.IsWhiteListClassNameOnly(t.Name, t.Namespace);
//			bool IsWhiteListNamespace = whiteList.IsWhiteListNamespace(t.Namespace);
//			bool IsWhiteListMethod = whiteList.IsWhiteListMethod(method.Name, t.Name, t.Namespace);
//			bool IsWhiteListClass = whiteList.IsWhiteListClass(t.Name, t.Namespace);
//			bool skipClass = ClassNeedSkip(t);

//			bool inObfuscateList = (isWhiteListNamespace || isWhiteListMethod || isWhiteListClass || isWhiteListClassNameOnly) &&
//									!skipClass && !skipMethod;
//			bool inWhiteList = skipMethod ||
//							   IsWhiteListNamespace ||
//							   IsWhiteListMethod ||
//							   IsWhiteListClass ||
//							   skipClass;
//			return IsChange(inObfuscateList, inWhiteList);
//		}
//		public bool IsChangeClass(TypeDefinition t)
//		{
//			bool isWhiteListNamespace = obfuscateList.IsWhiteListNamespace(t.Namespace);
//			bool isWhiteListClass = obfuscateList.IsWhiteListClass(t.Name, t.Namespace);
//			bool isWhiteListClassNameOnly = obfuscateList.IsWhiteListClassNameOnly(t.Name, t.Namespace);
//			bool skipClass = ClassNeedSkip(t);
//			bool IsWhiteListNamespace = whiteList.IsWhiteListNamespace(t.Namespace);
//			bool IsWhiteListClass = whiteList.IsWhiteListClass(t.Name, t.Namespace);
//			bool IsWhiteListClassNameOnly = whiteList.IsWhiteListClassNameOnly(t.Name, t.Namespace);
//			bool IsWhiteListBaseClassNameOnly = whiteList.IsWhiteListBaseClassNameOnly(t.BaseType?.Name);

//			bool inObfuscateList = (isWhiteListNamespace ||
//									isWhiteListClass &&
//									!isWhiteListClassNameOnly) &&
//									!skipClass;
//			bool inWhiteList = IsWhiteListNamespace ||
//							   IsWhiteListClass ||
//							   IsWhiteListClassNameOnly ||
//							   IsWhiteListBaseClassNameOnly ||
//							   skipClass;
//			return IsChange(inObfuscateList, inWhiteList);
//		}
//		public bool IsChangeNamespace(TypeDefinition t)
//		{
//			bool inObfuscateList = obfuscateList.IsWhiteListNamespace(t.Namespace) && !obfuscateList.IsWhiteListNamespcaeNameOnly(t.Namespace);
//			bool inWhiteList = whiteList.IsWhiteListNamespace(t.Namespace, true) || whiteList.IsWhiteListNamespcaeNameOnly(t.Namespace, true);
//			return IsChange(inObfuscateList, inWhiteList);
//		}
//        public void Init(ObfuscateType type)
//        {
//            obfuscateType = type;
//            Dictionary<WhiteListType, string> whiteListPathDic = new()
//			{
//				{ WhiteListType.Namespace, Const.WhiteList_NamespacePath },
//				{ WhiteListType.NameSpcaceNameOnly, Const.WhiteList_NamespaceNameOnlyPath },
//				{ WhiteListType.Class, Const.WhiteList_ClassPath },
//				{ WhiteListType.ClassNameOnly, Const.WhiteList_ClassNameOnlyPath },
//				{ WhiteListType.Method, Const.WhiteList_MethodPath },
//				{ WhiteListType.Member, Const.WhiteList_MemberPath },
//				{ WhiteListType.BaseClassNameOnly, Const.WhiteList_BaseClassNameOnlyPath }
//			};
//            Dictionary<WhiteListType, string> obfuscateListPathDic = new()
//			{
//				{ WhiteListType.Namespace, Const.ObfuscateList_NamespacePath },
//				{ WhiteListType.NameSpcaceNameOnly, Const.ObfuscateList_NamespaceExceptNamespaceNamePath },
//				{ WhiteListType.Class, Const.ObfuscateList_ClassPath },
//				{ WhiteListType.ClassNameOnly, Const.ObfuscateList_ClassExceptClassNamePath },
//				{ WhiteListType.Method, Const.ObfuscateList_MethodPath },
//				{ WhiteListType.Member, Const.ObfuscateList_MemberPath }
//			};
//            whiteList = new WhiteList(whiteListPathDic);
//            obfuscateList = new WhiteList(obfuscateListPathDic);
//        }
//		public bool MethodNeedSkip(MethodDefinition method)
//        {
//            return WhiteList.IsSkipMethod(method) ||
//                   method.IsVirtual ||
//                   method.ReturnType.FullName == "System.Collections.IEnumerator" ||
//                   method.IsConstructor ||
//                   method.Name.StartsWith(".") ||
//                   method.Name.Contains("<") ||
//                   // 带模板的函数不能混淆,否则在引用这个函数的地方会无法替换名字,暂时不知道原因
//				   method.HasGenericParameters;
//        }
//        public bool ClassNeedSkip(TypeDefinition type)
//        {
//			// 不能对枚举的名字进行混淆,混淆枚举会导致一些奇怪的崩溃,比如已知的就是调用Enum.IsEnumDefined时会崩溃,以及不支持跨dll引用的枚举类型作为函数默认参数
//			return type.Name.Contains("<") || type.IsEnum || type.Name.Contains("`");
//		}
//	}
//}