using AutoMapper;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes.ErrorMessages;
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
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Submission
{
    partial class StatementBusinessLogic : IStatementBusinessLogic
    {
        private readonly SubmissionOptions _submissionOptions;
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IMapper _mapper;

        public StatementBusinessLogic(SubmissionOptions submissionOptions,ISharedBusinessLogic sharedBusinessLogic, IMapper mapper)
        {
            _submissionOptions = submissionOptions;
            _sharedBusinessLogic = sharedBusinessLogic;
            _mapper = mapper;
        }

        private string GetDraftFilepath(long organisationId, int reportingYear) => Path.Combine(_submissionOptions.DraftsPath, $"{organisationId}_{reportingYear}.json");
        private string GetDraftBackupFilepath(long organisationId, int reportingYear) => Path.Combine(_submissionOptions.DraftsPath, $"{organisationId}_{reportingYear}.bak");

        private async Task<StatementModel> FindDraftStatementModelAsync(long organisationId, int reportingDeadlineYear)
        {
            var draftFilePath = GetDraftFilepath(organisationId, reportingDeadlineYear);
            
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath))
            {
                var draftJson=await _sharedBusinessLogic.FileRepository.ReadAsync(draftFilePath);
                return JsonConvert.DeserializeObject<StatementModel>(draftJson);
            }

            return null;
        }
        private async Task<Statement> FindSubmittedStatementAsync(long organisationId, int reportingDeadlineYear)
        {
            return await _sharedBusinessLogic.DataRepository
                .FirstOrDefaultAsync<Statement>(s => s.OrganisationId == organisationId && s.SubmissionDeadline.Year == reportingDeadlineYear);
        }

        private async Task DeleteDraftStatementModelAsync(long organisationId, int reportingDeadlineYear)
        {
            //Delete the original
            var draftFilePath = GetDraftFilepath(organisationId, reportingDeadlineYear);
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath))
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftFilePath);

            //Delete the backup
            var draftBackupFilePath = GetDraftBackupFilepath(organisationId, reportingDeadlineYear);
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftBackupFilePath);
        }
        public async Task<Outcome<StatementErrors, StatementModel>> GetSubmittedStatementModel(long organisationId, int reportingDeadlineYear)
        {
            //TODO: Validate method parameters

            //Try and get the organisation
            var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new Exception($"Invalid organisationId {organisationId}");

            var statement = organisation.Statements.FirstOrDefault(s => s.SubmissionDeadline.Year == reportingDeadlineYear);
            if (statement == null) return new Outcome<StatementErrors, StatementModel>(StatementErrors.NotFound);

            var statementModel = new StatementModel();

            //Copy the statement properties to the model
            _mapper.Map(statement, statementModel);

            //Return the successful model
            return new Outcome<StatementErrors, StatementModel>(statementModel);
        }

        public async Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModel(long organisationId, int reportingDeadlineYear, long userId)
        {
            //TODO: Validate method parameters

            //Try and get the organisation
            var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new Exception($"Invalid organisationId {organisationId}");

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(userId))
                return new Outcome<StatementErrors, StatementModel>(StatementErrors.Unauthorised);

            //Try and get the draft first
            var statementModel = await FindDraftStatementModelAsync(organisationId, reportingDeadlineYear);

            //Check if the existing statement is owned by another user
            if (statementModel != null && statementModel.UserId > 0 && statementModel.UserId != userId)
            {
                //Check if the existing statement is still locked 
                if (statementModel.Timestamp.AddMinutes(_submissionOptions.DraftTimeoutMinutes) > VirtualDateTime.Now)
                    return new Outcome<StatementErrors, StatementModel>(StatementErrors.Locked);

                //Delete the other users draft file and its backup
                await DeleteDraftStatementModelAsync(organisationId, reportingDeadlineYear);
            }

            statementModel = new StatementModel();
            statementModel.UserId = userId;
            statementModel.Timestamp = VirtualDateTime.Now;

            var submittedStatement = await FindSubmittedStatementAsync(organisationId, reportingDeadlineYear);
            if (submittedStatement != null)
            {
                //Check its not too late to edit 
                if (submittedStatement.SubmissionDeadline.Date.AddDays(0- _submissionOptions.DeadlineExtensionDays) <VirtualDateTime.Now.Date)
                    return new Outcome<StatementErrors, StatementModel>(StatementErrors.TooLate);

                //TODO Load data from statement entity into the statementmodel
            }

            //Save new statement model with timestamp to lock to new user
            await SaveDraftStatementModel(statementModel);

            return new Outcome<StatementErrors, StatementModel>(statementModel);
        }

        public async Task CancelEditDraftStatementModel(long organisationId, int reportingDeadlineYear)
        {
            //Get the current draft filepath
            var draftFilePath = GetDraftFilepath(organisationId, reportingDeadlineYear);

            //Get the backup draft filepath
            var draftBackupFilePath = GetDraftBackupFilepath(organisationId, reportingDeadlineYear);

            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
            {
                //Restore the original draft from the backup
                await _sharedBusinessLogic.FileRepository.CopyFileAsync(draftBackupFilePath, draftFilePath, true);

                //Delete the backup draft
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftBackupFilePath);
            }
        }

        public async Task SaveDraftStatementModel(StatementModel statementModel)
        {
            var organisation = _sharedBusinessLogic.DataRepository.Get<Organisation>(statementModel.OrganisationId);

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(statementModel.UserId))
                throw new SecurityException($"User {statementModel.UserId} does not have permission to submit for organisation {statementModel.OrganisationId}");

            //Get the current draft filepath
            var draftFilePath = GetDraftFilepath(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);

            //Create a backup if required
            var draftBackupFilePath = GetDraftBackupFilepath(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);
            if (!await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath) && await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath))
                await _sharedBusinessLogic.FileRepository.CopyFileAsync(draftFilePath, draftBackupFilePath, false);

            //Save the new draft data 
            var draftJson = JsonConvert.SerializeObject(statementModel);
            await _sharedBusinessLogic.FileRepository.WriteAsync(draftFilePath, Encoding.UTF8.GetBytes(draftJson));
        }

        public async Task SubmitDraftStatementModel(StatementModel statementModel)
        {
            var organisation = _sharedBusinessLogic.DataRepository.Get<Organisation>(statementModel.OrganisationId);

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(statementModel.UserId))
                throw new SecurityException($"User {statementModel.UserId} does not have permission to submit for organisation {statementModel.OrganisationId}");

            var previousStatement = await FindSubmittedStatementAsync(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);
            previousStatement.SetStatus(StatementStatuses.Retired, statementModel.UserId);

            var newStatement = new Statement();
            newStatement.Organisation = organisation;
            newStatement.SetStatus(StatementStatuses.Submitted, statementModel.UserId);
            organisation.Statements.Add(newStatement);

            //Copy all the other model propertiesto the entity
            _mapper.Map(statementModel, newStatement);

            //Save the changes to the database
            await _sharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Delete the draft file and its backup
            await DeleteDraftStatementModelAsync(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);
        }
    }

    public class StatementMapperProfile : Profile
    {
        public StatementMapperProfile()
        {
            // name things same as DB
            CreateMap<Statement, StatementModel>()
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // TODO - James Map this appropriately
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.SubmissionDeadline.Year))
                .ForMember(dest => dest.StatementSectors, opt => opt.MapFrom(src => src.Sectors.Select(s => s.StatementSectorTypeId)))
                .ForMember(dest => dest.StatementPolicies, opt => opt.MapFrom(src => src.Policies.Select(p => p.StatementPolicyTypeId)))
                .ForMember(dest => dest.Training, opt => opt.MapFrom(src => src.Training.Select(t => t.StatementTrainingTypeId)))
                .ForMember(dest => dest.RelevantRisks, opt => opt.MapFrom(src => src.RelevantRisks.Select(r => r.StatementRiskTypeId)))
                .ForMember(dest => dest.HighRisks, opt => opt.MapFrom(src => src.HighRisks.Select(r => r.StatementRiskTypeId)))
                .ForMember(dest => dest.LocationRisks, opt => opt.MapFrom(src => src.LocationRisks.Select(r => r.StatementRiskTypeId)))
                .ForMember(dest => dest.Diligences, opt => opt.MapFrom(src => src.Diligences.Select(d => d.StatementDiligenceTypeId)));
        }
    }
}
