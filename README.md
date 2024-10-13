# AiChatSample
An local AI chat example written in C# with WPF and Blazor.

![Screenshot](https://github.com/chrbaeu/AiChatSample/blob/4e50c97b3efdafa4a8277440336ccb3b9abf7868/Screenshot.png?raw=true)

Microsoft has simplified the use of AI models in C# with the [Microsoft.Extensions.AI](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/) NuGet package.

This repository contains a simple example of an local AI chat in .NET that was implemented with C# using WPF and Blazor.

The local AI model is executed and accessed via [Ollama](
https://ollama.com/). The URL and the model can be configured in the “appsettings.json”.

The example also shows how tool calling works by providing the model with a function for retrieving the current time. This allows the model to answer questions about the current time when the tools are activated.