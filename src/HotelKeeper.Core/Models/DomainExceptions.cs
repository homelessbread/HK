namespace HotelKeeper.Data;

/// <summary>
/// A business-rule violation tied to one input field. It still is a <see cref="DomainException"/>, so every
/// existing catch block and <c>ErrorText</c> keep working unchanged; a form can additionally read
/// <see cref="Field"/> and show the message next to that field instead of only in a banner.
/// </summary>
public class FieldValidationException(string field, string message) : DomainException(message)
{
    /// <summary>The label of the input the message is about, for example "Full name".</summary>
    public string Field { get; } = field;
}
