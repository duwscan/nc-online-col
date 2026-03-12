using Microsoft.AspNetCore.Mvc.Rendering;

namespace wnc.Features.IdentityAccess.Common;

public static class IdentityAccessSelectLists
{
    public static IReadOnlyList<SelectListItem> UserStatusOptions(string? selectedValue = null)
        => BuildOptions(
            [
                ("ACTIVE", "Active"),
                ("INACTIVE", "Inactive"),
                ("LOCKED", "Locked"),
                ("DELETED", "Deleted")
            ],
            selectedValue);

    public static IReadOnlyList<SelectListItem> AuthLogStatusOptions(string? selectedValue = null)
        => BuildOptions(
            [
                ("SUCCESS", "Success"),
                ("FAILED", "Failed"),
                ("LOCKED", "Locked")
            ],
            selectedValue);

    public static IReadOnlyList<SelectListItem> RevocationStateOptions(string? selectedValue = null)
        => BuildOptions(
            [
                ("ACTIVE", "Active"),
                ("REVOKED", "Revoked")
            ],
            selectedValue);

    public static IReadOnlyList<SelectListItem> BooleanOptions(bool? selectedValue, string trueLabel, string falseLabel)
        => new List<SelectListItem>
        {
            new() { Text = "All", Value = string.Empty, Selected = selectedValue is null },
            new() { Text = trueLabel, Value = bool.TrueString, Selected = selectedValue is true },
            new() { Text = falseLabel, Value = bool.FalseString, Selected = selectedValue is false }
        };

    public static IReadOnlyList<SelectListItem> BuildEntityOptions(
        IEnumerable<(Guid Id, string Label)> entities,
        Guid? selectedValue = null,
        string emptyText = "Select an option")
    {
        var items = new List<SelectListItem>
        {
            new() { Text = emptyText, Value = string.Empty, Selected = selectedValue is null }
        };

        items.AddRange(entities.Select(entity => new SelectListItem
        {
            Value = entity.Id.ToString(),
            Text = entity.Label,
            Selected = selectedValue == entity.Id
        }));

        return items;
    }

    private static IReadOnlyList<SelectListItem> BuildOptions(IEnumerable<(string Value, string Text)> options, string? selectedValue)
        => options.Select(option => new SelectListItem
        {
            Value = option.Value,
            Text = option.Text,
            Selected = string.Equals(option.Value, selectedValue, StringComparison.OrdinalIgnoreCase)
        }).ToList();
}
