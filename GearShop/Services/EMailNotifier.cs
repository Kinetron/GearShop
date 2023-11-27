using System.Net;
using System.Net.Mail;
using System.Text;
using GearShop.Contracts;

namespace GearShop.Services
{
    public class EMailNotifier : IEMailNotifier
	{
		private readonly string _senderEmail;
		private readonly string _senderPassword;
		private readonly string _companyName;

		public string LastError { get; private set; }

		public EMailNotifier(string senderEmail, string senderPassword, string companyName)
		{
			this._senderEmail = senderEmail;
			this._senderPassword = senderPassword;
			this._companyName = companyName;
		}

		/// <summary>
		/// Отправка писем с почтовых серверов mail.ru.
		/// </summary>
		/// <param name="receiverEmail"></param>
		/// <param name="subject"></param>
		/// <param name="body"></param>
		public bool SendFromMailRu(string receiverEmail, string subject, string body)
		{
			//В документации 465 порт, но у меня там работает только с OAuth
			//авторизацией, которую не поддерживает стандартный Smtp клиент в .NET
			//соотв. используем стандартный 587 с SSL
			//Пароль - это не пароль на аккаунт, а пароль для внешней программы.

			SmtpClient smtpClient = new SmtpClient("smtp.mail.ru", 587);
			smtpClient.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
			smtpClient.EnableSsl = true;

			using (MailMessage mailMessage = new MailMessage())
			{
				mailMessage.From = new MailAddress(_senderEmail, _companyName);
				mailMessage.To.Add(new MailAddress(receiverEmail, ""));
				mailMessage.Bcc.Add(new MailAddress(receiverEmail));

				mailMessage.Subject = subject; //Тема.
				mailMessage.IsBodyHtml = true;
				mailMessage.BodyEncoding = Encoding.UTF8;
				mailMessage.Body = body;
				try
				{
					smtpClient.Send(mailMessage);
				}
				catch (Exception ex)
				{
					LastError = $"Ошибка {ex.Message} {ex.StackTrace}";
					return false;
				}
			}

			return true;
		}
	}
}
