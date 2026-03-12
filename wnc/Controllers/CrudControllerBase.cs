using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;

namespace wnc.Controllers;

public abstract class CrudControllerBase(AppDbContext context) : Controller
{
    protected AppDbContext Context { get; } = context;

    protected async Task<bool> TrySaveChangesAsync(string errorMessage)
    {
        try
        {
            await Context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            return false;
        }
    }

    protected async Task<bool> TryDeleteAsync<TEntity>(TEntity entity, string errorMessage)
        where TEntity : class
    {
        Context.Remove(entity);

        try
        {
            await Context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = errorMessage;
            return false;
        }
    }
}
