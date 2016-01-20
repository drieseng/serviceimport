# ServiceImport

ServiceImport is a flexible wrapper around the WCF import infrastructure allowing far more complete support for contract-first Web Services.
It does so by applying a set of extensions before, while and after importing the WSDL and XSD contract(s).

While code-first is believed to be the more common (and widely accepted) approach in the .NET ecosystem, the experience I gathered while working on J2EE and SOA projects made this hard for me to swallow.

This small framework was born out of my strong desire to apply the contract-first style in the .NET environment I'm currently professionally - and passionately - engaged in.
Even though [svcutil](https://msdn.microsoft.com/en-us/library/aa347733(v=vs.110).aspx) and WCF in general do provide a good starting point engaging in the contract-first Web Services approach, there are all too many (both written and unwritten) restrictions that I could not cope with.
More on that later.

Developed in my precious and rare leisure time, this framework is currently tailored to my specific professional needs.
Increasing the level of configurability should be a rather minor task though.

## Why contract-first?

### Richness

XML Schema is a DSL allowing constructs and precisions that are impossible to represent in a class diagram.
It does so in way that is easy to understand and visualize.

**TO DO:** example of XSD fragment

### Versioning

**TO DO:** example of XSD fragment

### (Lack of) Friction

Introducing changes to a contract should be an explicit and very deliberate action.
In a code-first approach, there's all too little friction to keep one from making undesired changes.
Your mileage may vary of course.
 
## Features

### Abstract types

### IExtensibleDataObject

### Nillable / MinOcurs

### Capitalization style

### Support for additional XML Schema data types

### Reply action

