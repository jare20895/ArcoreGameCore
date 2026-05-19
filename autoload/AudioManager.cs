using Godot;
using Godot.Collections;

namespace ArcoreGameCore;

/// <summary>
/// Centralised audio playback. Manages BGM, SFX buses, and pooled one-shot players.
/// </summary>
public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; } = null!;

    private const int SfxPoolSize = 16;
    private const string BgmBus = "BGM";
    private const string SfxBus = "SFX";

    private AudioStreamPlayer _bgmPlayer = null!;
    private readonly AudioStreamPlayer[] _sfxPool = new AudioStreamPlayer[SfxPoolSize];
    private int _sfxPoolIndex;

    public float BgmVolume
    {
        get => _bgmPlayer.VolumeDb;
        set => _bgmPlayer.VolumeDb = value;
    }

    public override void _Ready()
    {
        Instance = this;

        _bgmPlayer = new AudioStreamPlayer { Bus = BgmBus };
        AddChild(_bgmPlayer);

        for (int i = 0; i < SfxPoolSize; i++)
        {
            _sfxPool[i] = new AudioStreamPlayer { Bus = SfxBus };
            AddChild(_sfxPool[i]);
        }
    }

    public void PlayBgm(AudioStream stream, bool loop = true, float fadeIn = 0f)
    {
        if (_bgmPlayer.Stream == stream && _bgmPlayer.Playing) return;
        _bgmPlayer.Stream = stream;
        _bgmPlayer.Play();
    }

    public void StopBgm() => _bgmPlayer.Stop();

    public void PlaySfx(AudioStream stream, float pitchVariance = 0f)
    {
        var player = _sfxPool[_sfxPoolIndex % SfxPoolSize];
        _sfxPoolIndex++;
        player.Stream = stream;
        player.PitchScale = pitchVariance > 0f
            ? 1f + GD.Randf() * pitchVariance - pitchVariance * 0.5f
            : 1f;
        player.Play();
    }

    public void SetBusMute(string bus, bool muted)
    {
        int idx = AudioServer.GetBusIndex(bus);
        if (idx >= 0) AudioServer.SetBusMute(idx, muted);
    }

    public void SetBusVolume(string bus, float db)
    {
        int idx = AudioServer.GetBusIndex(bus);
        if (idx >= 0) AudioServer.SetBusVolumeDb(idx, db);
    }
}
