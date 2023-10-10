using GameFramework.Core.Data;

public class Message
{
    private string _player;
    private string _message;

    public Message(string player, string message)
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
        return _player;
    }

    public override string ToString()
    {
        if (_player == null)
        {
            return "Username: " + _message;
        }
        else
        {
            return _player + ": " + _message;
        }
    }
}
