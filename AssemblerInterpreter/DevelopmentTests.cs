using NUnit.Framework;
using System;
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

		[Test]
		public void givenCodeWithOnlyRtn_whenParsed_thenReturnsImmediateInstructionForRtn()
		{
			Executable executable = CodeParser.ParseCode("RTN");
			Assert.IsTrue(executable.Instructions.Count == 1);
			Assert.AreEqual("rtn", executable.Instructions[0].Opcode);
		}

		[Test]
		public void givenCodeWithOnlyEnd_whenParsed_thenReturnsImmediateInstructionForEnd()
		{
			Executable executable = CodeParser.ParseCode("END");
			Assert.IsTrue(executable.Instructions.Count == 1);
			Assert.AreEqual("end", executable.Instructions[0].Opcode);
		}

		[Test]
		public void givenCodeWithBadOpcode_whenParsed_thenThrowsException()
		{
			Assert.Throws<Exception>(() => CodeParser.ParseCode("BLAH"));
		}

		[Test]
		public void givenCodeWithOnlyJmp_whenParsed_thenReturnsUnaryInstructionForJmp()
		{
			Executable executable = CodeParser.ParseCode("JMP label");
			Assert.IsTrue(executable.Instructions.Count == 1);
			Assert.AreEqual("jmp", executable.Instructions[0].Opcode);
		}

		[Test]
		public void givenCodeWithOnlyInc_whenParsed_thenReturnsUnaryInstructionForInc()
		{
			Executable executable = CodeParser.ParseCode("INC AX");
			Assert.IsTrue(executable.Instructions.Count == 1);
			Assert.AreEqual("inc", executable.Instructions[0].Opcode);
		}
	}
}