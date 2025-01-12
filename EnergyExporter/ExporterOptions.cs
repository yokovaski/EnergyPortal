namespace EnergyExporter;

public class ExporterOptions
{
    public ExporterDatabase DatabaseType { get; set; }
    public OperationMode OperationMode { get; set; }
}

public enum ExporterDatabase
{
    MySQL,
    PostgreSQL
}

public enum OperationMode
{
    Export,
    Import
}