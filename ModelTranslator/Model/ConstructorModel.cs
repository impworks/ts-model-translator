namespace ModelTranslator.Model
{
    using System.Collections.Generic;


    class ConstructorModel : CommentedItemBase
    {
        public List<ArgumentModel> Arguments { get; set; }
        public List<ArgumentModel> BaseCall { get; set; }
    }
}
