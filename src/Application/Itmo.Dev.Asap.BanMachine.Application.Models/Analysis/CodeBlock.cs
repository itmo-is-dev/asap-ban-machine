namespace Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

public record struct CodeBlock(string FilePath, int LineFrom, int LineTo, string Content);