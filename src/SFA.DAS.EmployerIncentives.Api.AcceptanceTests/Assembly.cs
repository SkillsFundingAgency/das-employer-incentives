using Xunit;

#if !DEBUG
  [assembly: CollectionBehavior(MaxParallelThreads = -1)]
#else
[assembly: CollectionBehavior(MaxParallelThreads = -1)]
#endif


