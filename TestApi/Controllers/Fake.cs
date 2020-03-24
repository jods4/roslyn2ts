using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApi.Controllers
{
    [Route("fake")]
    public class FakeController
    {
        [HttpPost("CallMe/{id}")]
        public string CallMeMaybe(int id, string version, [FromServices] object service, [FromBody] Dto body)
        {
            return "";
        }

        [HttpGet]
        public Dto SecondExample([FromQuery] string x1, [FromQuery(Name = "x2")] string? whatever)
        {
            return null!;
        }

        [HttpGet("/absolutely/no/return")]
        public void VoidMethod()
        { }

        [HttpGet]
        public object? Inferred()
        {
            var rnd = new Random();
            if (rnd.NextDouble() > 0.5)
                return null;
            else
                return new Dto { Id = 4 };
        }

        [HttpGet]
        public object Anonymous()
        {
            var rnd = new Random();
            string name = "hello";
            return from r in Enumerable.Range(1, 10)
                   select new
                   {
                       Id = r,
                       FirstName = rnd.NextDouble() > 0.5 ? new { name } : null,
                       Names = new[]
                       {
                           new { name }
                       },
                       NullableNames = new [] { new { name } }.Select(x => rnd.NextDouble() > 0.5 ? x : null),
                   };
        }

        public enum Color { Green, Red, Blue }

        public class Dto
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public DateTime date { get; set; }
            public DateTimeOffset dateOffset { get; set; }
            public float Float { get; set; }
            public double Double { get; set; }
            public decimal Decimal { get; set; }
            public short Short { get; set; }
            public long Long { get; set; }
            public Color Color { get; set; }
            public Nested Nested1 { get; set; } = null!;
            public Nested? Nested2 { get; set; }
            public int[] IntArray { get; set; } = null!;
            public Nested[] ObjArray { get; set; } = null!;
            public IQueryable<Nested> Queryable { get; set; } = null!;
            public IEnumerable<string> IEnumerable { get; set; } = null!;
        }

        public class Base
        {
            public int Inherited { get; set; }
            protected int Protected { get; set; }
        }

        public class Nested : Base
        {
            public bool Legit { get; set; }
            public int? NullableInt { get; set; }
            public string? NullableRef { get; set; }
            public int?[] IntArray { get; set; } = null!;
            public Nested?[] ObjArray { get; set; } = null!;
            private int Private { get; set; }
        }
    }
}
