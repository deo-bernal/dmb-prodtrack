using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.ReasonCodes;

namespace ProdTrack.Application.ReasonCodes.CreateReasonCode;

[RequiresPolicy(Policies.ManageMasterData)]
public sealed record CreateReasonCodeCommand(string Code, string Description, ReasonCategory Category) : ICommand<ReasonCodeModel>;
