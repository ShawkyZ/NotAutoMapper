using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Customer
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly Dob { get; set; }
        public IEnumerable<User> UserList { get; set; } = Enumerable.Empty<User>();
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
