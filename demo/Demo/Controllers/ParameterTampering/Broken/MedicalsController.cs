namespace Demo.Controllers.ParameterTampering.Broken;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/broken/[controller]")]
public sealed class MedicalsController : ControllerBase
{
    private static readonly IReadOnlyCollection<MedicalRecord> records =
    [
        new(Id: 1, PatientId: 101, Diagnosis: "Hypertension", Treatment: "Lisinopril 10mg daily"),
        new(Id: 2, PatientId: 101, Diagnosis: "Type 2 Diabetes", Treatment: "Metformin 500mg twice daily"),
        new(Id: 3, PatientId: 102, Diagnosis: "Asthma", Treatment: "Salbutamol inhaler as needed"),
        new(Id: 4, PatientId: 103, Diagnosis: "Migraine", Treatment: "Sumatriptan 50mg as needed"),
    ];

    // Vulnerable: fetches the record by id and returns it directly.
    // No check whether the requesting user actually owns this record.
    // A patient with id 101 can request /api/broken/medical-records/3
    // and silently read patient 102's private data.
    [HttpGet("{id:int}")]
    public ActionResult<MedicalRecord?> GetById(int id)
    {
        var record = records.FirstOrDefault(r => r.Id == id);
        if (record is null)
        {
            var error = new { message = $"Medical record {id} was not found." };
            return this.NotFound(error);
        }

        return this.Ok(record);
    }
}

public sealed record MedicalRecord(
    int Id,
    int PatientId,
    string Diagnosis,
    string Treatment);
