using System.Text.Json;
using System.Text.Json.Serialization;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccessLayer.Auditing;

// Records who changed which entity and how. The audit rows are written in the same transaction as the change,
// after it is saved, because the ids of added entities are only known then. Only async saves are audited.
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private const string Redacted = "[redacted]";

    private static readonly HashSet<Type> AuditedTypes =
        [typeof(User), typeof(Movie), typeof(Genre), typeof(Hall), typeof(Showtime), typeof(Reservation)];
    private static readonly HashSet<string> SensitiveProperties = [nameof(User.PasswordHash), nameof(User.SecurityStamp)];
    private static readonly JsonSerializerOptions JsonOptions = new() { Converters = { new JsonStringEnumConverter() } };

    private readonly IAuditUserAccessor _userAccessor;
    private List<(EntityEntry Entry, string Action, Dictionary<string, object?> Changes)>? _pending;
    private IDbContextTransaction? _transaction;
    private bool _writingAuditLogs;

    public AuditSaveChangesInterceptor(IAuditUserAccessor userAccessor)
    {
        _userAccessor = userAccessor;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (_writingAuditLogs) return result;
        var context = eventData.Context!;

        var pending = context.ChangeTracker.Entries()
            .Where(e => AuditedTypes.Contains(e.Metadata.ClrType)
                && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => (e, e.State.ToString(), CaptureChanges(e)))
            .Where(p => p.Item3.Count > 0)
            .ToList();
        if (pending.Count == 0) return result;

        _pending = pending;
        if (context.Database.CurrentTransaction is null)
            _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_pending is null || _writingAuditLogs) return result;

        var context = eventData.Context!;
        var now = DateTimeOffset.UtcNow;
        _writingAuditLogs = true;
        try
        {
            context.Set<AuditLog>().AddRange(_pending.Select(p => new AuditLog
            {
                UserId = _userAccessor.UserId,
                EntityName = p.Entry.Metadata.ClrType.Name,
                EntityId = string.Join(",", p.Entry.Metadata.FindPrimaryKey()!.Properties.Select(k => p.Entry.Property(k.Name).CurrentValue)),
                Action = p.Action,
                Changes = JsonSerializer.Serialize(p.Changes, JsonOptions),
                CreatedAt = now
            }));
            await context.SaveChangesAsync(cancellationToken);
            if (_transaction is not null) await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await ResetAsync();
        }

        return result;
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (_pending is not null && !_writingAuditLogs) await ResetAsync();
    }

    private async Task ResetAsync()
    {
        _pending = null;
        _writingAuditLogs = false;
        if (_transaction is null) return;
        await _transaction.DisposeAsync(); // rolls back if it was not committed
        _transaction = null;
    }

    private static Dictionary<string, object?> CaptureChanges(EntityEntry entry)
    {
        var properties = entry.Properties.Where(p => !p.Metadata.IsPrimaryKey() && !p.Metadata.IsConcurrencyToken);

        return entry.State switch
        {
            EntityState.Added => properties.ToDictionary(p => p.Metadata.Name, p => Mask(p.Metadata.Name, p.CurrentValue)),
            EntityState.Deleted => properties.ToDictionary(p => p.Metadata.Name, p => Mask(p.Metadata.Name, p.OriginalValue)),
            _ => properties
                .Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue))
                .ToDictionary(p => p.Metadata.Name, p => SensitiveProperties.Contains(p.Metadata.Name)
                    ? Redacted
                    : (object?)new { Old = p.OriginalValue, New = p.CurrentValue })
        };
    }

    private static object? Mask(string propertyName, object? value) =>
        SensitiveProperties.Contains(propertyName) ? Redacted : value;
}
