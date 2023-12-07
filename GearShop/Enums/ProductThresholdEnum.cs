namespace GearShop.Enums
{
	/// <summary>
	/// Пороги для определения количества продуктов-«Нет», «Мало», «Достаточно», «Много»
	/// </summary>
	public enum ProductThresholdEnum
	{
		/// <summary>
		/// Нет
		/// </summary>
		Empty = 0,

		EmptyThreshold = 0,

		/// <summary>
		/// Мало
		/// </summary>
		NotEnough = 1,

		NotEnoughThreshold = 5,

		/// <summary>
		/// Достаточно
		/// </summary>
		Enough = 2,
		EnoughThreshold = 15,

		/// <summary>
		/// Много
		/// </summary>
		Lot = 3
	}
}
