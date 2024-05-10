Samwise WASM allows to run the Samwise interpreter on a WebAssembly environment.

It is based on the Uno.Wasm.Bootstrap package in order to package the C# .NET code, 
and run it from a compatible browser environment or node.js;
and the Uno.Foundation.Runtime.WebAssembly package in order to execute javascript from C#.

# Compilation

In order to compile this extension you'll need .Net 6.0 SDK.

Then, run the following command within the terminal, from this folder:
dotnet build -c Release

the command will create the "out/app" folder inside the "vscode" directory
(which is the project for the Visual Studio Code extension).