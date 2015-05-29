namespace ModelTranslator.Model
{
    class PropertyModel : CommentedItemBase
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public bool HasSetter { get; set; }
    }
}
