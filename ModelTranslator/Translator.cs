namespace ModelTranslator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Instrumentation;
    using System.Text;
    using System.Text.RegularExpressions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using ModelTranslator.Model;


    class Translator
    {
        /// <summary>
        /// Translate class definitions from C# to TypeScript.
        /// </summary>
        public string Translate(string src)
        {
            var tree = CSharpSyntaxTree.ParseText(src);
            var classModel = ParseModel(tree);
            var result = RenderModel(classModel);
            return result;
        }

        #region Create model

        /// <summary>
        /// Parses a class in the source file into intermediate representation.
        /// </summary>
        private ClassModel ParseModel(SyntaxTree tree)
        {
            var classNode = tree.GetRoot()
                                .DescendantNodes()
                                .OfType<ClassDeclarationSyntax>()
                                .Single();

            var baseTypes = classNode.BaseList.Types.OfType<SimpleBaseTypeSyntax>()
                                     .Select(x => x.Type)
                                     .Where(x => x is IdentifierNameSyntax || x is GenericNameSyntax)
                                     .Select(x => x.ToString())
                                     .ToArray();

            var classModel = new ClassModel
            {
                Name = classNode.Identifier.Text,
                BaseType = baseTypes.FirstOrDefault(x => !x.StartsWith("I")),
                Interfaces = baseTypes.Where(x => x.StartsWith("I")).ToList(),

                Comment = ParseComment(classNode),

                Constructor = ParseConstructor(classNode),
                Fields = ParseFields(classNode).ToList(),
                Properties = ParseProperties(classNode).ToList(),
                Methods = ParseMethods(classNode).ToList()
            };

            return classModel;
        }

        /// <summary>
        /// Returns the list of fields in the class.
        /// </summary>
        private IEnumerable<FieldModel> ParseFields(ClassDeclarationSyntax classNode)
        {
            var fields = classNode.DescendantNodes()
                                  .OfType<FieldDeclarationSyntax>();

            foreach (var field in fields)
            {
                var decl = field.Declaration;
                var nameSyntax = decl.Variables.First();
                yield return new FieldModel
                {
                    Name = nameSyntax.Identifier.ToString(),
                    Type = decl.Type.ToString(),
                    InitializerCode = nameSyntax.Initializer != null ? nameSyntax.Initializer.Value.ToString() : null,
                    Comment = ParseComment(field)
                };
            }
        }

        /// <summary>
        /// Returns the list of properties in the class.
        /// </summary>
        private IEnumerable<PropertyModel> ParseProperties(ClassDeclarationSyntax classNode)
        {
            var properties = classNode.DescendantNodes()
                                      .OfType<PropertyDeclarationSyntax>();

            foreach (var pty in properties)
            {
                var accessors = pty.AccessorList.Accessors;
                var setter = accessors.FirstOrDefault(x => x.Keyword.Kind() == SyntaxKind.SetKeyword);

                yield return new PropertyModel
                {
                    Type = pty.Type.ToString(),
                    Name = pty.Identifier.Text,
                    HasSetter = setter != null,
                    Comment = ParseComment(pty)
                };
            }
        }

        /// <summary>
        /// Returns the list of methods in the class.
        /// </summary>
        private IEnumerable<MethodModel> ParseMethods(ClassDeclarationSyntax classNode)
        {
            var methods = classNode.DescendantNodes()
                                   .OfType<MethodDeclarationSyntax>();

            var restrictedNames = new[] { "Dispose", "Equals", "GetHashCode", "ObjectInvariant" };

            foreach (var method in methods)
            {
                if (restrictedNames.Contains(method.Identifier.Text))
                    continue;

                var args = method.ParameterList.Parameters;
                yield return new MethodModel
                {
                    Name = method.Identifier.Text,
                    Type = method.ReturnType.ToString(),
                    IsPrivate = method.Modifiers.Any(SyntaxKind.PrivateKeyword),

                    Arguments = args.Select(x => new ArgumentModel
                    {
                        Name = x.Identifier.Text,
                        Type = x.Type.ToString(),
                        InitializerCode = x.Default != null ? x.Default.Value.ToString() : null
                    }).ToList(),

                    Comment = ParseComment(method),
                };
            }
        }

        /// <summary>
        /// tot 
        /// </summary>
        /// <param name="classNode"></param>
        /// <returns></returns>
        private ConstructorModel ParseConstructor(ClassDeclarationSyntax classNode)
        {
            var ctorNode = classNode.DescendantNodes()
                                    .OfType<ConstructorDeclarationSyntax>()
                                    .FirstOrDefault();

            if (ctorNode == null)
                return null;

            var args = ctorNode.ParameterList.Parameters;
            var baseCallArgs = ctorNode.Initializer.ArgumentList.Arguments;

            return new ConstructorModel
            {
                Arguments = args.Select(x => new ArgumentModel
                {
                    Name = x.Identifier.Text,
                    Type = x.Type.ToString(),
                    InitializerCode = x.Default != null ? x.Default.Value.ToString() : null
                })
                .Where(x => x.Type != "ILogService")
                .ToList(),

                BaseCall = baseCallArgs.Select(x => new ArgumentModel { Name = x.Expression.ToString() }).ToList(),
                Comment = ParseComment(ctorNode)
            };
        }

        #endregion

        #region Comment parsing

        private static readonly Regex CommentSpaceRegex = new Regex("\n\\s+", RegexOptions.Compiled);
        private static readonly Regex CommentSummaryRegex = new Regex("<summary>(?<text>.+)</summary>", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        /// <summary>
        /// Returns the corresponding documentation comment.
        /// </summary>
        private string ParseComment(SyntaxNode node)
        {
            var trivia = node.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
            return trivia.FullSpan.IsEmpty ? null : "/// " + CommentSpaceRegex.Replace(trivia.ToString().Trim(), "\n");
        }

        /// <summary>
        /// Returns the bare text from comment.
        /// </summary>
        private string GetCommentSummary(string comment)
        {
            if(string.IsNullOrEmpty(comment))
                return null;

            var rawValue = CommentSummaryRegex.Match(comment).Groups["text"].Value.Replace("///", "").Replace('\n', ' ').Trim();
            return rawValue;
        }

        #endregion

        #region Name translation

        /// <summary>
        /// Applies a naming convention to field or property name.
        /// </summary>
        private string ApplyNameConvention(string name, bool isPrivate)
        {
            name = name.TrimStart('_');
            name = name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
            if (isPrivate)
                name = '_' + name;

            if (name.EndsWith("Subject"))
                return name.Substring(0, name.Length - "Subject".Length);

            if (name == "_subscribings")
                return "_subscriptions";

            return name;
        }

        private readonly Regex ListTypeConvention = new Regex("^List<(?<name>.+)>$", RegexOptions.Compiled);
        private readonly Regex SubjectTypeConvention = new Regex("^Subject<(?<name>.+)>$", RegexOptions.Compiled);
        private readonly Regex GenericTypeConvention = new Regex("^(?<type>[a-z][a-z0-9]*)<(?<name>.+)>$", RegexOptions.Compiled);

        private readonly Dictionary<string, string> BasicTypes = new Dictionary<string, string>
        {
            { "int", "number" },
            { "long", "number" },
            { "float", "number" },
            { "double", "number" },
            { "object", "any" },
            { "bool", "boolean" },
        };

        /// <summary>
        /// Applies a naming convention to types.
        /// </summary>
        private string ApplyTypeConvention(string type, bool useInterface = true)
        {
            if (BasicTypes.ContainsKey(type))
                return BasicTypes[type];

            var listMatch = ListTypeConvention.Match(type);
            if (listMatch.Success)
                return ApplyTypeConvention(listMatch.Groups["name"].Value) + "[]";

            var subjectMatch = SubjectTypeConvention.Match(type);
            if (subjectMatch.Success)
                return (useInterface ? "I" : "") + "Observable<" + ApplyTypeConvention(subjectMatch.Groups["name"].Value) + ">";

            var genericMatch = GenericTypeConvention.Match(type);
            if (genericMatch.Success)
                return ApplyTypeConvention(listMatch.Groups["type"].Value) + "<" + ApplyTypeConvention(listMatch.Groups["name"].Value) + ">";

            return type;
        }

        #endregion

        #region Render model to typescript

        /// <summary>
        /// Renders the model as Typescript code.
        /// </summary>
        private string RenderModel(ClassModel model)
        {
            var sb = new SourceBuilder();

            // class <X> [extends <Y>] [implements <Z1, Z2, ...>] {
            sb.AppendFormat("class {0} ", model.Name);
            if (!string.IsNullOrEmpty(model.BaseType))
                sb.AppendFormat("extends {0} ", model.BaseType);
            if (model.Interfaces.Any())
                sb.AppendFormat("implements {0} ", string.Join(", ", model.Interfaces));

            using (sb.NestedBlock())
            {
                if (!string.IsNullOrEmpty(model.Comment))
                    sb.Append(model.Comment);

                AppendFields(sb, model);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes the block of field declarations with comments, in alphabetical order, excluding special names.
        /// </summary>
        private void AppendFields(SourceBuilder sb, ClassModel model)
        {
            // add backing fields for properties
            var fieldsLookup = model.Fields.ToDictionary(x => x.Name, x => x);
            foreach (var pty in model.Properties)
            {
                var name = ApplyNameConvention(pty.Name, true);
                if(!fieldsLookup.ContainsKey(name))
                    fieldsLookup.Add(name, new FieldModel { Name = pty.Name, Type = pty.Type, Comment = pty.Comment });
            }

            var restrictions = new Func<FieldModel, bool>[] { x => x.Name == "_isDisposed", x => x.Type == "ILogService" };
            var allFields = fieldsLookup.Where(x => !restrictions.Any(y => y(x.Value))).Select(x => x.Value).ToList();

            if (!fieldsLookup.Any())
                return;

            sb.AppendRegion("Fields");

            foreach (var field in allFields.OrderBy(x => x.Name))
            {
                sb.AppendLine();
                sb.AppendLine("// " + GetCommentSummary(field.Comment));
                sb.AppendFormat("private {0}: {1};", ApplyNameConvention(field.Name, true), ApplyTypeConvention(field.Type));
                sb.AppendLine();
            }
        }

        #endregion
    }
}
