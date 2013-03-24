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

using EasyPeasy.Client;
using System.ComponentModel.Composition.Hosting;
using EasyPeasy.Example;

namespace EasyPeasy
{
    /// <summary> The program entry point class. </summary>
    public class Program
    {
        /// <summary> The main method. </summary>
        public static void Main()
        {
            Uri baseAddress = new Uri("http://localhost:52814");

            AssemblyCatalog catalog = new AssemblyCatalog(typeof(IEasyPeasyFactory).Assembly);
            CompositionContainer container = new CompositionContainer(catalog);

            IEasyPeasyFactory factory = container.GetExportedValue<IEasyPeasyFactory>();
            IContactService contactService = factory.Create<IContactService>(baseAddress);

            try
            {
                foreach (Contact contact in contactService.GetContacts())
                {
                    Console.WriteLine("Name: {0}, Address: {1}", contact.Name, contact.Address);
                }

                Contact singleContact = contactService.GetContact("Contact1");
                Console.WriteLine("Fetched contact by name, Name: {0}, Address: {1}", singleContact.Name, singleContact.Address);

                singleContact.Address = "Changed Address";

                Console.WriteLine("Updating address for contact1");
                contactService.UpdateContact(singleContact.Name, singleContact);

                contactService.UpdateContact("Contact3", "Updated using form param");

                Console.WriteLine("Re-fetching contact list");
                foreach (Contact contact in contactService.GetContacts())
                {
                    Console.WriteLine("Name: {0}, Address: {1}", contact.Name, contact.Address);
                }

                Console.WriteLine("Deleting contact 1");
                contactService.DeleteContact("Contact1");

                Console.WriteLine("Re-fetching contact list");
                foreach (Contact contact in contactService.GetContacts())
                {
                    Console.WriteLine("Name: {0}, Address: {1}", contact.Name, contact.Address);
                }
            }
            catch (WebException ex)
            {
                Console.Error.Write(ex);                
            }
            Console.ReadKey();
        }
    }
}
