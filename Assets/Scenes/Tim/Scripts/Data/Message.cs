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

    public string GetId()
    {
        return _player.Gamertag;
    }

    public override string ToString()
    {
        if (_player == null)
        {
            return "Username: " + _message;
        }
        else
        {
            return _player.Gamertag + ": " + _message;
        }
    }
}
