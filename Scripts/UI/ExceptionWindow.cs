using System.Collections.Specialized;
using System.IO;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;

namespace Game.Mods.DaggerLog
{
    public class ExceptionWindow : DaggerfallPopupWindow
    {
        protected TextLabel messageTextLabel;

        public string CurrentMessage = "Lorem Ipsum Dolar Set";
        public string ClipboardContents = string.Empty;

        public ExceptionWindow(IUserInterfaceManager uiManager) : base(uiManager)
        {
        }

        public override void OnPush()
        {
            if (messageTextLabel == null) return;
            messageTextLabel.Text = CurrentMessage;
        }

        protected override void Setup()
        {
            var logButtonTexture = Resources.Load<Texture2D>("hamburger_button");
            var textColor = new Color(0.0f, 0.5f, 0.0f, 0.4f);
            var backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.7f);

            ParentPanel.BackgroundColor = Color.clear;

            // Main panel
            var mainPanel = new Panel();
            mainPanel.Outline.Enabled = true;
            mainPanel.BackgroundColor = backgroundColor;
            mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.VerticalAlignment = VerticalAlignment.Middle;
            mainPanel.Size = new Vector2Int(200, 60);
            NativePanel.Components.Add(mainPanel);

            var header = new TextLabel();
            header.TextColor = Color.red;
            header.HorizontalAlignment = HorizontalAlignment.Center;
            header.Position = new Vector2Int(0, 5);
            //header.VerticalAlignment = VerticalAlignment.Top;
            header.Text = "Unhandled Exception";
            header.TextScale = 2f;
            mainPanel.Components.Add(header);

            messageTextLabel = new TextLabel();
            messageTextLabel.TextColor = Color.white;
            messageTextLabel.HorizontalAlignment = HorizontalAlignment.Center;
            messageTextLabel.VerticalAlignment = VerticalAlignment.Middle;
            messageTextLabel.MaxCharacters = 120;
            messageTextLabel.WrapText = true;
            messageTextLabel.WrapWords = true;
            messageTextLabel.MaxWidth = mainPanel.InteriorWidth - 2;
            messageTextLabel.Text = CurrentMessage;
            mainPanel.Components.Add(messageTextLabel);

            var allowIgnoreClick = false;
            var ignoreButton = new Button();
            ignoreButton.Size = new Vector2Int(50, 12);
            ignoreButton.Outline.Enabled = true;
            ignoreButton.BackgroundColor = textColor;
            ignoreButton.VerticalAlignment = VerticalAlignment.Bottom;
            ignoreButton.HorizontalAlignment = HorizontalAlignment.Left;
            ignoreButton.Label.Text = "Ignore";
            ignoreButton.Label.TextColor = Color.gray;
            ignoreButton.OnMouseClick += (_, __) =>
            {
                if (!allowIgnoreClick) return;
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                CloseWindow();
            };
            mainPanel.Components.Add(ignoreButton);

            var openLogFolder = new Button();
            openLogFolder.Size = new Vector2Int(7, 7);
            openLogFolder.Position = new Vector2Int(1, 0);
            openLogFolder.BackgroundTexture = logButtonTexture;
            openLogFolder.OnMouseClick += (_, __) =>
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                var path = Utils.GetLogFolderPath();
                if (string.IsNullOrEmpty(path)) return;
                if (!Directory.Exists(path)) return;
                System.Diagnostics.Process.Start(path);
            };
            mainPanel.Components.Add(openLogFolder);

            var copyButton = new Button();
            copyButton.Size = new Vector2Int(90, 12);
            copyButton.Outline.Enabled = true;
            copyButton.BackgroundColor = Color.clear;
            copyButton.VerticalAlignment = VerticalAlignment.Bottom;
            copyButton.HorizontalAlignment = HorizontalAlignment.Center;
            copyButton.Label.Text = "Copy to clipboard";
            copyButton.OnMouseClick += (_, __) =>
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                GUIUtility.systemCopyBuffer = ClipboardContents;
            };
            mainPanel.Components.Add(copyButton);

            var quitButton = new Button();
            quitButton.Size = new Vector2Int(50, 12);
            quitButton.Outline.Enabled = true;
            quitButton.BackgroundColor = Color.red;
            quitButton.VerticalAlignment = VerticalAlignment.Bottom;
            quitButton.HorizontalAlignment = HorizontalAlignment.Right;
            quitButton.Label.Text = "Quit";
            quitButton.OnMouseClick += (_, __) =>
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                var confirmExit = new DaggerfallMessageBox(uiManager, DaggerfallMessageBox.CommonMessageBoxButtons.YesNo, 1069, this);
                confirmExit.OnButtonClick += (sender, button) =>
                {
                    sender.CloseWindow();
                    if (button == DaggerfallMessageBox.MessageBoxButtons.Yes)
                    {
                        DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiExitGame);
                        CancelWindow();
                    }
                };
                confirmExit.Show();
            };
            mainPanel.Components.Add(quitButton);

            var checkbox = new Checkbox();
            checkbox.IsChecked = false;
            checkbox.HorizontalAlignment = HorizontalAlignment.Center;
            checkbox.Label.TextColor = textColor;
            checkbox.Label.Text = "I know what I am doing.";
            checkbox.Position = new Vector2Int(0, 60 - 12 - 3 - checkbox.Label.TextHeight);
            checkbox.OnToggleState += () =>
            {
                allowIgnoreClick = checkbox.IsChecked;
                ignoreButton.Label.TextColor = allowIgnoreClick ? DaggerfallUI.DaggerfallDefaultTextColor : Color.gray;
            };
            mainPanel.Components.Add(checkbox);

            AllowCancel = false;
            IsSetup = true;
        }
    }
}