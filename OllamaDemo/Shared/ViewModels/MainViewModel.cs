using CommunityToolkit.Mvvm.ComponentModel;
using OllamaDemo.LlmChat.ViewModels;
using OllamaDemo.LlmTaskRunner.ViewModels;
using OllamaDemo.SemanticSearch.ViewModels;
using OllamaDemo.StructuredData.ViewModels;
using OllamaDemo.Shared.ViewModels;

namespace OllamaDemo;

public sealed partial class MainViewModel(
        ExcelDataViewModel excelDataViewModel,
        StructuredDataViewModel structuredDataViewModel,
        SemanticSearchViewModel semanticSearchViewModel,
        LlmTaskRunnerViewModel llmTaskRunnerViewModel,
        LlmChatViewModel llmChatViewModel
    ) : ObservableObject, IDisposable
{
    [ObservableProperty]
    public partial ExcelDataViewModel ExcelDataViewModel { get; set; } = excelDataViewModel;

    [ObservableProperty]
    public partial StructuredDataViewModel StructuredDataViewModel { get; set; } = structuredDataViewModel;

    [ObservableProperty]
    public partial SemanticSearchViewModel SemanticSearchViewModel { get; set; } = semanticSearchViewModel;

    [ObservableProperty]
    public partial LlmTaskRunnerViewModel LlmTaskRunnerViewModel { get; set; } = llmTaskRunnerViewModel;

    [ObservableProperty]
    public partial LlmChatViewModel LlmChatViewModel { get; set; } = llmChatViewModel;

    public void Dispose()
    {
        foreach (var viewModel in new object[] { ExcelDataViewModel, LlmTaskRunnerViewModel, SemanticSearchViewModel })
        {
            if (viewModel is IDisposable disposable) { disposable.Dispose(); }
        }
    }

}
