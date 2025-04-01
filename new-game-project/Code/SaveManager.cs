using Godot;
using System;
using SieniPeli;
namespace SieniPeli {

public partial class SaveManager : Node
{

	private string savePath; // tallennuksen path (user mikälie menee appdataa jne)
	private const int TotalLevels = 18; // max levut 16 + 2 easter egg

	private bool firstLevels = false; // alustetaa onko ekan screenin/tokan/ jne levut tehty
	private bool secondLevels = false;

	private bool thirdLevels = false;

	public bool firstLevelsDone => firstLevels; // public getti levelselectionille
	public bool secondLevelsDone => secondLevels;
	public bool thirdLevelsDone => thirdLevels;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		savePath = "user://SaveData.cfg"; // default savepath settingsin vierelle erikseen
	}


public void SaveProgress (bool[] levelCompletion) { // tallennusmetodi
	if (levelCompletion.Length != TotalLevels) {
		GD.PrintErr("Incorrect number of levels");
		return;
	}
	ConfigFile config = new ConfigFile(); // ConfigFile o joku Godotin oma setti
	for (int i = 0; i < TotalLevels; i++)
        {
            // Tallennetaan otsikon "levels" alle "Level(i)" ja true tai false
            config.SetValue("Levels", "Level" + i, levelCompletion[i]);
        }

		Error err = config.Save(savePath); // error jos onnistuu tai ei
        if (err == Error.Ok)
        {
            GD.Print("Level progress saved successfully!");
        }
        else
        {
            GD.PrintErr("Failed to save level progress!");
        }
    }



	public bool[] LoadProgress() // ladataan progressio
    {
        bool[] levelCompletion = new bool[TotalLevels]; // sama taulukko

        ConfigFile config = new ConfigFile(); // sama filu

        //  latauserrortsekki
        Error err = config.Load(savePath);
        if (err != Error.Ok)
        {
            GD.PrintErr("Failed to load level progress file!");
            // Jos kusee ni default valuet eli kaikki false (ettei hajoo / jos ei oo viel savea ni tekee)
            for (int i = 0; i < TotalLevels; i++)
            {
                levelCompletion[i] = false;
            }
            return levelCompletion;
        }

        // Tarkistetaan mitä on tehty tai ei jos filu löytyy
        for (int i = 0; i < TotalLevels; i++)
        {
            levelCompletion[i] = (bool)config.GetValue("Levels", "Level" + i, false); // false perässä on default, jos ei löydy muuta
        }

        GD.Print("Level progress loaded successfully.");
        return levelCompletion;
    }

	public void MarkLevelCompleted(string sceneName) { // taso tehdyksi täplän kutsulla tason skenen nimellä
		int levelNumber = GetLevelNumberFromName(sceneName); //otetaan skenestä numero talteen

		if (levelNumber >= 0 && levelNumber < TotalLevels) { // onko oikean kokonen numero
			bool[] levelCompletion = LoadProgress(); // katotaan taulukko
			levelCompletion[levelNumber] = true; // merkitään tehty leveli numero true
			SaveProgress(levelCompletion); // tallennetaan
			GD.Print($"Level {sceneName} marked complete");
		} else {
			GD.PrintErr("Scene name invalid or levelnumber out of bounds");
			GD.Print(sceneName);
		}

	}


	public bool IsLevelCompleted(string levelName) { // tarkistetaa onko taso jo tehty (levelselectionille)
		bool[] levelCompletion = LoadProgress(); // taulukko haetaa taas
		int levelNumber = GetLevelNumberFromName(levelName); // ja numero levunnimestä
		if (levelNumber >= 0 && levelNumber < TotalLevels) {
			return levelCompletion[levelNumber]; // true tai false
		} else {
			GD.PrintErr("Invalid level name " + levelName);
			return false;
		}
	}
	private int GetLevelNumberFromName(string sceneName) { // otetaan levun skenen nimestä numero talteen
		switch (sceneName)
        {
            case "Level1": return 0;
            case "Level2": return 1;
            case "Level3": return 2;
            case "Level4": return 3;
            case "Level5": return 4;
            case "Level6": return 5;
            case "Level7": return 6;
            case "Level8": return 7;
            case "Level9": return 8;
            case "Level10": return 9;
            case "Level11": return 10;
            case "Level12": return 11;
            case "Level13": return 12;
            case "Level14": return 13;
            case "Level15": return 14;
            case "Level16": return 15;
            case "Levelhuussi": return 16; // nää varmaa sit kattoo ne easter egg tasot ku numeroperusteinen
            case "Levelmökki": return 17;
            default:
                return -1;  // Invalid skene = -1 ni ei mee läpi tarkistuksest
        }
    }

	public void Reset() { // resetataan jos halutaan
 	bool[] resetLevelCompletion = new bool[TotalLevels]; // Korvataan koko taulukko uudella jossa kaikki false

    ConfigFile config = new ConfigFile();

    // Kaikki falseksi
    for (int i = 0; i < TotalLevels; i++)
    {
        config.SetValue("Levels", "Level" + i, false);
    }

    // Tallennetaan uus falsetaulukko
    Error err = config.Save(savePath);
    if (err == Error.Ok)
    {
        GD.Print("Level progress has been reset successfully!");
    }
    else
    {
        GD.PrintErr("Failed to reset level progress!");
    }
}

public void SetLevelsDone (int levelScreen) { // tällä tallennetaan onko screen1/2/3 tehty jotta seuraavan screenin ekan tason voi tehä
	if (levelScreen == 1) {
		firstLevels = true;
	}
	if (levelScreen == 2) {
		secondLevels = true;
	}
	if (levelScreen == 3) {
		thirdLevels = true;
	}
}

	public int GetLevelsCompleted() { // simppelimpi tarkistin levelselectionin täpläcountterille (laskee truet taulukosta)

		bool[] levelCompletion = LoadProgress();
		int levelsCompleted = 0;

		foreach (bool completed in levelCompletion) {
			if (completed) {
			levelsCompleted++;
			}
		}

		return levelsCompleted;
	}


	public override void _Process(double delta)
	{
	}

}
}