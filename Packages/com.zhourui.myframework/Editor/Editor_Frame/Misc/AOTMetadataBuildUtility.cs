using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if USE_HYBRID_CLR
using HybridCLR.Editor.AOT;
#endif
public class AOTMetadataBuildUtility
{
	public static bool optimize(string resourcePath, IReadOnlyList<string> assemblyNames)
	{
#if USE_HYBRID_CLR
		try
		{
			long savedBytes = 0;
			foreach (string assemblyName in assemblyNames)
			{
				string path = Path.Combine(resourcePath, assemblyName + ".bytes");
				byte[] original = File.ReadAllBytes(path);
				byte[] optimized = AOTAssemblyMetadataStripper.Strip(original);
				if (optimized.Length >= original.Length)
				{
					continue;
				}
				string hash = FileUtility.generateFileMD5(original);
				string backup = Path.Combine("Library/AOTMetadataOriginal", hash, assemblyName);
				Directory.CreateDirectory(Path.GetDirectoryName(backup));
				File.WriteAllBytes(backup, original);
				File.WriteAllBytes(path, optimized);
				savedBytes += original.Length - optimized.Length;
				Debug.Log("AOT 补充元数据精简:" + assemblyName + "，" + original.Length + " -> " + optimized.Length + " bytes");
			}
			Debug.Log("AOT 补充元数据合计减少:" + savedBytes + " bytes");
			return true;
		}
		catch (Exception e)
		{
			Debug.LogError("AOT 补充元数据精简失败，停止打包:" + e);
			return false;
		}
#else
		return true;
#endif
	}
}
