using Avalonia.Controls;
using Microsoft.VisualBasic;
using Avalonia.Interactivity;
using System.ComponentModel;
using Avalonia.Automation;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System;
namespace Gestionnaire_de_librairie_Jeux_Videos;

public class Game : INotifyPropertyChanged 
{
    public string? Name { get; set; } ="Path of exile";
    public string? Date{ get; set; } ="133131313";
    public string? Synopsis{ get; set; } ="1313131";
    public string? Genre{ get; set; } ="13131313";
    public string? Rating{ get; set; } ="131313131";
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
    public ObservableCollection<Game> Games { get; } = new();
    public MainWindow()
    {
        InitializeComponent();
        Games.Add(new Game());
        DataContext = this;
        foreach (var game in Games)
        {
            Console.WriteLine(game.Name);
        }
        

        // GameList.ItemsSource = _Games;
        // foreach (var game in _Games)
        // {
        //     Console.WriteLine(game.Name);
        // }
    }


}

