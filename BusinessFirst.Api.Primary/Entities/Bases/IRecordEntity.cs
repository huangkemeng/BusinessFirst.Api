namespace RenameMe.Api.Primary.Entities.Bases
{
    /// <summary>
    /// 备份表请基于本接口
    /// </summary>
    public interface IRecordEntity<T> : IEntityPrimary where T : IEntityPrimary
    {
        /// <summary>
        /// 被备份的对象id
        /// </summary>
        public Guid OriginalId { get; set; }
        /// <summary>
        /// 追踪Id
        /// </summary>
        public string TraceId { get; set; }

        static abstract IRecordEntity<T> FromOriginal(T original);
    }
}
