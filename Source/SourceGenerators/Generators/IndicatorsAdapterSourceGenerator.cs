using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerators.Generators
{
    [Generator]
    public class IndicatorsAdapterSourceGenerator : ISourceGenerator
    {
        private const string UsingDirectives = "using Skender.Stock.Indicators;\nusing System.Collections.Generic;";
        private const string InterfaceName = "IIndicatorsAdapter";
        private const string ClassName = "IndicatorsAdapter";
        

        public void Execute(GeneratorExecutionContext context)
        {
            var indicatorType = context.Compilation.GetTypeByMetadataName("Skender.Stock.Indicators.Indicator") ?? throw new Exception();
            var methods = indicatorType
                .GetMembers()
                .Where(x => x.Kind == SymbolKind.Method)
                .Select(x => (IMethodSymbol)x)
                .Where(x => x.DeclaredAccessibility == Accessibility.Public && x.Name.StartsWith("Get") && x.IsExtensionMethod)
                .GroupBy(x => x.Name).Select(g => g.First());
            
            GenerateInterface(context, methods);
            GenerateImplementation(context, methods);
        }
        private void GenerateInterface(GeneratorExecutionContext context, IEnumerable<IMethodSymbol> methods)
        {
            var srcBuilder = new StringBuilder();
            srcBuilder.AppendLine(UsingDirectives);
            srcBuilder.AppendLine();
            srcBuilder.AppendLine("namespace Generated;");
            srcBuilder.AppendLine();
            srcBuilder.AppendLine($"public interface {InterfaceName}");
            srcBuilder.AppendLine("{");
            foreach (var method in methods)
            {
                var signature = GetSignature(method);
                if (signature.Contains("IEnumerable<TQuote>"))
                    continue;

                srcBuilder.AppendLine(GetXmlDocumentationComment(method));
                srcBuilder.AppendLine($"\tpublic {signature};");
                srcBuilder.AppendLine();
            }
            srcBuilder.AppendLine("}");

            context.AddSource($"{InterfaceName}.cs", SourceText.From(srcBuilder.ToString(), Encoding.UTF8));
        }
        private void GenerateImplementation(GeneratorExecutionContext context, IEnumerable<IMethodSymbol> methods)
        {
            var srcBuilder = new StringBuilder();
            srcBuilder.AppendLine(UsingDirectives);
            srcBuilder.AppendLine();
            srcBuilder.AppendLine("namespace Generated;");
            srcBuilder.AppendLine();
            srcBuilder.AppendLine($"public class {ClassName} : {InterfaceName}");
            srcBuilder.AppendLine("{");
            srcBuilder.AppendLine("\tprivate readonly IEnumerable<IQuote> Candlesticks;");
            srcBuilder.AppendLine("\tpublic IndicatorsAdapter(IEnumerable<IQuote> candlesticks) => this.Candlesticks = candlesticks;"); // constructor
            srcBuilder.AppendLine();
            srcBuilder.AppendLine("\t//// //// ////");
            srcBuilder.AppendLine();
            foreach (var method in methods)
            {
                var signature = GetSignature(method);
                if (signature.Contains("IEnumerable<TQuote>"))
                    continue;

                var parametersInCall = string.Join(", ", method.Parameters.Skip(1).Select(p => $"{p.Name}").ToArray());

                srcBuilder.AppendLine(GetXmlDocumentationComment(method));
                srcBuilder.AppendLine($"\tpublic {signature} => this.Candlesticks.{method.Name}({parametersInCall});");
                srcBuilder.AppendLine();
            }
            srcBuilder.AppendLine("}");

            context.AddSource($"{ClassName}.cs", SourceText.From(srcBuilder.ToString(), Encoding.UTF8));
        }
        private static string GetSignature(IMethodSymbol method)
        {
            var returnType = method.ReturnType.ToDisplayString();
            var name = method.Name;
            var parametersInSignature = string.Join(", ", method.Parameters.Skip(1).Select(p => p.ToDisplayString()).ToArray());
            return $"{returnType} {name}({parametersInSignature})".Replace("System.Collections.Generic.", string.Empty).Replace("Skender.Stock.Indicators.", string.Empty);
        }
        private static string GetXmlDocumentationComment(IMethodSymbol method)
        {
            var str = method.GetDocumentationCommentXml();
            return str == string.Empty ? string.Empty : $"\t\r\n\t///{str.Replace("\n", "\n\t///")}";
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
