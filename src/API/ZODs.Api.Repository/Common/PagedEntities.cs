namespace ZODs.Api.Repository.Common
{
    public class PagedEntities<TEntity>
    {
        public List<TEntity> Entities { get; set; } = new List<TEntity>();

        public int TotalCount { get; set; }
    }
}
