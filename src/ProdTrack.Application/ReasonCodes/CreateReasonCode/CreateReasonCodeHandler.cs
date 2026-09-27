using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.ReasonCodes;

namespace ProdTrack.Application.ReasonCodes.CreateReasonCode;

internal sealed class CreateReasonCodeHandler(IAppDbContext db) : ICommandHandler<CreateReasonCodeCommand, ReasonCodeModel>
{
    public async Task<Result<ReasonCodeModel>> HandleAsync(CreateReasonCodeCommand command, CancellationToken cancellationToken)
    {
        var created = ReasonCode.Create(command.Code, command.Description, command.Category);
        if (created.IsFailure)
        {
            return created.Error!;
        }

        var reason = created.Value;
        if (await db.ReasonCodes.AnyAsync(r => r.Code == reason.Code && r.Category == reason.Category, cancellationToken))
        {
            return ReasonCodeErrors.DuplicateCode(reason.Code, reason.Category);
        }

        db.ReasonCodes.Add(reason);
        await db.SaveChangesAsync(cancellationToken);
        return ReasonCodeModel.From(reason);
    }
}
