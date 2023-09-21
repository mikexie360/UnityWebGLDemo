using GameFramework.Core.Data;

public class Message
{
    private LobbyPlayerData _player;
    private string _message;

    public Message(LobbyPlayerData player, string message)
    {
        _player = player;
        _message = message;
    }

    public string GetMessage()
    {
        return _message;
    }

    public LobbyPlayerData GetId()
    {
        return _player;
    }
}
