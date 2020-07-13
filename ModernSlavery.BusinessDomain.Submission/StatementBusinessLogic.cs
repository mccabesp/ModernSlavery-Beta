using AutoMapper;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Submission
{
    partial class StatementBusinessLogic : IStatementBusinessLogic
    {
        readonly ISharedBusinessLogic SharedBusinessLogic;
        readonly IMapper Mapper;

        public StatementBusinessLogic(IMapper mapper, ISharedBusinessLogic sharedBusinessLogic)
        {
            Mapper = mapper;
            SharedBusinessLogic = sharedBusinessLogic;
        }

        async Task<Statement> FindStatementAsync(StatementModel model)
            => await FindStatementAsync(model.OrganisationId, model.Year);

        async Task<Statement> FindStatementAsync(Organisation organisation, int year)
            => await FindStatementAsync(organisation.OrganisationId, year);

        async Task<Statement> FindStatementAsync(long organisationId, int year)
        {
            return await SharedBusinessLogic.DataRepository
                .FirstOrDefaultAsync<Statement>(s => s.OrganisationId == organisationId && s.SubmissionDeadline.Year == year);
        }

        public async Task<StatementModel> GetStatementByOrganisationAndYear(Organisation organisation, int year)
        {
            var path = GetFileName(organisation.OrganisationId, year);
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(path))
            {
                var content = await SharedBusinessLogic.FileRepository.ReadAsync(path);

                var fileResult = JsonConvert.DeserializeObject<StatementModel>(content);

                return fileResult;
            }

            var statement = await FindStatementAsync(organisation, year);

            var dataResult = Mapper.Map<StatementModel>(statement);
            return dataResult;
        }

        public async Task<StatementActionResult> CanAccessStatement(User user, Organisation organisation, int reportingYear)
        {
            // only assigned users
            var assignment = organisation.UserOrganisations.FirstOrDefault(uo => uo.User == user);
            if (assignment == null)
                return StatementActionResult.Unauthorised;

            // only active/pending/new organisations
            if (!organisation.Status.IsAny(OrganisationStatuses.Active, OrganisationStatuses.Pending, OrganisationStatuses.New))
                return StatementActionResult.Unauthorised;

            var statement = await FindStatementAsync(organisation, reportingYear);
            if (statement != null && !statement.CanBeEdited)
                return StatementActionResult.Uneditable;

            return StatementActionResult.Success;
        }

        private async Task SaveToFile(StatementModel statement)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(statement, Formatting.Indented, settings));
            var fileName = GetFileName(statement.OrganisationId, statement.SubmissionDeadline.Year);
            await SharedBusinessLogic.FileRepository.WriteAsync(fileName, data);
        }

        private string GetFileName(long organisationId, int reportingYear)
            => $"{organisationId}_{reportingYear}.json";

        public async Task<StatementActionResult> SaveDraftStatement(User user, StatementModel statementModel)
        {
            var statement = await SharedBusinessLogic.DataRepository
                .FirstOrDefaultAsync<Statement>(s => s.OrganisationId == statementModel.OrganisationId && s.SubmissionDeadline.Year == statementModel.Year);

            if (!statement?.CanBeEdited ?? false)
            {
                return StatementActionResult.Uneditable;
            }

            // not been saved to DB
            await SaveToFile(statementModel);
            return StatementActionResult.Success;
        }

        public async Task<StatementActionResult> SaveStatement(User user, StatementModel statementModel)
        {
            var statement = await FindStatementAsync(statementModel);

            if (!statement?.CanBeEdited ?? false)
            {
                return StatementActionResult.Uneditable;
            }

            // Is this check enough?
            if (!statementModel.StatementId.HasValue)
            {
                var org = await SharedBusinessLogic.DataRepository
                    .FirstOrDefaultAsync<Organisation>(o => o.OrganisationId == statementModel.OrganisationId);
                statement.OrganisationId = statementModel.OrganisationId;
                statement.SubmissionDeadline = SharedBusinessLogic.GetAccountingStartDate(org.SectorType, VirtualDateTime.Now.Year);
            }

            SharedBusinessLogic.DataRepository.Update(statement);

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();
            SharedBusinessLogic.DataRepository.CommitTransaction();

            return StatementActionResult.Success;
        }
    }

    public class StatementMapperProfile : Profile
    {
        public StatementMapperProfile()
        {
            // name things same as DB
            CreateMap<Statement, StatementModel>()
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.SubmissionDeadline.Year))
                .ForMember(dest => dest.StatementSectors, opt => opt.MapFrom(src => src.Sectors.Select(s => new KeyValuePair<int, string>(s.StatementSectorTypeId, s.StatementSectorType.Description))))
                .ForMember(dest => dest.OtherSectorText, opt => opt.MapFrom(src => src.OtherSector))
                .ForMember(dest => dest.StatementPolicies, opt => opt.MapFrom(src => src.Policies.Select(p => new KeyValuePair<int, string>(p.StatementPolicyTypeId, p.StatementPolicyType.Description))))
                .ForMember(dest => dest.OtherPolicyText, opt => opt.MapFrom(src => src.OtherPolicy))
                .ForMember(dest => dest.StatementTrainingDivisions, opt => opt.MapFrom(src => src.TrainingDivisions.Select(t => new KeyValuePair<int, string>(t.StatementDivisionTypeId, t.StatementDivisionType.Description))))
                .ForMember(dest => dest.OtherTrainingText, opt => opt.MapFrom(src => src.OtherTrainingDivision))
                .ForMember(dest => dest.StatementRiskTypes, opt => opt.MapFrom(src => src.Risks.Select(r => new KeyValuePair<int, string>(r.StatementRiskTypeId, r.StatementRiskType.Description))))
                .ForMember(dest => dest.StatementDiligenceTypes, opt => opt.MapFrom(src => src.Diligences.Select(d => new KeyValuePair<int, string>(d.StatementDiligenceTypeId, d.StatementDiligenceType.Description))))
                // These should be added to the entity
                .ForMember(dest => dest.Countries, opt => opt.Ignore());
        }
    }
}
