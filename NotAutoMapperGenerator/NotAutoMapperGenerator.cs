using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotAutoMapperGenerator
{
    [Generator]
    public class NotAutoMapperGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var classMappers = GenerateMapper(context, syntaxTree);
                foreach (var classMapper in classMappers)
                {
                    context.AddSource($"{classMapper.Key}.Mapper.cs", SourceText.From(classMapper.Value, Encoding.UTF8));
                }

            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public static Dictionary<string, string> GenerateMapper(GeneratorExecutionContext context, SyntaxTree syntaxTree)
        {
            var srcToDestTemplate = @"
@usings

namespace @namespace
{
    public class @className
    {
        private @source _source { get; set; }

        public @className(@source source)
        {
            _source = source;
        }
        
        public @className AutoMap()
        {
            @propertiesCopyFromSrc
            return this;
        }

        @mapperMethods

        public @dest Build()
        {
            return new @dest
            {
                @propertiesCopy
            };
        }
    }
}";

            var mapperMethodTemplate = @"
        private @propertyType @backingField;
        public @className Set@propertyName(@propertyType @parameterName)
        {
            @backingField = @parameterName;
            return this;
        }";

            var destClassTemplate = @"
@usings

namespace @namespace
{
    public partial class @className
    {
        @properties
    }
}";
            var destClassPropertyTemplate = @"
        public @propertyType @propertyName {get; set;}";


            var res = new Dictionary<string, string>();

            var root = syntaxTree.GetRoot();
            

            var classesWithAttribute = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(cds => cds.AttributeLists.HasAttribute("AutoMapAttribute"))
                .ToList();

            foreach (var classDeclaration in classesWithAttribute)
            {
                var semanticModel = context.Compilation.GetSemanticModel(classDeclaration.Parent.SyntaxTree);
                
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                var attributeContstructorArgs = classSymbol.GetAttributes().First(attr => attr.AttributeClass.Name == "AutoMapAttribute").ConstructorArguments;
                
                var propertiesToIgnore = new List<string>();
                if (!attributeContstructorArgs.Any()) continue;
                var namespaceName = classDeclaration.FindParent<NamespaceDeclarationSyntax>().Name.ToString();
                var srcClassName = attributeContstructorArgs[0].Value.ToString().Replace($"{namespaceName}.", "");
                
                if (attributeContstructorArgs.Count() > 1)
                    propertiesToIgnore = attributeContstructorArgs[1].Values.Select(x=>x.Value.ToString()).ToList();

                var destClassName = classSymbol.Name;
                var className = $"Map{srcClassName}To{destClassName}";
                var srcClassRoot = GetSrcClassSyntaxTree(context, srcClassName).GetRoot();
                var srcClassDecleration = srcClassRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(x=>x.Identifier.ValueText == srcClassName);
                var srcClassUsings = (srcClassRoot as CompilationUnitSyntax).Usings.ToString();



                if (srcClassDecleration == null) continue;

                var publicProperties = srcClassDecleration.DescendantNodes().OfType<PropertyDeclarationSyntax>().Where(prop => prop.Modifiers.All(m => m.ToString() == "public")).ToList();
                var propertiesToCreate = publicProperties.Where(prop => !propertiesToIgnore.Contains(prop.Identifier.ValueText) && prop.Modifiers.All(modifier=>modifier.ValueText != "static")).Select(x=>new PropertyInfo(x));

                var mapperClass = new StringBuilder();
                var partialDestClass = new StringBuilder();


                var mapperMethods = new StringBuilder();
                var destClassProperties = new StringBuilder();
                var propertiesCopy = new StringBuilder();
                var propertiesCopyFromSrc = new StringBuilder();

                foreach (var prop in propertiesToCreate)
                {
                    mapperMethods.AppendLine(mapperMethodTemplate
                        .Replace("@className", className)
                        .Replace("@propertyName", prop.Name)
                        .Replace("@parameterName", prop.ParameterName)
                        .Replace("@propertyType", prop.Type)
                        .Replace("@backingField", prop.BackingFieldName));

                    destClassProperties.AppendLine(destClassPropertyTemplate)
                        .Replace("@propertyName", prop.Name)
                        .Replace("@propertyType", prop.Type);

                    propertiesCopy.AppendLine($"{prop.Name} = {prop.BackingFieldName},");
                    propertiesCopyFromSrc.AppendLine($"{prop.BackingFieldName} = _source.{prop.Name};");
                }

                mapperClass.AppendLine(srcToDestTemplate
                    .Replace("@usings", srcClassUsings)
                    .Replace("@namespace", namespaceName)
                    .Replace("@className", className)
                    .Replace("@source", srcClassName)
                    .Replace("@dest", destClassName)
                    .Replace("@propertiesCopyFromSrc", propertiesCopyFromSrc.ToString())
                    .Replace("@propertiesCopy", propertiesCopy.ToString())
                    .Replace("@mapperMethods", mapperMethods.ToString()));

                partialDestClass.AppendLine(destClassTemplate
                    .Replace("@usings", srcClassUsings)
                    .Replace("@namespace", namespaceName)
                    .Replace("@className", destClassName)
                    .Replace("@properties", destClassProperties.ToString()));

                res[className] = mapperClass.ToString();
                res[destClassName] = partialDestClass.ToString();
            }


            return res;
        }

        private static SyntaxTree GetSrcClassSyntaxTree(GeneratorExecutionContext context, string className)
        {
            var srcClassTree = context.Compilation.SyntaxTrees.FirstOrDefault(tree=>tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Any(x=>x.Identifier.Text == className));
            return srcClassTree;
        }
    }
}
