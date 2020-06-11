This is an early draft/concept document detailing how scripting should be implemented in Luna. None of this is final and is subject to change at any point in time. As of the time of writing this document, none of this is implemented and should be considered a concept at best.

# Idea
Ideally, scripting with Luna will be done via reflection. You'll create a library and reference the Luna runner. Through this, you'll have access to all Luna types and such. Scripting will make heavy use of attributes. For example, we'd add attributes to denote what event/object/script the method would hook into. We could also specify when the given method would run (before/after, a specific instruction location, etc.) These scripted methods would have access to all of the runner's functions, though that will likely require some sort of special method signatures that can be called via the runner instead of retrieving data from the stack directly. Scripted methods could also inject and emit custom instructions into the parsed instruction list in LCode classes.

# Custom Events
Making custom events should be as simple as instantiating the LCode class and assigning it to an object through attributes. An example:
```csharp
[EventBind("Object1", "Create", EventBinding.Override)] // object, event, bindtype
public static LCode Object1_CustomCreate() {
    LCode _codeInst = new LCode();
    _codeInst.Instructions.Add(new Instructions.PushImmediate(new LValue(LType.String, (string)"Hello World!"))); // value
    _codeInst.Instructions.Add(new Instructions.Call("show_debug_message", 1))); // func, argcount
    _codeInst.Parse(); // parse for branches and arrays
    return _codeInst;
}
```
This code will assign the returned LCode class to the "Create" event of "Object1", it'll print out the text "Hello World!". Note `EventBinding.Override`, this will override the event.

```csharp
enum EventBinding {
    Prepend,
    Override,
    Append
}
```

The above enum would be used to either prepend (add the code onto the front of the pre-existing code), override (replace the event with the returned code), or append (add the code to the back of the pre-existing code)

# Injecting/Modifiyng Instructions
Injecting your own instructions or modifiying instructions of an event would be similar to event binding, but instead of returning a new LCode class, a method would be passed the instruction list and you'd work off of that.

```csharp
public static LValue instance_destroy2(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
    _assets.InstanceList.Remove(_environment.Instance);
    Console.WriteLine("Destroyed instance with injected event");
    return LValue.Real(0);
}

[BytecodeInject("Object1", "Create")]
public static void Object1_CustomBytecode(LCode _code) {
    List<Instruction> _instructionsGet = _code.Instructions;
    for(int i = 0; i < _instructionsGet.Count; i++) {
        if (_instructionsGet[i].Opcode == Opcode.call) {
            Instruction.Call _instructionEdit = _instructionsGet[i] as Instruction.Call;
            if (_instructionEdit.FunctionName == "instance_destroy") {
                _instructionEdit.Function = instance_destroy2;
            }
        }
    }
}
```

# Custom Assets
Creating custom assets will require all pre-existing asset classes to have a special constructor specifically for scripting, taking all required information as arguments instead of reading from the WAD.
