# Echo.Net

A lightweight ultra-fast UDP library to broadcast application states across a LAN

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

## History
- seperated example code project and library dll project. general code cleanup.
- switched from Newtonsoft.Json to ZeroFormatter. Now all codegen happens at compile-time, broadcasts are direct byte arrays for maximum performance. Removes all overhead of json/string/refection and other runtime penalties. 
- added algorithm to correctly resolve subnet mask and broadcast address from the assigned IP address. This allows echo to work on any virtual all LAN setups out of the box with zero configuration
