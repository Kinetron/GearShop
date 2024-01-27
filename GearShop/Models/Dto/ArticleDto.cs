using System.Security.Policy;

namespace GearShop.Models.Dto
{
	/// <summary>
	/// Данные для создания статей.
	/// </summary>
	public class ArticleDto
	{
		public int Id { get; set; }

		public int ParentId { get; set; }

		/// <summary>
		/// Заголовок статьи.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Описание, отображаемое под заголовком. 
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Главная картинка статьи, отображается слева от текста.
		/// </summary>
		public string TitleImage { get; set; }

		/// <summary>
		/// Содержимое статьи.
		/// </summary>
		public string Content { get; set; }


		/// <summary>
		/// Date created article.
		/// </summary>
		public string PublishDate { get; set; }

		/// <summary>
		/// Имя родительской страницы.
		/// </summary>
		public string ParentPageName { get; set; }
	}
}
