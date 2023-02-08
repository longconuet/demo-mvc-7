namespace WebApplication7.Requests
{
    public class DatatableRequest
    {
        public int Draw { get; set; }
        public int? Start { get; set; }
        public int? Length { get; set; }
        public IEnumerable<DatatableOrder> Order { get; set; }
        public IEnumerable<DatatableColumn> Columns { get; set; }
        public DatatableSearch Search { get; set; }
    }

    public class DatatableColumn
    {
        public string Name { get; set; } = "";
    }

    public class DatatableOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; } = "";
    }

    public class DatatableSearch
    {
        public string Value { get; set; } = "";
    }
}
