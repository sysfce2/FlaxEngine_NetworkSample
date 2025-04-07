using FlaxEditor;
using FlaxEngine;
using FlaxEngine.GUI;
using FlaxEngine.Networking;

public class ChatScript : Script
{
    public ControlReference<TextBox> MessageBox;
    public ControlReference<Panel> Panel;
    public ControlReference<VerticalPanel> VertPanel;

    public FontReference ChatFont;

    private bool _isWriting;
    private int _chatIndex;

    public bool IsWriting
    {
        get => _isWriting;
    }

    /// <inheritdoc/>
    public override void OnEnable()
    {
        _isWriting = false;
        MessageBox.Control.Clear();
        VertPanel.Control.DisposeChildren();
        MessageBox.UIControl.IsActive = false;
        _chatIndex = 0;
    }

    public override void OnDisable()
    {
        VertPanel.Control.DisposeChildren();
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        var chatMessages = GameSession.Instance.ChatMessages;
        if (chatMessages.Count - 1 >= _chatIndex)
        {
            VertPanel.Control.BackgroundColor = new Color(0, 0, 0, 0.28f);
            while (_chatIndex < chatMessages.Count)
            {
                var l = VertPanel.Control.AddChild<Label>();
                var player = GameSession.Instance.GetPlayer(chatMessages[_chatIndex].Sender);
                var name = string.Empty;
                if (player != null)
                {
                    name = player.Name;
                }

                l.Font = ChatFont;
                l.Text = name + " : " + chatMessages[_chatIndex].Message;
                l.HorizontalAlignment = TextAlignment.Near;
                _chatIndex++;
            }
        }

        if (!_isWriting && Input.GetKeyUp(KeyboardKeys.Return))
        {
            _isWriting = true;
            MessageBox.UIControl.IsActive = true;
            MessageBox.Control.Focus();
            MessageBox.Control.Clear();
        }
        else if (_isWriting && Input.GetKeyUp(KeyboardKeys.Return))
        {
            _isWriting = false;
            MessageBox.UIControl.IsActive = false;
            if (MessageBox.Control.Text != string.Empty)
            {
                GameSession.Instance.AddChatMessage(GameSession.Instance.LocalPlayer.ID, MessageBox.Control.Text);
                ChatMessagePacket p = new ChatMessagePacket();
                p.Message = MessageBox.Control.Text;
                p.SenderID = GameSession.Instance.LocalPlayer.ID;
                NetworkSession.Instance.Send(p, NetworkChannelType.Reliable);
                MessageBox.Control.Clear();
            }
#if FLAX_EDITOR
            Editor.Instance.Windows.GameWin.Focus();
#endif
        }

        Panel.Control.ScrollViewTo(VertPanel.Control.Bounds.BottomLeft, true);
    }
}
