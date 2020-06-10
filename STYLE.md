This WIP document was made to cover the expected code-style for this repository. Please follow this style as closely as possible to make pull requests as simple as possible. Note that the examples below are mostly non-sensical and written for the sake of being examples.

# Conditionals
### When writing a conditional, be explicit with the expected result.

✔️ Do:
```csharp
bool a = true;
if (a == true) {
    //
} else if (a == false) {
    //
}
```

❌ Don't:
```csharp
bool a = true;
if (a) {
    //
} else if (!a) {
    //
}
```

# Referencing Class Members
### When writing code inside of a classes' method, refer to members using `this.`.

✔️ Do:
```csharp
class MyClass {
    public int a = 10;
    public int b = 10;

    public int Add() {
        return this.a + this.b;
    }
}
```

❌ Don't:
```csharp
class MyClass {
    public int a = 10;
    public int b = 10;

    public int Add() {
        return a + b;
    }
}
```

# Local Variables
### When using local variables, prefix their names using an underscore. This *does not* need to be followed for variables used as iterators in loops.

✔️ Do:
```csharp
string _myCharacterName = "John";
Console.WriteLine("My name is " + _myCharacterName);
```

❌ Don't:
```csharp
string myCharacterName = "John";
Console.WriteLine("My name is " + myCharacterName);
```

# Types
### Avoid using `var` for types whenever possible. This *does not* need to be followed for one-off variables.

✔️ Do:
```csharp
LInstance _instGlobal = _game.Instances[(double)LVariableScope.Global];
double _instX = _instGlobal.Variables["x"];
Console.WriteLine("Instance X position is: {0}", _instX);
```

❌ Don't:
```csharp
var _instGlobal = _game.Instances[(double)LVariableScope.Global];
var _instX = _instGlobal.Variables["x"];
Console.WriteLine("Instance X position is: {0}", _instX);
```

# Brackets
### Brackets of all kinds should be put on the same line as the loop or conditional

✔️ Do:
```csharp
int a = 0;
if (a > 0) {
    //
}

for(int i = 0; i < 100; i++) {
    //
}
```

❌ Don't:
```csharp
int a = 0;
if (a > 0)
{
    //
}

for(int i = 0; i < 100; i++)
{
    //
}
```