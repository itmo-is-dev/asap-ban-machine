namespace Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

public readonly record struct AnalysisId(long Value)
{
    public override string ToString()
        => Value.ToString();
}