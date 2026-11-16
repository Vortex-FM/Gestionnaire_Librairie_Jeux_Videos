using Avalonia.Controls;
using Microsoft.VisualBasic;
using Avalonia.Interactivity;
using System.ComponentModel;
using Avalonia.Automation;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
namespace Gestionnaire_de_librairie_Jeux_Videos;

public class Game : INotifyPropertyChanged 
{
    public string Name { get; set; } ="";
    public string Date{ get; set; } ="";
    public string Synopsis{ get; set; } ="";
    public string Genre{ get; set; } ="";
    public string Rating{ get; set; } ="";
    private bool _Disponible{ get; set; } =true;
    public bool Disponible
    {
        get => _Disponible;
        set
        {
            if (_Disponible == value) return;
            _Disponible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Disponible)));
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}
public partial class MainWindow : Window
{
    private readonly ObservableCollection<Game> _Games = new();
    public MainWindow()
    {
        InitializeComponent();
        GameList.ItemsSource = _Games;
    }


}

