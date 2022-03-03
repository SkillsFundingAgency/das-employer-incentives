using Xunit;

#if !DEBUG
  [assembly: CollectionBehavior(MaxParallelThreads = 40)]
#else
[assembly: CollectionBehavior(MaxParallelThreads = 40)]
#endif


