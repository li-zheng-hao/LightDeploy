namespace LightDeployApp;

public class ColumnNameAttribute : System.Attribute
{
    public ColumnNameAttribute(string name) { this.Name = name; }
    public string Name { get; set; }
}