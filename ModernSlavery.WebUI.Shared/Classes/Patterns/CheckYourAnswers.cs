using System.Collections.Generic;
using System.Linq;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Shared.Classes.Patterns
{
    [Partial("Patterns/CheckYourAnswers")]
    public class CheckYourAnswers
    {
        public CheckYourAnswers(params IAnswer[] items)
        {
            Items = items.Where(x => x != null).ToArray();
        }

        public IAnswer[] Items { get; set; }
    }

    public interface IAnswer
    {
        string Name { get; }
    }

    public class StringCheckYourAnswer : IAnswer
    {
        public StringCheckYourAnswer(string name, string value, string valueId = "")
        {
            Name = name;
            Value = value;
            ValueId = valueId;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public string ValueId { get; set; }
    }

    public class ListCheckYourAnswer : IAnswer
    {
        public ListCheckYourAnswer(string name, IEnumerable<string> values, string valueId = "")
        {
            Name = name;
            Values = values.ToList();
            ValueId = valueId;
        }

        public string Name { get; set; }
        public List<string> Values { get; set; }
        public string ValueId { get; set; }

    }
}