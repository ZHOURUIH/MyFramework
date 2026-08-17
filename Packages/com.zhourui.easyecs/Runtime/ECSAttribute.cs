using System;

namespace EasyECS
{
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public sealed class ECSAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public sealed class NotECSAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ECSGeneratedForAttribute : Attribute
	{
		public Type SourceType { get; }
		public ECSGeneratedForAttribute(Type sourceType)
		{
			SourceType = sourceType;
		}
	}
}