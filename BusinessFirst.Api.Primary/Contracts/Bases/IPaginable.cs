namespace RenameMe.Api.Primary.Contracts.Bases
{
    public interface IPaginable
    {
        public int Offset { get; set; }

        public int PageSize { get; set; }
    }
}
