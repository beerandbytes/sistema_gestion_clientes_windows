using Avalonia.Controls;
using Avalonia.Layout;

namespace GestorClientes.Helpers;

public static class DialogHelper
{
    public static async Task ShowMessageAsync(Window parent, string message, string title)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var textBlock = new TextBlock 
        { 
            Text = message, 
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(20, 20, 20, 10)
        };
        
        var button = new Button { Content = "OK", Width = 80, Margin = new Avalonia.Thickness(20, 10, 20, 20), HorizontalAlignment = HorizontalAlignment.Right };
        button.Click += (s, e) => dialog.Close();
        
        var stackPanel = new StackPanel 
        { 
            Orientation = Avalonia.Layout.Orientation.Vertical
        };
        stackPanel.Children.Add(textBlock);
        stackPanel.Children.Add(button);
        dialog.Content = stackPanel;
        
        await dialog.ShowDialog(parent);
    }

    public static async Task<bool> ShowConfirmAsync(Window parent, string message, string title)
    {
        var result = false;
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var textBlock = new TextBlock 
        { 
            Text = message, 
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(20, 20, 20, 10)
        };
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(20, 10, 20, 20)
        };
        
        var btnSi = new Button { Content = "Sí", Width = 80 };
        var btnNo = new Button { Content = "No", Width = 80 };
        
        btnSi.Click += (s, e) => { result = true; dialog.Close(); };
        btnNo.Click += (s, e) => { result = false; dialog.Close(); };
        
        buttonPanel.Children.Add(btnSi);
        buttonPanel.Children.Add(btnNo);
        
        var stackPanel = new StackPanel 
        { 
            Orientation = Avalonia.Layout.Orientation.Vertical
        };
        stackPanel.Children.Add(textBlock);
        stackPanel.Children.Add(buttonPanel);
        dialog.Content = stackPanel;
        
        await dialog.ShowDialog(parent);
        return result;
    }
}

