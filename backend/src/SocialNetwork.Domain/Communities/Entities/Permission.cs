namespace SocialNetwork.Domain.Communities.Entities;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> Roles { get; set; }
        = new List<RolePermission>();
}