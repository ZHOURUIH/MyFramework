
class ClassInfo
{
	public string mClassName;
	public string mHeaderContent;
	public string mSourceContent;
	public List<string> mFields = new();
	public ClassInfo(string name, string header, string source, List<string> fields)
	{
		mClassName = name;
		mHeaderContent = header;
		mSourceContent = source;
		mFields = fields;
	}
}

class Program
{
	static void Main()
	{
		string exeDir = AppDomain.CurrentDomain.BaseDirectory + "../";
		var files = Directory.GetFiles(exeDir, "*.*", SearchOption.AllDirectories);
		Dictionary<string, string> allFileContent = new();
		foreach (var file in files)
		{
			if (!file.EndsWith(".h") && !file.EndsWith(".hpp") && !file.EndsWith(".cpp"))
			{
				continue;
			}
			allFileContent.Add(file, File.ReadAllText(file));
		}
		Dictionary<string, ClassInfo> allClassFields = new();
		foreach (var item in allFileContent)
		{
			string fileName = item.Key;
			string headerContent = item.Value;
			// 先遍历头文件,获取类定义信息
			if (!fileName.EndsWith(".h"))
			{
				continue;
			}
			// 查找类定义
			foreach (var className in GetClassNames(headerContent))
			{
				// 获取类体（第一个大括号内的内容）
				string classBody = GetClassBody(headerContent, className);
				if (classBody == null)
				{
					continue;
				}
				// 获取成员变量
				string sourceFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".cpp";
				allFileContent.TryGetValue(sourceFileName, out string sourceContent);
				allClassFields.Add(className, new(className, headerContent, sourceContent, GetClassFields(classBody)));
			}
		}

		foreach (ClassInfo item in allClassFields.Values)
		{
			// 查找 resetProperty 函数体
			string resetBody = GetResetPropertyBody(item.mHeaderContent, item.mClassName, allFileContent);
			if (resetBody == null)
			{
				continue;
			}

			// 检查字段是否被重置
			CheckFieldsReset(item.mClassName, item.mFields, resetBody);
		}
		Console.WriteLine("按任意键退出...");
		Console.ReadKey(true);
	}

	// ------------------- 获取类名 -------------------
	static List<string> GetClassNames(string content)
	{
		string[] lines = content.Split('\r', '\n');
		var names = new List<string>();
		foreach (string line in lines)
		{
			if (!line.StartsWith("class "))
			{
				continue;
			}
			string className = "";
			// 有继承
			int index0;
			if (line.Contains(':'))
			{
				index0 = line.IndexOf(':');
			}
			// 没有继承
			else
			{
				index0 = line.Length;
			}
			int classNameEnd = -1;
			for (int j = index0 - 1; j >= 0; --j)
			{
				if (classNameEnd >= 0)
				{
					if (line[j] == ' ' || line[j] == '\t')
					{
						className = line.Substring(j + 1, classNameEnd - j);
						break;
					}
				}
				else
				{
					if (line[j] != ' ' && line[j] != '\t')
					{
						classNameEnd = j;
						continue;
					}
				}
			}
			names.Add(className);
		}
		return names;
	}
	static string GetClassBody(string code, string className)
	{
		int n = code.Length;
		int i = 0;
		while (i < n)
		{
			SkipSpacesAndComments(code, ref i);

			if (i >= n)
			{
				break;
			}

			int save = i;
			string kw = ReadToken(code, ref i);
			if (kw != "class" && kw != "struct")
			{
				i = save + 1;
				continue;
			}

			SkipSpacesAndComments(code, ref i);

			// read tokens until ':' or '{' or ';' to find last identifier
			string lastIdent = null;
			while (i < n)
			{
				SkipSpacesAndComments(code, ref i);
				if (i >= n)
				{
					break;
				}
				char c = code[i];
				if (c == ':' || c == '{' || c == ';')
				{
					break;
				}

				string tok = ReadToken(code, ref i);
				if (!string.IsNullOrEmpty(tok) && IsIdentifier(tok))
				{
					lastIdent = tok;
				}
				else if (string.IsNullOrEmpty(tok))
				{
					i++;
				}
			}

			if (lastIdent == null || lastIdent != className)
			{
				continue;
			}

			// find opening '{' (skip over inheritance list, attributes etc.)
			int j = i;
			bool foundBrace = false;
			while (j < n)
			{
				SkipSpacesAndComments(code, ref j);
				if (j >= n)
				{
					break;
				}
				if (code[j] == '{')
				{
					foundBrace = true;
					break;
				}
				if (code[j] == ';')
				{
					// forward declaration, not a definition
					foundBrace = false;
					break;
				}
				j++;
			}
			if (!foundBrace)
			{
				continue;
			}

			int openPos = j;
			int pos = openPos + 1;
			int depth = 1;
			while (pos < n && depth > 0)
			{
				char ch = code[pos];
				if (ch == '"' || ch == '\'')
				{
					SkipStringLiteral(code, ref pos);
					continue;
				}
				if (ch == '/')
				{
					// comments
					if (pos + 1 < n)
					{
						if (code[pos + 1] == '/')
						{
							pos += 2;
							while (pos < n && code[pos] != '\n')
							{
								pos++;
							}
							continue;
						}
						else if (code[pos + 1] == '*')
						{
							pos += 2;
							while (pos + 1 < n && !(code[pos] == '*' && code[pos + 1] == '/'))
							{
								pos++;
							}
							if (pos + 1 < n)
							{
								pos += 2;
							}
							continue;
						}
					}
				}
				if (ch == '{')
				{
					depth++;
				}
				else if (ch == '}')
				{
					depth--;
				}
				pos++;
			}

			if (depth != 0)
			{
				return null;
			}

			int contentStart = openPos + 1;
			int contentEndExclusive = pos - 1; // pos points after matching '}'
			if (contentEndExclusive <= contentStart)
			{
				return "";
			}
			return code.Substring(contentStart, contentEndExclusive - contentStart);
		}
		return null;
	}
	static void SkipSpacesAndComments(string code, ref int i)
	{
		int n = code.Length;
		while (i < n)
		{
			// whitespace
			if (char.IsWhiteSpace(code[i])) { i++; continue; }

			// line comment //
			if (code[i] == '/' && i + 1 < n && code[i + 1] == '/')
			{
				i += 2;
				while (i < n && code[i] != '\n') i++;
				continue;
			}

			// block comment /* */
			if (code[i] == '/' && i + 1 < n && code[i + 1] == '*')
			{
				i += 2;
				while (i + 1 < n && !(code[i] == '*' && code[i + 1] == '/')) i++;
				if (i + 1 < n) i += 2;
				continue;
			}
			// preprocessor directive: skip whole line if starts with '#'
			if (code[i] == '#')
			{
				while (i < n && code[i] != '\n') i++;
				continue;
			}
			// not space/comment
			break;
		}
	}
	static bool IsIdentifier(string tok)
	{
		if (string.IsNullOrEmpty(tok))
		{
			return false;
		}
		if (!(char.IsLetter(tok[0]) || tok[0] == '_'))
		{
			return false;
		}
		for (int k = 1; k < tok.Length; k++)
		{
			if (!(char.IsLetterOrDigit(tok[k]) || tok[k] == '_'))
			{
				return false;
			}
		}
		return true;
	}
	// Read next token: identifier, operator, punctuator or literal-ish
	static string ReadToken(string code, ref int i)
	{
		int n = code.Length;
		SkipSpacesAndComments(code, ref i);
		if (i >= n)
		{
			return null;
		}

		char c = code[i];
		// identifiers or keywords
		if (char.IsLetter(c) || c == '_')
		{
			int start = i;
			i++;
			while (i < n && (char.IsLetterOrDigit(code[i]) || code[i] == '_'))
			{
				i++;
			}
			return code.Substring(start, i - start);
		}
		// numbers (we don't need them but consume)
		if (char.IsDigit(c))
		{
			int start = i++;
			while (i < n && (char.IsLetterOrDigit(code[i]) || code[i] == '.' || code[i] == 'x' || code[i] == 'X' || code[i] == 'u' || code[i] == 'U'))
			{
				i++;
			}
			return code.Substring(start, i - start);
		}
		// single char tokens
		i++;
		return c.ToString();
	}
	static void SkipStringLiteral(string code, ref int i)
	{
		int n = code.Length;
		if (i >= n)
		{
			return;
		}
		char quote = code[i];
		i++; // skip opening quote
		while (i < n)
		{
			if (code[i] == '\\')
			{
				// escape, skip next char
				i += 2;
				continue;
			}
			if (code[i] == quote)
			{
				i++;
				break;
			}
			i++;
		}
	}
	// 判断是否为 C/C++ 标识符字符（字母/数字/下划线）
	static bool IsIdentChar(char c)
	{
		return char.IsLetterOrDigit(c) || c == '_';
	}
	// ------------------- 收集第一层成员变量 -------------------
	static List<string> GetClassFields(string classBody)
	{
		List<string> fields = new();
		int braceLevel = 0;
		foreach (var line in classBody.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
		{
			string trimmed = line.Trim();

			// 跳过注释/空行
			if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//"))
			{
				continue;
			}
			// 跳过 public: private:
			if (trimmed == "public:" || trimmed == "protected:" || trimmed == "private:")
			{
				continue;
			}
			// 跳过函数
			if (trimmed.Contains("("))
			{
				continue;
			}
			// 跳过 static/const/constexpr
			if (trimmed.StartsWith("static ") ||
				trimmed.StartsWith("const ") ||
				trimmed.StartsWith("constexpr "))
			{ 
				continue;
			}

			// 更新 braceLevel（跳过内部类/函数）
			foreach (char c in trimmed)
			{
				if (c == '{')
				{
					braceLevel++;
				}
				else if (c == '}')
				{
					braceLevel--;
				}
			}
			if (braceLevel != 0)
			{
				continue;
			}
			// 如果结尾有注释,则移除注释
			if (trimmed.Contains("//"))
			{
				trimmed = trimmed.Substring(0, trimmed.IndexOf("//"));
				trimmed = trimmed.TrimEnd();
			}
			// 必须是字段声明，必须以 ; 结尾
			if (!trimmed.EndsWith(";"))
			{
				continue;
			}
			string fieldName = ExtractFieldName(trimmed);
			if (fieldName != null)
			{
				fields.Add(fieldName);
			}
		}
		return fields;
	}
	static string ExtractFieldName(string line)
	{
		// 去掉末尾分号
		line = line.Substring(0, line.Length - 1).Trim();
		// 如果有等号，找 "=" 左边最近的一个标识符
		int eq = line.IndexOf('=');
		string leftPart = (eq >= 0) ? line.Substring(0, eq).Trim() : line;
		// 分割 token
		string[] tokens = leftPart.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0)
		{
			return null;
		}

		// 找最后一个可能的字段名 token,去掉 * 或 &
		string last = tokens[tokens.Length - 1].Trim('*', '&');
		// 必须是合法 C++ 标识符
		if (IsIdentifier(last))
		{
			return last;
		}
		return null;
	}
	// 判断从 start 开始是否匹配 ident（且前后不是标识符字符）
	static bool MatchIdentifierAt(string s, int start, string ident)
	{
		if (start + ident.Length > s.Length)
		{
			return false;
		}
		if (!s.AsSpan(start, ident.Length).SequenceEqual(ident.AsSpan()))
		{
			return false;
		}
		if (start > 0 && IsIdentChar(s[start - 1]))
		{
			return false;
		}
		int after = start + ident.Length;
		if (after < s.Length && IsIdentChar(s[after]))
		{
			return false;
		}
		return true;
	}
	// 在 classBody 中查找 inline resetProperty() { ... }
	static string GetResetPropertyBody_InClassBody(string classBody)
	{
		if (string.IsNullOrEmpty(classBody))
		{
			return null;
		}

		string key = "resetProperty";
		int idx = 0;
		while (true)
		{
			idx = classBody.IndexOf(key, idx, StringComparison.Ordinal);
			if (idx < 0)
			{
				return null;
			}

			// 确保是独立标识符
			if (!MatchIdentifierAt(classBody, idx, key))
			{
				idx += key.Length;
				continue;
			}

			// 查找前面最近的非空白字符（用于排除 base::resetProperty / obj.resetProperty / ptr->resetProperty）
			int prev = idx - 1;
			while (prev >= 0 && char.IsWhiteSpace(classBody[prev]))
			{
				prev--;
			}
			if (prev >= 0)
			{
				char pc = classBody[prev];
				if (pc == ':' || pc == '.' || pc == '>') // '::' / '.' / '->'
				{
					idx += key.Length;
					continue; // 这显然是调用或限定，不是定义
				}
			}

			// 找 '(' （中间只能有空白或注释）
			int p = idx + key.Length;
			// allow whitespace/comments between name and '('
			SkipSpacesAndComments(classBody, ref p);
			if (p >= classBody.Length || classBody[p] != '(')
			{
				idx += key.Length;
				continue;
			}

			// 找配对的 ')'
			int p2 = p + 1;
			int paren = 1;
			while (p2 < classBody.Length && paren > 0)
			{
				char c = classBody[p2];
				if (c == '(')
				{
					paren++;
				}
				else if (c == ')')
				{
					paren--;
				}
				else if (c == '"' || c == '\'')
				{
					SkipStringLiteral(classBody, ref p2);
				}
				p2++;
			}
			if (paren != 0)
			{
				return null; // 不完整的括号对，放弃
			}

			// 从 p2 开始，跳过空白/注释和允许的修饰关键词（const/override/noexcept/final/attributes等）
			int b = p2;
			while (true)
			{
				int before = b;
				SkipSpacesAndComments(classBody, ref b);
				if (b >= classBody.Length)
				{
					break;
				}
				// if it's a '{' then we have a definition inline
				if (classBody[b] == '{')
				{
					break;
				}
				// if it's a ';' then it's only a declaration/forward-declaration or call; skip
				if (classBody[b] == ';') 
				{
					b = -1;
					break; 
				}
				// read next token to see if it's a known qualifier/attribute, otherwise stop skipping
				// token can be identifier or '[' (attribute)
				if (classBody[b] == '[')
				{
					// attribute [[...]] or C++11 attributes — skip till matching ]
					b++;
					int attrDepth = 1;
					while (b < classBody.Length && attrDepth > 0)
					{
						if (classBody[b] == '[')
						{
							attrDepth++;
						}
						else if (classBody[b] == ']')
						{
							attrDepth--;
						}
						else if (classBody[b] == '"' || classBody[b] == '\'')
						{
							SkipStringLiteral(classBody, ref b);
						}
						b++;
					}
					continue;
				}

				// collect token
				int tstart = b;
				while (b < classBody.Length && (IsIdentChar(classBody[b]) || classBody[b] == ':'))
				{
					b++;
				}
				if (b == tstart)
				{
					break;
				}
				string token = classBody.Substring(tstart, b - tstart);
				// allow these tokens to appear between ')' and '{'
				if (token == "const" || token == "override" || token == "noexcept" || token == "final" || token == "throw")
				{
					continue;
				}
				// anything else: stop scanning; only accept if next non-space is '{'
				break;
			}

			if (b == -1)
			{
				// found ';' — this is a declaration or call, not a definition
				idx += key.Length;
				continue;
			}

			// skip spaces/comments to find '{'
			SkipSpacesAndComments(classBody, ref b);
			if (b < classBody.Length && classBody[b] == '{')
			{
				// found opening brace; capture balanced block
				int start = b + 1;
				int depth = 1;
				int pos = start;
				while (pos < classBody.Length && depth > 0)
				{
					char ch = classBody[pos];
					if (ch == '"' || ch == '\'')
					{
						SkipStringLiteral(classBody, ref pos);
					}
					else if (ch == '{')
					{
						depth++;
					}
					else if (ch == '}')
					{
						depth--;
					}
					pos++;
				}
				if (depth == 0)
				{
					// return content inside braces (exclude the outer braces)
					return classBody.Substring(start, pos - start - 1);
				}
				return null;
			}
			else
			{
				// no '{' -> not a definition here
				idx += key.Length;
				continue;
			}
		}
	}
	static string FindResetPropertyBodyInProject(string className, Dictionary<string, string> allFiles)
	{
		foreach (string code in allFiles.Values)
		{
			int pos = 0;
			while (true)
			{
				// 找 className 出现处
				pos = code.IndexOf(className, pos, StringComparison.Ordinal);
				if (pos < 0)
				{
					break;
				}

				// 确保 className 是完整标识符 (前后边界都不是字母/数字/_)
				bool leftOk = (pos == 0) || !IsIdentChar(code[pos - 1]);
				int afterName = pos + className.Length;
				bool rightOk = (afterName >= code.Length) || !IsIdentChar(code[afterName]);
				if (!leftOk || !rightOk)
				{
					pos = afterName;
					continue;
				}

				// 跳空白
				int p = afterName;
				while (p < code.Length && char.IsWhiteSpace(code[p]))
				{
					p++;
				}

				// 期望 "::"
				if (p + 1 < code.Length && code[p] == ':' && code[p + 1] == ':')
				{
					p += 2;
				}
				else
				{
					pos = afterName;
					continue;
				}

				// 跳空白
				while (p < code.Length && char.IsWhiteSpace(code[p]))
				{
					p++;
				}

				// 期望 resetProperty
				const string key = "resetProperty";
				if (p + key.Length <= code.Length && code.AsSpan(p, key.Length).SequenceEqual(key.AsSpan()))
				{
					// 确保关键字边界
					if (p + key.Length < code.Length && IsIdentChar(code[p + key.Length]))
					{
						pos = p + key.Length;
						continue;
					}

					int after = p + key.Length;
					// 跳空白
					while (after < code.Length && char.IsWhiteSpace(code[after]))
					{
						after++;
					}
					if (after >= code.Length || code[after] != '(')
					{
						pos = after;
						continue;
					}

					// 找匹配的 ')'
					int q2 = after + 1;
					int paren = 1;
					while (q2 < code.Length && paren > 0)
					{
						if (code[q2] == '"' || code[q2] == '\'')
						{
							SkipStringLiteral(code, ref q2);
						}
						else if (code[q2] == '(')
						{
							paren++;
						}
						else if (code[q2] == ')')
						{
							paren--;
						}
						q2++;
					}
					if (paren != 0) 
					{
						pos = after; 
						continue; 
					}

					// 跳空白/注释
					int b = q2;
					SkipSpacesAndComments(code, ref b);
					if (b < code.Length && code[b] == '{')
					{
						// 找匹配 }
						int start = b + 1;
						int depth = 1, k = start;
						while (k < code.Length && depth > 0)
						{
							if (code[k] == '"' || code[k] == '\'')
							{
								SkipStringLiteral(code, ref k);
							}
							else if (code[k] == '{')
							{
								depth++;
							}
							else if (code[k] == '}')
							{
								depth--;
							}
							k++;
						}
						if (depth == 0)
						{
							return code.Substring(start, k - start - 1);
						}
					}
				}
				pos = afterName;
			}
		}
		return null;
	}
	// headerContent: 头文件全文； allFiles: path->content（包含头和源）
	// 如果 header 里有 class body，先在 class body 内查；若没，则在 allFiles 中查 ClassName::resetProperty
	static string GetResetPropertyBody(string headerContent, string className, Dictionary<string, string> allFiles)
	{
		// 1) 如果 header 包含 class 定义，提取 class body 并查 inline 定义
		string classBody = GetClassBody(headerContent, className); // 你已有或用我们之前的实现
		if (classBody != null)
		{
			string inline = GetResetPropertyBody_InClassBody(classBody);
			if (inline != null)
			{
				return inline;
			}
		}
		// 2) 在项目所有文件查 ClassName::resetProperty 的实现
		return FindResetPropertyBodyInProject(className, allFiles);
	}
	// ------------------- 检查字段是否被重置 -------------------
	static bool IsFieldReset(string field, string resetBody)
	{
		string[] lines = resetBody.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string line in lines)
		{
			string t = line.Trim();
			// 1. 赋值判断
			if (t.Contains(field) && t.Contains("="))
			{
				// 确保是 "field =" 而不是 "otherField = field"
				int idx = t.IndexOf(field);
				if (idx >= 0 && t.IndexOf('=') > idx)
				{
					return true;
				}
			}
			// 2. 调用方法判断： field.xxx(...)
			if (t.StartsWith(field + ".") && t.Contains("(") && t.Contains(")"))
			{
				return true;
			}
			if (t.StartsWith(field + "->") && t.Contains("(") && t.Contains(")"))
			{
				return true;
			}
		}
		return false;
	}
	static void CheckFieldsReset(string className, List<string> fields, string resetBody)
	{
		foreach (string field in fields)
		{
			if (!IsFieldReset(field, resetBody))
			{
				Console.WriteLine($"[WARN] {className}::{field} 可能没有在 resetProperty 中重置!");
			}
		}
	}
}