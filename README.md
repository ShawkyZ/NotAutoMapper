# NotAutoMapper
An attempt to automatically map objects in C# without reflection and only by using Code generation.

## How it works
The code Generator will scan for all classes with Attribute "Automap". 
It expects an empty class "Mapping Destination Class" to be partial and decorated with the attribute.,
This attribute takes for now two args: Type and Properties to Ignore.

It will then fetch the public properties of this Type (Mapping Source Class) in the first argument and create two classes.

* Createa a partial class with all the properties that shouldn't be ignored from the mapping source class.
* Create a mapper that can be used fluently to map between the two types.

## Map$SrcTo$Type Class
The mapper has an AutoMap method that can be called to automatically map all properties from the source.
It can also be used explicitly set values for the properties.


# Example
In this example we have Class Customer and we want to map all it's properties except CreatedDate and UpdatedDate to the type CustomerView
```c#
    public class Customer
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly Dob { get; set; }
    }
```
```c#
    [AutoMap(typeof(Customer), ignoreProperty: new string[] { "CreatedDate", "UpdateDate" })]
    public partial class SampleCustomerView
    {

    }
```


The Source Generator will then create the following two classes
```c#
    public partial class SampleCustomerView
    {
        
        public Guid Id {get; set;}

        public DateTime UpdatedDate {get; set;}

        public string Name {get; set;}

        public DateOnly Dob {get; set;}

    }
```

```c#
    public class MapCustomerToSampleCustomerView
    {
        private Customer _source { get; set; }

        public MapCustomerToSampleCustomerView(Customer source)
        {
            _source = source;
        }
        
        public MapCustomerToSampleCustomerView AutoMap()
        {
            _id = _source.Id;
            _updatedDate = _source.UpdatedDate;
            _name = _source.Name;
            _dob = _source.Dob;

            return this;
        }

        
        private Guid _id;
        public MapCustomerToSampleCustomerView SetId(Guid id)
        {
            _id = id;
            return this;
        }

        private DateTime _updatedDate;
        public MapCustomerToSampleCustomerView SetUpdatedDate(DateTime updatedDate)
        {
            _updatedDate = updatedDate;
            return this;
        }

        private string _name;
        public MapCustomerToSampleCustomerView SetName(string name)
        {
            _name = name;
            return this;
        }

        private DateOnly _dob;
        public MapCustomerToSampleCustomerView SetDob(DateOnly dob)
        {
            _dob = dob;
            return this;
        }


        public SampleCustomerView Build()
        {
            return new SampleCustomerView
            {
                Id = _id,
                UpdatedDate = _updatedDate,
                Name = _name,
                Dob = _dob,

            };
        }
    }

```


Using the Mapper:
```c#
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

```
