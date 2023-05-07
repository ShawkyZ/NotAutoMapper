using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NotAutoMapperGenerator
{
    internal struct PropertyInfo
    {
        public PropertyInfo(PropertyDeclarationSyntax property) : this()
        {
            Type = property.Type.ToString();
            Name = property.Identifier.ToString();
            ParameterName = $"{ Name[0].ToString().ToLower()}{ Name.Remove(0, 1)}";
            BackingFieldName = $"_{ParameterName}";
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string ParameterName { get; set; }
        public string BackingFieldName { get; set; }
    }
}
