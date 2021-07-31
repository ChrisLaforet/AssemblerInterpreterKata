using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblerInterpreter
{
	// This is the code and entrypoint for the Codewars kata (Interpret)
	//
	// Unfortunately, there is no modularizing of the code for Codewars - it
	// all has to fit in one single class pasted into the solution window.
	public class AssemblerInterpreter
	{
		public static string Interpret(string input)
		{
			Processor processor = new Processor();
			return processor.Execute(CodeParser.ParseCode(input));
		}


		// Processor support --------------------------

		public class Processor
		{
			private Flags flags = new Flags();
			private Stack<int> stack = new Stack<int>();
			private int ip = 0;
			private string outputMessage = string.Empty;
			private Dictionary<string, Register> registers = new Dictionary<string, Register>();

			public string Execute(Executable program)
			{
				PrepareRegisters(program.Registers);
				return ProcessInstructions(program) ? outputMessage : null;
			}

			private void PrepareRegisters(List<RegisterMoniker> registerMonikers)
			{
				foreach (var moniker in registerMonikers)
				{
					var register = new Register(moniker.Moniker, 0);
					registers.Add(register.Moniker, register);
				}
			}

			private bool ProcessInstructions(Executable program)
			{
				bool gotEnd = false;
				while (true)
				{
					if (ip >= program.Instructions.Count)
					{
						// if IP beyond program steps - exit dirty
						break;
					}

					Instruction instruction = program.Instructions[ip];
					if (instruction.Opcode == "ret")
					{
						ip = stack.Pop();
						continue;
					}
					else if (instruction.Opcode == "end")
					{
						gotEnd = true;
						break;
					}
					else if (instruction is LabelUnaryInstruction)
					{
						if (ProcessInstruction(instruction as LabelUnaryInstruction, program.Targets))
						{
							continue;
						}
					}
					else if (instruction is RegisterUnaryInstruction)
					{
						ProcessInstruction(instruction as RegisterUnaryInstruction);

					}
					else if (instruction is BinaryInstruction)
					{
						ProcessInstruction(instruction as BinaryInstruction);
					}
					else if (instruction is MsgInstruction)
					{
						ProcessInstruction(instruction as MsgInstruction);
					}
					else
					{
						throw new Exception("Unknown instruction type provided");
					}

					++ip;
				}

				return gotEnd;
			}

			private bool ProcessInstruction(LabelUnaryInstruction instruction, List<Target> targets)
			{
				if (instruction.Opcode == "jne" && !flags.NotEqual)
				{
					return false;
				}
				else if (instruction.Opcode == "je" && !flags.Equal)
				{
					return false;
				}
				else if (instruction.Opcode == "jg" && !flags.GreaterThan)
				{
					return false;
				}
				else if (instruction.Opcode == "jge" && !flags.GreaterThanOrEqual)
				{
					return false;
				}
				else if (instruction.Opcode == "jl" && !flags.LessThan)
				{
					return false;
				}
				else if (instruction.Opcode == "jle" && !flags.LessThanOrEqual)
				{
					return false;
				}
				else if (instruction.Opcode == "call")
				{
					stack.Push(ip + 1);
				}
				JumpToLabel(instruction.Label.Name, targets);
				return true;
			}

			private void ProcessInstruction(RegisterUnaryInstruction instruction)
			{
				var register = GetRegisterFromMoniker(instruction.Register);
				switch (instruction.Opcode)
				{
					case "inc":
						register.Inc();
						break;

					case "dec":
						register.Dec();
						break;

					default:
						throw new Exception("Unknown unary instruction opcode provided");
				}
			}

			private void ProcessInstruction(BinaryInstruction instruction)
			{
				if (instruction.Opcode == "cmp")
				{
					flags.Reset();
					int sourceValue = GetValueFromOperand(instruction.Source);
					int targetValue = GetValueFromOperand(instruction.Target);
					if (sourceValue > targetValue)
					{
						flags.GreaterThan = true;
					}
					else if (sourceValue < targetValue)
					{
						flags.LessThan = true;
					}
					else
					{
						flags.Equal = true;
					}
					return;
				}

				var targetRegister = GetRegisterFromMoniker((RegisterMoniker)instruction.Target);
				if (instruction.Opcode == "mov")
				{
					targetRegister.Value = GetValueFromOperand(instruction.Source);
				}
				else if (instruction.Opcode == "add")
				{
					targetRegister.Add(GetValueFromOperand(instruction.Source));
				}
				else if (instruction.Opcode == "sub")
				{
					targetRegister.Sub(GetValueFromOperand(instruction.Source));
				}
				else if (instruction.Opcode == "mul")
				{
					targetRegister.Mul(GetValueFromOperand(instruction.Source));
				}
				else if (instruction.Opcode == "div")
				{
					targetRegister.Div(GetValueFromOperand(instruction.Source));
				}
				else
				{
					throw new Exception("Unknown binary instruction opcode provided");
				}
			}

			private void ProcessInstruction(MsgInstruction instruction)
			{
				var builder = new StringBuilder();
				foreach (var param in instruction.Parameters)
				{
					if (param is ConstantText)
					{
						builder.Append(((ConstantText)param).Text);
					}
					else if (param is RegisterMoniker)
					{
						var register = GetRegisterFromMoniker((RegisterMoniker)param);
						builder.Append(register.Value.ToString());
					}
					else
					{
						throw new Exception("Invalid parameter type found for msg");
					}
				}

				outputMessage = builder.ToString();
			}

			private Register GetRegisterFromMoniker(RegisterMoniker moniker)
			{
				return registers[moniker.Moniker];
			}

			private int GetValueFromOperand(IOperand operand)
			{
				if (operand is RegisterMoniker)
				{
					var register = GetRegisterFromMoniker((RegisterMoniker)operand);
					return register.Value;
				}
				else if (operand is Constant)
				{
					return ((Constant)operand).Value;
				}
				throw new Exception("Invalid operand for value extraction");
			}

			private void JumpToLabel(string label, List<Target> targets)
			{
				foreach (var target in targets)
				{
					if (target.Label == label)
					{
						ip = target.Offset;
						return;
					}
				}
				throw new Exception("Found jump to a non existent label " + label);
			}
		}

		private class Flags
		{
			public bool Equal { get; set; }
			public bool LessThan { get; set; }
			public bool GreaterThan { get; set; }

			public void Reset()
			{
				Equal = false;
				LessThan = false;
				GreaterThan = false;
			}

			public bool GreaterThanOrEqual { 
				get
				{
					return Equal || GreaterThan;
				}
			}

			public bool LessThanOrEqual
			{
				get
				{
					return Equal || LessThan;
				}
			}

			public bool NotEqual
			{
				get
				{
					return !Equal;
				}
			}
		}

		private class Register
		{
			public Register(string moniker, int value)
			{
				Moniker = moniker;
				Value = value;
			}

			public string Moniker { get; private set; }
			public int Value { get; set; }

			public void Inc() => Value = Value + 1;
			public void Dec() => Value = Value - 1;
			public void Add(int addend) => Value = Value + addend;
			public void Add(Register register) => Add(register.Value);
			public void Sub(int subtrahend) => Value = Value - subtrahend;
			public void Sub(Register register) => Sub(register.Value);
			public void Mul(int multiplier) => Value = Value * multiplier;
			public void Mul(Register register) => Mul(register.Value);
			public void Div(int divisor) => Value = Value / divisor;
			public void Div(Register register) => Div(register.Value);
			public void Mov(int value) => Value = value;
			public void Mov(Register register) => Mov(register.Value);
			public void Cmp(int value, Flags flags)
			{
				flags.Reset();
				if (Value < value)
					flags.LessThan = true;
				else if (Value > value)
					flags.GreaterThan = true;
				else
					flags.Equal = true;
			}
			public void Cmp(Register register, Flags flags) => Cmp(register.Value, flags);
		}


		// Parser support -----------------------------

		public class CodeParser
		{
			public static Executable ParseCode(String code)
			{
				CodeParser parser = new CodeParser();
				return parser.Parse(code);
			}

			private CodeParser()
			{
			}

			private readonly List<Instruction> instructions = new List<Instruction>();
			private readonly List<Target> targets = new List<Target>();
			private readonly List<RegisterMoniker> registers = new List<RegisterMoniker>();

			public Executable Parse(String code)
			{
				Tokenize(SplitByDelimiter(code));
				return new Executable(instructions, targets, registers);
			}

			private void Tokenize(string[] lines)
			{
				foreach (var line in lines)
				{
					var toParse = line.Trim();
					if (toParse.Length == 0 || toParse.StartsWith(';'))
					{
						continue;
					}
					if (toParse.EndsWith(":"))
					{
						var label = toParse.Substring(0, toParse.Length - 1);
						targets.Add(new Target(label, instructions.Count));
					} else
					{
						ParseLine(toParse);
					}
				}
			}

			private void AddRegister(RegisterMoniker register)
			{
				if (!registers.Exists(current => current.Moniker == register.Moniker))
				{
					registers.Add(register);
				}
			}

			private void AddInstruction(ImmediateInstruction instruction)
			{
				instructions.Add(instruction);
			}

			private void AddInstruction(MsgInstruction instruction)
			{
				instructions.Add(instruction);
				foreach (IOperand operand in instruction.Parameters)
				{
					if (operand is RegisterMoniker)
					{
						AddRegister(operand as RegisterMoniker);
					}
				}
			}

			private void AddInstruction(UnaryInstruction instruction)
			{
				instructions.Add(instruction);
				if (instruction is RegisterUnaryInstruction)
				{
					var unary = instruction as RegisterUnaryInstruction;
					AddRegister(unary.Register);
				}
			}

			private void AddInstruction(BinaryInstruction instruction)
			{
				instructions.Add(instruction);
				if (instruction.Source is RegisterMoniker)
				{
					AddRegister(instruction.Source as RegisterMoniker);
				}
				if (instruction.Target is RegisterMoniker)
				{
					AddRegister(instruction.Target as RegisterMoniker);
				}
			}

			private readonly List<string> ImmediateOpcodes = new List<string>(new string[] { "ret", "end" });
			private readonly List<string> RegisterUnaryOpcodes = new List<string>(new string[] { "inc", "dec" });
			private readonly List<string> LabelUnaryOpcodes = new List<string>(new string[] { "jmp", "jne", "je", "jge", "jg", "jle", "jl", "call" });
			private readonly List<string> RegisterBinaryOpcodes = new List<string>(new string[] { "mov", "add", "sub", "mul", "div" });
			private readonly List<string> AnyBinaryOpcodes = new List<string>(new string[] { "cmp" });
			private readonly List<string> MessageOpcodes = new List<string>(new string[] { "msg" });


			private void ParseLine(string line)
			{
				List<string> tokens = Tokenize(line.Clone() as string);
				var opcode = tokens[0];
				if (ImmediateOpcodes.Contains(opcode))
				{
					AddInstruction(new ImmediateInstruction(opcode));
					return;
				}
				else if (MessageOpcodes.Contains(opcode))
				{
					if (tokens.Count >= 2)
					{
						AddInstruction(ParseMsgInstructionFrom(opcode, line));
						return;
					}
				}
				else if (tokens.Count == 2)
				{
					if (RegisterUnaryOpcodes.Contains(opcode))
					{
						AddInstruction(new RegisterUnaryInstruction(opcode, new RegisterMoniker(tokens[1])));
						return;
					}
					else if (LabelUnaryOpcodes.Contains(opcode))
					{
						AddInstruction(new LabelUnaryInstruction(opcode, new Label(tokens[1])));
						return;
					}
				}
				else if (tokens.Count == 3)
				{
					IOperand target = CreateOperand(tokens[1]);
					IOperand source = CreateOperand(tokens[2]);
					if (RegisterBinaryOpcodes.Contains(opcode))
					{
						if (target is Constant)
						{
							throw new Exception("Invalid target type - cannot be constant");
						}
						AddInstruction(new BinaryInstruction(opcode, target, source));
						return;
					}
					else if (AnyBinaryOpcodes.Contains(opcode))
					{
						AddInstruction(new BinaryInstruction(opcode, target, source));
						return;
					}
				}
				else if (tokens.Count > 3)
				{
					throw new Exception("Invalid argument count");
				}
				throw new Exception("Invalid opcode " + opcode + " found");
			}

			private List<string> Tokenize(string line)
			{
				string[] parts = TokenizeAndStandardize(line);
				List<string> tokens = new List<string>();
				tokens.Add(parts[0].Trim());
				if (parts.Length == 2)
				{
					if (parts[1].Contains(','))
					{
						string[] subParts = parts[1].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						if (subParts.Length != 2)
						{
							throw new Exception("Invalid argument count");
						}
						tokens.Add(subParts[0].Trim());
						tokens.Add(subParts[1].Trim());
					}
					else
					{
						tokens.Add(parts[1].Trim());
					}
				}
				else if (parts.Length > 2)
				{
					for (int index = 1; index < parts.Length; index++)
					{
						tokens.Add(parts[index]);
					}
				}

				return tokens;
			}

			private string[] TokenizeAndStandardize(string line)
			{
				line = line.ToLower();
				string[] parts = line.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length <= 2)
				{
					return parts;
				}
				else if (parts.Length == 3)
				{
					if ((parts[1].EndsWith(',') && !parts[2].Contains(',')) ||
						parts[2].StartsWith(','))
					{
						List<string> tokens = new List<string>();
						tokens.Add(parts[0]);
						tokens.Add(parts[1] + parts[2]);
						return tokens.ToArray();
					}
				}
				else if (parts.Length == 4 && parts[2] == ",")
				{
					if (!parts[1].Contains(',') && !parts[3].Contains(','))
					{
						List<string> tokens = new List<string>();
						tokens.Add(parts[0]);
						tokens.Add(parts[1] + parts[2] + parts[3]);
						return tokens.ToArray();
					}
				}
				return parts;
			}

			private MsgInstruction ParseMsgInstructionFrom(string opcode, string rawLine)
			{
				rawLine = rawLine.Trim();
				var paramLine = rawLine.Substring(opcode.Length).Trim();
				List<string> parameters = SplitMsgParamsFrom(paramLine);
				List<IOperand> operands = new List<IOperand>();
				foreach (string parameter in parameters)
				{
					if (parameter.StartsWith('\''))
					{
						operands.Add(new ConstantText(parameter.Substring(1, parameter.Length - 2)));
					}
					else
					{
						IOperand operand = CreateOperand(parameter);
						if (operand is RegisterMoniker)
						{
							operands.Add(operand);
						}
						else
						{
							throw new Exception("Invalid argument type - only text strings and registers are permitted msg parameter types");
						}
					}
				}

				return new MsgInstruction(opcode, operands[0], operands.GetRange(1, operands.Count - 1).ToArray());
			}

			private List<string> SplitMsgParamsFrom(string paramLine)
			{
				List<string> parameters = new List<string>();
				int startOffset = 0;
				int offset = 0;
				bool inQuote = false;
				while (true)
				{
					if (offset == paramLine.Length)
					{
						if (startOffset >= offset)
						{
							throw new Exception("Malformed msg parameters ending with empty parameter");
						}
						var text = paramLine.Substring(startOffset, offset - startOffset).Trim();
						if (text.Length == 0)
						{
							throw new Exception("Malformed msg parameter - zero length");
						}
						parameters.Add(text);
						break;
					}
					else if (paramLine[offset] == '\'')
					{
						inQuote = !inQuote;
					}
					else if (!inQuote && paramLine[offset] == ',')
					{
						var text = paramLine.Substring(startOffset, offset - startOffset).Trim();
						if (text.Length == 0)
						{
							throw new Exception("Malformed msg parameter - zero length");
						}
						parameters.Add(text);
						startOffset = offset + 1;
					}
					++offset;
				}

				return parameters;
			}

			private IOperand CreateOperand(string token)
			{
				int value;
				if (int.TryParse(token, out value))
				{
					return new Constant(value);
				}
				return new RegisterMoniker(token);
			}

			private string[] SplitByDelimiter(string code)
			{
				return code.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			}
		}

		public class Executable
		{
			public Executable(List<Instruction> instructions, List<Target> targets, List<RegisterMoniker> registers)
			{
				Instructions = instructions;
				Targets = targets;
				Registers = registers;
			}

			public string Name { get; private set; }
			public List<Instruction> Instructions { get; private set; }
			public List<Target> Targets { get; private set; }
			public List<RegisterMoniker> Registers { get; private set; }
		}

		public class Instruction
		{
			public string Opcode { get; protected set; }
		}

		public class ImmediateInstruction : Instruction
		{
			public ImmediateInstruction(string opcode) => Opcode = opcode;
		}

		public interface IOperand { }

		public class RegisterMoniker : IOperand
		{
			public RegisterMoniker(string moniker) => Moniker = moniker.ToLower();

			public string Moniker { get; private set; }
		}

		public class Label
		{
			public Label(string name) => Name = name;

			public string Name { get; private set; }
		}

		public class Constant : IOperand
		{
			public Constant(int value) => Value = value;

			public int Value { get; private set; }
		}

		public class ConstantText : IOperand
		{
			public ConstantText(string text) => Text = text;

			public string Text { get; private set; }
		}

		public class UnaryInstruction : Instruction
		{
			protected UnaryInstruction(string opcode) => Opcode = opcode;
		}

		public class RegisterUnaryInstruction : UnaryInstruction
		{
			public RegisterUnaryInstruction(string opcode, RegisterMoniker register) 
				: base(opcode) => Register = register;

			public RegisterMoniker Register { get; private set; }
		}

		public class LabelUnaryInstruction : UnaryInstruction
		{
			public LabelUnaryInstruction(string opcode, Label label)
				: base(opcode) => Label = label;

			public Label Label { get; private set; }
		}

		public class BinaryInstruction : Instruction
		{
			public BinaryInstruction(string opcode, IOperand target, IOperand source) 
			{
				Opcode = opcode;
				Target = target;
				Source = source;
			}

			public IOperand Target { get; private set; }
			public IOperand Source { get; private set; }
		}

		public class MsgInstruction : Instruction
		{
			public MsgInstruction(string opcode, IOperand param0, params IOperand[] extraParams)
			{
				Opcode = opcode;
				List<IOperand> parameters = new List<IOperand>();
				parameters.Add(param0);
				foreach (IOperand operand in extraParams)
				{
					parameters.Add(operand);
				}
				Parameters = parameters.ToArray();
			}

			public IOperand[] Parameters { get; private set; }
		}

		public class Target
		{
			public Target(string label, int offset)
			{
				Label = label;
				Offset = offset;
			}

			public string Label { get; private set; }
			public int Offset { get; private set; }
		}
	}
}
