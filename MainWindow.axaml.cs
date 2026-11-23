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

using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;


public class Game : INotifyPropertyChanged 
{
    private string? _Name { get; set; } ="";
    public string? Date{ get; set; } ="";
    public string? Synopsis{ get; set; } ="";
    public string? Genre{ get; set; } ="";
    public string? Rating{ get; set; } ="";
    private bool _Disponible { get; set; } = true;
    public string? Url { get; set; }="";
    public string? Name
    {
        get => _Name;
        set
        {
            if (_Name == value) return;
            _Name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
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
    private Game? _jeuselectionne;
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        ChargerJeux();
    }
    private static readonly HttpClient _http = new();
    private async Task ChargerImageAsync(string? Url)
    {
        GameImage.Source = null;
        if (string.IsNullOrWhiteSpace(Url)) return;
        try
        {
            var bytes = await _http.GetByteArrayAsync(Url);
            using var ms = new MemoryStream(bytes);
            GameImage.Source = new Bitmap(ms);
        }
        catch (Exception ex) when (ex is HttpRequestException or ArgumentException
                                       or NotSupportedException or InvalidOperationException)
        {
            Console.WriteLine($"Image impossible à charger : {ex.Message}");
        }
    }
    private async void AfficherInfo(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Game game }) return;
        _jeuselectionne = game;
        GameName.Text = game.Name;
        GameDate.Text = game.Date;
        GameGenre.Text = game.Genre;
        GameRating.Text = game.Rating;
        GameSynopsis.Text = game.Synopsis;
        GameUrl.Text = game.Url;

        GameInfos.Clear();
        await ChargerImageAsync(game.Url);
        GameInfos.Add($"Nom : {game.Name}");
        GameInfos.Add($"Date : {game.Date}");
        GameInfos.Add($"Genre : {game.Genre}");
        GameInfos.Add($"Synopsis : {game.Synopsis}");
        GameInfos.Add($"Rating : {game.Rating}");
        GameInfos.Add($"Disponible : {(game.Disponible ? "Oui" : "Non")}");
    }
    private void ClikCheck(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox { DataContext: Game game } cb) return;

        game.Disponible = cb.IsChecked == true;
        SauvegarderJeux();

    }
    private void Supprimer(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Game game }) return;
        Games.Remove(game);
        SearchResults.Remove(game);
        SauvegarderJeux();
    }
    private void ChargerJeux()
    {
        
        if (!File.Exists(LibPath)) return;
        try
        {
            string json = File.ReadAllText(LibPath);
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
    private void Rechercher(object? sender, RoutedEventArgs e)
    {
        var q = SearchBox.Text?.Trim();
        SearchResults.Clear();
        if (string.IsNullOrEmpty(q)) return;

        var trouves = Games.Where(g =>
            g.Name != null && g.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        foreach (var g in trouves) SearchResults.Add(g);

        AucunResultat.IsVisible = SearchResults.Count == 0;
        PopUpResultats.IsOpen = true;
    }
    private void ChoisirResultat(object? sender, RoutedEventArgs e)
    {
        PopUpResultats.IsOpen = false;
        AfficherInfo(sender, e);
    }
    private void Ajouter(object? sender, RoutedEventArgs e)
    {
        var name = NomJeu.Text?.Trim();
        var date = DateJeu.Text?.Trim();
        var Synopsis = SynopsisJeu.Text?.Trim();
        var Rating = JeuScore.Text?.Trim();
        var Genre = GenreJeu.Text?.Trim();
        var Url = UrlJeu.Text?.Trim();
        if (string.IsNullOrEmpty(name)) return;
        if (Games.Any(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase))) return;
        Games.Add(new Game
        {
            Name = name,
            Date = date,
            Synopsis = Synopsis,
            Rating = Rating,
            Genre = Genre,
            Url = Url
        });
        SauvegarderJeux();

        NomJeu.Text = "";
        DateJeu.Text = "";
        SynopsisJeu.Text = "";
        JeuScore.Text = "";
        GenreJeu.Text = "";
        UrlJeu.Text = "";
    }
    private void ModiferAfficher(object? sender, RoutedEventArgs e)
    {
        if (_jeuselectionne == null) return;
        ModifierInfos.IsVisible = true;
    }
    private async void Modifier(object? sender, RoutedEventArgs e)
    {
        if (_jeuselectionne == null) return;
        var name = GameName.Text?.Trim();
        _jeuselectionne.Name = name;
        _jeuselectionne.Date = GameDate.Text?.Trim();
        _jeuselectionne.Synopsis = GameSynopsis.Text?.Trim();
        _jeuselectionne.Rating = GameRating.Text?.Trim();
        _jeuselectionne.Genre = GameGenre.Text?.Trim();
        _jeuselectionne.Url = GameUrl.Text?.Trim();
        SauvegarderJeux();
        ModifierInfos.IsVisible = false;
    }
    private void SauvegarderJeux()
    {
        var json = JsonSerializer.Serialize(Games, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(LibPath, json);
    }
}
