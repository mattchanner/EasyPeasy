// -----------------------------------------------------------------------------
// <copyright file="ContactController.cs">
// 
//  The MIT License (MIT)
//  Copyright © 2015 Matt Channer (mchanner at gmail dot com)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace EasyPeasy.Example.Server
{
    public class ContactController : ApiController
    {
        private static IList<Contact> contactDb = new List<Contact>(new [] 
        {
            new Contact { Address = "Address1", Name = "Contact1" },
            new Contact { Address = "Address2", Name = "Contact2" },
            new Contact { Address = "Address3", Name = "Contact3" }
        });

        // GET api/contact
        public IEnumerable<Contact> Get()
        {
            return contactDb;
        }

        // GET api/contact/contact1
        public Contact Get(string id)
        {
            return contactDb.FirstOrDefault(c => c.Name == id);
        }

        // POST api/contact
        public void Post([FromBody]Contact contact)
        {
            contactDb.Add(contact);
        }

        // PUT api/contact/contact1
        public void Put(string id, [FromBody]Contact value)
        {
            Contact contact = this.Get(id);
            if (contact != null)
                contact.Address = value.Address;
        }

        // DELETE api/contact/contact1
        public void Delete(string id)
        {
            for (int i = contactDb.Count - 1; i >= 0; i--)
            {
                if (contactDb[i].Name == id)
                    contactDb.RemoveAt(i);
            }
        }
    }
}

