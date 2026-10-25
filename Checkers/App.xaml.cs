using System.Windows;
using Checkers.View;
using Checkers.ViewModel;

namespace Checkers;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var viewModel = new GameViewModel();

        var mainWindow = new View.MainWindow
        {
            DataContext = viewModel
        };

        mainWindow.Show();
    }
}
