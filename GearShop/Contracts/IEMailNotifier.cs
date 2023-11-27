namespace GearShop.Contracts;

/// <summary>
/// Уведомления посредством электронной почты.
/// </summary>
public interface IEMailNotifier
{
	string LastError { get; }

	/// <summary>
	/// Отправка писем с почтовых серверов mail.ru.
	/// </summary>
	/// <param name="receiverEmail"></param>
	/// <param name="subject"></param>
	/// <param name="body"></param>
	bool SendFromMailRu(string receiverEmail, string subject, string body);
}