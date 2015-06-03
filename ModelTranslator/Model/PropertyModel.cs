namespace ModelTranslator.Model
{
    using System.Collections.Generic;


    class PropertyModel : CommentedItemBase
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public AccessorKind? GetterKind { get; set; }
        public AccessorKind? SetterKind { get; set; }
        public List<ContractAssertionModel> SetterContractAssertions { get; set; }
    }

    enum AccessorKind
    {
        BackingField,
        ModelProxy,
        Custom
    }
}
