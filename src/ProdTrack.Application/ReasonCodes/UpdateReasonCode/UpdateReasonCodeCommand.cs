using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.ReasonCodes.UpdateReasonCode;

/// <summary>Updates the description or (de)activates a reason code. Deactivated codes stay visible in history (PT-017).</summary>
[RequiresPolicy(Policies.ManageMasterData)]
public sealed record UpdateReasonCodeCommand(int Id, string Description, bool IsActive, byte[]? ExpectedVersion = null) : ICommand<ReasonCodeModel>;
