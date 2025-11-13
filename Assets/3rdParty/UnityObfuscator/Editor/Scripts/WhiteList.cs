//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;
//using Mono.Cecil;

//namespace Flower.UnityObfuscator
//{
//    internal class WhiteList
//    {
//        public static readonly char sperateChar = '|';
//        public static readonly char nullChar = '*';
//        private Dictionary<WhiteListType, List<string>> dic = new();
//		// unity的函数不混淆
//		private static HashSet<string> unityMethodWhiteList = new()
//        {
//            "Awake",
//            "FixedUpdate",
//            "LateUpdate",
//            "OnAnimatorIK",
//            "OnAnimatorMove",
//            "OnApplicationFocus",
//            "OnApplicationPause",
//            "OnApplicationQuit",
//            "OnAudioFilterRead",
//            "OnBecameInvisible",
//            "OnBecameVisible",
//            "OnCollisionEnter",
//            "OnCollisionEnter2D",
//            "OnCollisionExit",
//            "OnCollisionExit2D",
//            "OnCollisionStay",
//            "OnCollisionStay2D",
//            "OnConnectedToServer",
//            "OnControllerColliderHit",
//            "OnDestroy",
//            "OnDisable",
//            "OnDisconnectedFromServer",
//            "OnDrawGizmos",
//            "OnDrawGizmosSelected",
//            "OnEnable",
//            "OnFailedToConnect",
//            "OnFailedToConnectToMasterServer",
//            "OnGUI",
//            "OnJointBreak",
//            "OnJointBreak2D",
//            "OnMasterServerEvent",
//            "OnMouseDown",
//            "OnMouseDrag",
//            "OnMouseEnter",
//            "OnMouseExit",
//            "OnMouseOver",
//            "OnMouseUp",
//            "OnMouseUpAsButton",
//            "OnNetworkInstantiate",
//            "OnParticleCollision",
//            "OnParticleTrigger",
//            "OnPlayerConnected",
//            "OnPlayerDisconnected",
//            "OnPostRender",
//            "OnPreCull",
//            "OnPreRender",
//            "OnRenderImage",
//            "OnRenderObject",
//            "OnSerializeNetworkView",
//            "OnServerInitialized",
//            "OnTransformChildrenChanged",
//            "OnTransformParentChanged",
//            "OnTriggerEnter",
//            "OnTriggerEnter2D",
//            "OnTriggerExit",
//            "OnTriggerExit2D",
//            "OnTriggerStay",
//            "OnTriggerStay2D",
//            "OnValidate",
//            "OnWillRenderObject",
//            "Reset",
//            "Start",
//            "Update",
//        };
//        public WhiteList(Dictionary<WhiteListType, string> configFilePathDic)
//        {
//            foreach (var item in configFilePathDic)
//            {
//                LoadWhiteList(item.Key, item.Value);
//            }
//            CheckWhiteListData();
//        }
//        private void LoadWhiteList(WhiteListType whiteListType, string path)
//        {
//            if (dic.ContainsKey(whiteListType))
//            {
//                Debug.LogError("Init White List Error: Key Duplicated");
//                return;
//            }
//            dic.Add(whiteListType, new(File.ReadAllLines(Application.dataPath + path)));
//        }
//		static bool check(string str, int splitCount) { return !(str.Contains(" ") || str.Split(sperateChar).Length != splitCount); }
//		private void CheckWhiteListData()
//        {
//			foreach (var item in dic)
//            {
//                switch (item.Key)
//                {
//                    case WhiteListType.Namespace:
//                        foreach (string str in item.Value)
//                        {
//                            if (!check(str, 1))
//                            {
//								Debug.LogError("Check White List Data Error: " + item.Key.ToString() + ":" + str);
//							}
//                        }
//                        break;
//                    case WhiteListType.Class:
//                        foreach (string str in item.Value)
//                        {
//                            if (!check(str, 2))
//                            {
//								Debug.LogError("Check White List Data Error: " + item.Key.ToString() + ":" + str);
//							}
//                        }
//                        break;
//                    case WhiteListType.Method:
//                        foreach (string str in item.Value)
//                        {
//                            if (!check(str, 3))
//                            {
//								Debug.LogError("Check White List Data Error: " + item.Key.ToString() + ":" + str);
//							}
//                        }
//                        break;
//                    case WhiteListType.Member:
//                        foreach (string str in item.Value)
//                        {
//                            if (!check(str, 3))
//                            {
//								Debug.LogError("Check White List Data Error: " + item.Key.ToString() + ":" + str);
//							}
//                        }
//                        break;
//                }
//            }
//        }
//        public bool IsWhiteListNamespace(string _namespace, bool checkEmpty = false)
//        {
//            if (string.IsNullOrEmpty(_namespace))
//            {
//                return checkEmpty;
//            }
//            return Check(_namespace.Split('.'), WhiteListType.Namespace);
//        }
//        public bool IsWhiteListNamespcaeNameOnly(string _namespace, bool checkEmpty = false)
//        {
//            if (string.IsNullOrEmpty(_namespace))
//            {
//                return checkEmpty;
//            }
//            return Check(_namespace.Split('.'), WhiteListType.NameSpcaceNameOnly);
//        }
//        public bool IsWhiteListClass(string className, string _namespace = null)
//        {
//            if (string.IsNullOrEmpty(className))
//            {
//                Debug.LogError("Class Param Error:" + className);
//                return false;
//            }
//            return Check(new string[] { (string.IsNullOrEmpty(_namespace) ? nullChar.ToString() : _namespace), className }, WhiteListType.Class);
//        }
//        public bool IsWhiteListClassNameOnly(string className, string _namespace = null)
//        {
//            if (string.IsNullOrEmpty(className))
//            {
//                Debug.LogError("Class Param Error:" + className);
//                return false;
//            }
//            return Check(new string[] { (string.IsNullOrEmpty(_namespace) ? nullChar.ToString() : _namespace), className }, WhiteListType.ClassNameOnly);
//        }
//		public bool IsWhiteListBaseClassNameOnly(string baseClassName)
//		{
//			if (string.IsNullOrEmpty(baseClassName))
//			{
//				return false;
//			}
//            if (baseClassName == typeof(MonoBehaviour).Name)
//            {
//                return true;
//            }
//            return Check(new string[] { baseClassName }, WhiteListType.BaseClassNameOnly);
//		}
//		public bool IsWhiteListMethod(string method, string className = null, string _namespace = null)
//        {
//            if (string.IsNullOrEmpty(method))
//            {
//                Debug.LogError("Method Param Error:" + method);
//                return false;
//            }
//            string _namespaceStr = string.IsNullOrEmpty(_namespace) ? nullChar.ToString() : _namespace;
//            string _classNameStr = string.IsNullOrEmpty(className) ? nullChar.ToString() : className;
//            return Check(new string[] { _namespaceStr, _classNameStr, method }, WhiteListType.Method);
//        }
//        public bool IsWhiteListClassMember(string member, string className = null, string _namespace = null)
//        {
//            if (string.IsNullOrEmpty(member))
//            {
//                Debug.LogError("Member Param Error:" + member);
//                return false;
//            }
//            return Check(new string[] { (string.IsNullOrEmpty(_namespace) ? nullChar.ToString() : _namespace), (className == null ? nullChar.ToString() : className), member }, WhiteListType.Member);
//        }
//        public static bool IsSkipMethod(MethodDefinition method)
//        {
//            return string.IsNullOrEmpty(method.Name) || unityMethodWhiteList.Contains(method.Name);
//        }
//        private bool Check(string[] strs, WhiteListType whiteListType)
//        {
//            if (!dic.TryGetValue(whiteListType, out var list))
//            {
//				return false;
//			}
//            foreach (string item in list)
//            {
//                string[] whiteList = item.Split(sperateChar);
//                bool partCheck = true;
//                for (int i = 0; i < whiteList.Length; i++)
//                {
//                    if (whiteList[i] != strs[i] && whiteList[i] != nullChar.ToString())
//                    {
//                        partCheck = false;
//                        break;
//                    }
//                }
//                if (partCheck)
//                {
//					return true;
//				}
//            }
//            return false;
//        }
//    }
//}