using System.IO.Compression;
using System.Net.Http.Headers;
using System.Security.Policy;

namespace PriceUploader.Services
{
	/// <summary>
	/// Сервис загрузки файлов.
	/// </summary>
	internal class WebClient
	{
		/// <summary>
		/// Загрузка файла на сервер.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="filePath"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public async Task<HttpResponseMessage> Upload(string url, string filePath, string userName, string password)
		{
			using (var httpClient = new HttpClient())
			{
				var form = new MultipartFormDataContent();
				form.Add(new StringContent(userName), name: "userName");
				form.Add(new StringContent(password), name: "password");
				
				byte[] fileData = File.ReadAllBytes(filePath);

				ByteArrayContent byteContent = new ByteArrayContent(fileData);

				byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

				form.Add(byteContent, "file", Path.GetFileName(filePath));
				return await httpClient.PostAsync(url, form);
			}
		}

		public async Task<HttpResponseMessage> PostAsync(string url, KeyValuePair<string,string> content, string userName, string password)
		{
			using (var httpClient = new HttpClient())
			{
				var form = new MultipartFormDataContent();
				form.Add(new StringContent(userName), name: "userName");
				form.Add(new StringContent(password), name: "password");
				form.Add(new StringContent(content.Value), name: content.Key);

				return await httpClient.PostAsync(url, form);
			}
		}
	}
}
