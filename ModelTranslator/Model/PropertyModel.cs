namespace ModelTranslator.Model
{
    using System.Collections.Generic;


    class PropertyModel : CommentedItemBase
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public bool HasSetter { get; set; }
        public List<ContractAssertionModel> SetterContractAssertions { get; set; }
    }
}
