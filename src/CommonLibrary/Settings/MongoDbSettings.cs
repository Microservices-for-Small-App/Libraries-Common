namespace CommonLibrary.Settings;

public class MongoDbSettings
{
    private string? _connectionString;

    public string? Host { get; init; }

    public int Port { get; init; }

    public string ConnectionString
    {
        get
        {
            return string.IsNullOrWhiteSpace(_connectionString) ? $"mongodb://{Host}:{Port}" : _connectionString;
        }

        init { _connectionString = value; }
    }
}

// public string ConnectionString => $"mongodb://{Host}:{Port}";