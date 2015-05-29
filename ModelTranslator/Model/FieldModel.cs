namespace ModelTranslator.Model
{
    class FieldModel : CommentedItemBase
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public string InitializerCode { get; set; }
    }
}
