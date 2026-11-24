using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebScout.Web.Common.Domain;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Invalida la caché de <see cref="IResolutorGrupoScout"/> cuando se guarda un cambio en <c>GruposScout</c>
/// (alta, baja, activación, renombrado o cambio de <c>Paleta</c>), venga de los ajustes del grupo,
/// del panel federativo o de cualquier otro punto. Así la paleta guardada se aplica en la siguiente
/// petición sin esperar a que caduque la caché.
/// </summary>
public sealed class InvalidacionCacheGruposInterceptor : SaveChangesInterceptor
{
    private readonly IResolutorGrupoScout _resolutor;
    private readonly ConditionalWeakTable<DbContext, HashSet<string>> _pendientes = new();

    public InvalidacionCacheGruposInterceptor(IResolutorGrupoScout resolutor)
    {
        _resolutor = resolutor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        RegistrarSlugsModificados(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        RegistrarSlugsModificados(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        InvalidarPendientes(eventData.Context);
        return result;
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        InvalidarPendientes(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData) => InvalidarPendientes(eventData.Context);

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        InvalidarPendientes(eventData.Context);
        return Task.CompletedTask;
    }

    private void RegistrarSlugsModificados(DbContext? context)
    {
        if (context is null) return;

        HashSet<string>? slugs = null;
        foreach (var entry in context.ChangeTracker.Entries<GrupoScout>())
        {
            var paletaCambiada = entry.Reference(g => g.Paleta).TargetEntry?.State
                is EntityState.Added or EntityState.Modified or EntityState.Deleted;

            if (entry.State is EntityState.Unchanged or EntityState.Detached && !paletaCambiada)
            {
                continue;
            }

            slugs ??= _pendientes.GetOrCreateValue(context);
            slugs.Add(entry.Entity.Slug);

            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                var slugOriginal = entry.Property(g => g.Slug).OriginalValue;
                if (!string.IsNullOrWhiteSpace(slugOriginal))
                {
                    slugs.Add(slugOriginal);
                }
            }
        }
    }

    private void InvalidarPendientes(DbContext? context)
    {
        if (context is null || !_pendientes.TryGetValue(context, out var slugs)) return;

        foreach (var slug in slugs)
        {
            _resolutor.Invalidar(slug);
        }

        _pendientes.Remove(context);
    }
}
