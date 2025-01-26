# AiChatSample
An local AI chat example written in C# with WPF and Blazor.

![Logo](https://raw.githubusercontent.com/chrbaeu/AiChatSample/refs/heads/main/AiChatSample/Screenshot.png)

Microsoft has simplified the use of AI models in C# with the [Microsoft.Extensions.AI](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/) NuGet package.

This repository contains a simple example of an local AI chat in .NET that was implemented with C# using WPF and Blazor.

The local AI model is executed and accessed via [Ollama](
https://ollama.com/). The URL and the model can be configured in the “appsettings.json”.

The example also shows how tool calling works by providing the model with a function for retrieving the current time and the ability to enable and disable the dark mode.

As more advanced features, the sample also includes the integration of a vision model for image analysis and the option to provide data as embeddings used in that chat as additional context.

![Logo](https://raw.githubusercontent.com/chrbaeu/AiChatSample/refs/heads/main/AiChatSample/Screenshots.png)