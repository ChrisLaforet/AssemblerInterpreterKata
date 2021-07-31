using NUnit.Framework;
using static AssemblerInterpreter.AssemblerInterpreter;

namespace AssemblerInterpreter
{
	// My TDD tests for driving the project

	[TestFixture]
	public class DevelopmentTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void givenEmptyCode_whenParsed_thenReturnsEmptyInstructionList()
		{
			Assert.IsTrue(CodeParser.ParseCode("").Instructions.Count == 0);
		}

		[Test]
		public void givenEmptyCode_whenParsed_thenReturnsEmptyTargetList()
		{
			Assert.IsTrue(CodeParser.ParseCode("").Targets.Count == 0);
		}

		[Test]
		public void givenCodeWithOnlyComment_whenParsed_thenReturnsEmptyInstructionList()
		{
			Assert.IsTrue(CodeParser.ParseCode("; Comment Line").Instructions.Count == 0);
		}

		[Test]
		public void givenCodeWithOnlyLabel_whenParsed_thenReturnsTargetForLabel()
		{
			Executable executable = CodeParser.ParseCode("label:");
			Assert.IsTrue(executable.Targets.Count == 1);
			Assert.AreEqual("label", executable.Targets[0].Label);
		}
	}
}