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
		public void givenCodeWithOnlyRet_whenParsed_thenReturnsImmediateInstructionForRet()
		{
			Executable executable = CodeParser.ParseCode("RET");
			Assert.IsTrue(executable.Instructions.Count == 1);
			Assert.AreEqual("ret", executable.Instructions[0].Opcode);
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

		[Test]
		public void givenCodeWithOnlyMulAnd2Registers_whenParsed_thenReturnsBinaryInstructionForMul()
		{
			Executable executable = CodeParser.ParseCode("MUL AX,BX");
			Assert.IsTrue(executable.Instructions.Count == 1);
			Assert.AreEqual("mul", executable.Instructions[0].Opcode);
			Assert.AreEqual(2, executable.Registers.Count);
		}

		[Test]
		public void givenCodeWithOnlyAddAnd1Register_whenParsed_thenReturnsBinaryInstructionForAdd()
		{
			Executable executable = CodeParser.ParseCode("ADD AX, 123");
			Assert.IsTrue(executable.Instructions.Count == 1);
			Assert.AreEqual("add", executable.Instructions[0].Opcode);
			Assert.AreEqual(1, executable.Registers.Count);
		}

		[Test]
		public void givenCodeWithOnlyMsgWithNonStringParameters_whenParsed_thenReturnsMsgInstruction()
		{
			Executable executable = CodeParser.ParseCode("msg AX, BX, CX");
			Assert.IsTrue(executable.Instructions.Count == 1);
			Assert.AreEqual("msg", executable.Instructions[0].Opcode);
			MsgInstruction msgInstruction = executable.Instructions[0] as MsgInstruction;
			Assert.AreEqual(3, msgInstruction.Parameters.Length);
		}

		[Test]
		public void givenCodeWithOnlyMsgContainingStrings_whenParsed_thenReturnsMsgInstruction()
		{
			Executable executable = CodeParser.ParseCode("msg 'Notice ', AX, ' is not equal to value of ', CX");
			Assert.IsTrue(executable.Instructions.Count == 1);
			Assert.AreEqual("msg", executable.Instructions[0].Opcode);
			MsgInstruction msgInstruction = executable.Instructions[0] as MsgInstruction;
			Assert.AreEqual(4, msgInstruction.Parameters.Length);
		}

		[Test]
		public void givenCodeWithOnlyMsgContainingConstants_whenParsed_thenThrowsException()
		{
			Assert.Throws<Exception>(() => CodeParser.ParseCode("msg AX, 'text', 123, BX"));
		}

		[Test]
		public void givenEmptyCode_whenInterpreted_thenReturnsNull()
		{
			Assert.IsNull(AssemblerInterpreter.Interpret(""));
		}

		[Test]
		public void givenCodeWithCommentOnly_whenInterpreted_thenReturnsNull()
		{
			Assert.IsNull(AssemblerInterpreter.Interpret("; ignore this"));
		}

		[Test]
		public void givenCodeWithEnd_whenInterpreted_thenReturnsEmptyStringOnCleanExit()
		{
			Assert.AreEqual("", AssemblerInterpreter.Interpret("\nend\n"));
		}

		[Test]
		public void givenCodeWithMsgAndEnd_whenInterpreted_thenReturnsMsgContent()
		{
			Assert.AreEqual("Hello World", AssemblerInterpreter.Interpret("msg 'Hello World'\nend\n"));
		}

		[Test]
		public void givenCodeWithIncOnRegisterMsgAndEnd_whenInterpreted_thenReturnsMsgContentWithRegisterValue()
		{
			Assert.AreEqual("1", AssemblerInterpreter.Interpret("inc AX\nmsg AX\nend\n"));
		}

		[Test]
		public void givenCodeWithAddOnRegister_whenInterpreted_thenReturnsMsgContent()
		{
			Assert.AreEqual("20", AssemblerInterpreter.Interpret("mov AX,15\nadd AX, 5\nmsg AX\nend\n"));

		}

		[Test]
		public void givenCodeWithDivOnRegister_whenInterpreted_thenReturnsMsgContent()
		{
			Assert.AreEqual("3", AssemblerInterpreter.Interpret("mov AX,15\n div\tAX, 5 \nmsg AX\nend\n"));
		}

		[Test]
		public void givenCodeWithCmpEqualsOnRegister_whenInterpreted_thenReturnsEqualMsgContent()
		{
			Assert.AreEqual("Equal", AssemblerInterpreter.Interpret("mov AX,15\ncmp ax, 15\nje equals\nmsg 'Not equal'\njmp exit\nequals:\nmsg 'Equal'\njmp exit\nexit:\nend\n"));
		}

		[Test]
		public void givenCodeWithCmpNotEqualsOnRegister_whenInterpreted_thenReturnsNotEqualMsgContent()
		{
			Assert.AreEqual("Not equal", AssemblerInterpreter.Interpret("mov AX,15\ncmp ax, 91\nje equals\nmsg 'Not equal'\njmp exit\nequals:\nmsg 'Equal'\njmp exit\nexit:\nend\n"));
		}
	}
}