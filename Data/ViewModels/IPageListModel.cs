using X.PagedList;

namespace Data.ViewModels
{
	public class IPageListModel<T> : IIPageListModel
	{
		public IPageListModel()
		{
			CanFilterByDateRange = true;
		}
		public DateTime? StudyFromDate { get; set; }
		public DateTime? StudyToDate { get; set; }
		public int? AgeTo { get; set; }
		public int? AgeFrom { get; set; }
		private string? _keyword;
		public string? Keyword
		{
			get => string.IsNullOrWhiteSpace(_keyword) ? null : _keyword.Trim().ToLower();
			set => _keyword = value;
		}
		public string? SearchAction { get; set; }
		public string? SearchController { get; set; }
		public IPagedList<T> Model { get; set; }
		public bool CanFilterByDateRange { get; set; }
		public string? Gender { get; set; }

	}
	public interface IIPageListModel
	{
		DateTime? StudyFromDate { get; set; }
		string? Keyword { get; set; }
		string? SearchAction { get; set; }
		string? SearchController { get; set; }
		DateTime? StudyToDate { get; set; }
		bool CanFilterByDateRange { get; set; }
		int? AgeTo { get; set; }
		int? AgeFrom { get; set; }
		string? Gender { get; set; }
	}
}
