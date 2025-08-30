using System;
using System.Collections.Generic;

namespace BMATM.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>The entity if found, null otherwise</returns>
        T GetById(int id);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>Collection of all entities</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Inserts a new entity.
        /// </summary>
        /// <param name="entity">The entity to insert</param>
        /// <returns>The ID of the inserted entity</returns>
        int Insert(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>True if update was successful, false otherwise</returns>
        bool Update(T entity);

        /// <summary>
        /// Deletes an entity by ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        bool Delete(int id);

        /// <summary>
        /// Gets entities with pagination support.
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <returns>Collection of entities</returns>
        IEnumerable<T> GetPaged(int skip, int take);

        /// <summary>
        /// Gets the total count of entities.
        /// </summary>
        /// <returns>Total count</returns>
        int GetCount();

        /// <summary>
        /// Checks if an entity exists by ID.
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>True if entity exists, false otherwise</returns>
        bool Exists(int id);
    }

    /// <summary>
    /// Extended repository interface with additional common operations.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IExtendedRepository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// Gets entities that match a custom condition.
        /// </summary>
        /// <param name="predicate">The condition to match</param>
        /// <returns>Collection of matching entities</returns>
        IEnumerable<T> Find(Func<T, bool> predicate);

        /// <summary>
        /// Gets a single entity that matches a condition.
        /// </summary>
        /// <param name="predicate">The condition to match</param>
        /// <returns>The matching entity or null</returns>
        T FindSingle(Func<T, bool> predicate);

        /// <summary>
        /// Bulk insert operation.
        /// </summary>
        /// <param name="entities">Collection of entities to insert</param>
        /// <returns>Number of entities inserted</returns>
        int BulkInsert(IEnumerable<T> entities);

        /// <summary>
        /// Bulk update operation.
        /// </summary>
        /// <param name="entities">Collection of entities to update</param>
        /// <returns>Number of entities updated</returns>
        int BulkUpdate(IEnumerable<T> entities);

        /// <summary>
        /// Bulk delete operation.
        /// </summary>
        /// <param name="ids">Collection of IDs to delete</param>
        /// <returns>Number of entities deleted</returns>
        int BulkDelete(IEnumerable<int> ids);
    }
}