namespace ModelTranslator.Model
{
    class ContractAssertionModel
    {
        public string ArgumentName { get; set; }
        public ContractAssertionKind Kind { get; set; }
        public string RawExpression { get; set; }
    }

    enum ContractAssertionKind
    {
        IsNotNull,
        GreaterThanZero,
        GreaterOrEqualThanZero,
        CountGreaterThanZero,
        IsNotEmptyString,
        Other
    }
}
