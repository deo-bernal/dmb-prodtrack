using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.ReasonCodes;

namespace ProdTrack.Application.ReasonCodes.UpdateReasonCode;

internal sealed class UpdateReasonCodeHandler(IAppDbContext db) : ICommandHandler<UpdateReasonCodeCommand, ReasonCodeModel>
{
    public async Task<Result<ReasonCodeModel>> HandleAsync(UpdateReasonCodeCommand command, CancellationToken cancellationToken)
    {
        var reason = await db.ReasonCodes.FindAsync([command.Id], cancellationToken);
        if (reason is null)
        {
            return ReasonCodeErrors.NotFound(command.Id);
        }

        var result = reason.Update(command.Description, command.IsActive);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return ReasonCodeModel.From(reason);
    }
}
