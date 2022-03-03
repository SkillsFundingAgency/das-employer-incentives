
#if !DEBUG
  [assembly: CollectionBehavior(MaxParallelThreads = 40)]
#else
using Xunit;

[assembly: CollectionBehavior(MaxParallelThreads = 40)]
#endif


