# Echo

A lightweight UDP library to broadcast application states across a LAN in .NET

Example:

````
 var stateBroadcast = new StateBroadcast<ExampleState>((src, result)=>{
    foreach (var kvp in result){
        Console.Log(kvp.Key + "-> " + kvp.Value.Fruit + ": " + kvp.Value.Quantity);
    }
 });
 
 //... later
 stateBroadcast.Send(new ExampleState { Fruit="apple", Quantity=2 });
```` 
Example output:
````
    192.168.0.21-> apple: 2
    192.168.0.32-> pear: 4
    192.168.0.26-> blueberry: 14
````
