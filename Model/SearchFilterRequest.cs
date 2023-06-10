using Model.Enums;
using System.Diagnostics;

namespace Model
{
    [DebuggerDisplay("Name: {PropertyName}, Op: {OperationType.ToString()}, Value: {Value}, Range: {Range}")]
    public class SearchFilterRequest
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
        public FilterType FilterType { get; set; }
        public OperationType OperationType { get; set; }
        public string Range { get; set; }
    }
}