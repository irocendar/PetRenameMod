using StardewModdingAPI.Utilities;

namespace PetRenameMod;

public sealed class ModConfig
{
    public bool Enabled { get; set; } = true;
    public KeybindList KeyBind { get; set; } = KeybindList.Parse("Delete");
}