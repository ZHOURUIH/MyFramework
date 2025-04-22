using Mono.Cecil;
using UnityEditor;
using UnityEngine;
using System;
using Mono.Cecil.Cil;

namespace Flower.UnityObfuscator
{
    public class CodeObfuscator
    {
        public static void DoObfuscate(string[] assemblyPath, string nameMapFilePath)
        {
			if (Application.isPlaying || EditorApplication.isCompiling)
            {
                Debug.Log("You need stop play mode or wait compiling finished");
                return;
            }

            if (assemblyPath.Length <= 0)
            {
                Debug.LogError("Obfuscate dll paths length: 0");
				return;
            }
			DefaultAssemblyResolver resolver = new();
            resolver.AddSearchDirectory("./Library/ScriptAssemblies");
			// Windows的路径
			string editorExePath = EditorApplication.applicationPath;
			editorExePath = editorExePath.Substring(0, editorExePath.LastIndexOf('/'));
			resolver.AddSearchDirectory(editorExePath + "/Data/Managed");
			resolver.AddSearchDirectory(editorExePath + "/Data/Managed/UnityEngine");
			// mac中的路径
			resolver.AddSearchDirectory(EditorApplication.applicationPath + "/Contents/Managed/UnityEngine");
			resolver.AddSearchDirectory(EditorApplication.applicationPath + "/Contents/Managed");
			ReaderParameters readerParameters = new(){ AssemblyResolver = resolver, ReadSymbols = false };
            AssemblyDefinition[] assemblies = new AssemblyDefinition[assemblyPath.Length];
            for (int i = 0; i < assemblyPath.Length; ++i)
            {
                var assembly = AssemblyDefinition.ReadAssembly(assemblyPath[i], readerParameters);
                if (assembly == null)
                {
                    Debug.LogError(string.Format("Code Obfuscate Load assembly failed: {0}", assemblyPath[i]));
                    return;
                }
                assemblies[i] = assembly;
            }
			
            try
            {
				// 初始化组件
                NameObfuscate.Instance.Init(ObfuscateType.WhiteList);
                NameFactory.Instance.Load();

                // 混淆
                foreach (AssemblyDefinition assembly in assemblies)
                {
					foreach (TypeDefinition type in assembly.MainModule.Types)
					{
						if (type.IsClass)
						{
							if (NameObfuscate.Instance.ClassNeedSkip(type))
							{
								continue;
							}

							// 修改字段名
							// 带有Serialzable标签的类的成员变量不能修改名字,会导致序列化错误
							if (!type.IsSerializable)
							{
								foreach (FieldDefinition field in type.Fields)
								{
									if (NameObfuscate.Instance.IsChangeField(type, field.Name))
									{
										field.Name = NameFactory.Instance.GetRandomName(NameType.Filed, ObfuscateItemFactory.Create(field));
									}
								}
							}

							// 修改属性名
							foreach (PropertyDefinition property in type.Properties)
							{
								if (NameObfuscate.Instance.IsChangeProperty(type, property.Name))
								{
									property.Name = NameFactory.Instance.GetRandomName(NameType.Property, ObfuscateItemFactory.Create(property));
								}
							}

							// 修改方法名
							foreach (MethodDefinition method in type.Methods)
							{
								if (NameObfuscate.Instance.IsChangeMethod(type, method))
								{
									method.Name = NameFactory.Instance.GetRandomName(NameType.Method, ObfuscateItemFactory.Create(method));
								}
								// 混淆局部变量名字
								if (method.Body != null)
								{
									foreach (VariableDefinition localVar in method.Body.Variables)
									{
										localVar.Name = NameFactory.Instance.GetRandomName(localVar.Name);
									}
								}
								// 混淆函数参数名
								foreach (ParameterDefinition parameter in method.Parameters)
								{
									parameter.Name = NameFactory.Instance.GetRandomName(parameter.Name);
								}
								// 函数模板名
								foreach (GenericParameter parameter in method.GenericParameters)
								{
									parameter.Name = NameFactory.Instance.GetRandomName(parameter.Name);
								}
							}

							// 修改类名
							if (NameObfuscate.Instance.IsChangeClass(type))
							{
								type.Name = NameFactory.Instance.GetRandomName(NameType.Class, ObfuscateItemFactory.Create(type));
							}

							// 修改名字空间名
							if (NameObfuscate.Instance.IsChangeNamespace(type))
							{
								type.Namespace = NameFactory.Instance.GetRandomName(NameType.Namespace, ObfuscateItemFactory.Create(type.Namespace, type.Module));
							}
						}
					}
				}

                // 把每个dll对其他被混淆的dll的引用名字修改为混淆后的名字
                foreach (AssemblyDefinition assembly in assemblies)
                {
					foreach (MemberReference item in assembly.MainModule.GetMemberReferences())
                    {
                        if (item is FieldReference fieldReference)
                        {
                            item.Name = NameFactory.Instance.TryGetExistRandomName(NameType.Filed, new FieldObfuscateItem(fieldReference));
                        }
                        else if (item is PropertyReference propertyReference)
                        {
                            item.Name = NameFactory.Instance.TryGetExistRandomName(NameType.Property, new PropertyObfuscateItem(propertyReference));
                        }
                        else if (item is MethodReference methodReference)
                        {
							item.Name = NameFactory.Instance.TryGetExistRandomName(NameType.Method, new MethodObfuscateItem(methodReference));
                        }
                    }
					foreach (TypeReference item in assembly.MainModule.GetTypeReferences())
					{
						TypeDefinition typeDefinition = item.Resolve();
						NamespaceObfuscateItem namespaceObfuscateItem = ObfuscateItemFactory.Create(typeDefinition.Namespace, typeDefinition.Module);
						item.Name = NameFactory.Instance.TryGetExistRandomName(NameType.Class, ObfuscateItemFactory.Create(typeDefinition));
						item.Namespace = NameFactory.Instance.TryGetExistRandomName(NameType.Namespace, namespaceObfuscateItem);
					}
				}

				for (int i = 0; i < assemblies.Length; ++i)
                {
                    assemblies[i].Write(assemblyPath[i], new WriterParameters { WriteSymbols = false });
				}
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Code Obfuscate failed: {0}", ex));
            }
            finally
            {
                foreach (AssemblyDefinition item in assemblies)
                {
                    item.MainModule.SymbolReader?.Dispose();
                }
                //输出 名字-混淆后名字 的map
                NameFactory.Instance.OutputNameMap(nameMapFilePath);
            }
        }
    }
}