using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Memory;

namespace RemoteControlApi
{
	/// <summary>
	/// Сервис загрузки файлов.
	/// </summary>
	public class WebSender
	{
		/// <summary>
		/// Factory for safe use httpClient.
		/// </summary>
		IHttpClientFactory _httpClientFactory;

		/// <summary>
		/// Last error occur in process work service methods.
		/// </summary>
		public string LastError { get; private set; }

		public WebSender()
		{
			var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
			_httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
		}

		/// <summary>
		/// Загрузка файла на сервер.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="filePath"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public async Task<HttpResponseMessage> Upload(string url, string filePath, string userName, string password, string shopName)
		{
			using (var httpClient = new HttpClient())
			{
				var form = new MultipartFormDataContent();
				form.Add(new StringContent(userName), name: "userName");
				form.Add(new StringContent(password), name: "password");
				form.Add(new StringContent(shopName), name: "shopName");

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

		public async Task<HttpResponseMessage> GetAsync(string url, string userName, string password)
		{
			using (var httpClient = new HttpClient())
			{
				var form = new MultipartFormDataContent();
			
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri(url),
				};

				request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ "userName", userName },
					{ "password", password }
				});
				
				return await httpClient.SendAsync(request);
			}
		}

		public async Task<HttpResponseMessage> GetByParamAsync(string url, string param, string paramName)
		{
			using (var httpClient = new HttpClient())
			{
				var form = new MultipartFormDataContent();

				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri(url),
				};

				request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ paramName, param }
				});

				return await httpClient.SendAsync(request);
			}
		}

		/// <summary>
		/// Download file from server.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public async Task<bool> DownloadFile(string url, string userName, string password)
		{
			try
			{
				var client = _httpClientFactory.CreateClient();

				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Post,
					RequestUri = new Uri(url),
				};

				request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ "userName", userName },
					{ "password", password }
				});

				//Don't copy all file in memory.
				//var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
				//var stream = await response.Content.ReadAsStreamAsync();

				//using var filestream = new FileStream("123", FileMode.Create);

				//await stream.CopyToAsync(filestream);

				var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

				using var contentStream = await response.Content.ReadAsStreamAsync();

				long? totalBytesRead =  response.Content.Headers.ContentLength;
				var readCount = 0L;
				var buffer = new byte[8192];
				var isMoreToRead = true;

				using var fileStream = new FileStream("abc", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

				do
				{
					var bytesRead = await contentStream.ReadAsync(buffer);
					if (bytesRead == 0)
					{
						isMoreToRead = false;

						//if (progressChanged(totalBytes, totalBytesRead, calculatePercentage(totalBytes, totalBytesRead)))
						//{
						//	throw new OperationCanceledException();
						//}
						continue;
					}

					await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));

					totalBytesRead += bytesRead;
					readCount++;

					if (readCount % 100 == 0)
					{
						//if (progressChanged(totalBytes, totalBytesRead, calculatePercentage(totalBytes, totalBytesRead)))
						//{
						//	throw new OperationCanceledException();
						//}
					}
				}
				while (isMoreToRead);



				//var contentLength = response.Content.Headers.ContentLength;

				//using (var download = await response.Content.ReadAsStreamsync())
				//using (var fileStream = new FileStream("abc", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
				//{
				//	do
				//	{
				//		var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
				//		if (bytesRead == 0)
				//		{
				//			isMoreToRead = false;
				//			TriggerProgressChanged(totalDownloadSize, totalBytesRead);
				//			continue;
				//		}

				//		await fileStream.WriteAsync(buffer, 0, bytesRead);

				//		totalBytesRead += bytesRead;
				//		readCount += 1;

				//		if (readCount % 100 == 0)
				//			TriggerProgressChanged(totalDownloadSize, totalBytesRead);
				//	}
				//	while (isMoreToRead);
				//}




				//Int64? responseLength = response.Content.Headers.ContentLength;
				//	if (responseLength.HasValue)
				//	{
				//		using (Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
				//		using (StreamReader rdr = new StreamReader(responseStream))
				//		{
				//			Int64 totalBytesRead = 0;
				//			StringBuilder sb = new StringBuilder(capacity: (int)responseLength.Value); // Note that `capacity` is in 16-bit UTF-16 chars, but responseLength is in bytes, though assuming UTF-8 it evens-out.

				//			Char[] charBuffer = new Char[4096];
				//			while (true)
				//			{
				//				Int32 read = await rdr.ReadAsync(charBuffer).ConfigureAwait(false);
				//				sb.Append(charBuffer, 0, read);
				//				if (read == 0)
				//				{
				//					// Reached end.
				//					//progress.Report("Finished reading response content.");
				//					break;
				//				}
				//				else
				//				{
				//					//progress.Report(String.Format(CultureInfo.CurrentCulture, "Read {0:N0} / {1:N0} chars (or bytes).", sb.Length, resposneLength.Value);
				//				}
				//			}
				//		}
				//	}
				//	else
				//	{
				//		//progress.Report("No Content-Length header in response. Will read response until EOF.");

				//		//string result = await content.ReadAsStringAsync();
				//	}

				//progress.Report("Finished reading response content.");



			}
			catch (Exception e)
			{
				LastError = $"{e.Message} {e.StackTrace}";
				return false;
			}
	
			return true;
		}
	}
}
