// -----------------------------------------------------------------------
// <copyright file="ExpectedIL.cs" company="IDBS">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

using EasyPeasy.Core;

namespace EasyPeasy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Expected class definition
    /// </summary>
    public class ExpectedIL : RestClient, IRestEntityService
    {
        public ExpectedIL() : base("/services/1.0", MediaType.ApplicationXml, MediaType.ApplicationXml)
        {
        }

        /// <summary>
        /// Gets an entity based on it's identifier
        /// </summary>
        /// <param name="entityId"> The entity id.  </param>
        /// <param name="number"> The number value. </param>
        /// <returns>The entity string</returns>
        public Task<string> GetEntity(string entityId, int number)
        {
            return base.AsyncHandleGeneric<string>("/entity", "GET", new ParameterData[]
                {
                    new ParameterData
                        {
                            IsPathParam = true,
                            IsQueryStringParam = false,
                            ParameterName = "entityId",
                            ParameterValue = entityId
                        }, 
                    new ParameterData
                        {
                            IsPathParam = false,
                            IsQueryStringParam = true,
                            ParameterName = "number",
                            ParameterValue = number
                        }
                });
        }

        /// <summary>
        /// Adds an entity
        /// </summary>
        /// <param name="entityId">The entity id</param>
        /// <param name="q">Query string parameter</param>
        /// <returns>The result of adding</returns>
        public Task<string> AddEntity(string entityId, string q)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds an entity
        /// </summary>
        /// <param name="entityId">The entity id</param>
        /// <param name="q">Query string parameter</param>
        /// <returns>The result of adding</returns>
        public List<string> GetEntityList(string parentEntityId)
        {
            throw new NotImplementedException();
        }

        public void PutEntityId(string parentEntityId)
        {
            throw new NotImplementedException();
        }

        public Task PutEntityIdAsync(string parentEntityId)
        {
            throw new NotImplementedException();
        }
    }
}
