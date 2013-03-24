// -----------------------------------------------------------------------------
// <copyright file="IContactService.cs">
// 
//  The MIT License (MIT)
//  Copyright © 2013 Matt Channer (mchanner at gmail dot com)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a 
//  copy of this software and associated documentation files (the “Software”),
//  to deal in the Software without restriction, including without limitation 
//  the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//  and/or sell copies of the Software, and to permit persons to whom the 
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included 
//  in all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS 
//  OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
//  THE SOFTWARE.
// </copyright>
// ------------------------------------------------------------------------------

using EasyPeasy.Attributes;
using EasyPeasy.Example;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyPeasy
{
    [Path("/api/contact"), 
     Consumes(MediaType.ApplicationJson), 
     Produces(MediaType.ApplicationJson)]
    public interface IContactService
    {
        [GET, Path("/{name}")]
        Contact GetContact([PathParam("name")] string name);

        [GET, Path("/{name}")]
        Task<Contact> GetContactAsync([PathParam("name")] string name);

        [GET, Path("/")]
        List<Contact> GetContacts();

        [GET, Path("/")]
        Task<List<Contact>> GetContactsAsync();

        [POST, Path("/")]
        void CreateContact(Contact contact);

        [POST, Path("/")]
        Task CreateContactAsync(Contact contact);

        [PUT, Path("/{name}")]
        void UpdateContact([PathParam("name")] string name, Contact contact);

        [PUT, Path("/{name}"), Consumes(MediaType.ApplicationUrlEncoded)]
        void UpdateContact([PathParam("name")] string name, [FormParam("address")] string address);

        [PUT, Path("/{name}")]
        Task UpdateContactAsync([PathParam("name")] string name, Contact contact);

        [DELETE, Path("/{name}")]
        void DeleteContact([PathParam("name")] string name);

        [DELETE, Path("/{name}")]
        Task DeleteContactAsync([PathParam("name")] string name);
    }
}
