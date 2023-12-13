namespace ProniaAB104.Areas.Admin.ViewModels
{
    public class PaginateVM<T> where T : class,new()
    {
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
        public int Limit { get; set; }
        public List<T> Items { get; set; }

    }
}
