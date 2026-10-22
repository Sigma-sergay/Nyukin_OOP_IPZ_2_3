namespace lab12v19;

/// <summary>
/// Сутність за варіантом 19: сповіщення для користувача.
/// </summary>
public class Notification : IEntity
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecipientId { get; set; }
    public bool IsRead { get; set; }

    public override string ToString() =>
        $"#{Id} для користувача {RecipientId}: \"{Message}\" (прочитано: {(IsRead ? "так" : "ні")})";
}
