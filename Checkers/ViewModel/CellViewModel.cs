using Checkers.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Checkers.ViewModel;

public partial class CellViewModel : ObservableObject
{
    public Position Position { get; init; }

    [ObservableProperty]
    private bool _borderOpacity = true;

    public required SolidColorBrush BackgroundColor{ get; init; }

    [ObservableProperty]
    private BitmapImage? _figureImage = null;
    public RelayCommand? ClickCommand { get; set; }
}
