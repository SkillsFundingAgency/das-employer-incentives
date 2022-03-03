using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 15, Scope = ExecutionScope.ClassLevel)]
