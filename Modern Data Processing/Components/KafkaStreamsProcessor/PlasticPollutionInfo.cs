namespace KafkaStreamsProcessor;

/// <summary>
/// Represents detailed information about plastic pollution in a specific country and year.
/// </summary>
/// <param name="Country">The name of the country where plastic pollution data was recorded.</param>
/// <param name="Year">The year of data collection.</param>
/// <param name="ParentCompany">The name of the parent company responsible for the plastic waste.</param>
/// <param name="Empty">The amount of empty plastic containers.</param>
/// <param name="Hdpe">The amount of High-Density Polyethylene (HDPE) plastic waste.</param>
/// <param name="Idpe">The amount of Low-Density Polyethylene (LDPE) plastic waste.</param>
/// <param name="O">The amount of other types of plastic waste.</param>
/// <param name="Pet">The amount of Polyethylene Terephthalate (PET) plastic waste.</param>
/// <param name="Pp">The amount of Polypropylene (PP) plastic waste.</param>
/// <param name="Ps">The amount of Polystyrene (PS) plastic waste.</param>
/// <param name="Pvc">The amount of Polyvinyl Chloride (PVC) plastic waste.</param>
/// <param name="GrandTotal">The total count of all plastic waste types collected.</param>
/// <param name="NumEvents">The number of events where data was collected.</param>
/// <param name="Volunteers">The number of volunteers who participated in data collection.</param>
internal sealed record PlasticPollutionInfo(
    string Country,
    int Year,
    string ParentCompany,
    int Empty,
    int Hdpe,
    int Idpe,
    int O,
    int Pet,
    int Pp,
    int Ps,
    int Pvc,
    int GrandTotal,
    int NumEvents,
    int Volunteers
);