// -----------------------------------------------------------------------------
// <copyright file="Program.cs">
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

using System;
using System.Net;
using System.ComponentModel.Composition.Hosting;
using EasyPeasy;

using EasyPeasy.Example;

namespace EasyPeasy
{
    /// <summary> 
    /// An example client, showing how to use EasyPeasy to generate a usable client
    /// from a configured service interface.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            // This is the based address of the server which gets passed into the 
            // creation method
            Uri baseAddress = new Uri("http://localhost:9000");

            // The factory can be created using MEF, the following is an example of how
            // you could do this:
            AssemblyCatalog catalog = new AssemblyCatalog(typeof(IEasyPeasyFactory).Assembly);
            CompositionContainer container = new CompositionContainer(catalog);

            IEasyPeasyFactory factory = container.GetExportedValue<IEasyPeasyFactory>();

            // An alternative would be the more direct way:
            // IEasyPeasyFactory factory = new EasyPeasyFactory(new DefaultMediaTypeRegistry());

            // Auto generate an implementation of the IContactService interface.
            // The implementation is configured via the interface attributes to determine each
            // methods end point, serialization formats etc.
            IContactService contactService = factory.Create<IContactService>(baseAddress);

            // The following are examples of using the implementation

            // 1 - fetch a list of contacts from the server and print them out
            // This method call maps to:
            // GET http://localhost:9000/api/contact
            foreach (Contact contact in contactService.GetContacts())
            {
                Console.WriteLine("Name: {0}, Address: {1}", contact.Name, contact.Address);
            }

            // Fetch a specific contact.  The value passed in here is used in the URL
            // GET http://localhost:9000/api/contact/Contact1
            Contact singleContact = contactService.GetContact("Contact1");
            Console.WriteLine("Fetched contact by name, Name: {0}, Address: {1}", singleContact.Name, singleContact.Address);

            singleContact.Address = "Changed Address";

            Console.WriteLine("Updating address for contact1");

            // Updates the contact on the server.  The supplied name is mapped to the URL,
            // the contact is serialized to the body as XML based on the Produces attribute
            contactService.UpdateContact(singleContact.Name, singleContact);

            // Another example of updating the address for a contact. This example
            // uses form encoded parameters to send the data:
            //
            // PUT   http://localhost:9000/api/contact/Contact3
            // BODY: address=Updated_using_form_param
            contactService.UpdateContact("Contact3", "Updated_using_form_param");

            // Show the updates worked by reloading the data and printing the updated values
            Console.WriteLine("Re-fetching contact list");
            foreach (Contact contact in contactService.GetContacts())
            {
                Console.WriteLine("Name: {0}, Address: {1}", contact.Name, contact.Address);
            }

            // Deletes a contact on the server using similar path mappings.
            // The DELETE attribute determines the verb to use:
            //
            // DELETE http://localhost:9000/api/contact/Contact1
            Console.WriteLine("Deleting contact 1");
            contactService.DeleteContact("Contact1");

            // Reload to show the contact has been deleted
            Console.WriteLine("Re-fetching contact list");
            foreach (Contact contact in contactService.GetContacts())
            {
                Console.WriteLine("Name: {0}, Address: {1}", contact.Name, contact.Address);
            }

            Console.ReadKey();
        }
    }
}
