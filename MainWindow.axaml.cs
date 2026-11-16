using Avalonia.Controls;
using Microsoft.VisualBasic;
using Avalonia.Interactivity;
using System.ComponentModel;
using Avalonia.Automation;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System;
using System.IO;
using System.Text.Json;
namespace Gestionnaire_de_librairie_Jeux_Videos;

public class Game : INotifyPropertyChanged 
{
    public string? Name { get; set; } ="";
    public string? Date{ get; set; } ="";
    public string? Synopsis{ get; set; } ="";
    public string? Genre{ get; set; } ="";
    public string? Rating{ get; set; } ="";
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
    public ObservableCollection<string> GameInfos { get; } = new();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        ChargerJeux();

    }
    private void AfficherInfo(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Game game }) return;
        GameInfos.Clear();
        GameInfos.Add($"Nom : {game.Name}");
        GameInfos.Add($"Date : {game.Date}");
        GameInfos.Add($"Genre : {game.Genre}");
        GameInfos.Add($"Synopsis : {game.Synopsis}");
        GameInfos.Add($"Rating : {game.Rating}");
        GameInfos.Add($"Disponible : {(game.Disponible ? "Oui" : "Non")}");

    }
    private void ChargerJeux()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Librairie.json");
        if (!File.Exists(path)) return;
        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var game = JsonSerializer.Deserialize<ObservableCollection<Game>>(json, options);
            Games.Clear();
            if (game != null)
            {
                foreach (var g in game) Games.Add(g);
            }
        }
        catch(JsonException ex)
        {
            Console.WriteLine($"JSON invalide : {ex.Message}");
        }
    }


}

