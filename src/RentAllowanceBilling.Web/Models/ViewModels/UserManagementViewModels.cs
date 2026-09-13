namespace RentAllowanceBilling.Web.Models.ViewModels;

public class UserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

public class EditUserRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public List<Branch>? Branches { get; set; }
    public List<string> SelectedRoles { get; set; } = new();
    public string[] AllRoles { get; set; } = Roles.All;
}
