using System.ComponentModel.DataAnnotations;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Jobs.Commands;

public sealed record JobCommand
(
    long Id,
    string Title,
    string Description,
    decimal Salary,
    [EnumDataType(typeof(JobTypeEnum))]
    JobTypeEnum JobType,
    ICollection<JobApplication> Applications
);