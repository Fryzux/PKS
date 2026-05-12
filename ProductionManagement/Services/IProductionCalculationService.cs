namespace ProductionManagement.Services;

public interface IProductionCalculationService
{
    int CalculateMinutes(int quantity, int productionTimePerUnit, float efficiencyFactor);
    DateTime CalculateEndDate(DateTime startDate, int totalMinutes);
}

public class ProductionCalculationService : IProductionCalculationService
{
    public int CalculateMinutes(int quantity, int productionTimePerUnit, float efficiencyFactor)
    {
        if (efficiencyFactor <= 0) efficiencyFactor = 1.0f;
        return (int)Math.Ceiling((quantity * productionTimePerUnit) / (double)efficiencyFactor);
    }

    public DateTime CalculateEndDate(DateTime startDate, int totalMinutes)
        => startDate.AddMinutes(totalMinutes);
}
