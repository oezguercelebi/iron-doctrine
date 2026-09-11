using System;
using Godot;
using IronDoctrine.Contracts;
using IronDoctrine.Client;
using IronDoctrine.Sim;

namespace IronDoctrine;

// Composition root only: the scene owns presentation, the plain C# match owns all gameplay.
public partial class Main : Node3D
{
    public override void _Ready()
    {
        try
        {
            var config = GameConfig.FromJson(Godot.FileAccess.GetFileAsString("res://data/slice1.placeholders.json"));
            IMatch CreateMatch() => new MatchFactory().Create(config, config.CreateSetup());
            var client = new MatchClient();
            client.Initialize(CreateMatch(), CreateMatch);
            AddChild(client);
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
            var message = new Label
            {
                Text = "Iron Doctrine could not start. Check the local run log.\n" + exception.Message,
                Position = new Vector2(32, 32)
            };
            AddChild(message);
            GetTree().Quit(1);
        }
    }
}
