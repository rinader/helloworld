using System.ServiceModel;
using Crossover.Builder.Common.Models;

namespace Crossover.Builder.Common.Interfaces
{
    [ServiceContract]
    public interface ICommandService
    {
        [OperationContract]
        Instruction GetInstruction();

        [OperationContract]
        void SetResult(Instruction instruction, string result);
    }
}