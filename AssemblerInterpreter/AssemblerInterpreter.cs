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
					}

				}
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

		public interface Instruction { }


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
