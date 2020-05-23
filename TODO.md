Here's just a small list of things that I wanna do, so stuff works properly!
## Current
- push instruction machine broke (LArgumentType.Variable seems to be wrong)
- implement a special Luna value class instead of using `dynamic`
- make `Interpreter` class have a stack and variable list that can be used for different scopes, i guess? ie: support `with` statement
- use OpenTK to render graphics and retrieve user input

## Long Term
- implement reflection-based scripting that allows the user to hook into events/scripts and inject their own bytecode (sorta like Mono.Cecil)
- allow assets to be added at runtime like sprites, sounds, etc
- run any somewhat complicated game in Luna
- implement realtime debugging/watching for built games
- disassembly viewer for viewing script bytecode
- pre-parsing of `LCode` classes to improve performance (instead of decoding instructions on the fly, we'll decode the whole script on load and then interpret the list of decoded instructions and arguments)
- proper error catching
- parsing EVERY chunk
- implement support for running pre-2.3 WADs (probably not)