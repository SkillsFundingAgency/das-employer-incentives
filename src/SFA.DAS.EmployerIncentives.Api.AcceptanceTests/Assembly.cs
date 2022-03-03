using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !DEBUG
[assembly: Parallelize(Workers = 40, Scope = ExecutionScope.ClassLevel)]
#else
[assembly: Parallelize(Workers = 5, Scope = ExecutionScope.ClassLevel)]
#endif


