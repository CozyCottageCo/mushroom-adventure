using Godot;
using System;
using SieniPeli;
using System.Collections.Generic;

namespace SieniPeli
{
    public partial class Touch : Node2D
    {
        private Line2D _line; // alustettu 2d -viiva
        private TileMapLayer _tileMapLayer; // alustettu tilemaplayer
        private bool _drawing = false; // alustettu piirto true/false

        [Export] private string _cellScenePath = "res://Level/Cell.tscn";
        private Sieni _sieni;

        // Tilien / gridin koko
        private int tileWidth = 24;
        private int tileHeight = 24;

        // ruudun koko / marginaalit (ei mee tasan)
        private int screenWidth = 320;
        private int screenHeight = 180;
        private int margin = 4;

        private List<Vector2I> tilesUsed = new List<Vector2I>();
        private Vector2I lastTile = new Vector2I(-1, -1);

        public override void _Ready()
        {
            _line = new Line2D // initialisoidaan viiva, attribuutit
            {
                Width = 5,
                DefaultColor = Colors.Red
            };
            AddChild(_line); // lisää lapsinoden

            // hakee tilemaplayerin mitä muokataan
            _tileMapLayer = GetNode<TileMapLayer>("browntiles");

            // Debug
            if (_tileMapLayer == null)
            {
                GD.PrintErr("Error: TileMapLayer 'browntiles' not found!");
            }
            else
            {
                GD.Print("TileMapLayer 'browntiles' successfully found.");
            }

            PackedScene sieniScene = ResourceLoader.Load<PackedScene>("res://Level/Sieni.tscn"); // sieni scenen path
            _sieni = (Sieni)sieniScene.Instantiate(); // Instantiate the Sieni scene


            AddChild(_sieni); // Add Sieni to the scene tree dynamically

            GD.Print("Sieni node successfully instantiated and added to scene.");
    }


        public override void _Input(InputEvent @event) // kosketusmetodi
        {
            if (@event is InputEventScreenTouch touch)
            {
                if (touch.Pressed) // kun kosketaan:
                {
                    tilesUsed.Clear();
                    // viivan piirto alkaa, piirtäminen true
                    _line.Points = new Vector2[0];
                    _drawing = true;

                    // Debug kosketuksel
                    GD.Print($"Touch started at ({touch.Position.X}, {touch.Position.Y})");
                    // Poistaa tiilen (emuloi tiilien merkkailua)
                    EraseTileAtPosition(touch.Position);
                }
                else if (!touch.Pressed && _drawing)
                {
                    // Jos kosketus irtoo, piirto loppuu
                    _drawing = false;

                    // Debug touch release
                    GD.Print($"Touch released at ({touch.Position.X}, {touch.Position.Y})");

                     GD.Print("Tiles reached during the touch:");
                        foreach (var tile in tilesUsed)
                        {
                            GD.Print($"Tile: ({tile.X}, {tile.Y})");
                        }

                    if (_sieni != null) {
                        _sieni.Move(tilesUsed.ToArray()); // kutsutaan sienen liikutusmetodi (lista arrayksi jne)
                    } else {
                        GD.PrintErr("Sieni not found :(");
                    }
                }
            }
            else if (@event is InputEventScreenDrag drag && _drawing) // mut onki jatkuvaa piirtoo
            {
                // lisää viivaan pointteja
                _line.AddPoint(drag.Position);
                // kokoajan poistelee tiilejä alta, debug positiosta
                GD.Print($"Dragging at ({drag.Position.X}, {drag.Position.Y})");
                EraseTileAtPosition(drag.Position);
            }
        }

        private void EraseTileAtPosition(Vector2 position) //metodi tiilien poistolle (tää ois sit joku merkkausmetodi oikees tjsp)
        {
            // Marginaalit pois ni tarkempi alue piirtämisel
            float adjustedX = position.X - margin;
            float adjustedY = position.Y - margin;

            // Tarkistetaa et alueen sisäl (ei kyl estä ulkopuolel menoo atm)
            if (adjustedX < 0 || adjustedY < 0 || adjustedX >= screenWidth - 2 * margin || adjustedY >= screenHeight - 2 * margin)
            {
                GD.Print($"Adjusted position ({adjustedX}, {adjustedY}) is out of bounds, skipping.");
                return;
            }

            // Muuttaa tiilien koordinaatit helpommiks (120, 72  / 24 -> 5,3)
            int tileX = (int)(adjustedX / tileWidth);
            int tileY = (int)(adjustedY / tileHeight);

            GD.Print($"Touch at ({position.X}, {position.Y}) maps to tile ({tileX}, {tileY})"); //debug koordinaatit / tiilet

            if (lastTile.X != tileX || lastTile.Y != tileY) {
                Vector2I mapCoords = new Vector2I(tileX, tileY);

                var tileCell = _tileMapLayer.GetCellSourceId(mapCoords);
                    GD.Print($"Tile at ({tileX}, {tileY}) is {(tileCell == -1 ? "empty" : "occupied")}.");

                    // If the tile is not already empty (id = -1), remove it
                    if (tileCell != -1)
                    {
                        _tileMapLayer.SetCell(mapCoords, -1);  // Remove the tile
                        GD.Print($"Removed tile at ({tileX}, {tileY})");
                    }

                    // Add the new tile to the reachedTiles list
                    tilesUsed.Add(mapCoords);

                    // Update the last erased tile to the current one
                    lastTile = new Vector2I(tileX, tileY);
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        { //ei tee mihtään
        }
    }
}
