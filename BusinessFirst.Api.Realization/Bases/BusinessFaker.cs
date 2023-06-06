using AutoBogus;

namespace RenameMe.Api.Realization.Bases
{
    public class BusinessFaker<T> : AutoFaker<T> where T : class
    {
        public BusinessFaker()
        {
            RuleForType(typeof(int), f => f.Random.Int(0, 100));
            RuleForType(typeof(float), f => f.Random.Float(0, 100));
            RuleForType(typeof(double), f => f.Random.Double(0, 100));
            RuleForType(typeof(decimal), f => f.Random.Decimal(0, 100));
            Configure(option =>
            {
                option.WithLocale("zh_CN");
            });
        }

        public static ValueTask<List<T>> CreateAsync(int number)
        {
            BusinessFaker<T> businessFaker = new();
            return new ValueTask<List<T>>(businessFaker.Generate(number));
        }

        public static ValueTask<T> CreateAsync()
        {
            BusinessFaker<T> businessFaker = new();
            return new ValueTask<T>(businessFaker.Generate());
        }
    }
}
