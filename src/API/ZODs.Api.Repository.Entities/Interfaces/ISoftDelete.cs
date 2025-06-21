namespace ZODs.Api.Repository.Entities.Interfaces
{
    /// <summary>
    /// Interface that define properties for soft delete.
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Gets or sets a value indicating whether entity is soft deleted or not.
        /// </summary>
        /// <value>True if entity is soft deleted, otherwise False.</value>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the date the entity was deleted.
        /// </summary>
        /// <value>The date and time the entity was deleted.</value>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the member that deleted the entity.
        /// </summary>
        /// <value>D of the member that deleted the entity.</value>
        public string DeletedById { get; set; }
    }
}
