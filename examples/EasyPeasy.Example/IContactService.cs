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
    // The top level Path attribute ensure all methods are made relative to this.
	//
    // The Produces attribute specifies the default serializer to use when writing data
    // to the body of the request and also determines the value of the Content-Type header.
	//
    // The Consume attribute is used to determine the expected content type of the response and
	// is also used to set the Accept header.
	//
    [Path("/api/contact"), 
     Consumes(MediaType.ApplicationXml),
     Produces(MediaType.ApplicationXml)]
    public interface IContactService
    {
        // Synchronous GET request. The /{name} path attribute is relative to /api/contract 
		// (which is defined on the interface).
        // The /{name} value is mapped to the method argument using the PathParam attribute
        [GET, Path("/{name}")]
        Contact GetContact([PathParam("name")] string name);

        // Asynchrounous version of the GetContact call. To make an async call, simply return 
		// a Task<T>, or for void operations, a Task
        [GET, Path("/{name}")]
        Task<Contact> GetContactAsync([PathParam("name")] string name);

        // Gets the list of contacts from the server.  The '/' path simply maps to the /api/contact
        [GET, Path("/")]
        List<Contact> GetContacts();

        // Posts a new contact to the server.  The supplied Contact will be added to the body 
		// using the serializer defined by the Produces attribute (in this case, application/xml).
        // The most common formats are supported out of the box, but can be extended if required using
        // EasyPeasyFactory.Registry.RegisterCustomTypeHandler
        [POST, Path("/")]
        void CreateContact(Contact contact);

        // As above, but an async version
        [POST, Path("/")]
        Task CreateContactAsync(Contact contact);

        // Updates a new contact py putting an update contact to the /api/contact/{name} endpoint.
        // By default, an argument without an attribute will be written as the body of the request.
        [PUT, Path("/{name}")]
        void UpdateContact([PathParam("name")] string name, Contact contact);

        // Updates the address of a contact by sending a url encoded form e.g.:
        // address=value
        // This is an example of overriding the interfaces media types on a per method basis.
        [PUT, Path("/{name}"), Produces(MediaType.ApplicationUrlEncoded)]
        void UpdateContact([PathParam("name")] string name, [FormParam("address")] string address);

        // Deletes a contact on the server
        [DELETE, Path("/{name}")]
        void DeleteContact([PathParam("name")] string name);
    }
}
