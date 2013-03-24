using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EasyPeasy.Example.Services.Controllers
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
        public Contact Get(string name)
        {
            return contactDb.FirstOrDefault(c => c.Name == name);
        }

        // POST api/contact
        public void Post([FromBody]Contact contact)
        {
            contactDb.Add(contact);
        }

        // PUT api/contact/contact1
        public void Put(string name, [FromBody]Contact value)
        {
            Contact contact = this.Get(name);
            if (contact != null)
                contact.Address = value.Address;
        }
        
        // DELETE api/contact/contact1
        public void Delete(string name)
        {
            for (int i = contactDb.Count - 1; i >= 0; i--)
            {
                if (contactDb[i].Name == name)
                    contactDb.RemoveAt(i);
            }
        }
    }
}
