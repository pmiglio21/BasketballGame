using Godot;
using System.Collections.Generic;
using System;
using Constants;
using Screens;

namespace Root
{
    public partial class RootSceneSwapper : Node
    {
        #region "Global" Properties

        //public ScreenNames PriorSceneName;

        #endregion

        #region Components

        private Control _rootGuiControl;

        private AudioStreamPlayer _uiAudioStreamPlayer;

        #endregion

        #region Screens

        private TitleScreen _titleScreen;

        #endregion

        public override void _Ready()
        {
            _rootGuiControl = FindChild("GUI") as Control;
            _uiAudioStreamPlayer = FindChild("UiSoundEffectsAudioStreamPlayer") as AudioStreamPlayer;

            _titleScreen = FindChild("TitleScreen") as TitleScreen;
            _playScreen = FindChild("PlayScreen") as PlayScreen;

            _titleScreen.GoToPlayScreen += OnTitleScreenRootGoToPlayModeScreen;
            _titleScreen.QuitGame += QuitGame;

            //ReadLastOpenedData();
            //LoadOriginalSettings();
        }

        #region Go-To-Screens Methods

        #region From Title Screen

        private void OnTitleScreenRootGoToPlayModeScreen()
        {
            ChangeSceneToPlayScreen(_titleScreen);
        }


        #endregion


        #region General Use

        private void ChangeSceneToTitleScreen(Control currentUiScene)
        {
            _rootGuiControl.AddChild(_titleScreen);

            _rootGuiControl.RemoveChild(currentUiScene);

            //_titleScreen.GrabFocusOfTopButton();
        }

        public override void _Notification(int notificationCode)
        {
            if (notificationCode == NotificationWMCloseRequest)
            {
                QuitGame();
            }
        }

        private void QuitGame()
        {
            try
            {
                //SaveOutLastOpenedData();

                _titleScreen?.QueueFree();

                GetTree().Quit();
            }
            catch (Exception ex)
            {
                GD.PushError(ex.Message);
            }
        }

        #endregion

        #region Play Screen

        public void ChangeSceneToPlayScreen(Control currentUiScene)
        {
            _rootGuiControl.AddChild(_playScreen);

            _rootGuiControl.RemoveChild(currentUiScene);
        }

        #endregion

        #endregion

        #region Audio Players

        private bool _isUiAudioStreamPlayerMuted = false;

        public void PlayUiSoundEffect(string soundPath)
        {
            AudioStream audioStream = ResourceLoader.Load(soundPath) as AudioStream;

            if (!_isUiAudioStreamPlayerMuted)
            {
                _uiAudioStreamPlayer.Stream = audioStream;

                _uiAudioStreamPlayer.Play();
            }
        }

        public void ChangeMenuSoundsVolume(float volume)
        {
            if (volume == 0)
            {
                _isUiAudioStreamPlayerMuted = true;
            }
            else
            {
                _isUiAudioStreamPlayerMuted = false;

                _uiAudioStreamPlayer.VolumeDb = 20 * volume;
            }
        }

        #endregion

        #region Game Rules

        private void SaveOutLastOpenedData()
        {
            try
            {
                ////Rewrite all available rulesets, with the matching ruleset now updated
                //using (FileAccess lastOpenedDataFilePath = FileAccess.Open(PersistentFilePaths.LastOpenedDataFilePath, FileAccess.ModeFlags.Write))    //Using Write to truncate table, instead of ReadWrite
                //{
                //    string serializedRuleset = JsonConvert.SerializeObject(CurrentGameRules);

                //    //Forces the file writer to write at the end of the file instead of the beginning.
                //    //Necessary to call or else the file's first line will be overwritten by StoreLine.
                //    lastOpenedDataFilePath.SeekEnd();

                //    lastOpenedDataFilePath.StoreLine(serializedRuleset);
                //}
            }
            catch (Exception ex)
            {
                GD.PushError(ex.Message);
            }
        }

        private void ReadLastOpenedData()
        {
            try
            {
                //if (FileAccess.FileExists(PersistentFilePaths.RulesetsFilePath))
                //{
                //    using (FileAccess lastOpenedDataFilePath = FileAccess.Open(PersistentFilePaths.LastOpenedDataFilePath, FileAccess.ModeFlags.Read))
                //    {
                //        if (lastOpenedDataFilePath != null)
                //        {
                //            var currentLine = lastOpenedDataFilePath.GetLine();

                //            while (!string.IsNullOrWhiteSpace(currentLine))
                //            {
                //                GameRules deserializedRuleset = JsonConvert.DeserializeObject<GameRules>(currentLine);

                //                if (deserializedRuleset != null)
                //                {
                //                    CurrentGameRules = deserializedRuleset;
                //                    CurrentGameRules.RulesetName = string.Empty;
                //                }

                //                currentLine = lastOpenedDataFilePath.GetLine();
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                GD.PushError(ex.Message);
            }
        }

        #endregion

        #region Settings

        private void LoadOriginalSettings()
        {
            //var settingsData = new Godot.Collections.Dictionary();
            //var config = new ConfigFile();

            //// Load data from a file.
            //// Found in C:\Users\pmigl\AppData\Roaming\Godot\app_userdata\Multiplayer Godot Game Godot 4
            //Error error = config.Load(PersistentFilePaths.GameSettingsFilePath);

            //////If the file didn't load, ignore it.
            ////if (error != Error.Ok)
            ////{
            ////	return;
            ////}
            //if (error == Error.Ok)
            //{
            //    // Iterate over all sections.
            //    foreach (string section in config.GetSections())
            //    {
            //        // Fetch the data for each section.
            //        CurrentSettings.MusicVolume = (float)config.GetValue(section, "music_volume");
            //        CurrentSettings.SoundEffectsVolume = (float)config.GetValue(section, "menu_sounds_volume");
            //        CurrentSettings.DungeonSoundsVolume = (float)config.GetValue(section, "dungeon_sounds_volume");
            //        CurrentSettings.FullscreenState = (string)config.GetValue(section, "fullscreen_state");
            //    }
            //}

            //if (CurrentSettings.FullscreenState != GlobalConstants.OffOnOptionOn)
            //{
            //    //DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized); //TODO: Undo this
            //}
            //else
            //{
            //    DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            //}
        }

        #endregion
    }
}
