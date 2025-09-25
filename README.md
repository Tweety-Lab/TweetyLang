# TweetyLang
TweetyLang is a native programming language designed around the philsophy of tools that ***Just Work***. It aims to bridge the gap between the ease of use of a high-level language like C#, and the control of low-level languages like C.

## Usage
For help using the compiler, see the compiler breakdown [here](#tweetylang-cli).

TweetyLang is build around the concept of **modules**. A module serves to structure your code and also supports behind-the-scenes compilation processes, everything in TweetyLang exists within a module.

**A Module looks like the following:**
```TweetyLang
module Program
{
    public i32 MyMethod() 
    {
        return 64;
    }
}
```
### Debugging Errors
The default TweetyLang compiler outputs errors in the following format:
```Terminal
Compilation failed in File.tl!
error(14, 15): Tried to call private function 'NumberFunc'.
```

In this example, the error occurs in `File.tl` at line `14`, column `15`.

## Compiler Architecture
The TweetyLang compiler is split up into several projects:

### TweetyLang.Parser
The Parser is responsible for tokenization and parsing of TweetyLang source code, converting a source string into a data representation through an [Abstract Syntax Tree](https://en.wikipedia.org/wiki/Abstract_syntax_tree). This AST is then used in later compilation stages. This project can also be used standalone to create tools that require parsing of TweetyLang (i.e., a program that counts all functions or lists all modules in a source file).

#### Traversing the AST:
```CSharp
// PARSE
var tree = TweetyLangSyntaxTree.ParseText(SOURCE);

// Find the module called "MyModule"
var module = tree.Root.Modules.FirstOrDefault(m => m.Name == "MyModule");

// Find first public function in the module
var function = module?.Functions.FirstOrDefault(f => f.AccessModifier == "public");

// Print its name
Console.WriteLine(function?.Name);
```

**Given this source code:**
```TweetyLang
module MyModule
{
    public bool AwesomeMethod() 
    {
        return true;
    }
}
```
**The console output would be:**
```Terminal
AwesomeMethod
```

### TweetyLang.Emitter
The Emitter takes the AST from the previous parser stage and emits [Intermediate Representation](https://en.wikipedia.org/wiki/Intermediate_representation) (IR) from it. TweetyLang uses LLVM IR, which allows for optimizations and targeting multiple hardware architectures (including the web!).

### TweetyLang (CLI)
This is the main program that brings together all the other components to compile TweetyLang code. It provides a command-line interface for building TweetyLang projects.

### CLI Commands

#### `new`
Creates a new template TweetyLang project.

#### `build`
Builds the project in the current directory.
