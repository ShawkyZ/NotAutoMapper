// See https://aka.ms/new-console-template for more information

using Sample;

Console.WriteLine("Hello, World!");
var source = new Customer
{
    Id = Guid.NewGuid(),
    CreatedDate = DateTime.Now,
    UpdatedDate = DateTime.Now.AddDays(1),
    Dob = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
    Name = "Bob"
};

var dest = new MapCustomerToSampleCustomerView(source)
    .AutoMap()
    // Override Automapped value
    .SetName("Alice")
    .SetDob(DateOnly.FromDateTime(DateTime.Now.AddDays(10)))
    .Build();

Console.WriteLine(source.Dob);
Console.WriteLine(dest.Dob);
Console.WriteLine(source.Name);
Console.WriteLine(dest.Name);