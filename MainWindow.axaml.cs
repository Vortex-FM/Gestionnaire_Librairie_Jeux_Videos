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
    private Bitmap? _cover;

    [JsonIgnore]   
    public Bitmap? Cover
    {
        get => _cover;
        set
        {
            if (_cover == value) return;
            _cover = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cover)));
        }
    }
}
public partial class MainWindow : Window
{
    public ObservableCollection<Game> Games { get; } = new();
    public ObservableCollection<Game> SearchResults { get; } = new();
    private static string LibPath = Path.Combine(AppContext.BaseDirectory, "Librairie.json");
    private Game? _jeuselectionne;
    private static readonly HttpClient _http = CreerClient();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        ChargerJeux();
        _ = ChargerCouverturesAsync();
    }
    private static HttpClient CreerClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) GestionnaireJeux/1.0");
        return client;
    }
     private async Task ChargerCouverturesAsync()
    {
        await Task.WhenAll(Games.ToList().Select(async g => g.Cover = await ChargerBitmapAsync(g.Url)));
    }

    private async Task<Bitmap?> ChargerBitmapAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            var bytes = await _http.GetByteArrayAsync(url);
            using var ms = new MemoryStream(bytes);
            return new Bitmap(ms);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Image impossible à charger ({url}) : {ex.Message}");
            return null;
        }
    }
    private async void AfficherInfo(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Game game }) return;
        AfficherJeu(game);
    }
    private void AfficherJeu(Game game)
    {

        _jeuselectionne = game;
        PanneauInfos.DataContext = null;   
        PanneauInfos.DataContext = game;
        PanneauInfos.IsVisible = true;
        ModifierInfos.IsVisible = false;

        GameName.Text = game.Name;
        GameDate.Text = game.Date;
        GameGenre.Text = game.Genre;
        GameRating.Text = game.Rating;
        GameSynopsis.Text = game.Synopsis;
        GameUrl.Text = game.Url;
    }
    private async void ClikCheck(object? sender, RoutedEventArgs e)
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

    if (_jeuselectionne == game)
    {
        _jeuselectionne = null;
        PanneauInfos.DataContext = null;
        PanneauInfos.IsVisible = false;
        ModifierInfos.IsVisible = false;
    }
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
     private void SauvegarderJeux()
    {
        var json = JsonSerializer.Serialize(Games, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(LibPath, json);
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
    private async void Ajouter(object? sender, RoutedEventArgs e)
    {
        var name = NomJeu.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            Ajouterinfos.IsVisible = false;
            return;
        }
        if (Games.Any(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase))) return;

        var jeu = new Game
        {
            Name = name,
            Date = DateJeu.Text?.Trim(),
            Synopsis = SynopsisJeu.Text?.Trim(),
            Rating = JeuScore.Text?.Trim(),
            Genre = GenreJeu.Text?.Trim(),
            Url = UrlJeu.Text?.Trim()
        };
        Games.Add(jeu);
        SauvegarderJeux();

        NomJeu.Text = DateJeu.Text = SynopsisJeu.Text = JeuScore.Text = GenreJeu.Text = UrlJeu.Text = "";
        jeu.Cover = await ChargerBitmapAsync(jeu.Url);
        Ajouterinfos.IsVisible = false;
    }
    private void ModiferAfficher(object? sender, RoutedEventArgs e)
    {
        if (_jeuselectionne == null) return;
        ModifierInfos.IsVisible = true;
    }
    private void AfficherAjouterInfos(object? sender, RoutedEventArgs e) => Ajouterinfos.IsVisible = true;
        private async void Modifier(object? sender, RoutedEventArgs e)
    {
        if (_jeuselectionne == null) return;
        var name = GameName.Text?.Trim();
        if (string.IsNullOrEmpty(name)) return;

        var jeu = _jeuselectionne;
        var ancienneUrl = jeu.Url;

        jeu.Name = name;
        jeu.Date = GameDate.Text?.Trim();
        jeu.Synopsis = GameSynopsis.Text?.Trim();
        jeu.Rating = GameRating.Text?.Trim();
        jeu.Genre = GameGenre.Text?.Trim();
        jeu.Url = GameUrl.Text?.Trim();
        SauvegarderJeux();

        AfficherJeu(jeu);                      
        if (ancienneUrl != jeu.Url)
            jeu.Cover = await ChargerBitmapAsync(jeu.Url);
    }
}
