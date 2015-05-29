namespace ModelTranslator
{
    using System;
    using System.Collections.Generic;
    using System.Text;


    /// <summary>
    /// A wrapper around StringBuilder than handles indentation.
    /// </summary>
    class SourceBuilder
    {
        public SourceBuilder()
        {
            _Builder = new StringBuilder();
            _NestingLevel = 0;
        }

        private int _NestingLevel;
        private readonly StringBuilder _Builder;

        /// <summary>
        /// Returns a Disposable nested block.
        /// </summary>
        public IDisposable NestedBlock()
        {
            return new SourceBuilderNester(this);
        }

        /// <summary>
        /// Appends the string.
        /// </summary>
        public void Append(string str)
        {
            foreach(var line in ProcessString(str))
                _Builder.Append(line);
        }

        /// <summary>
        /// Appends the string with formatting.
        /// </summary>
        public void AppendFormat(string str, params object[] args)
        {
            foreach (var line in ProcessString(string.Format(str, args)))
                _Builder.Append(line);
        }

        /// <summary>
        /// Appends the string with trailing newline.
        /// </summary>
        public void AppendLine(string str = null)
        {
            Append(str);
            _Builder.AppendLine();
        }

        /// <summary>
        /// Appends a preformatted region.
        /// </summary>
        public void AppendRegion(string regionName)
        {
            AppendLine();
            AppendLine();
            AppendLine("// -----------------------------------");
            AppendLine("// " + regionName);
            AppendLine("// -----------------------------------");
        }

        /// <summary>
        /// Compiles the source to string.
        /// </summary>
        public override string ToString()
        {
            return _Builder.ToString();
        }

        #region Nesting management

        /// <summary>
        /// Processes the string, adding nesting.
        /// </summary>
        private IEnumerable<string> ProcessString(string str)
        {
            if(str == null)
                yield break;

            var lines = str.Split('\n');
            var isFirst = true;
            foreach (var line in lines)
            {
                if (!isFirst) yield return "\n";

                for (var i = 0; i < _NestingLevel; i++)
                    yield return "    ";

                yield return line;

                isFirst = false;
            }
        }

        private class SourceBuilderNester : IDisposable
        {
            public SourceBuilderNester(SourceBuilder parent)
            {
                _Parent = parent;

                _Parent.Append("{");

                _Parent._NestingLevel++;

                _Parent.AppendLine();
                _Parent.AppendLine();
            }

            private readonly SourceBuilder _Parent;

            public void Dispose()
            {
                _Parent.AppendLine();
                _Parent.AppendLine();
                _Parent._NestingLevel--;
                _Parent.Append("}");
            }
        }

        #endregion
    }
}
