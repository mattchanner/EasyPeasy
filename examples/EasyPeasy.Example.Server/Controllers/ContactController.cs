using Microsoft.AspNetCore.Mvc;

namespace EasyPeasy.Example.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private static readonly List<Contact> ContactDb =
    [
        new Contact { Address = "Address1", Name = "Contact1" },
        new Contact { Address = "Address2", Name = "Contact2" },
        new Contact { Address = "Address3", Name = "Contact3" },
    ];

    // GET api/contact
    [HttpGet]
    public IEnumerable<Contact> Get() => ContactDb;

    // GET api/contact/contact1
    [HttpGet("{id}")]
    public ActionResult<Contact> Get(string id)
    {
        var contact = ContactDb.FirstOrDefault(c => c.Name == id);
        return contact is null ? NotFound() : contact;
    }

    // POST api/contact
    [HttpPost]
    public IActionResult Post([FromBody] Contact contact)
    {
        ContactDb.Add(contact);
        return CreatedAtAction(nameof(Get), new { id = contact.Name }, contact);
    }

    // PUT api/contact/contact1
    [HttpPut("{id}")]
    public IActionResult Put(string id, [FromBody] Contact value)
    {
        var contact = ContactDb.FirstOrDefault(c => c.Name == id);
        if (contact is null)
        {
            return NotFound();
        }

        contact.Address = value.Address;
        return NoContent();
    }

    // PUT api/contact/contact1/address (form encoded: address=...)
    [HttpPut("{id}/address")]
    [Consumes("application/x-www-form-urlencoded")]
    public IActionResult PutAddress(string id, [FromForm] string address)
    {
        var contact = ContactDb.FirstOrDefault(c => c.Name == id);
        if (contact is null)
        {
            return NotFound();
        }

        contact.Address = address;
        return NoContent();
    }

    // DELETE api/contact/contact1
    [HttpDelete("{id}")]
    public IActionResult Delete(string id) =>
        ContactDb.RemoveAll(c => c.Name == id) > 0 ? NoContent() : NotFound();
}
