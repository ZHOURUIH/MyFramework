using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
[Serializable]
public class DynamicDownloadConfig
{
	public string Version;
	public List<string> Directories;
	public static bool tryParse(string content, string version, out DynamicDownloadConfig config)
	{
		config = null;
		if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(version))
		{
			return false;
		}
		try
		{
			JObject data = JObject.Parse(content);
			if (data[nameof(Version)]?.Type != JTokenType.String || data[nameof(Version)].Value<string>() != version ||
				data[nameof(Directories)] is not JArray directories)
			{
				return false;
			}
			DynamicDownloadConfig parsed = new() { Version = version, Directories = new() };
			foreach (JToken item in directories)
			{
				if (item.Type != JTokenType.String)
				{
					return false;
				}
				string directory = item.Value<string>();
				if (string.IsNullOrWhiteSpace(directory) || directory != directory.Trim() ||
					directory.StartsWith("/") || directory.Contains("\\") || directory.Contains(":") ||
					directory.Contains("*") || directory.Contains("?") || directory.Contains("\n") || directory.Contains("\r"))
				{
					return false;
				}
				string[] parts = directory.TrimEnd('/').Split('/');
				foreach (string part in parts)
				{
					if (part.Length == 0 || part == "." || part == "..")
					{
						return false;
					}
				}
				parsed.Directories.Add(directory.ToLowerInvariant());
			}
			config = parsed;
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
