namespace ModelTranslator.Model
{
    using System.Collections.Generic;


    class MethodModel: CommentedItemBase
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsPrivate { get; set; }

        public List<ArgumentModel> Arguments { get; set; }
    }
}
