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

        private Sieni _sieni;

        // Grid and screen settings
        private int tileWidth = 24;
        private int tileHeight = 24;
        private int screenWidth = 320;
        private int screenHeight = 180;
        private int margin = 4;

        private List<Vector2I> tilesUsed = new List<Vector2I>();

        private ColorRect highLightRect;

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
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventScreenTouch touch)
            {
                if (touch.Pressed)
                {
                    tilesUsed.Clear(); // Reset tiles used
                    _line.Points = new Vector2[0]; // Clear the line
                    _drawing = true;

                    GD.Print($"Touch started at ({touch.Position.X}, {touch.Position.Y})");
                    UpdateTileAtPosition(touch.Position);
                }
                else if (!touch.Pressed && _drawing)
                {
                    _drawing = false;

                    GD.Print($"Touch released at ({touch.Position.X}, {touch.Position.Y})");

                    GD.Print("Tiles reached during the touch:");
                    foreach (var tile in tilesUsed)
                    {
                        GD.Print($"Tile: ({tile.X}, {tile.Y})");
                    }

                    _sieni?.Move(tilesUsed.ToArray());
                }
            }
            else if (@event is InputEventScreenDrag drag && _drawing)
            {
                _line.AddPoint(drag.Position);
                GD.Print($"Dragging at ({drag.Position.X}, {drag.Position.Y})");
                UpdateTileAtPosition(drag.Position);
            }
        }

        // Method to handle both tile tracking and highlighting
        private void UpdateTileAtPosition(Vector2 position)
        {
            // Adjust position based on margin and check bounds
            Vector2 adjustedPosition = position - new Vector2(margin, margin);
            if (adjustedPosition.X < 0 || adjustedPosition.Y < 0 || adjustedPosition.X >= screenWidth - 2 * margin || adjustedPosition.Y >= screenHeight - 2 * margin)
            {
                GD.Print($"Adjusted position {adjustedPosition} is out of bounds, skipping.");
                return;
            }

            // Convert to tile grid coordinates
            int tileX = (int)(adjustedPosition.X / tileWidth);
            int tileY = (int)(adjustedPosition.Y / tileHeight);

            GD.Print($"Touch at ({position.X}, {position.Y}) maps to tile ({tileX}, {tileY})");

            // Update the highlight
            highLightRect.Position = new Vector2(
                tileX * tileWidth + (tileWidth - highLightRect.Size.X) / 2 + margin,
                tileY * tileHeight + (tileHeight - highLightRect.Size.Y) / 2 + margin);
            highLightRect.Visible = true;

            // Add the tile to the list if it's not already present
            Vector2I mapCoords = new Vector2I(tileX, tileY);
            if (!tilesUsed.Contains(mapCoords))
            {
                tilesUsed.Add(mapCoords);
            }
        }

        // Empty Process method, not needed in this context
        public override void _Process(double delta) { }
    }
}
