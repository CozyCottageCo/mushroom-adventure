using Godot;
using System;

public partial class SceneTransition : CanvasLayer
{
    [Export] private AnimationPlayer animationPlayer;
    [Export] private ColorRect fadeRect;

    private string nextScenePath;

    public override void _Ready()
    {

        fadeRect.Modulate = new Color(1, 1, 1, 0);  // Alpha = 0 (fully transparent)
		//fadeRect.Visible = false;
    }

    // Call this method to fade out
    public async void FadeToScene(string scenePath)
    {
		fadeRect.Visible = true;
        nextScenePath = scenePath;

        // Play fade-out part of the animation (fade to black)
        animationPlayer.Play("fade_out");

        // Wait for the fade-out to complete (assuming it's the first half of the animation)
        await ToSignal(animationPlayer, "animation_finished");

        // After fade-out is complete, change to the next scene
        GetTree().ChangeSceneToFile(nextScenePath);


        // Play fade-in part of the animation (fade back in)
        // Assuming that the second part of the animation fades in after scene is loaded
        animationPlayer.Play("fade_in");  // Plays the animation backwards (fade in)
    }

	public async void FadeToCurrentScene()
{
    ColorRect fadeRect = GetNode<ColorRect>("FadeRect");
    AnimationPlayer animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

    fadeRect.Visible = true;
    animPlayer.Play("fade_out");
    await ToSignal(animPlayer, "animation_finished");

    string currentScene = GetTree().CurrentScene.SceneFilePath;
    GetTree().ChangeSceneToFile(currentScene);

    animPlayer.Play("fade_in");
}

}
