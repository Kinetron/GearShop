using HybridCryptLib.Models;

namespace GearShop.Contracts;

public interface ICryptoService
{
    /// <summary>
    /// Шифрует пользовательские данные.
    /// </summary>
    /// <param name="info"></param>
   byte[] Crypt(UserInfo info);

    /// <summary>
    /// Декодирует пользовательские данные.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="privateKeyPath"></param>
    /// <param name="privateKeyPassword"></param>
    /// <returns></returns>
    UserInfo DeCrypt(byte[] data);
}