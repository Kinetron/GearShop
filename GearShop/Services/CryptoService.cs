using System.Text;
using GearShop.Contracts;
using HybridCryptLib.Models;
using HybridCryptLib.Services;
using Wangkanai.Exceptions;

namespace GearShop.Services
{
    /// <summary>
    /// Содержит основные методы шифрования. Используемые сервисом.
    /// </summary>
    public class CryptoService : ICryptoService
	{
		/// <summary>
		/// Путь к сертификату открытого ключа.
		/// </summary>
		private readonly string _certPath;

		/// <summary>
		/// Путь к закрытому ключу.
		/// </summary>
		private readonly string _privateKeyPath;

		/// <summary>
		/// Пароль к ключу.
		/// </summary>
		private readonly string _keyPassword;

		public CryptoService(IConfiguration config)
		{
			_certPath = config["CryptoService:CertPath"];
			_privateKeyPath = config["CryptoService:KeyPath"];
			_keyPassword = config["CryptoService:KeyPwd"];

			if (string.IsNullOrEmpty(_certPath) || string.IsNullOrEmpty(_privateKeyPath))
			{
				throw new ArgumentEmptyException();
			}
		}
		
		/// <summary>
		/// Шифрует пользовательские данные.
		/// </summary>
		/// <param name="info"></param>
		public byte[] Crypt(UserInfo info)
		{
			CryptUserInfo cryptUserInfo = new CryptUserInfo();
			return cryptUserInfo.Crypt(info, _certPath);
		}

		/// <summary>
		/// Декодирует пользовательские данные.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="privateKeyPath"></param>
		/// <param name="privateKeyPassword"></param>
		/// <returns></returns>
		public UserInfo DeCrypt(byte[] data)
		{
			CryptUserInfo cryptUserInfo = new CryptUserInfo();
			return cryptUserInfo.DeCrypt(data, _privateKeyPath, _keyPassword);
		}
	}
}
