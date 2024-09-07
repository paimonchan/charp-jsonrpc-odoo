# PROGRESS TRACKER
## API
- [x]   Api Read
- [x]   Api Create
- [x]   Api Update
- [x]   Api Delete
- [ ]   Api Action
- [ ]   Api Count
- [ ]   Api Group

## Metadata
- [ ]   Api Version
- [x]   Api List Table
- [x]   Api List Column

## Transaction
- [ ]   Error Handler

## Documentation
- [ ]    Doc Step By Step to Setup Odoo SaaS
- [ ]    Doc Step By Step to Get Credential

# HOW TO USE

## Example
```csharp
using Odoo;
public class Test
{
    static void Main(string[] args)
    {
        JsonRPC.Configure(new Credential
        {
            userid      = 0, // user id from odoo menu `user & companies`
            username    = "" // user name from odoo menu `user & companies`,
            password    = "" // api key from odoo,
            database    = "" // database name,
            uri         = "" // odoo url (include the port for ex: 443)
        });

        TestRead();
    }

    static void TestRead()
    {
        // limit data
        int limit = 7;

        // column te be fetch, good to used to reduce volume datas
        string[] fields = new string[] { "id", "name", "type" };

        // filter data (for ex: filter product by active = true)
        object[] filters = new object[]
        {
            new object[] {"active", "=", true},
        };

        var result = JsonRPC.Read("product.product", filters, fields, limit);
        Console.WriteLine(result);
    }
}
```

## Example CRUD
### CREATE
```csharp
static void Create()
{
    // column to insert
    Object[] vals = new Object[] {
        new {
            name            = "Product Sharp 01",
            type            = "product",
        },
        new {
            name            = "Product Sharp 02",
            type            = "product",
        },
    };

    var result = JsonRPC.Create("product.product", vals);
    Console.WriteLine(result);
}

```

### READ
```csharp
static void Read()
{
    // limit data
    int limit = 7;

    // column te be fetch, good to used to reduce volume datas
    string[] fields = new string[] { "id", "name", "type" };

    // filter data (for ex: filter product by active = true)
    object[] filters = new object[]
    {
        new object[] {"active", "=", true},
    };

    var result = JsonRPC.Read("product.product", filters, fields, limit);
    Console.WriteLine(result);
}
```

### UPDATE
```csharp
static void Update()
{
    // record id
    int recordId = {{put id record here}};
    int[] recordIds = new int[] { recordId };

    // column to insert
    Object val = new {
        name                = "Product Sharp (UPDATE1)",
        type                = "product",
    };

    var result = JsonRPC.Update("product.product", recordIds, val);
    Console.WriteLine(result);
}
```

### DELETE
```csharp
static void Delete()
{
    // record id
    int recordId = {{put id record here}};
    int[] recordIds = new int[] { recordId };

    var result = JsonRPC.Delete("product.product", recordIds);
    Console.WriteLine(result);
}
```

