using Microsoft.Maui.Controls;
using WGUTermTracker.ViewModels;

namespace WGUTermTracker.Views;

[QueryProperty(nameof(Editor), "Editor")]
public partial class AssessmentDetailPage : ContentPage
{
    private AssessmentEditorViewModel? editor;

    public AssessmentDetailPage()
    {
        InitializeComponent();
    }

    public AssessmentEditorViewModel? Editor
    {
        get => editor;
        set
        {
            if (value is null)
            {
                return;
            }

            editor = value;
            BindingContext = value;
        }
    }
}