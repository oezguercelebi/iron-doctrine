using Godot;
using System;
using System.Collections.Generic;

namespace IronDoctrine.Client;

/// <summary>Original short synthesized signals, with no sampled or external audio.</summary>
public partial class FieldAudio : Node
{
    private readonly Dictionary<string, ulong> _last = new();
    private readonly Dictionary<string, AudioStreamWav> _sounds = new();
    private readonly AudioStreamPlayer[] _players = new AudioStreamPlayer[4];
    private int _next;
    public override void _Ready()
    {
        for (int i = 0; i < _players.Length; i++) { _players[i] = new AudioStreamPlayer { VolumeDb = -19 }; AddChild(_players[i]); }
    }
    public void Notify(string id)
    {
        if (!id.StartsWith("vo.", StringComparison.Ordinal) && !id.StartsWith("sfx.", StringComparison.Ordinal)) return;
        ulong now = Time.GetTicksMsec();
        ulong gate = id == "vo.under_attack" ? 4000UL : id.StartsWith("vo.", StringComparison.Ordinal) ? 500UL : 70UL;
        if (_last.TryGetValue(id, out ulong last) && now - last < gate) return;
        _last[id] = now;
        if (!_sounds.TryGetValue(id, out var sound))
        {
            (float a, float b, float seconds) = id switch
            {
                "vo.power" => (310, 220, .3f), "vo.funds" => (270, 180, .2f),
                "vo.building_done" => (520, 780, .27f), "vo.unit_ready" => (640, 880, .18f),
                "vo.under_attack" => (850, 470, .32f), "vo.victory" => (660, 990, .65f),
                "vo.defeat" => (330, 160, .65f), "sfx.invalid" => (180, 120, .13f),
                "sfx.rally" => (440, 660, .1f), "sfx.place" => (980, 1240, .08f), _ => (720, 940, .06f)
            };
            const int rate = 22050;
            int samples = (int)(rate * seconds);
            var data = new byte[samples * 2];
            for (int i = 0; i < samples; i++)
            {
                float progress = i / (float)samples;
                float frequency = progress < .5f ? a : b;
                float envelope = Math.Min(1, progress * 35) * Math.Min(1, (1 - progress) * 14);
                short value = (short)(Math.Sin(Math.Tau * frequency * i / rate) * envelope * 13000);
                data[i * 2] = (byte)value; data[i * 2 + 1] = (byte)(value >> 8);
            }
            sound = new AudioStreamWav { Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = rate, Data = data };
            _sounds.Add(id, sound);
        }
        var player = _players[_next++ % _players.Length];
        player.Stream = sound;
        player.Play();
    }
}
