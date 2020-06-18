![Banner](https://github.com/nommiin/Luna/raw/master/Assets/LunaBanner.png)
<p align="center">
  <b>A custom implementation of the GameMaker 2.3 runtime in C#</b>
</p>

# Features
- Parses the GameMaker IFF file for some assets
- Allows for object instantiation
- Runs an event loop and executes objects' bytecode
- Implements input and rendering via OpenTK

# Why?
Nothing like this really exists. I don't want to make this to be some sort of super performant crazy alternative to the base GameMaker runner, instead I think it'd be cool to implement improved/new features the base Runner doesn't have. Long term I'd like to have Luna running most GM games (unlikely). Maybe I could add some sort of scripting implementation to allow for modding games at runtime. Who knows! This is mostly just a fun project for learning how stuff works. Do I know what I'm doing? Not really!

# Goals
See "Long Term" header in [TODO.md](https://github.com/nommiin/Luna/blob/master/TODO.md)

# Notes
- GameMaker is copyright of [YoYo Games Ltd.](https://www.yoyogames.com/)
- Target Runtime: 23.1.1.146

# Special Thanks
- Thanks to [@Shana6](https://github.com/Shana6) for the numerous contributions 
- Thanks to [@YellowAfterlife](https://github.com/YellowAfterlife) for assistance with figuring out bytecode
- Thanks to [@DatZach](https://github.com/DatZach) for assistance in improving Luna's performance early on
- Thanks to [@i_am_thirteen](https://twitter.com/i_am_thirteen) for designing Luna's logo
