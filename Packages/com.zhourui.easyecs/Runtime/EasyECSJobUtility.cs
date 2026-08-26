using Unity.Jobs.LowLevel.Unsafe;
namespace EasyECS
{
	public static class EasyECSJobUtility
	{
		public const int JOB_COUNT_PER_WORKER = 8;
		public const int MIN_ENTITY_COUNT_PER_JOB = 256;
		public static int calculateChunkSize(int entityCount)
		{
			if (entityCount <= 0)
			{
				return 1;
			}
			int workerCount = JobsUtility.JobWorkerCount;
			if (workerCount < 1)
			{
				workerCount = 1;
			}
			int targetJobCount = workerCount * JOB_COUNT_PER_WORKER;
			int maxJobCountByEntity = entityCount / MIN_ENTITY_COUNT_PER_JOB;
			if (maxJobCountByEntity < 1)
			{
				maxJobCountByEntity = 1;
			}
			int jobCount = targetJobCount < maxJobCountByEntity ? targetJobCount : maxJobCountByEntity;
			return (entityCount + jobCount - 1) / jobCount;
		}
	}
}
