//using System;
//using System.Collections.Generic;
//using Mono.Cecil;

//namespace Flower.UnityObfuscator
//{
//    internal static class ObfuscateItemFactory
//    {
//        public static NamespaceObfuscateItem Create(string @namespace, ModuleDefinition moduleDefinition) { return new(@namespace, moduleDefinition); }
//        public static TypeObfuscateItem Create(TypeDefinition typeDefinition) { return new(typeDefinition); }
//        public static MethodObfuscateItem Create(MethodDefinition methodDefinition) { return new(methodDefinition); }
//        public static PropertyObfuscateItem Create(PropertyDefinition propertyDefinition) { return new(propertyDefinition); }
//        public static FieldObfuscateItem Create(FieldDefinition fieldDefinition) { return new(fieldDefinition); }
//    }
//    internal abstract class BaseObfuscateItem : IEquatable<BaseObfuscateItem>
//    {
//        protected string moduleName;
//        public abstract string Name { get; }
//        public override int GetHashCode() { return this.GetHashCode(); }
//        public override bool Equals(object obj) { return obj is BaseObfuscateItem item && Equals(item); }
//        public abstract bool Equals(BaseObfuscateItem other);
//        public override string ToString() { return moduleName; }
//        public static bool operator ==(BaseObfuscateItem a, BaseObfuscateItem b) { return a.Equals(b); }
//        public static bool operator !=(BaseObfuscateItem a, BaseObfuscateItem b) { return !(a == b); }
//    }
//    internal class NamespaceObfuscateItem : BaseObfuscateItem
//    {
//        private string @namespace;
//        public NamespaceObfuscateItem(string @namespace, ModuleDefinition moduleDefinition)
//        {
//            this.@namespace = @namespace;
//            moduleName = moduleDefinition.Name;
//        }
//        public override string Name { get { return @namespace; } }
//        public override int GetHashCode() { return @namespace.GetHashCode(); }
//        public override bool Equals(object obj) { return obj is NamespaceObfuscateItem item && Equals(item); }
//        public override bool Equals(BaseObfuscateItem other) { return other is NamespaceObfuscateItem item && Equals(item); }
//        public bool Equals(NamespaceObfuscateItem other) { return @namespace == other.@namespace; }
//        public override string ToString() { return string.Format("[{0}]{1}", moduleName, @namespace); }
//        public static bool operator ==(NamespaceObfuscateItem a, NamespaceObfuscateItem b) { return a.Equals(b); }
//        public static bool operator !=(NamespaceObfuscateItem a, NamespaceObfuscateItem b) { return !(a == b); }
//    }
//    internal class TypeObfuscateItem : BaseObfuscateItem
//    {
//        private string @namespace;
//        private string typeName;
//        public TypeObfuscateItem(TypeDefinition typeDefinition)
//        {
//            moduleName = typeDefinition.Module.Name;
//            @namespace = typeDefinition.Namespace;
//            typeName = typeDefinition.Name;
//        }
//        public override string Name { get { return typeName; } }
//        public override int GetHashCode() { return @namespace.GetHashCode() ^ typeName.GetHashCode(); }
//        public override bool Equals(object obj) { return obj is TypeObfuscateItem item && Equals(item); }
//        public override bool Equals(BaseObfuscateItem other) { return other is TypeObfuscateItem item && Equals(item); }
//        public bool Equals(TypeObfuscateItem other) { return @namespace == other.@namespace && typeName == other.typeName; }
//        public override string ToString()
//        {
//            if (string.IsNullOrEmpty(@namespace))
//            {
//				return string.Format("[{0}]{1}", moduleName, typeName);
//			}
//            else
//            {
//				return string.Format("[{0}]{1}.{2}", moduleName, @namespace, typeName);
//			}
//        }
//        public static bool operator ==(TypeObfuscateItem a, TypeObfuscateItem b) { return a.Equals(b); }
//        public static bool operator !=(TypeObfuscateItem a, TypeObfuscateItem b) { return !(a == b); }
//    }
//    internal class MethodObfuscateItem : BaseObfuscateItem
//    {
//        private string @namespace;
//        private string typeName;
//        private string methodName;
//        private int genericParamCount;
//        private List<string> paramList = new();
//        public MethodObfuscateItem(MethodDefinition methodDefinition)
//        {
//            moduleName = methodDefinition.Module.Name;
//            @namespace = methodDefinition.DeclaringType.Namespace;
//            typeName = methodDefinition.DeclaringType.Name;
//            methodName = methodDefinition.Name;
//            genericParamCount = methodDefinition.GenericParameters?.Count ?? 0;
//            foreach (ParameterDefinition param in methodDefinition.Parameters)
//            {
//				// 参数类型可能是已经被混淆过的,查找原始的参数类型名
//				paramList.Add(NameFactory.Instance.GetOldNameFromNewName(param.ParameterType.Name, NameType.Class));
//			}
//		}
//        public MethodObfuscateItem(MethodReference methodReference)
//        {
//			@namespace = methodReference.DeclaringType.Namespace;
//            moduleName = "";
//            typeName = methodReference.DeclaringType.Name;
//            methodName = methodReference.Name;
//            genericParamCount = methodReference.GenericParameters?.Count ?? 0;
//            foreach (var param in methodReference.Parameters)
//            {
//				// 参数类型可能是已经被混淆过的,查找原始的参数类型名
//				paramList.Add(NameFactory.Instance.GetOldNameFromNewName(param.ParameterType.Name, NameType.Class));
//			}
//		}
//        public override string Name { get { return methodName; } }
//		public override int GetHashCode() 
//        {
//            int paramHash = 0;
//            foreach (string param in paramList)
//            {
//                paramHash ^= param.GetHashCode();
//            }
//            return @namespace.GetHashCode() ^ typeName.GetHashCode() ^ methodName.GetHashCode() ^ paramHash; 
//        }
//		public override bool Equals(object obj) { return obj is MethodObfuscateItem item && Equals(item); }
//        public override bool Equals(BaseObfuscateItem other) { return other is MethodObfuscateItem item && Equals(item); }
//        public bool Equals(MethodObfuscateItem other) 
//        {
//            int count = paramList.Count;
//            if (count != other.paramList.Count)
//            {
//                return false;
//            }
//            for (int i = 0; i < count; ++i)
//            {
//                if (paramList[i] != other.paramList[i])
//                {
//                    return false;
//                }
//            }
//            return @namespace == other.@namespace && 
//                   typeName == other.typeName && 
//                   methodName == other.methodName &&
//				   genericParamCount == other.genericParamCount;
//        }
//        public override string ToString()
//        {
//            if (string.IsNullOrEmpty(@namespace))
//            {
//				return string.Format("[{0}]{1}.{2}", moduleName, typeName, methodName);
//			}
//            else
//            {
//				return string.Format("[{0}]{1}.{2}.{3}<{4}>", moduleName, @namespace, typeName, methodName, genericParamCount);
//			}
//        }
//        public static bool operator ==(MethodObfuscateItem a, MethodObfuscateItem b) { return a.Equals(b); }
//        public static bool operator !=(MethodObfuscateItem a, MethodObfuscateItem b) { return !(a == b); }
//    }
//    internal class PropertyObfuscateItem : BaseObfuscateItem
//    {
//        private string @namespace;
//        private string typeName;
//        private string propertyName;
//        public PropertyObfuscateItem(PropertyDefinition propertyDefinition)
//        {
//            moduleName = propertyDefinition.Module.Name;
//            @namespace = propertyDefinition.DeclaringType.Namespace;
//            typeName = propertyDefinition.DeclaringType.Name;
//            propertyName = propertyDefinition.Name;
//        }
//        public PropertyObfuscateItem(PropertyReference propertyReference)
//        {
//			@namespace = propertyReference.DeclaringType.Namespace;
//            moduleName = "";
//            typeName = propertyReference.DeclaringType.Name;
//            propertyName = propertyReference.Name;
//        }
//        public override int GetHashCode() { return @namespace.GetHashCode() ^ typeName.GetHashCode() ^ propertyName.GetHashCode(); }
//        public override string Name { get { return propertyName; } }
//        public override bool Equals(object obj) { return obj is PropertyObfuscateItem item && Equals(item); }
//        public override bool Equals(BaseObfuscateItem other) { return other is PropertyObfuscateItem item && Equals(item); }
//        public bool Equals(PropertyObfuscateItem other) { return @namespace == other.@namespace && typeName == other.typeName && propertyName == other.propertyName; }
//        public override string ToString()
//        {
//            if (string.IsNullOrEmpty(@namespace))
//            {
//				return string.Format("[{0}]{1}.{2}", moduleName, typeName, propertyName);
//			}
//            else
//            {
//				return string.Format("[{0}]{1}.{2}.{3}", moduleName, @namespace, typeName, propertyName);
//			}
//        }
//        public static bool operator ==(PropertyObfuscateItem a, PropertyObfuscateItem b) { return a.Equals(b); }
//        public static bool operator !=(PropertyObfuscateItem a, PropertyObfuscateItem b) { return !(a == b); }
//    }
//    internal class FieldObfuscateItem : BaseObfuscateItem
//    {
//        private string @namespace;
//        private string typeName;
//        private string fieldName;
//        public FieldObfuscateItem(FieldDefinition fieldDefinition)
//        {
//            moduleName = fieldDefinition.Module.Name;
//            @namespace = fieldDefinition.DeclaringType.Namespace;
//            typeName = fieldDefinition.DeclaringType.Name;
//            fieldName = fieldDefinition.Name;
//        }
//        public FieldObfuscateItem(FieldReference fieldReference)
//        {
//			@namespace = fieldReference.DeclaringType.Namespace;
//            moduleName = "";
//            typeName = fieldReference.DeclaringType.Name;
//            fieldName = fieldReference.Name;
//        }
//        public override string Name { get { return fieldName; } }
//        public override int GetHashCode() { return @namespace.GetHashCode() ^ typeName.GetHashCode() ^ fieldName.GetHashCode(); }
//        public override bool Equals(object obj) { return obj is FieldObfuscateItem item && Equals(item); }
//        public override bool Equals(BaseObfuscateItem other) { return other is FieldObfuscateItem item && Equals(item); }
//        public bool Equals(FieldObfuscateItem other) { return @namespace == other.@namespace && typeName == other.typeName && fieldName == other.fieldName; }
//        public override string ToString()
//        {
//            if (string.IsNullOrEmpty(@namespace))
//            {
//				return string.Format("[{0}]{1}.{2}", moduleName, typeName, fieldName);
//			}
//            else
//            {
//				return string.Format("[{0}]{1}.{2}.{3}", moduleName, @namespace, typeName, fieldName);
//			}
//        }
//        public static bool operator ==(FieldObfuscateItem a, FieldObfuscateItem b) { return a.Equals(b); }
//        public static bool operator !=(FieldObfuscateItem a, FieldObfuscateItem b) { return !(a == b); }
//    }
//}