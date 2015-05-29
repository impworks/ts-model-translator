namespace ModelTranslator.Model
{
    using System.Collections.Generic;


    class ClassModel: CommentedItemBase
    {
        public string Name { get; set; }
        public string BaseType { get; set; }
        public List<string> Interfaces { get; set; }

        public ConstructorModel Constructor { get; set; }
        public List<FieldModel> Fields { get; set; }
        public List<PropertyModel> Properties { get; set; }
        public List<PropertyModel> Events { get; set; }
        public List<MethodModel> Methods { get; set; }
    }
}
