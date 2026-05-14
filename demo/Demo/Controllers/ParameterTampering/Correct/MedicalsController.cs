namespace Demo.Controllers.ParameterTampering.Correct;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/correct/[controller]")]
public sealed class MedicalsController : ControllerBase
{
    private static readonly IReadOnlyCollection<MedicalRecord> records =
    [
        new(Id: 1, PatientId: 101, Diagnosis: "Hypertension", Treatment: "Lisinopril 10mg daily"),
        new(Id: 2, PatientId: 101, Diagnosis: "Type 2 Diabetes", Treatment: "Metformin 500mg twice daily"),
        new(Id: 3, PatientId: 102, Diagnosis: "Asthma", Treatment: "Salbutamol inhaler as needed"),
        new(Id: 4, PatientId: 103, Diagnosis: "Migraine", Treatment: "Sumatriptan 50mg as needed"),
    ];

    // NOTE: this is still vulnerable — currentUserId comes from the request header,
    // which the client controls freely. This demo only illustrates *where* the ownership
    // check should happen. In a real app, currentUserId must come from a server-issued
    // and server-validated JWT — never from a value the client supplies directly.
    [HttpGet("{id:int}")]
    public ActionResult<MedicalRecord?> GetById(
        [FromHeader(Name = "X-Current-User-Id")] int currentUserId,
        int id)
    {
        var record = records.FirstOrDefault(r => r.Id == id);
        if (record is null)
        {
            var error = new { message = $"Medical record {id} was not found." };
            return this.NotFound(error);
        }

        if (record.PatientId != currentUserId)
        {
            return this.StatusCode(StatusCodes.Status403Forbidden);
        }

        return this.Ok(record);
    }
}

public sealed record MedicalRecord(
    int Id,
    int PatientId,
    string Diagnosis,
    string Treatment);
