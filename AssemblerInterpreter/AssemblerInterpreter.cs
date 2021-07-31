using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblerInterpreter
{
	// This is the code and entrypoint for the Codewars kata (Interpret)
	public class AssemblerInterpreter
	{
		public static string Interpret(string input)
		{
			// Your code here!
			return null;
		}

		// Processor support --------------------------


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

			public Executable Parse(String code)
			{
				Tokenize(SplitByDelimiter(code));
				return new Executable(instructions, targets);
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

			private readonly List<string> ImmediateOpcodes = new List<string>(new string[] { "ret", "end" });
			private readonly List<string> RegisterUnaryOpcodes = new List<string>(new string[] { "inc", "dec" });
			private readonly List<string> LabelUnaryOpcodes = new List<string>(new string[] { "jmp", "jne", "je", "jge", "jg", "jle", "jl", "call" });
			private readonly List<string> RegisterBinaryOpcodes = new List<string>(new string[] { "mov", "add", "sub", "mul", "div" });
			private readonly List<string> AnyBinaryOpcodes = new List<string>(new string[] { "cmp" });

			private void ParseLine(string line)
			{
				List<string> tokens = Tokenize(line);
				var opcode = tokens[0];
				if (ImmediateOpcodes.Contains(opcode))
				{
					instructions.Add(new ImmediateInstruction(opcode));
					return;
				}
				else if (tokens.Count == 2)
				{
					if (RegisterUnaryOpcodes.Contains(opcode))
					{
						instructions.Add(new RegisterUnaryInstruction(opcode, new Register(tokens[1])));
						return;
					}
					else if (LabelUnaryOpcodes.Contains(opcode))
					{
						instructions.Add(new LabelUnaryInstruction(opcode, new Label(tokens[1])));
						return;
					}
				}
				else if (tokens.Count == 3)
				{
					IBinaryOperand target = CreateOperand(tokens[1]);
					IBinaryOperand source = CreateOperand(tokens[2]);
					if (RegisterBinaryOpcodes.Contains(opcode))
					{
						if (target is Constant)
						{
							throw new Exception("Invalid target type - cannot be constant");
						}
						instructions.Add(new BinaryInstruction(opcode, target, source));
						return;
					}
					else if (AnyBinaryOpcodes.Contains(opcode))
					{
						instructions.Add(new BinaryInstruction(opcode, target, source));
					}
				}
				throw new Exception("Invalid opcode {opcode} found");
			}

			private List<string> Tokenize(string line)
			{
				line = line.ToLower();

				List<string> tokens = new List<string>();
				string[] parts = line.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
				else if (parts.Length != 1)
				{
					throw new Exception("Invalid argument count");
				}
				return tokens;
			}

			private IBinaryOperand CreateOperand(string token)
			{
				int value;
				if (int.TryParse(token, out value))
				{
					return new Constant(value);
				}
				return new Register(token);
			}

			private string[] SplitByDelimiter(string code)
			{
				return code.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			}
		}

		public class Executable
		{
			public Executable(List<Instruction> instructions, List<Target> targets)
			{
				Instructions = instructions;
				Targets = targets;
			}

			public string Name { get; private set; }
			public List<Instruction> Instructions { get; private set; }
			public List<Target> Targets { get; private set; }
		}

		public class Instruction
		{
			public string Opcode { get; protected set; }
		}

		public class ImmediateInstruction : Instruction
		{
			public ImmediateInstruction(string opcode) => Opcode = opcode;
		}

		public interface IBinaryOperand { }

		public class Register : IBinaryOperand
		{
			public Register(string name) => Name = name;

			public string Name { get; private set; }
		}

		public class Label
		{
			public Label(string name) => Name = name;

			public string Name { get; private set; }
		}

		public class Constant : IBinaryOperand
		{
			public Constant(int value) => Value = value;

			public int Value { get; private set; }
		}

		public class UnaryInstruction : Instruction
		{
			protected UnaryInstruction(string opcode) => Opcode = opcode;
		}

		public class RegisterUnaryInstruction : UnaryInstruction
		{
			public RegisterUnaryInstruction(string opcode, Register register) 
				: base(opcode) => Register = register;

			public Register Register { get; private set; }
		}

		public class LabelUnaryInstruction : UnaryInstruction
		{
			public LabelUnaryInstruction(string opcode, Label label)
				: base(opcode) => Label = label;

			public Label Label { get; private set; }
		}

		public class BinaryInstruction : Instruction
		{
			public BinaryInstruction(string opcode, IBinaryOperand target, IBinaryOperand source) 
			{
				Opcode = opcode;
				Target = target;
				Source = source;
			}

			public IBinaryOperand Target { get; private set; }
			public IBinaryOperand Source { get; private set; }
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
