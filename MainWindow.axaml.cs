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
using System.Linq;
using Avalonia.Metadata;
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
    public ObservableCollection<Game> SearchResults { get; } = new();
    private static string LibPath = Path.Combine(AppContext.BaseDirectory, "Librairie.json");
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
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON invalide : {ex.Message}");
        }
    }
    private  void Rechercher(object? sender, RoutedEventArgs e)
    {
        var q = SearchBox.Text?.Trim();
        SearchResults.Clear();
        if (string.IsNullOrEmpty(q)) return;

        var trouves = Games.Where(g =>
            g.Name != null && g.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        foreach (var g in trouves) SearchResults.Add(g);
        
    }
    private void Ajouter(object? sender, RoutedEventArgs e)
    {
        var name = GameName.Text?.Trim();
        var date = GameDate.Text?.Trim();
        var Synopsis = GameSynopsis.Text?.Trim();
        var Rating = GameRating.Text?.Trim();
        var Genre = GameGenre.Text?.Trim();
        if (string.IsNullOrEmpty(name)) return;
        if (Games.Any(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase))) return;

    Games.Add(new Game
    {
        Name = name,
        Date = date,
        Synopsis = Synopsis,
        Rating = Rating,
        Genre = Genre
    });
    SauvegarderJeux();

    GameName.Text = "";
    GameDate.Text = "";
    GameSynopsis.Text = "";
    GameRating.Text = "";
    GameGenre.Text = "";
    }
    private void SauvegarderJeux()
    {
        var json = JsonSerializer.Serialize(Games, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(LibPath, json);
    }
    
}
