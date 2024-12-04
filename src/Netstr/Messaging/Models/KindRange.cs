namespace Netstr.Messaging.Models
{
    public record KindRange(int MinKind, int MaxKind)
    { 
        public static KindRange Parse(string range)
        {
            int minKind = int.MinValue;
            int maxKind = int.MaxValue;

            var x = range.Split("-", StringSplitOptions.TrimEntries);

            if (x.Length > 2)
            {
                throw new ArgumentException($"Value '{range}' is invalid for a KindRange");
            }

            if (x.Length == 1)
            {
                if (!int.TryParse(x[0], out var i))
                {
                    throw new ArgumentException($"Value '{range}' is invalid for a KindRange");
                }

                minKind = i;
                maxKind = i;
            }
            else
            {
                if (x[0].Length > 0)
                {
                    minKind = int.Parse(x[0]);
                }

                if (x[1].Length > 0)
                {
                    maxKind = int.Parse(x[1]);
                }
            }

            return new KindRange(minKind, maxKind);
        }
    }
}
