using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 5, Scope = ExecutionScope.ClassLevel)]
