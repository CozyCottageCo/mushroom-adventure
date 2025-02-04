using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;

namespace SieniPeli
{
	public partial class Grid : Node2D
	{
		[Export] private string _cellScenePath = "res://Level/Cell.tscn";
		[Export] private int _width = 0; // laitetaa 24x24 editoris
		[Export] private int _height = 0;

		private Vector2 _cameraposition = new Vector2(0, 0); // kameranoden positio
		private Vector2 _offset = new Vector2(4, 4); //marginaali

		public int Width => _width; // basically shortened version of  { get { return width } }
		public int Height => _height; // nää on ne julkiset propertyt

		[Export] public Vector2I _cellSize = Vector2I.Zero;
		// Vector2I on integeriä kullekin koordinaatille yksikkönä käyttävä vektorityyppi.

		// Tähän 2-uloitteiseen taulukkoon on tallennettu gridin solut. Alussa taulukkoa ei ole, vaan
		// muuttujassa on tyhjä viittaus (null). Taulukko pitää luoda pelin alussa (esim. _Ready-metodissa).
		private Cell[,] _cells = null;

		public override void _Ready()
		{
			_cells = new Cell[Width, Height];
			// Alusta _cells taulukko

			// Laske se piste, josta taulukon rakentaminen aloitetaan. Koska 1. solu luodaan gridin vasempaan
			// yläkulmaan, on meidän laskettava sitä koordinaattia vastaava piste. Oletetaan Gridin pivot-pisteen
			// olevan kameran keskellä (https://en.wikipedia.org/wiki/Pivot_point).
			Vector2 offset = new Vector2((_width * _cellSize.X) / 2, (_height * _cellSize.Y) / 2);
			Vector2 startCorner = _cameraposition + _offset;

			// Lataa Cell-scene. Luomme tästä uuden olion kutakin ruutua kohden.
			PackedScene cellScene = ResourceLoader.Load<PackedScene>(_cellScenePath);
			if (cellScene == null)
			{
				GD.PrintErr("Cell sceneä ei löydy! Gridiä ei voi luoda!");
				return;
			}

			// Alustetaan Grid kahdella sisäkkäisellä for-silmukalla.
			for (int x = 0; x < _width; ++x)
			{
				for (int y = 0; y < _height; ++y)
				{
					// Luo uusi olio Cell-scenestä.
					Cell cell = cellScene.Instantiate<Cell>();
					// Lisää juuri luotu Cell-olio gridin Nodepuuhun.
					AddChild(cell);


					Vector2 cellPosition = startCorner + new Vector2(x * _cellSize.X, y * _cellSize.Y);
					cell.GlobalPosition = cellPosition;
					// Laske ja aseta ruudun sijainti niin maailman koordinaatistossa kuin
					// ruudukonkin koordinaatistossa. Aseta ruudun sijainti käyttäen cell.Position propertyä.

					_cells[x, y] = cell;

					// Tallenna ruutu tietorakenteeseen oikealle paikalle.
				}
			}
		}

// getworldposition ei tee sienipelis atm mitn
	public bool GetWorldPosition(Vector2I gridPosition, out Vector2 worldPositionClamped)
{

    // Check if the given target position is valid; 16 16 is the lowest, so cant go <0, but 0,0 is valid as no movement
    if (gridPosition.X >= 0 && gridPosition.Y >= 0)
    {
        Vector2 worldPosition = new Vector2(gridPosition.X * _cellSize.X, gridPosition.Y * _cellSize.Y); // (7,5) turned back into 112, 82
		float clampedX = Mathf.Clamp(worldPosition.X, 16, 240); //take targetworldposition X, check it is between borders, force if not (so cant go beyond)
		float clampedY= Mathf.Clamp(worldPosition.Y, 16, 208); // same for y (magic numbers I guess atm)
		worldPositionClamped = new Vector2(clampedX, clampedY); // recreate coordinates with clamped values
        return true;  // Position is valid
    }

    worldPositionClamped = Vector2.Zero;
    return false;  // Position is invalid, no move
}
	}
}