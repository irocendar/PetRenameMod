using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;

namespace PetRenameMod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        
        /*********
         ** Properties
         *********/
        /// <summary>The mod configuration from the player.</summary>
        private ModConfig Config;
        private Pet? _pet = null;
        
        /*********
         ** Public methods
         *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        /*********
         ** Private methods
         *********/
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            
            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );
            
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable/Disable Pet Renaming Keybind",
                getValue: () => this.Config.Enabled,
                setValue: value => this.Config.Enabled = value
            );
            
            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => "Pet Renaming Keybind",
                getValue: () => this.Config.KeyBind,
                setValue: value => this.Config.KeyBind = value
            );
        }
        
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            if (this.Config.KeyBind.JustPressed())
            {
                this.CheckNearby(Game1.player.currentLocation, Game1.player.StandingPixel.X, Game1.player.StandingPixel.Y, Game1.player);
                // this.Monitor.Log($"{Game1.player.getPet().Name}", LogLevel.Debug);
                // Game1.activeClickableMenu = new NamingMenu(RenameAnimal, "Enter New Pet Name:", Game1.player.getPet().Name);
                // this.Monitor.Log($"{Game1.player.getPet().Name}", LogLevel.Debug);
            }
        }

        private void RenameAnimal(string name)
        {
            this._pet!.Name = name;
            Game1.exitActiveMenu();
        }

        private void CheckNearby(GameLocation location, int x, int y, Farmer who)
        {
            Monitor.Log($"{Game1.player.facingDirection}", LogLevel.Debug);
            
            int targetX = Game1.player.TilePoint.X;
            int targetY = Game1.player.TilePoint.Y;
            switch (Game1.player.FacingDirection)
            {
                case 0:
                    targetY -= 1;
                    break;
                case 1:
                    targetX += 1;
                    break;
                case 2:
                    targetY += 1;
                    break;
                default:
                    targetX -= 1;
                    break;
            }

            this._pet = null;
            
            foreach (NPC current in location.characters)
            {
                if (current is Pet && !current.hideFromAnimalSocialMenu.Value)
                {
                    if (!(current.Tile == Game1.player.Tile))
                        continue;
                    this._pet = (Pet)current;
                    Monitor.Log($"Name: {current.Name}, Position: {current.Position}, Tile: {current.TilePoint}", LogLevel.Debug);
                    break;
                }
            }
            
            if (this._pet is null)
                foreach (NPC current in location.characters)
                {
                    if (current is Pet && !current.hideFromAnimalSocialMenu.Value)
                    {
                        if (!(current.TilePoint.X == targetX && current.TilePoint.Y == targetY))
                            continue;
                        this._pet = (Pet)current;
                        Monitor.Log($"Name: {current.Name}, Position: {current.Position}, Tile: {current.TilePoint}", LogLevel.Debug);
                    }
                }

            if (this._pet is null)
                return;
            
            Game1.activeClickableMenu = new NamingMenu(RenameAnimal, "Enter New Pet Name:", this._pet.Name);

        }
    }
}