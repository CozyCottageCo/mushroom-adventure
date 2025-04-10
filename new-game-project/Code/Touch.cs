using Godot;
using System;
using SieniPeli;
using System.Collections.Generic;

namespace SieniPeli
{
    public partial class Touch : Node2D
    {
        private Line2D _line;
        private TileMapLayer _tileMapLayer;
        private bool _drawing = false;
        private bool _drawingDone = false;

        private Sieni _sieni;
        [Export] AudioStreamPlayer2D _nappi1 = null;
        [Export] AudioStreamPlayer2D _nappi2 = null;
        [Export] AudioStreamPlayer2D _nappi3 = null;

        private Sprite2D _täplä;
        private Vector2I _täpläTile;

        // Grid and screen settings
        private int tileWidth = 32;
        private int tileHeight = 32;
        private int screenWidth = 640;
        private int screenHeight = 360;

        private List<Vector2I> tilesUsed = new List<Vector2I>();
        private List<Vector2I> savedPath = new List<Vector2I>();
        private Vector2I lastMapCoord = new Vector2I(-1, -1); // alustettu null vector viimeselle positiolle

        private ColorRect highLightRect;
        private Button _goButton;
        private Button _redoButton;


        private TextureButton _menuButton;

        private Control _menuPanel = null; // alustettu menupaneeli
        public Control _kolariScreen = null;
        public Control _kolariScreenTie = null;
        public Control _kolariScreenVesi = null;

        public Control _kolariScreenVajaa = null;

        public TextureRect _voittoScreen = null;
        private bool _buttonsVisible = false;
        public bool _kolariActive = false;
        Color lineColor = new Color(1.0f, 0.0f, 0.0f, 0.5f); //sama ku Colors.Red,50% opacity

        private SettingsController settings = null;
        private bool toggleMode = false;
        private bool stopBool = false;


        public override void _Ready()
        {

                var musicPlayer = GetNode<MusicPlayer>("/root/MusicPlayer");
                musicPlayer.PlayMusicForCurrentScene();
                // tason musalataus

             Node currentScene = GetTree().CurrentScene; // katotaa current scene
            string scenePath = currentScene.SceneFilePath;
            string sceneName = scenePath.GetFile().Replace(".tscn", "");
            SaveManager saveManager = GetNode<SaveManager>("/root/SaveManager"); // savemanager alustus

        if (!saveManager.IsLevelCompleted(sceneName)) { // tarkistetaa onko current level tehty jo; jos ei, katotaa tuleeks tutoriaali
            if (sceneName == "Level1") { // eka tutoriaali lvl 1 jne
                var tutorial = GetNode<TutorialScene>("TutorialScene");
                tutorial.TutorialActivated(1); // magic number joo mut aktivoi ekan tutoriaalin jne
            }
             if (sceneName == "Level2") {
                var tutorial = GetNode<TutorialScene>("TutorialScene");
                tutorial.TutorialActivated(2);
            }
             if (sceneName == "Level7") {
                var tutorial = GetNode<TutorialScene>("TutorialScene");
                tutorial.TutorialActivated(3);
            }
        }


            settings = GetNode<SettingsController>("/root/SettingsController");
            if (settings == null) {
                GD.PrintErr("no find settings");
            }

            _line = new Line2D // settingsit piirtoviivalle
            {

                Width = 5,
                DefaultColor = lineColor
            };
            AddChild(_line);


            _menuPanel = GetNode<Control>("PauseMenuPanel"); // haetaa menupaneeli
             if (_menuPanel != null)
            {
                _menuPanel.Visible = false; // piilos eka
                GD.Print("Menu found"); //debug
            } else {
                GD.Print("Menu not found");
            }

            _kolariScreen = GetNodeOrNull<Control>("KolariScreenÖtökkä");
    if (_kolariScreen != null)
    {
        GD.Print("KolariScreenTie found");

        _kolariScreen.Visible = false;
        // Ensure button is connected
    }
    else
    {
        GD.PrintErr("KolariScreenÖtökkä not found");
    }

     _kolariScreenTie = GetNodeOrNull<Control>("KolariScreenTie");
    if (_kolariScreenTie != null)
    {
        GD.Print("KolariScreenTie found");

        _kolariScreenTie.Visible = false;
        // Ensure button is connected
    }
    else
    {
        GD.PrintErr("KolariScreenTie not found");
    }

     _kolariScreenVesi = GetNodeOrNull<Control>("KolariScreenVesi");
    if (_kolariScreenVesi != null)
    {
        GD.Print("KolariScreenTie found");

        _kolariScreenVesi.Visible = false;
        // Ensure button is connected
    }
    else
    {
        GD.PrintErr("KolariScreenVesi not found");
    }

     _kolariScreenVajaa = GetNodeOrNull<Control>("KolariScreenVajaa");
    if (_kolariScreenVajaa != null)
    {
        GD.Print("KolariScreen Vajaa found");

        _kolariScreenVajaa.Visible = false;
        // Ensure button is connected
    }
    else
    {
        GD.PrintErr("KolariScreenVajaa not found");
    }


             _voittoScreen = GetNode<TextureRect>("VoittoScreen"); // haetaa kolariscreeni
             if (_voittoScreen != null)
            {
                _voittoScreen.Visible = false; // piilos eka
                GD.Print("VoittoScreen found"); //debug
            } else {
                GD.Print("VoittoScreen not found");
            }
            _sieni = GetNode<Sieni>("Sieni");

            GD.Print("Sieni node successfully instantiated and added to scene.");

            _täplä = GetNode<Sprite2D>("Täplä"); // haetaa täplä ja sen positio
            _täpläTile = new Vector2I((int)(_täplä.GlobalPosition.X / tileWidth), (int)(_täplä.GlobalPosition.Y / tileHeight));

            highLightRect = new ColorRect
            {
                Color = new Color(1f, 1f, 0f, 0.4f), // Yellow transparent
                Size = new Vector2(tileWidth, tileHeight),
                Visible = false
            };
            AddChild(highLightRect);

            _goButton = GetNode<Button>("/root/Node2D/Go"); // go ja redo buttonit käyttöön
            _redoButton = GetNode<Button>("/root/Node2D/Redo");
            _menuButton = GetNode<TextureButton>("/root/Node2D/Menu");

            _goButton.Visible = false;
            _redoButton.Visible = false; // aluksi piiloon

            _goButton.Connect("pressed", new Callable(this, nameof(OnGoButtonPressed))); // funktio (painettu) ja metodikutsu mitä si tapahtuu
            _redoButton.Connect("pressed", new Callable(this, nameof(OnRedoButtonPressed)));
            _menuButton.Connect("pressed", new Callable(this, nameof(OnMenuButtonPressed)));




         }

        public override void _Input(InputEvent @event)
        {
         if (_buttonsVisible) // ei sallita mitään muuta ku napit ku ne o näkyvis
                {
                    if (@event is InputEventScreenTouch touchButton && touchButton.Pressed)
                    {
                        if (_goButton.GetRect().HasPoint(touchButton.Position) || _redoButton.GetRect().HasPoint(touchButton.Position))
                        {
                            return; // ^ nappien koskeminen ok, minkään muun ei
                        }
                    }
                    return;
                }
                // Jos napit ei näkyvil, edetään..
                // pausetus koskemalla näyttöön
                if (@event is InputEventScreenTouch screenTouch)
                {
                    toggleMode = settings.GetMovementToggle(); // haetaan asetusten current toggletilanne

                GD.Print($"Current toggle = {toggleMode}, settings {settings.GetMovementToggle()}");
                    if (toggleMode) { // jos toggle asetuksis true
                        if (screenTouch.IsPressed()) {
                            if (_sieni.isMoving)// jos sallittu kosketus & sieni on liikkeessä
                        {
                            if (!stopBool) {
                            stopBool = true; // erilline bool joka pitää paikallaa vaik ei paina putkeen
                            _sieni.controlSpeed(0); // Speed nollaan

                        } else {
                                stopBool = false; // erilline bool päästää seuraavasta klikistä liikkeelle taas
                                _sieni.controlSpeed(75);
                            }
                        }
                    }
                } else {

                if (screenTouch.IsPressed()) // jos togglemode false, mennää perus: kun kosketaan...
                    {
                        if (_sieni.isMoving) // jos sallittu kosketus & sieni on liikkeessä
                        {
                            _sieni.controlSpeed(0); // Speed nollaan
                        }
                    }
                    else // jos ei koske / vapauttaa
                    {
                        if (_sieni.isMoving) // ja sieni on liikkeessä
                        {
                            _sieni.controlSpeed(75); // Lets mennään lehmät
                        }
                    }
                }
                }

                // jos sieni ei liikkeessä (eikä ne napit ruudul), saa piirtää
                if (@event is InputEventScreenTouch touch && !_sieni.isMoving)
                {
                    if (touch.Pressed)
                    {
                        Vector2I sieniTile = new Vector2I ( // Muuttaa sienen koordinaatit tile koordinaateiksi
                            (int)(_sieni.GlobalPosition.X / tileWidth),
                            (int)(_sieni.GlobalPosition.Y / tileHeight)
                        );
                        GD.Print(sieniTile);

                        Vector2I touchTile = new Vector2I( // Muuttaa piirretyn viivan tile koordinaateiksi
                            (int)(touch.Position.X / tileWidth),
                            (int)(touch.Position.Y / tileHeight)
                        );

                        if (sieniTile != touchTile) // Ingoorataan piirretty viiva jos se ei ala sienen koordinaateista
                        {
                            GD.Print("Invalid start position! You must start drawing from Sieni's position.");
                            return;
                        }

                        tilesUsed.Clear(); // tyhjennetää käytetyt koordinaatit jne
                        savedPath.Clear();
                        savedPath.AddRange(tilesUsed);

                        _line.Points = new Vector2[0]; // tyhjennetää viiva
                        _drawing = true; // piirto päälle

                        //GD.Print($"Touch started at ({touch.Position.X}, {touch.Position.Y})");
                        UpdateTileAtPosition(touch.Position); // kursorin highlight metodikutsu
                    }
                    if (!touch.Pressed && _drawing) // jos kosketus irtoo kesken piirtämisen
                    {
                        _drawing = false; // piirto loppu

                     if (tilesUsed.Count <= 3)
                        {
                            GD.Print("Path is too short!");
                            tilesUsed.Clear();
                            savedPath.Clear();
                            _line.Points = new Vector2[0];
                            _goButton.Visible = false;
                            _redoButton.Visible = false;
                            _buttonsVisible = false;
                            return;

                        }

                        GD.Print($"Touch released at ({touch.Position.X}, {touch.Position.Y})");

                        /*Vector2I lastTile = tilesUsed[tilesUsed.Count -1]; // katotaa mihi jäi viiva
                        if (lastTile != _täpläTile) { // jos viivan loppu != täplän positio
                            GD.Print("Line must end at täplä!"); // ei kelpaa
                            tilesUsed.Clear(); //uusiks reset käytetyt tiilet
                            _line.Points = new Vector2[0]; // uusiks reset viiva
                            return;
                        }*/ // koodi, jos halutaan ettei piirto voi loppua muualle kuin täplään

                        _goButton.Visible = true; // napit näkyviin
                        _redoButton.Visible = true;
                        _buttonsVisible = true; // Napit näkyvil true = piirto estetty

                        GD.Print("Tiles reached during the touch:");
                        foreach (var tile in tilesUsed)
                        {
                            GD.Print($"Tile: ({tile.X}, {tile.Y})"); // debuggia jne
                        }
                    }
                }
                else if (@event is InputEventScreenDrag drag && _drawing) // ja itse piirto
                {
                    _line.AddPoint(drag.Position);
                   // GD.Print($"Dragging at ({drag.Position.X}, {drag.Position.Y})");
                    UpdateTileAtPosition(drag.Position);
                }

        }

        private void OnGoButtonPressed() { // kun go nappia painettu, liikutaan
            _nappi1.Play();
            GD.Print("Go chosen! Sieni is moving...");
            _sieni?.Move(tilesUsed.ToArray()); // reitti parametrina sienen liikutuksel (en ees tie mitä toi ? tekee)
            _goButton.Visible = false;
            _redoButton.Visible = false; // ja napit taas pois
            _buttonsVisible = false;
        }

        private void OnRedoButtonPressed() { // kun redo nappia painetaan, reset kaikki, napit piiloon taas
            _nappi2.Play();
            GD.Print("Redo needed! Resetting...");
            tilesUsed.Clear();
            tilesUsed.AddRange(savedPath);

            _goButton.Visible = false;
            _redoButton.Visible = false;
            _buttonsVisible = false;
            highLightRect.Visible = false;
            _line.Points = new Vector2[0];
        }
        // Method to handle both tile tracking and highlighting
        private void UpdateTileAtPosition(Vector2 position) // träkkää piirtämisen tiilet ja highlightaa niitä
        {
            // Adjust position based on margin (deleted) and check bounds
            Vector2 adjustedPosition = position;
            if (adjustedPosition.X < 0 || adjustedPosition.Y < 0 || adjustedPosition.X >= screenWidth || adjustedPosition.Y >= screenHeight )
            {
                GD.Print($"Adjusted position {adjustedPosition} is out of bounds, skipping.");
                return; //ei kyl skippaa mitää atm :D
            }

            // Convert to tile grid coordinates
            int tileX = (int)(adjustedPosition.X / tileWidth);
            int tileY = (int)(adjustedPosition.Y / tileHeight);


            if (tilesUsed.Count > 0)
                 {
                    Vector2I lastTile = tilesUsed[tilesUsed.Count - 1]; // träkätää viimestä tiiliä

                    // tarkistetaan onko diagonaali liikkuminen (molemmat X&Y nousee eikä vaan toinen (1,1))
                    if (Math.Abs(lastTile.X - tileX) > 0 && Math.Abs(lastTile.Y - tileY) > 0)
                    {
                        GD.Print("Diagonal movement detected, adding ghost tile.");

                        // Alustetaan haamutiili
                        Vector2I ghostTile;

                        // Katotaan onko järkevämpää mennä sivusuunnassa vai pystysuunnassa
                        if (Math.Abs(lastTile.X - tileX) > Math.Abs(lastTile.Y - tileY)) // jos vaakasuunta vahvempi (1,1 -> 3,2: X kasvaa 2, Y vaan 1, X voittaa)
                        {
                            ghostTile = new Vector2I(lastTile.X, tileY); // liikutaan vaakasuunnassa
                        }
                        else
                        {
                            ghostTile = new Vector2I(tileX, lastTile.Y); // muuten pystysuunnassa
                        }

                        // ja siis lisätää vaan sillon haamutiili jos se ei oo viimisin (eli duplikaatti maholline, mut ei jää junnaa)
                        if (!ghostTile.Equals(lastTile))
                        {
                            tilesUsed.Add(ghostTile); // lisätää välitiili
                            GD.Print($"Added ghost tile: {ghostTile}");
                        }
                    }
                }


            // lisätää tiili normaalisti / perään jos oli diagonal ja haamutiili eka

            Vector2I mapCoords = new Vector2I(tileX, tileY);
          //  GD.Print($"Touch at ({position.X}, {position.Y}) maps to tile ({tileX}, {tileY})"); debugviesti

            // Update the highlight
            highLightRect.Position = new Vector2(
                tileX * tileWidth + (tileWidth - highLightRect.Size.X) / 2 ,
                tileY * tileHeight + (tileHeight - highLightRect.Size.Y) / 2);
            highLightRect.Visible = true; // highlight-neliö seuraa kursorii

            // Add tile to the list IF it is not the last used tile (to allow for duplicate coordinates, e.g retracing steps)

            if (!mapCoords.Equals(lastMapCoord))
            {
                tilesUsed.Add(mapCoords); // vaan uudet koordinaatit tallentuu (32 jaolliset aina ku int?)
                lastMapCoord = mapCoords;
                GD.Print(mapCoords);
            }
        }

        public async void OnMenuButtonPressed() { // menunappia painettaes paneeli näkyvii
        if (_menuPanel != null)
            {
                _menuPanel.Visible = !_menuPanel.Visible; // jos näkyvis, pois, ja päinvastoi
                if(_menuPanel.Visible == true) {
                    _nappi3.Play();
                    await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                    GetTree().Paused = true;
                    _buttonsVisible = true; // samal flipataa tää buttonsvisible ettei paina läpi
                } else if(_menuPanel.Visible == false) {
                    _nappi2.Play();
                    await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                    GetTree().Paused = false;
                    _buttonsVisible = false;
                }
    }
        }

        public async void Kolari(string kohde) {
            GD.Print(kohde);
            if (_kolariActive) return; // Prevent multiple calls
            _kolariActive = true;
            if (kohde != "Tie" && kohde != "Vesi" && kohde != "Vajaa") { // atm vaa jos ei oo vesi tai tie, mut omil ötököil omat
                if (_kolariScreen != null)
                    {
                    _kolariScreen.Visible = !_kolariScreen.Visible; // jos näkyvis, pois, ja päinvastoi
                    if(_kolariScreen.Visible == true) {
                        _buttonsVisible = true; // samal flipataa tää buttonsvisible ettei paina läpi
                        GetTree().Paused = true; // pausettaa pelin myös
                    } else if(_kolariScreen.Visible == false) {
                        _buttonsVisible = false;
                    }
                }
            }
             else if (kohde.StartsWith("Tie")) { // oma screen
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout"); // hetke venailu, että ehtii kävellä tielle
                if (_kolariScreenTie != null)
                    {
                    _kolariScreenTie.Visible = !_kolariScreenTie.Visible; // jos näkyvis, pois, ja päinvastoi
                    if(_kolariScreenTie.Visible == true) {
                        _buttonsVisible = true; // samal flipataa tää buttonsvisible ettei paina läpi
                        GetTree().Paused = true;
                    } else if(_kolariScreenTie.Visible == false) {
                        _buttonsVisible = false;
                    }
                }
            }
            else if (kohde.StartsWith("Vesi")) { // oma screen
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout"); // hetke venailu, että ehtii kävellä tielle
                if (_kolariScreenVesi != null)
                    {
                    _kolariScreenVesi.Visible = !_kolariScreenVesi.Visible; // jos näkyvis, pois, ja päinvastoi
                    if(_kolariScreenVesi.Visible == true) {
                        _buttonsVisible = true; // samal flipataa tää buttonsvisible ettei paina läpi
                        GetTree().Paused = true; // pausettaa pelin myös
                    } else if(_kolariScreenVesi.Visible == false) {
                        _buttonsVisible = false;
                    }
                }
            }
            else if (kohde.StartsWith("Vajaa")) {
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout"); // hetke venailu, että ehtii astua ruutuu
                if (_kolariScreenVajaa != null)
                    {
                    _kolariScreenVajaa.Visible = !_kolariScreenVajaa.Visible; // jos näkyvis, pois, ja päinvastoi
                    if(_kolariScreenVajaa.Visible == true) {
                        _buttonsVisible = true; // samal flipataa tää buttonsvisible ettei paina läpi
                        GetTree().Paused = true; // pausettaa pelin myös
                    } else if(_kolariScreenVesi.Visible == false) {
                        _buttonsVisible = false;
                    }
                }
            }
        }

        public void Voitto() { // voittopaneeli näkyville, piirto jne pois
            if (_voittoScreen != null)
            {
                _voittoScreen.Visible = !_voittoScreen.Visible; // jos näkyvis, pois, ja päinvastoi
                if(_voittoScreen.Visible == true) {
                    _buttonsVisible = true; // samal flipataa tää buttonsvisible ettei paina läpi
                } else if(_voittoScreen.Visible == false) {
                    _buttonsVisible = false;
                }
        }
        }

 // Empty Process method, not needed in this context
        public override void _Process(double delta) {

        }


    }
}


