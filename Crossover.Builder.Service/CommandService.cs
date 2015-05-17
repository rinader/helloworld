using System;
using System.ServiceModel;
using Crossover.Builder.Common.Interfaces;
using Crossover.Builder.Common.Models;

namespace Crossover.Builder.Service
{
    public class CommandService : ICommandService
    {
        public Instruction GetInstruction()
        {
            Console.WriteLine("User \"{0}\" is getting instruction",OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name);
            return Instruction.Build;
        }

        public void SetResult(Instruction instruction, string result)
        {
            switch (instruction)
            {
                case Instruction.Build:
                case Instruction.Test:
                case Instruction.Stop:
                    break;
                default:
                    throw new NotSupportedException(string.Format("Instruction \"{0}\" does not supported", instruction));
            }
        }
    }
}