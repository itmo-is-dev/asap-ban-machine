using Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;

namespace Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine.Models;

public record BanMachineAnalysisRequest(SubmissionContent FirstSubmission, SubmissionContent SecondSubmission);