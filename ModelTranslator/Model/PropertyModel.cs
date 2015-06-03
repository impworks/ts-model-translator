namespace ModelTranslator.Model
{
    using System.Collections.Generic;


    class PropertyModel : CommentedItemBase
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public AccessorKind? Getter { get; set; }
        public AccessorKind? Setter { get; set; }
        public List<ContractAssertionModel> SetterContractAssertions { get; set; }
    }

    enum AccessorKind
    {
        BackingField,
        ModelProxy,
        Custom
    }
}
