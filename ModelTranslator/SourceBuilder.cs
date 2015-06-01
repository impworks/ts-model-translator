namespace ModelTranslator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    /// <summary>
    /// A wrapper around StringBuilder than handles indentation and spacing.
    /// </summary>
    class SourceBuilder
    {
        public SourceBuilder(string separator = null)
        {
            _NestingLevel = 0;
            _Lines = new List<string>();
            _Separator = separator ?? Environment.NewLine;
        }

        private int _NestingLevel;
        private readonly List<string> _Lines;
        private readonly string _Separator;

        /// <summary>
        /// Returns a Disposable nested block.
        /// </summary>
        public IDisposable NestedBlock()
        {
            return new SourceBuilderNester(this);
        }

        /// <summary>
        /// Returns a disposable spaced block.
        /// </summary>
        public IDisposable SpacedBlock()
        {
            return new SourceBuilderSpacer(this);
        }

        /// <summary>
        /// Builds a line.
        /// </summary>
        public SourceLineBuilder Line()
        {
            return new SourceLineBuilder(this);
        }

        /// <summary>
        /// Appends a line of code (or several) with padding applied.
        /// </summary>
        public void Append(string str)
        {
            foreach (var line in ProcessString(str))
                _Lines.Add(line);
        }

        /// <summary>
        /// Appends a formatted line of code (or several) with padding applied.
        /// </summary>
        public void Append(string str, params object[] args)
        {
            var actualStr = string.Format(str, args);
            foreach (var line in ProcessString(actualStr))
                _Lines.Add(line);
        }

        /// <summary>
        /// Appends a preformatted region.
        /// </summary>
        public void AppendRegionHeader(string regionName)
        {
            using (SpacedBlock())
            {
                Append("// -----------------------------------");
                Append("// {0}", regionName);
                Append("// -----------------------------------");
            }
        }

        /// <summary>
        /// Compiles the source to string, crunching empty lines.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            var isLastEmpty = false;
            for(var i = 0; i < _Lines.Count; i++)
            {
                var line = _Lines[i];
                var isCurrentEmpty = string.IsNullOrWhiteSpace(line);
                if (!isCurrentEmpty || !isLastEmpty)
                {
                    sb.Append(line);

                    if (_Lines.Count > i + 1)
                    {
                        // special case: pull opening brace onto current line
                        var nextLine = _Lines[i + 1];
                        sb.Append(nextLine.Trim() != "{" ? _Separator : " ");
                    }
                }

                isLastEmpty = isCurrentEmpty;
            }

            return sb.ToString();
        }

        #region Nesting management

        /// <summary>
        /// Processes the string, adding nesting.
        /// </summary>
        private IEnumerable<string> ProcessString(string str)
        {
            return str == null
                ? new string[] { null }
                : str.Split('\n').Select(line => line == "{" ? line : new string(' ', _NestingLevel * 4) + line);
        }

        /// <summary>
        /// Declares a syntactical block of code with curly braces and indentation.
        /// </summary>
        private class SourceBuilderNester : IDisposable
        {
            public SourceBuilderNester(SourceBuilder parent)
            {
                _Parent = parent;

                _Parent.Append("{");
                _Parent._NestingLevel++;
            }

            private readonly SourceBuilder _Parent;

            public void Dispose()
            {
                _Parent._NestingLevel--;
                _Parent.Append("}");
            }
        }

        /// <summary>
        /// Declares a logical block of code with empty lines above and below.
        /// </summary>
        private class SourceBuilderSpacer : IDisposable
        {
            public SourceBuilderSpacer(SourceBuilder parent)
            {
                _Parent = parent;

                _Parent.Append("");
            }

            private readonly SourceBuilder _Parent;

            public void Dispose()
            {
                _Parent.Append("");
            }
        }

        /// <summary>
        /// Concatenates several pieces of a line into one.
        /// </summary>
        public class SourceLineBuilder : SourceBuilder, IDisposable
        {
            public SourceLineBuilder(SourceBuilder parent) : base(" ")
            {
                _Parent = parent;
            }

            private readonly SourceBuilder _Parent;

            public void Dispose()
            {
                _Parent.Append(ToString());
            }
        }

        #endregion
    }
}
