using System;
using System.Diagnostics;
using EasyECS;
using UnityEngine;
using Debug = UnityEngine.Debug;

[ECS]
public struct EasyECSParityIntData
{
	public int mValue;
	public EasyECSParityIntData(int value)
	{
		mValue = value;
	}
}
[ECS]
public struct EasyECSParityUVData
{
	public float x;
	public float y;
	public EasyECSParityUVData(Vector2 uv)
	{
		x = uv.x;
		y = uv.y;
	}
}
[ECS]
public struct EasyECSParityRangeData
{
	public int mStart;
	public int mEnd;
	public EasyECSParityRangeData(int start, int end)
	{
		mStart = start;
		mEnd = end;
	}
}
[ECS]
public struct EasyECSParityBoolData
{
	public bool mValue;
	public EasyECSParityBoolData(bool value)
	{
		mValue = value;
	}
}
[ECS]
public struct EasyECSParityColor32Data
{
	public byte r;
	public byte g;
	public byte b;
	public byte a;
	public EasyECSParityColor32Data(Color32 color)
	{
		r = color.r;
		g = color.g;
		b = color.b;
		a = color.a;
	}
}
public static class EasyECSBuiltInParityBenchmark
{
	private const int ENTITY_COUNT = 500000;
	private const int STRUCTURAL_COUNT = 20000;
	private const int STRUCTURAL_OPERATION_COUNT = 256;
	private const int SEARCH_REPEAT_COUNT = 64;
	private const int BINARY_SEARCH_REPEAT_COUNT = 65536;
	private const int DICTIONARY_COUNT = 200000;
	private const int DICTIONARY_OPERATION_COUNT = 100000;
	private const int SAMPLE_COUNT = 15;
	private const int WARMUP_COUNT = 3;
	private const int MICRO_CASE_REPEAT_COUNT = 64;
	private static long mResultSink;
	private static int mReleaseRegressionCount;
	private static readonly double mTickToMilliseconds = 1000.0 / Stopwatch.Frequency;
	public static void runBenchmark()
	{
		mReleaseRegressionCount = 0;
		Debug.Log("================ EasyECS BuiltIn Parity Benchmark Start ================");
		Debug.Log("Baseline:Int={int mValue},Bool={bool mValue},Vector2={float x,float y},Vector2Int={int mStart,int mEnd},Color32={byte r,g,b,a}");
		Debug.Log("EntityCount:" + ENTITY_COUNT + ",SampleCount:" + SAMPLE_COUNT + ",WarmupCount:" + WARMUP_COUNT);
		runIntListBenchmark();
		runIntIndependentBaselineDiagnostics();
		runBoolListBenchmark();
		runVector2ListBenchmark();
		runVector2IntListBenchmark();
		runColor32ListBenchmark();
		runIntDictionaryBenchmark();
		Debug.Log("BuiltIn Parity ReleaseRegressionCount:" + mReleaseRegressionCount);
		if (mReleaseRegressionCount == 0)
		{
			Debug.Log("================ EasyECS BuiltIn Parity Benchmark PASS =================");
		}
		else
		{
			Debug.LogError("================ EasyECS BuiltIn Parity Benchmark REGRESSION:" + mReleaseRegressionCount + " =================");
		}
		Debug.Log("ResultSink:" + mResultSink);
	}
	private static void runIntListBenchmark()
	{
		EasyECSParityIntData[] baselineValues = new EasyECSParityIntData[ENTITY_COUNT];
		int[] builtInValues = new int[ENTITY_COUNT];
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			baselineValues[i] = new EasyECSParityIntData(i);
			builtInValues[i] = i;
		}
		EasyECSParityIntData_ECSList baseline = new EasyECSParityIntData_ECSList(ENTITY_COUNT);
		Int_ECSList builtIn = new Int_ECSList(ENTITY_COUNT);
		EasyECSParityIntData_ECSList baselineSearch = new EasyECSParityIntData_ECSList(ENTITY_COUNT);
		Int_ECSList builtInSearch = new Int_ECSList(ENTITY_COUNT);
		EasyECSParityIntData[] structuralBaselineValues = new EasyECSParityIntData[STRUCTURAL_COUNT];
		int[] structuralBuiltInValues = new int[STRUCTURAL_COUNT];
		EasyECSParityIntData[] baselineCopyDestination = new EasyECSParityIntData[ENTITY_COUNT];
		int[] builtInCopyDestination = new int[ENTITY_COUNT];
		try
		{
			baseline.AddRange(baselineValues);
			builtIn.AddRange(builtInValues);
			baselineSearch.AddRange(baselineValues);
			builtInSearch.AddRange(builtInValues);
			for (int i = 0; i < STRUCTURAL_COUNT; ++i)
			{
				structuralBaselineValues[i] = baselineValues[i];
				structuralBuiltInValues[i] = builtInValues[i];
			}
			runCase("Int Add无Resize", () => baseline.Clear(), () => builtIn.Clear(), () =>
			{
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					baseline.Add(new EasyECSParityIntData(i));
				}
				return baseline.Count;
			}, () =>
			{
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					builtIn.Add(i);
				}
				return builtIn.Count;
			});
			baseline.Clear();
			builtIn.Clear();
			baseline.AddRange(baselineValues);
			builtIn.AddRange(builtInValues);
			runCase("Int Get读取", null, null, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					sum += baseline.Get(i).mValue;
				}
				return sum;
			}, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					sum += builtIn.Get(i);
				}
				return sum;
			});
			runCase("Int Indexer读写", null, null, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					baseline[i].mValue += 1;
					sum += baseline[i].mValue;
				}
				return sum;
			}, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					builtIn[i] += 1;
					sum += builtIn[i];
				}
				return sum;
			});
			runCase("Int Direct Column读写", null, null, () =>
			{
				long sum = 0;
				var column = baseline.getValueColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					column[i] += 1;
					sum += column[i];
				}
				return sum;
			}, () =>
			{
				long sum = 0;
				var column = builtIn.getValueColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					column[i] += 1;
					sum += column[i];
				}
				return sum;
			});
			runCase("Int AddRange数组", () => baseline.Clear(), () => builtIn.Clear(), () =>
			{
				baseline.AddRange(baselineValues);
				return baseline.Count;
			}, () =>
			{
				builtIn.AddRange(builtInValues);
				return builtIn.Count;
			});
			runCase("Int AddRange自身", () =>
			{
				baseline.Clear();
				baseline.AddRange(structuralBaselineValues);
			}, () =>
			{
				builtIn.Clear();
				builtIn.AddRange(structuralBuiltInValues);
			}, () =>
			{
				baseline.AddRange(baseline);
				return baseline.Count;
			}, () =>
			{
				builtIn.AddRange(builtIn);
				return builtIn.Count;
			}, MICRO_CASE_REPEAT_COUNT);
			runCase("Int Insert中间", () =>
			{
				baseline.Clear();
				baseline.AddRange(structuralBaselineValues);
			}, () =>
			{
				builtIn.Clear();
				builtIn.AddRange(structuralBuiltInValues);
			}, () =>
			{
				for (int i = 0; i < STRUCTURAL_OPERATION_COUNT; ++i)
				{
					baseline.Insert(baseline.Count >> 1, new EasyECSParityIntData(i));
				}
				return baseline.Count;
			}, () =>
			{
				for (int i = 0; i < STRUCTURAL_OPERATION_COUNT; ++i)
				{
					builtIn.Insert(builtIn.Count >> 1, i);
				}
				return builtIn.Count;
			});
			runCase("Int RemoveAt中间", () =>
			{
				baseline.Clear();
				baseline.AddRange(structuralBaselineValues);
			}, () =>
			{
				builtIn.Clear();
				builtIn.AddRange(structuralBuiltInValues);
			}, () =>
			{
				for (int i = 0; i < STRUCTURAL_OPERATION_COUNT; ++i)
				{
					baseline.RemoveAt(baseline.Count >> 1);
				}
				return baseline.Count;
			}, () =>
			{
				for (int i = 0; i < STRUCTURAL_OPERATION_COUNT; ++i)
				{
					builtIn.RemoveAt(builtIn.Count >> 1);
				}
				return builtIn.Count;
			});
			baseline.Clear();
			builtIn.Clear();
			baseline.AddRange(baselineValues);
			builtIn.AddRange(builtInValues);
			EasyECSParityIntData_ECSList resizeBaseline = null;
			Int_ECSList resizeBuiltIn = null;
			try
			{
				runCase("Int Forced Resize 32768->65536", () =>
				{
					resizeBaseline?.Dispose();
					resizeBaseline = new EasyECSParityIntData_ECSList(32768);
					resizeBaseline.AddRange(baselineValues, 0, 32768);
				}, () =>
				{
					resizeBuiltIn?.Dispose();
					resizeBuiltIn = new Int_ECSList(32768);
					resizeBuiltIn.AddRange(builtInValues, 0, 32768);
				}, () =>
				{
					resizeBaseline.EnsureCapacity(65536);
					return resizeBaseline.Capacity;
				}, () =>
				{
					resizeBuiltIn.EnsureCapacity(65536);
					return resizeBuiltIn.Capacity;
				}, MICRO_CASE_REPEAT_COUNT);
			}
			finally
			{
				resizeBaseline?.Dispose();
				resizeBuiltIn?.Dispose();
			}
			runCase("Int CopyTo全量", null, null, () =>
			{
				baseline.CopyTo(baselineCopyDestination, 0);
				return baselineCopyDestination[ENTITY_COUNT - 1].mValue;
			}, () =>
			{
				builtIn.CopyTo(builtInCopyDestination, 0);
				return builtInCopyDestination[ENTITY_COUNT - 1];
			});
			runCase("Int Contains末尾", null, null, () =>
			{
				long sum = 0;
				for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
				{
					sum += baselineSearch.ContainsByValue(ENTITY_COUNT - 1) ? 1 : 0;
				}
				return sum;
			}, () =>
			{
				long sum = 0;
				for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
				{
					sum += builtInSearch.Contains(ENTITY_COUNT - 1) ? 1 : 0;
				}
				return sum;
			});
			runCase("Int BinarySearch", null, null, () =>
			{
				long sum = 0;
				for (int i = 0; i < BINARY_SEARCH_REPEAT_COUNT; ++i)
				{
					sum += baselineSearch.BinarySearchByValue((i * 7919) % ENTITY_COUNT);
				}
				return sum;
			}, () =>
			{
				long sum = 0;
				for (int i = 0; i < BINARY_SEARCH_REPEAT_COUNT; ++i)
				{
					sum += builtInSearch.BinarySearch((i * 7919) % ENTITY_COUNT);
				}
				return sum;
			});
			EasyECSParityIntData[] descendingBaseline = new EasyECSParityIntData[STRUCTURAL_COUNT];
			int[] descendingBuiltIn = new int[STRUCTURAL_COUNT];
			for (int i = 0; i < STRUCTURAL_COUNT; ++i)
			{
				int value = STRUCTURAL_COUNT - i;
				descendingBaseline[i] = new EasyECSParityIntData(value);
				descendingBuiltIn[i] = value;
			}
			runCase("Int Sort", () =>
			{
				baseline.Clear();
				baseline.AddRange(descendingBaseline);
			}, () =>
			{
				builtIn.Clear();
				builtIn.AddRange(descendingBuiltIn);
			}, () =>
			{
				baseline.SortByValue();
				return baseline.Get(0).mValue;
			}, () =>
			{
				builtIn.Sort();
				return builtIn.Get(0);
			});
		}
		finally
		{
			baseline.Dispose();
			builtIn.Dispose();
			baselineSearch.Dispose();
			builtInSearch.Dispose();
		}
	}
	private static void runVector2ListBenchmark()
	{
		EasyECSParityUVData[] baselineValues = new EasyECSParityUVData[ENTITY_COUNT];
		Vector2[] builtInValues = new Vector2[ENTITY_COUNT];
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			Vector2 uv = new Vector2(i * 0.0001f, i * 0.0002f);
			baselineValues[i] = new EasyECSParityUVData(uv);
			builtInValues[i] = uv;
		}
		EasyECSParityUVData_ECSList baseline = new EasyECSParityUVData_ECSList(ENTITY_COUNT);
		Vector2_ECSList builtIn = new Vector2_ECSList(ENTITY_COUNT);
		try
		{
			baseline.AddRange(baselineValues);
			builtIn.AddRange(builtInValues);
			runCase("Vector2 AddRange数组", () => baseline.Clear(), () => builtIn.Clear(), () =>
			{
				baseline.AddRange(baselineValues);
				return baseline.Count;
			}, () =>
			{
				builtIn.AddRange(builtInValues);
				return builtIn.Count;
			});
			runCase("Vector2 Get读取", null, null, () =>
			{
				double sum = 0.0;
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					EasyECSParityUVData uv = baseline.Get(i);
					sum += uv.x + uv.y;
				}
				return (long)sum;
			}, () =>
			{
				double sum = 0.0;
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					Vector2 uv = builtIn.Get(i);
					sum += uv.x + uv.y;
				}
				return (long)sum;
			});
			runCase("Vector2 Direct双float Column读写", null, null, () =>
			{
				double sum = 0.0;
				var x = baseline.getXColumn();
				var y = baseline.getYColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					x[i] += 0.001f;
					y[i] += 0.002f;
					sum += x[i] + y[i];
				}
				return (long)sum;
			}, () =>
			{
				double sum = 0.0;
				var x = builtIn.getXColumn();
				var y = builtIn.getYColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					x[i] += 0.001f;
					y[i] += 0.002f;
					sum += x[i] + y[i];
				}
				return (long)sum;
			});
			runCase("Vector2 Direct仅X Column", null, null, () =>
			{
				double sum = 0.0; var x = baseline.getXColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i) { x[i] += 0.001f; sum += x[i]; }
				return (long)sum;
			}, () =>
			{
				double sum = 0.0; var x = builtIn.getXColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i) { x[i] += 0.001f; sum += x[i]; }
				return (long)sum;
			});
		}
		finally
		{
			baseline.Dispose();
			builtIn.Dispose();
		}
	}
	private static void runIntIndependentBaselineDiagnostics()
	{
		EasyECSParityIntData[] values = new EasyECSParityIntData[ENTITY_COUNT];
		EasyECSParityIntData[] structuralValues = new EasyECSParityIntData[STRUCTURAL_COUNT];
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			values[i] = new EasyECSParityIntData(i);
		}
		for (int i = 0; i < STRUCTURAL_COUNT; ++i)
		{
			structuralValues[i] = values[i];
		}
		EasyECSParityIntData_ECSList first = new EasyECSParityIntData_ECSList(ENTITY_COUNT);
		EasyECSParityIntData_ECSList second = new EasyECSParityIntData_ECSList(ENTITY_COUNT);
		try
		{
			first.AddRange(values);
			second.AddRange(values);
			runDiagnosticCase("Int SelfControl Get IndependentBaseline", null, null, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					sum += first.Get(i).mValue;
				}
				return sum;
			}, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i)
				{
					sum += second.Get(i).mValue;
				}
				return sum;
			});
			runDiagnosticCase("Int SelfControl AddRange自身 IndependentBaseline", () =>
			{
				first.Clear();
				first.AddRange(structuralValues);
			}, () =>
			{
				second.Clear();
				second.AddRange(structuralValues);
			}, () =>
			{
				first.AddRange(first);
				return first.Count;
			}, () =>
			{
				second.AddRange(second);
				return second.Count;
			}, MICRO_CASE_REPEAT_COUNT);
			runDiagnosticCase("Int SelfControl Insert IndependentBaseline", () =>
			{
				first.Clear();
				first.AddRange(structuralValues);
			}, () =>
			{
				second.Clear();
				second.AddRange(structuralValues);
			}, () =>
			{
				for (int i = 0; i < STRUCTURAL_OPERATION_COUNT; ++i)
				{
					first.Insert(first.Count >> 1, new EasyECSParityIntData(i));
				}
				return first.Count;
			}, () =>
			{
				for (int i = 0; i < STRUCTURAL_OPERATION_COUNT; ++i)
				{
					second.Insert(second.Count >> 1, new EasyECSParityIntData(i));
				}
				return second.Count;
			});
		}
		finally
		{
			first.Dispose();
			second.Dispose();
		}
		EasyECSParityIntData_ECSList resizeFirst = null;
		EasyECSParityIntData_ECSList resizeSecond = null;
		try
		{
			runDiagnosticCase("Int SelfControl Resize IndependentBaseline", () =>
			{
				resizeFirst?.Dispose();
				resizeFirst = new EasyECSParityIntData_ECSList(32768);
				resizeFirst.AddRange(values, 0, 32768);
			}, () =>
			{
				resizeSecond?.Dispose();
				resizeSecond = new EasyECSParityIntData_ECSList(32768);
				resizeSecond.AddRange(values, 0, 32768);
			}, () =>
			{
				resizeFirst.EnsureCapacity(65536);
				return resizeFirst.Capacity;
			}, () =>
			{
				resizeSecond.EnsureCapacity(65536);
				return resizeSecond.Capacity;
			}, MICRO_CASE_REPEAT_COUNT);
		}
		finally
		{
			resizeFirst?.Dispose();
			resizeSecond?.Dispose();
		}
	}
	private static void runBoolListBenchmark()
	{
		EasyECSParityBoolData_ECSList baseline = new EasyECSParityBoolData_ECSList(ENTITY_COUNT);
		Bool_ECSList builtIn = new Bool_ECSList(ENTITY_COUNT);
		try
		{
			for (int i = 0; i < ENTITY_COUNT; ++i)
			{
				bool value = (i & 1) == 0;
				baseline.Add(new EasyECSParityBoolData(value));
				builtIn.Add(value);
			}
			runCase("Bool Get读取", null, null, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i) sum += baseline.Get(i).mValue ? 1 : 0;
				return sum;
			}, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i) sum += builtIn.Get(i) ? 1 : 0;
				return sum;
			});
			runCase("Bool Direct Column", null, null, () =>
			{
				long sum = 0;
				var column = baseline.getValueColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i) { column[i] = !column[i]; sum += column[i] ? 1 : 0; }
				return sum;
			}, () =>
			{
				long sum = 0;
				var column = builtIn.getValueColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i) { column[i] = !column[i]; sum += column[i] ? 1 : 0; }
				return sum;
			});
		}
		finally
		{
			baseline.Dispose();
			builtIn.Dispose();
		}
	}
	private static void runVector2IntListBenchmark()
	{
		EasyECSParityRangeData[] baselineValues = new EasyECSParityRangeData[ENTITY_COUNT];
		Vector2Int[] builtInValues = new Vector2Int[ENTITY_COUNT];
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			baselineValues[i] = new EasyECSParityRangeData(i, i + 8);
			builtInValues[i] = new Vector2Int(i, i + 8);
		}
		EasyECSParityRangeData_ECSList baseline = new EasyECSParityRangeData_ECSList(ENTITY_COUNT);
		Vector2Int_ECSList builtIn = new Vector2Int_ECSList(ENTITY_COUNT);
		try
		{
			baseline.AddRange(baselineValues);
			builtIn.AddRange(builtInValues);
			runCase("Vector2Int Get读取(双int SoA)", null, null, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i) { EasyECSParityRangeData value = baseline.Get(i); sum += value.mStart + value.mEnd; }
				return sum;
			}, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i) { Vector2Int value = builtIn.Get(i); sum += value.x + value.y; }
				return sum;
			});
			runCase("Vector2Int Direct双Column读写", null, null, () =>
			{
				long sum = 0;
				var start = baseline.getStartColumn();
				var end = baseline.getEndColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i) { start[i] += 1; end[i] += 2; sum += start[i] + end[i]; }
				return sum;
			}, () =>
			{
				long sum = 0;
				var x = builtIn.getXColumn();
				var y = builtIn.getYColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i) { x[i] += 1; y[i] += 2; sum += x[i] + y[i]; }
				return sum;
			});
			runCase("Vector2Int AddRange数组", () => baseline.Clear(), () => builtIn.Clear(), () => { baseline.AddRange(baselineValues); return baseline.Count; }, () => { builtIn.AddRange(builtInValues); return builtIn.Count; });
		}
		finally
		{
			baseline.Dispose();
			builtIn.Dispose();
		}
	}
	private static void runColor32ListBenchmark()
	{
		EasyECSParityColor32Data[] baselineValues = new EasyECSParityColor32Data[ENTITY_COUNT];
		Color32[] builtInValues = new Color32[ENTITY_COUNT];
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			Color32 color = new Color32((byte)i, (byte)(i >> 1), (byte)(i >> 2), 255);
			baselineValues[i] = new EasyECSParityColor32Data(color);
			builtInValues[i] = color;
		}
		EasyECSParityColor32Data_ECSList baseline = new EasyECSParityColor32Data_ECSList(ENTITY_COUNT);
		Color32_ECSList builtIn = new Color32_ECSList(ENTITY_COUNT);
		try
		{
			baseline.AddRange(baselineValues);
			builtIn.AddRange(builtInValues);
			runCase("Color32 AddRange数组", () => baseline.Clear(), () => builtIn.Clear(), () =>
			{
				baseline.AddRange(baselineValues);
				return baseline.Count;
			}, () =>
			{
				builtIn.AddRange(builtInValues);
				return builtIn.Count;
			});
			runCase("Color32 Get读取", null, null, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i) { EasyECSParityColor32Data c = baseline.Get(i); sum += c.r + c.g + c.b + c.a; }
				return sum;
			}, () =>
			{
				long sum = 0;
				for (int i = 0; i < ENTITY_COUNT; ++i) { Color32 c = builtIn.Get(i); sum += c.r + c.g + c.b + c.a; }
				return sum;
			});
			runCase("Color32 Direct四byte Column", null, null, () =>
			{
				long sum = 0; var r = baseline.getRColumn(); var a = baseline.getAColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i) { a[i] = 254; sum += r[i]; }
				return sum;
			}, () =>
			{
				long sum = 0; var r = builtIn.getRColumn(); var a = builtIn.getAColumn();
				for (int i = 0; i < ENTITY_COUNT; ++i) { a[i] = 254; sum += r[i]; }
				return sum;
			});
		}
		finally
		{
			baseline.Dispose();
			builtIn.Dispose();
		}
	}

	private static void runIntDictionaryBenchmark()
	{
		EasyECSParityIntData_ECSDictionary<int> baseline = new EasyECSParityIntData_ECSDictionary<int>(DICTIONARY_COUNT);
		Int_ECSDictionary<int> builtIn = new Int_ECSDictionary<int>(DICTIONARY_COUNT);
		int[] keys = new int[DICTIONARY_OPERATION_COUNT];
		try
		{
			for (int i = 0; i < DICTIONARY_COUNT; ++i)
			{
				baseline.Add(i, new EasyECSParityIntData(i));
				builtIn.Add(i, i);
			}
			for (int i = 0; i < keys.Length; ++i)
			{
				keys[i] = (i * 7919) % DICTIONARY_COUNT;
			}
			runCase("Int Dictionary Add无Resize", () => baseline.Clear(), () => builtIn.Clear(), () =>
			{
				for (int i = 0; i < DICTIONARY_COUNT; ++i)
				{
					baseline.Add(i, new EasyECSParityIntData(i));
				}
				return baseline.Count;
			}, () =>
			{
				for (int i = 0; i < DICTIONARY_COUNT; ++i)
				{
					builtIn.Add(i, i);
				}
				return builtIn.Count;
			});
			runCase("Int Dictionary TryGetValue", null, null, () =>
			{
				long sum = 0;
				for (int i = 0; i < keys.Length; ++i)
				{
					baseline.TryGetValueByValue(keys[i], out int value);
					sum += value;
				}
				return sum;
			}, () =>
			{
				long sum = 0;
				for (int i = 0; i < keys.Length; ++i)
				{
					builtIn.TryGetValue(keys[i], out int value);
					sum += value;
				}
				return sum;
			});
			runCase("Int Dictionary SetValue", null, null, () =>
			{
				for (int i = 0; i < keys.Length; ++i)
				{
					baseline.SetValueByValue(keys[i], i);
				}
				return baseline.Count;
			}, () =>
			{
				for (int i = 0; i < keys.Length; ++i)
				{
					builtIn.SetValue(keys[i], i);
				}
				return builtIn.Count;
			});
			runCase("Int Dictionary GetIndex+Direct", null, null, () =>
			{
				long sum = 0;
				var column = baseline.getValueColumn();
				for (int i = 0; i < keys.Length; ++i)
				{
					int index = baseline.GetIndex(keys[i]);
					column[index] += 1;
					sum += column[index];
				}
				return sum;
			}, () =>
			{
				long sum = 0;
				var column = builtIn.getValueColumn();
				for (int i = 0; i < keys.Length; ++i)
				{
					int index = builtIn.GetIndex(keys[i]);
					column[index] += 1;
					sum += column[index];
				}
				return sum;
			});
		}
		finally
		{
			baseline.Dispose();
			builtIn.Dispose();
		}
	}
	private static void runCase(string name, Action prepareBaseline, Action prepareBuiltIn, Func<long> baselineAction, Func<long> builtInAction, int measureRepeatCount = 1)
	{
		for (int i = 0; i < WARMUP_COUNT; ++i)
		{
			warmupRepeated(prepareBaseline, baselineAction, measureRepeatCount);
			warmupRepeated(prepareBuiltIn, builtInAction, measureRepeatCount);
		}
		double[] baselineSamples = new double[SAMPLE_COUNT];
		double[] builtInSamples = new double[SAMPLE_COUNT];
		double[] pairedRatioSamples = new double[SAMPLE_COUNT];
		for (int i = 0; i < SAMPLE_COUNT; ++i)
		{
			if ((i & 1) == 0)
			{
				baselineSamples[i] = measureRepeated(prepareBaseline, baselineAction, measureRepeatCount);
				builtInSamples[i] = measureRepeated(prepareBuiltIn, builtInAction, measureRepeatCount);
			}
			else
			{
				builtInSamples[i] = measureRepeated(prepareBuiltIn, builtInAction, measureRepeatCount);
				baselineSamples[i] = measureRepeated(prepareBaseline, baselineAction, measureRepeatCount);
			}
			pairedRatioSamples[i] = builtInSamples[i] / baselineSamples[i];
		}
		Array.Sort(baselineSamples);
		Array.Sort(builtInSamples);
		Array.Sort(pairedRatioSamples);
		double baselineMedian = baselineSamples[SAMPLE_COUNT >> 1];
		double builtInMedian = builtInSamples[SAMPLE_COUNT >> 1];
		double medianRatio = builtInMedian / baselineMedian;
		double pairedMedianRatio = pairedRatioSamples[SAMPLE_COUNT >> 1];
		bool releasePass = pairedMedianRatio <= 1.0;
		bool noisePass = pairedMedianRatio <= 1.03;
		if (!releasePass)
		{
			++mReleaseRegressionCount;
		}
		Debug.Log("\n================ " + name + " ================\n" +
			"手写[ECS]      Median:" + baselineMedian.ToString("F3").PadLeft(9) + " ms\n" +
			"BuiltIn       Median:" + builtInMedian.ToString("F3").PadLeft(9) + " ms\n" +
			"MedianRatio          : " + medianRatio.ToString("F3") + "x\n" +
			"PairedMedianRatio    : " + pairedMedianRatio.ToString("F3") + "x\n" +
			"MeasureRepeat        : " + measureRepeatCount + "\n" +
			"Release Gate(<=1.00x): " + (releasePass ? "PASS" : "FAIL") + "\n" +
			"Noise Gate(<=1.03x)  : " + (noisePass ? "PASS" : "FAIL") + "\n" +
			"==================================================");
	}
	private static void runDiagnosticCase(string name, Action prepareFirst, Action prepareSecond, Func<long> firstAction, Func<long> secondAction, int measureRepeatCount = 1)
	{
		for (int i = 0; i < WARMUP_COUNT; ++i)
		{
			warmupRepeated(prepareFirst, firstAction, measureRepeatCount);
			warmupRepeated(prepareSecond, secondAction, measureRepeatCount);
		}
		double[] firstSamples = new double[SAMPLE_COUNT];
		double[] secondSamples = new double[SAMPLE_COUNT];
		double[] pairedRatioSamples = new double[SAMPLE_COUNT];
		for (int i = 0; i < SAMPLE_COUNT; ++i)
		{
			if ((i & 1) == 0)
			{
				firstSamples[i] = measureRepeated(prepareFirst, firstAction, measureRepeatCount);
				secondSamples[i] = measureRepeated(prepareSecond, secondAction, measureRepeatCount);
			}
			else
			{
				secondSamples[i] = measureRepeated(prepareSecond, secondAction, measureRepeatCount);
				firstSamples[i] = measureRepeated(prepareFirst, firstAction, measureRepeatCount);
			}
			pairedRatioSamples[i] = secondSamples[i] / firstSamples[i];
		}
		Array.Sort(firstSamples);
		Array.Sort(secondSamples);
		Array.Sort(pairedRatioSamples);
		double firstMedian = firstSamples[SAMPLE_COUNT >> 1];
		double secondMedian = secondSamples[SAMPLE_COUNT >> 1];
		double medianRatio = secondMedian / firstMedian;
		double pairedMedianRatio = pairedRatioSamples[SAMPLE_COUNT >> 1];
		Debug.Log("\n================ " + name + " ================\n" +
			"First          Median:" + firstMedian.ToString("F3").PadLeft(9) + " ms\n" +
			"Second         Median:" + secondMedian.ToString("F3").PadLeft(9) + " ms\n" +
			"MedianRatio          : " + medianRatio.ToString("F3") + "x\n" +
			"PairedMedianRatio    : " + pairedMedianRatio.ToString("F3") + "x\n" +
			"MeasureRepeat        : " + measureRepeatCount + "\n" +
			"Diagnostic only      : NO RELEASE GATE\n" +
			"==================================================");
	}
	private static void warmupRepeated(Action prepare, Func<long> action, int repeatCount)
	{
		for (int i = 0; i < repeatCount; ++i)
		{
			prepare?.Invoke();
			mResultSink ^= action();
		}
	}
	private static double measureRepeated(Action prepare, Func<long> action, int repeatCount)
	{
		long totalTicks = 0;
		for (int i = 0; i < repeatCount; ++i)
		{
			prepare?.Invoke();
			long start = Stopwatch.GetTimestamp();
			long result = action();
			long end = Stopwatch.GetTimestamp();
			totalTicks += end - start;
			mResultSink ^= result;
		}
		return totalTicks * mTickToMilliseconds / repeatCount;
	}

}
