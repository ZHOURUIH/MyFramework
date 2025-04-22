using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flower.UnityObfuscator
{
    internal class NameFactory
    {
        private class NameCollection
        {
            public Dictionary<BaseObfuscateItem, string> old_new_Dic = new();
			public static Random random;
			public static HashSet<string> set = new();
			public static void Init(int randomSeed)
			{
				random = new Random(randomSeed);
				set.Clear();
			}
			public bool haveRandomName(BaseObfuscateItem obfuscateItem)
            {
                return old_new_Dic.ContainsKey(obfuscateItem);
            }
            public string GetNewName(BaseObfuscateItem obfuscateItem)
            {
                if (!old_new_Dic.TryGetValue(obfuscateItem, out string newName))
                {
                    newName = GetAName(obfuscateItem.Name);
                    old_new_Dic.Add(obfuscateItem, newName);
                }
                return newName;
            }
			public string GetOldName(string newName)
            {
                foreach (var namePair in old_new_Dic)
                {
                    if (namePair.Value == newName)
                    {
                        return namePair.Key.Name;
					}
                }
                return null;
            }
			// 这里传入原始名字是为了在某些时候方便调试
			public string GetAName(string oldName)
            {
                return GetANameFromRandomChar();
            }
			public static string GetANameFromRandomChar()
			{
				string result;
				int max = 1000;
				int current = 0;
				do
				{
					StringBuilder sb = new(Const.RandomNameLen);
					for (int i = 0; i < Const.RandomNameLen; ++i)
					{
						sb.Append(Const.randomCharArray[random.Next(0, Const.randomCharArray.Length)]);
					}
					result = sb.ToString();
					if (++current > max)
					{
						throw new Exception("Not enough random name");
					}
				} while (set.Contains(result));
				set.Add(result);
				return result;
			}
		}
        private Dictionary<NameType, NameCollection> NameCollectionDic = new();
        private static NameFactory _instance;
        public static NameFactory Instance { get { return _instance; } }
        static NameFactory() { _instance = new NameFactory(); }
		private NameFactory() { }
		public void Load()
        {
			NameCollection.Init((int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds);
            NameCollectionDic.Clear();
            foreach (NameType v in Enum.GetValues(typeof(NameType)))
            {
                NameCollectionDic.Add(v, new());
            }
        }
		public string TryGetExistRandomName(NameType nameType, BaseObfuscateItem obfuscateItem)
		{
			if (NameCollectionDic.TryGetValue(nameType, out NameCollection collection) &&
				collection.haveRandomName(obfuscateItem))
			{
				return collection.GetNewName(obfuscateItem);
			}
			return obfuscateItem.Name;
		}
        public string GetRandomName(NameType nameType, BaseObfuscateItem obfuscateItem)
        {
			if (NameCollectionDic.TryGetValue(nameType, out NameCollection collection))
			{
                return collection.GetNewName(obfuscateItem);
            }
            return string.Empty;
        }
        public string GetRandomName(string oldName)
        {
            return NameCollectionDic[NameType.Other].GetAName(oldName);
        }
        public string GetOldNameFromNewName(string newName, NameType type)
        {
            NameCollectionDic.TryGetValue(type, out NameCollection collection);
            return collection?.GetOldName(newName) ?? newName;
		}
        public void OutputNameMap(string path)
        {
            string result = string.Empty;
            foreach (var collection in NameCollectionDic)
            {
                if (collection.Key == NameType.Other)
                {
                    continue;
                }
                result += "-------------" + collection.Key.ToString() + "-------------\n";
                foreach (var item in collection.Value.old_new_Dic)
                {
                    result += item.Key.Name + " ===> " + item.Value + "\n";
                }
            }
            File.WriteAllText(path, result);
        }
    }
}