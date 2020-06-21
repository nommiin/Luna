# Constructors
Below is notes/research on how constructors are handled in GML

## Bytecode:
```
00000000 : 080000b6                 b         0x00000020

00000004 : ffff05c3030000a0         pushb.v   $argument0$
0000000c : 010002d904000000         call.i    $show_debug_message$
00000014 : 0000059e                 popz.v
00000018 : f9ff0fff                 setstatic -7
0000001c : 0000029d                 exit.i
00000020 : 000002c005000000         push.i    5
00000028 : 00005207                 conv.i.v
0000002c : 000002d906000000         call.i    $@@NullObject@@$
00000034 : 020002d907000000         call.i    $method$
0000003c : 00000586                 dup.v
00000040 : faff0f84                 pushi.e   -6
00000044 : 0000554508000080         pop.v.v   $Test$
0000004c : 0000059e                 popz.v
00000050 : 20000f84                 pushi.e   32
00000054 : 00005207                 conv.i.v
00000058 : 000002c005000000         push.i    5
00000060 : 00005207                 conv.i.v
00000064 : 020002d909000000         call.i    $@@NewGMLObject@@$
0000006c : ffff55450a0000a0         pop.v.v   $a$
```

## Annotated:
```js
// Constructor Body
show_debug_message(argument0);

Test = method(@@NullObject@@(), 5);

@@NewGMLObject@@(5, 32);
```

## Notes:
- It looks like the constructor body is stored at the top of the event
