namespace BackEndWaterFloodApp.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class RoleAccessAttribute : Attribute
{
    public string[] AllowedRoles { get; }

    public RoleAccessAttribute(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}
