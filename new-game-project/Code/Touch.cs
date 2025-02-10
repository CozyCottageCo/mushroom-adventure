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

        // Grid and screen settings
        private int tileWidth = 32;
        private int tileHeight = 32;
        private int screenWidth = 640;
        private int screenHeight = 480;
        private int margin;

        private List<Vector2I> tilesUsed = new List<Vector2I>();
        private List<Vector2I> savedPath = new List<Vector2I>();

        private ColorRect highLightRect;
        private Button _goButton;
        private Button _redoButton;
        private bool _buttonsVisible = false;

        public override void _Ready()
        {
            _line = new Line2D
            {
                Width = 5,
                DefaultColor = Colors.Red
            };
            AddChild(_line);

            // Get tilemap layer
            _tileMapLayer = GetNode<TileMapLayer>("Map");
            if (_tileMapLayer == null)
            {
                GD.PrintErr("Error: TileMapLayer 'Map' not found!");
            }
            else
            {
                GD.Print("TileMapLayer 'Map' successfully found.");
            }

            PackedScene sieniScene = ResourceLoader.Load<PackedScene>("res://Level/Sieni.tscn");
            _sieni = (Sieni)sieniScene.Instantiate();
            AddChild(_sieni);

            GD.Print("Sieni node successfully instantiated and added to scene.");

            highLightRect = new ColorRect
            {
                Color = new Color(1f, 1f, 0f, 0.2f), // Yellow transparent
                Size = new Vector2(tileWidth, tileHeight),
                Visible = false
            };
            AddChild(highLightRect);

            _goButton = GetNode<Button>("/root/Node2D/Go"); // go ja redo buttonit käyttöön
            _redoButton = GetNode<Button>("/root/Node2D/Redo");
            _goButton.Visible = false;
            _redoButton.Visible = false; // aluksi piiloon

            _goButton.Connect("pressed", new Callable(this, nameof(OnGoButtonPressed))); // funktio (painettu) ja metodikutsu mitä si tapahtuu
            _redoButton.Connect("pressed", new Callable(this, nameof(OnRedoButtonPressed)));
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
                    if (screenTouch.IsPressed()) // kun kosketaan...
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

                // jos sieni ei liikkeessä (eikä ne napit ruudul), saa piirtää
                if (@event is InputEventScreenTouch touch && !_sieni.isMoving)
                {
                    if (touch.Pressed)
                    {
                        tilesUsed.Clear(); // tyhjennetää käytetyt koordinaatit jne
                        savedPath.Clear();
                        savedPath.AddRange(tilesUsed);

                        _line.Points = new Vector2[0]; // tyhjennetää viiva
                        _drawing = true; // piirto päälle

                        GD.Print($"Touch started at ({touch.Position.X}, {touch.Position.Y})");
                        UpdateTileAtPosition(touch.Position); // kursorin highlight metodikutsu
                    }
                    else if (!touch.Pressed && _drawing) // jos kosketus irtoo kesken piirtämisen
                    {
                        _drawing = false; // piirto loppu

                        GD.Print($"Touch released at ({touch.Position.X}, {touch.Position.Y})");

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
                    GD.Print($"Dragging at ({drag.Position.X}, {drag.Position.Y})");
                    UpdateTileAtPosition(drag.Position);
                }
                    }

        private void OnGoButtonPressed() { // kun go nappia painettu, liikutaan
            GD.Print("Go chosen! Sieni is moving...");
            _sieni?.Move(tilesUsed.ToArray()); // reitti parametrina sienen liikutuksel (en ees tie mitä toi ? tekee)
            _goButton.Visible = false;
            _redoButton.Visible = false; // ja napit taas pois
            _buttonsVisible = false;
        }

        private void OnRedoButtonPressed() { // kun redo nappia painetaan, reset kaikki, napit piiloon taas
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
            // Adjust position based on margin and check bounds
            Vector2 adjustedPosition = position - new Vector2(margin, margin); // -4px marginaalit et nurkka on 0,0 eikä 4,4 jne
            if (adjustedPosition.X < 0 || adjustedPosition.Y < 0 || adjustedPosition.X >= screenWidth - 2 * margin || adjustedPosition.Y >= screenHeight - 2 * margin)
            {
                GD.Print($"Adjusted position {adjustedPosition} is out of bounds, skipping.");
                return; //ei kyl skippaa mitää atm :D
            }

            // Convert to tile grid coordinates
            int tileX = (int)(adjustedPosition.X / tileWidth);
            int tileY = (int)(adjustedPosition.Y / tileHeight);

            GD.Print($"Touch at ({position.X}, {position.Y}) maps to tile ({tileX}, {tileY})");

            // Update the highlight
            highLightRect.Position = new Vector2(
                tileX * tileWidth + (tileWidth - highLightRect.Size.X) / 2 + margin,
                tileY * tileHeight + (tileHeight - highLightRect.Size.Y) / 2 + margin);
            highLightRect.Visible = true; // highlight-neliö seuraa kursorii

            // Add the tile to the list if it's not already present
            Vector2I mapCoords = new Vector2I(tileX, tileY);
            if (!tilesUsed.Contains(mapCoords))
            {
                tilesUsed.Add(mapCoords); // vaan uudet koordinaatit tallentuu (24 jaolliset aina ku int?)
            }
        }

        // Empty Process method, not needed in this context
        public override void _Process(double delta) {

        }


    }
}


