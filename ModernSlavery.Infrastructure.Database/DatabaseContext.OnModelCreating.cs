using Microsoft.EntityFrameworkCore;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Entities.Views;
using System.Linq;

namespace ModernSlavery.Infrastructure.Database
{
    public partial class DatabaseContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Map the correct entity to table names
            modelBuilder.Entity<Organisation>().ToTable("Organisations");
            modelBuilder.Entity<OrganisationAddress>().ToTable("OrganisationAddresses");
            modelBuilder.Entity<OrganisationName>().ToTable("OrganisationNames");
            modelBuilder.Entity<OrganisationReference>().ToTable("OrganisationReferences");
            modelBuilder.Entity<OrganisationScope>().ToTable("OrganisationScopes");
            modelBuilder.Entity<OrganisationSicCode>().ToTable("OrganisationSicCodes");
            modelBuilder.Entity<Statement>().ToTable("Statements");
            modelBuilder.Entity<Return>().ToTable("Returns");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserStatus>().ToTable("UserStatus");
            modelBuilder.Entity<PublicSectorType>().ToTable("PublicSectorTypes");
            modelBuilder.Entity<OrganisationPublicSectorType>().ToTable("OrganisationPublicSectorTypes");
            modelBuilder.Entity<Feedback>().ToTable("Feedback");
            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
            modelBuilder.Entity<ReminderEmail>().ToTable("ReminderEmails");

            #region AddressStatus

            modelBuilder.Entity<AddressStatus>(
                entity =>
                {
                    entity.HasKey(e => e.AddressStatusId).HasName("PK_dbo.AddressStatus");

                    entity.HasIndex(e => e.AddressId)
                        .HasName("IX_AddressId");

                    entity.HasIndex(e => e.ByUserId)
                        .HasName("IX_ByUserId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);
                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.HasOne(d => d.Address)
                        .WithMany(p => p.AddressStatuses)
                        .HasForeignKey(d => d.AddressId)
                        .HasConstraintName("FK_dbo.AddressStatus_dbo.OrganisationAddresses_AddressId");

                    entity.HasOne(d => d.ByUser)
                        .WithMany(p => p.AddressStatus)
                        .HasForeignKey(d => d.ByUserId)
                        .HasConstraintName("FK_dbo.AddressStatus_dbo.Users_ByUserId");
                });

            #endregion

            #region OrganisationAddress

            modelBuilder.Entity<OrganisationAddress>(
                entity =>
                {
                    entity.HasKey(e => e.AddressId).HasName("PK_dbo.OrganisationAddresses");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.Address1).HasMaxLength(100);

                    entity.Property(e => e.Address2).HasMaxLength(100);

                    entity.Property(e => e.Address3).HasMaxLength(100);

                    entity.Property(e => e.Country).HasMaxLength(100);

                    entity.Property(e => e.County).HasMaxLength(100);

                    entity.Property(e => e.PoBox).HasMaxLength(30);

                    entity.Property(e => e.PostCode).HasMaxLength(20);

                    entity.Property(e => e.Source).HasMaxLength(255);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.Property(e => e.TownCity).HasMaxLength(100);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationAddresses)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationAddresses_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region OrganisationName

            modelBuilder.Entity<OrganisationName>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationNameId).HasName("PK_dbo.OrganisationNames");

                    entity.HasIndex(e => e.Created)
                        .HasName("IX_Created");

                    entity.HasIndex(e => e.Name)
                        .HasName("IX_Name");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.Property(e => e.Name)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.Source).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationNames)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationNames_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region OrganisationReference

            modelBuilder.Entity<OrganisationReference>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationReferenceId).HasName("PK_dbo.OrganisationReferences");

                    entity.HasIndex(e => e.Created)
                        .HasName("IX_Created");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.ReferenceName)
                        .HasName("IX_ReferenceName");

                    entity.HasIndex(e => e.ReferenceValue)
                        .HasName("IX_ReferenceValue");

                    entity.Property(e => e.ReferenceName)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.ReferenceValue)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationReferences)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationReferences_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region Organisation

            modelBuilder.Entity<Organisation>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationId).HasName("PK_dbo.Organisations");

                    entity.HasIndex(e => e.CompanyNumber)
                        .HasName("idx_Organisations_CompanyNumber")
                        .IsUnique()
                        .HasFilter("([CompanyNumber] IS NOT NULL)");

                    entity.HasIndex(e => e.DUNSNumber)
                        .HasName("idx_Organisations_DUNSNumber")
                        .IsUnique()
                        .HasFilter("([DUNSNumber] IS NOT NULL)");

                    entity.HasIndex(e => e.EmployerReference)
                        .HasName("idx_Organisations_EmployerReference")
                        .IsUnique()
                        .HasFilter("([EmployerReference] IS NOT NULL)");

                    entity.HasIndex(e => e.LatestAddressId)
                        .HasName("IX_LatestAddressId");

                    entity.HasIndex(e => e.LatestReturnId)
                        .HasName("IX_LatestReturnId");

                    entity.HasIndex(e => e.LatestScopeId)
                        .HasName("IX_LatestScopeId");

                    entity.HasIndex(e => e.OrganisationName)
                        .HasName("IX_OrganisationName");

                    entity.HasIndex(e => e.SectorType)
                        .HasName("IX_SectorTypeId");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.HasIndex(e => new { e.LatestRegistrationUserId, e.LatestRegistrationOrganisationId })
                        .HasName("IX_LatestRegistration_UserId_LatestRegistration_OrganisationId");

                    entity.Property(e => e.Status).HasColumnName("StatusId");
                    entity.Property(e => e.SectorType).HasColumnName("SectorTypeId");

                    entity.Property(e => e.CompanyNumber).HasMaxLength(10);

                    entity.Property(e => e.DUNSNumber)
                        .HasMaxLength(10);

                    entity.Property(e => e.EmployerReference).HasMaxLength(10);

                    entity.Property(e => e.LatestRegistrationOrganisationId)
                        .HasColumnName("LatestRegistration_OrganisationId");

                    entity.Property(e => e.LatestRegistrationUserId).HasColumnName("LatestRegistration_UserId");

                    entity.Property(e => e.OrganisationName)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.LatestAddress)
                        .WithMany(p => p.Organisations)
                        .HasForeignKey(d => d.LatestAddressId)
                        .HasConstraintName("FK_dbo.Organisations_dbo.OrganisationAddresses_LatestAddressId");

                    entity.HasOne(d => d.LatestReturn)
                        .WithMany(p => p.Organisations)
                        .HasForeignKey(d => d.LatestReturnId)
                        .HasConstraintName("FK_dbo.Organisations_dbo.Returns_LatestReturnId");

                    entity.HasOne(d => d.LatestScope)
                        .WithMany(p => p.Organisations)
                        .HasForeignKey(d => d.LatestScopeId)
                        .HasConstraintName("FK_dbo.Organisations_dbo.OrganisationScopes_LatestScopeId");

                    entity.HasOne(d => d.LatestRegistration)
                        .WithMany(p => p.Organisations)
                        .HasForeignKey(d => new { d.LatestRegistrationUserId, d.LatestRegistrationOrganisationId })
                        .HasConstraintName(
                            "FK_dbo.Organisations_dbo.UserOrganisations_LatestRegistration_UserId_LatestRegistration_OrganisationId");

                    entity.HasOne(d => d.LatestPublicSectorType)
                        .WithMany(x => x.Organisations)
                        .HasForeignKey(d => d.LatestPublicSectorTypeId)
                        .HasConstraintName(
                            "FK_dbo.Organisations_dbo.OrganisationPublicSectorTypes_LatestPublicSectorTypeId");
                });

            #endregion

            #region OrganisationScope

            modelBuilder.Entity<OrganisationScope>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationScopeId).HasName("PK_dbo.OrganisationScopes");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.RegisterStatus)
                        .HasName("IX_RegisterStatusId");

                    entity.HasIndex(e => e.ScopeStatusDate)
                        .HasName("IX_ScopeStatusDate");

                    entity.HasIndex(e => e.ScopeStatus)
                        .HasName("IX_ScopeStatusId");

                    entity.HasIndex(e => e.SubmissionDeadline)
                        .HasName("IX_SubmissionDeadline");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.ScopeStatus).HasColumnName("ScopeStatusId");
                    entity.Property(e => e.RegisterStatus).HasColumnName("RegisterStatusId");
                    entity.Property(e => e.Status).HasColumnName("StatusId").HasDefaultValueSql("((0))");

                    entity.Property(e => e.CampaignId).HasMaxLength(50);

                    entity.Property(e => e.ContactEmailAddress).HasMaxLength(255);

                    entity.Property(e => e.ContactFirstname).HasMaxLength(50);

                    entity.Property(e => e.ContactLastname).HasMaxLength(50);

                    entity.Property(e => e.Reason).HasMaxLength(1000);

                    entity.Property(e => e.SubmissionDeadline)
                        .HasColumnType("date")
                        .HasDefaultValueSql("('1900-01-01T00:00:00.000')");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationScopes)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationScopes_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region OrganisationSicCode

            modelBuilder.Entity<OrganisationSicCode>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationSicCodeId).HasName("PK_dbo.OrganisationSicCodes");

                    entity.HasIndex(e => e.Created)
                        .HasName("IX_Created");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.Retired)
                        .HasName("IX_Retired");

                    entity.HasIndex(e => e.SicCodeId)
                        .HasName("IX_SicCodeId");

                    entity.Property(e => e.Source).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationSicCodes)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationSicCodes_dbo.Organisations_OrganisationId");

                    entity.HasOne(d => d.SicCode)
                        .WithMany(p => p.OrganisationSicCodes)
                        .HasForeignKey(d => d.SicCodeId)
                        .HasConstraintName("FK_dbo.OrganisationSicCodes_dbo.SicCodes_SicCodeId");
                });

            #endregion

            #region OrganisationStatus

            modelBuilder.Entity<OrganisationStatus>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationStatusId).HasName("PK_dbo.OrganisationStatus");
                    entity.HasIndex(e => e.ByUserId)
                        .HasName("IX_ByUserId");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.ByUser)
                        .WithMany(p => p.OrganisationStatus)
                        .HasForeignKey(d => d.ByUserId)
                        .HasConstraintName("FK_dbo.OrganisationStatus_dbo.Users_ByUserId");

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationStatuses)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationStatus_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region Return

            modelBuilder.Entity<Return>(
                entity =>
                {
                    entity.HasKey(e => e.ReturnId).HasName("PK_dbo.Returns");

                    entity.HasIndex(e => e.AccountingDate)
                        .HasName("IX_AccountingDate");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.CompanyLinkToGPGInfo)
                        .HasMaxLength(255);

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.EHRCResponse).HasColumnName("EHRCResponse").HasDefaultValueSql("((0))");
                    entity.Property(e => e.MinEmployees).HasDefaultValueSql("((0))");
                    entity.Property(e => e.MaxEmployees).HasDefaultValueSql("((0))");

                    entity.Property(e => e.FirstName).HasMaxLength(50);

                    entity.Property(e => e.JobTitle).HasMaxLength(100);

                    entity.Property(e => e.LastName).HasMaxLength(50);

                    entity.Property(e => e.LateReason).HasMaxLength(200);

                    entity.Property(e => e.Modifications).HasMaxLength(200);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.Returns)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.Returns_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region ReturnStatus

            modelBuilder.Entity<ReturnStatus>(
                entity =>
                {
                    entity.HasKey(e => e.ReturnStatusId).HasName("PK_dbo.ReturnStatus");

                    entity.HasIndex(e => e.ByUserId)
                        .HasName("IX_ByUserId");

                    entity.HasIndex(e => e.ReturnId)
                        .HasName("IX_ReturnId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.ByUser)
                        .WithMany(p => p.ReturnStatus)
                        .HasForeignKey(d => d.ByUserId)
                        .HasConstraintName("FK_dbo.ReturnStatus_dbo.Users_ByUserId");

                    entity.HasOne(d => d.Return)
                        .WithMany(p => p.ReturnStatuses)
                        .HasForeignKey(d => d.ReturnId)
                        .HasConstraintName("FK_dbo.ReturnStatus_dbo.Returns_ReturnId");
                });

            #endregion

            #region Statement

            modelBuilder.Entity<Statement>(
                entity =>
                {
                    entity.HasKey(e => e.StatementId)
                        .HasName("PK_dbo.Statements");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.StatementStartDate)
                        .HasName("IX_ReportingStartDate");

                    entity.HasIndex(e => e.StatementEndDate)
                        .HasName("IX_ReportingEndDate");

                    entity.Property(e => e.SubmissionDeadline)
                        .HasColumnType("Date");

                    entity.HasIndex(e => e.SubmissionDeadline)
                        .HasName("IX_SubmissionDeadline");

                    entity.HasIndex(e => e.IncludesGoals)
                        .HasName("IX_IncludesGoals");

                    entity.Property(e => e.GoalsDetails)
                        .HasMaxLength(250);

                    entity.HasIndex(e => e.IncludesStructure)
                        .HasName("IX_IncludesStructure");

                    entity.Property(e => e.StructureDetails)
                        .HasMaxLength(250);

                    entity.HasIndex(e => e.IncludesPolicies)
                        .HasName("IX_IncludesPolicies");

                    entity.Property(e => e.PolicyDetails)
                        .HasMaxLength(250);

                    entity.HasIndex(e => e.IncludesDueDiligence)
                        .HasName("IX_IncludesDueDiligence");

                    entity.Property(e => e.DueDiligenceDetails)
                        .HasMaxLength(250);

                    entity.HasIndex(e => e.IncludesRisks)
                        .HasName("IX_IncludesRisks");

                    entity.Property(e => e.RisksDetails)
                        .HasMaxLength(250);

                    entity.HasIndex(e => e.IncludesTraining)
                        .HasName("IX_IncludesTraining");

                    entity.Property(e => e.TrainingDetails)
                        .HasMaxLength(250);

                    entity.Property(e => e.Status)
                        .HasColumnName("StatusId");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.StatusDetails)
                        .HasMaxLength(255);

                    entity.Property(e => e.OtherRelavantRisks)
                        .HasMaxLength(250);

                    entity.Property(e => e.JobTitle)
                        .HasMaxLength(100);

                    entity.Property(e => e.FirstName)
                        .HasMaxLength(50);

                    entity.Property(e => e.LastName)
                        .HasMaxLength(50);

                    entity.HasIndex(e => e.MinTurnover)
                        .HasName("IX_MinTurnover");

                    entity.HasIndex(e => e.MaxTurnover)
                        .HasName("IX_MaxTurnover");

                    entity.Property(e => e.LateReason)
                        .HasMaxLength(200);

                    entity.Property(e => e.EHRCResponse)
                        .HasColumnName("EHRCResponse")
                        .HasDefaultValueSql("((0))");

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.Statements)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.Statements_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region StatementDivisionType

            modelBuilder.Entity<StatementTrainingType>(
                entity =>
                {
                    entity.HasKey(e => e.StatementTrainingTypeId)
                        .HasName("PK_dbo.StatementDivisionTypes");

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(250);
                });

            #endregion

            #region StatementTrainingDivision

            modelBuilder.Entity<StatementTraining>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementTrainingTypeId, e.StatementId })
                        .HasName("PK_dbo.StatementTrainingDivisions");

                    entity.HasOne(e => e.StatementTrainingType)
                        .WithMany()
                        .HasForeignKey(e => e.StatementTrainingTypeId)
                        .HasConstraintName("FK_dbo.StatementTrainingDivisions_dbo.StatementDivisionTypes_StatmentDivisionTypeId");

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.TrainingTypes)
                        .HasForeignKey(e => e.StatementId)
                        .HasConstraintName("FK_dbo.StatementTrainingDivisions_dbo.Statements_StatementId");
                });

            #endregion

            #region StatementDiligence

            modelBuilder.Entity<StatementDiligence>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementDiligenceTypeId, e.StatementId })
                        .HasName("PK_dbo.StatementDiligences");

                    entity.HasOne(e => e.StatementDiligenceType)
                        .WithMany()
                        .HasForeignKey(e => e.StatementDiligenceTypeId)
                        .HasConstraintName("FK_dbo.StatementDiligences_dbo.StatementDiligenceType_StatementDiligenceTypeId");

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.Diligences)
                        .HasForeignKey(e => e.StatementId)
                        .HasConstraintName("FK_dbo.StatementDiligences_dbo.Statements_StatementId");
                });

            #endregion

            #region StatementDiligenceType

            modelBuilder.Entity<StatementDiligenceType>(
                entity =>
                {
                    entity.HasKey(e => e.StatementDiligenceTypeId)
                        .HasName("PK_dbo.StatementDiligenceTypes");

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(250);
                });

            #endregion

            #region StatementPolicyType

            modelBuilder.Entity<StatementPolicyType>(
                entity =>
                {
                    entity.HasKey(e => e.StatementPolicyTypeId)
                        .HasName("PK_dbo.StatementPolicyTypes");

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(250);
                });

            #endregion

            #region StatementPolicy

            modelBuilder.Entity<StatementPolicy>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementPolicyTypeId, e.StatementId })
                        .HasName("PK_dbo.StatementPolicies");

                    entity.HasOne(e => e.StatementPolicyType)
                        .WithMany()
                        .HasForeignKey(e => e.StatementPolicyTypeId)
                        .HasConstraintName("FK_dbo.StatementPolicies_dbo.StatementPolicyType_StatementPolicyTypeId");

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.Policies)
                        .HasForeignKey(e => e.StatementId)
                        .HasConstraintName("FK_dbo.StatementPolicies_dbo.Statements_StatementId");
                });

            #endregion

            #region StatementRisk

            modelBuilder.Entity<StatementRisk>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementRiskTypeId, e.StatementId })
                        .HasName("PK_dbo.StatementRisks");

                    entity.HasOne(e => e.StatementRiskType)
                        .WithMany()
                        .HasForeignKey(e => e.StatementRiskTypeId)
                        .HasConstraintName("FK_dbo.StatementRisk_dbo.StatementRiskType_StatementRiskTypeId");

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.RelevantRisks)
                        .HasForeignKey(e => e.StatementId)
                        .HasConstraintName("FK_dbo.StatementRisks_dbo.Statements_StatementId");
                });

            #endregion

            #region StatementHighRisk

            modelBuilder.Entity<StatementHighRisk>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementRiskTypeId, e.StatementId })
                        .HasName("PK_dbo.StatementHighRisks");

                    entity.HasOne(e => e.StatementRiskType)
                        .WithMany()
                        .HasForeignKey(e => e.StatementRiskTypeId)
                        .HasConstraintName("FK_dbo.StatementHighRisks_dbo.StatementRiskType_StatementRiskTypeId");

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.HighRisks)
                        .HasForeignKey(e => e.StatementId)
                        .HasConstraintName("FK_dbo.StatementHighRisks_dbo.Statements_StatementId");
                });

            #endregion

            #region StatementRiskType

            modelBuilder.Entity<StatementRiskType>(
                entity =>
                {
                    entity.HasKey(e => e.StatementRiskTypeId)
                        .HasName("PK_dbo.StatementRiskTypes");

                    entity.HasOne(e => e.ParentRiskType)
                        .WithMany()
                        .HasForeignKey(e => e.ParentRiskTypeId)
                        .HasConstraintName("FK_dbo.StatementRisks_dbo.StatementRisks_StatementRiskId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .IsRequired();

                    entity.Property(e => e.Description)
                        .HasMaxLength(250);
                });

            #endregion

            #region StatementRiskType

            modelBuilder.Entity<StatementRiskCountry>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementRiskTypeId, e.StatementId })
                        .HasName("PK_dbo.StatementRiskTypes");
                });

            #endregion


            #region StatementOrganisation

            modelBuilder.Entity<StatementOrganisation>(
                entity =>
                {
                    entity.HasKey(e => e.StatementOrganisationId)
                        .HasName("PK_dbo.StatementOrganisations");

                    entity.HasOne(e => e.Statement)
                        .WithMany()
                        .HasForeignKey(e => e.StatementId)
                        .HasConstraintName("FK_dbo.StatementOrganisations_dbo.Statements_StatementId");

                    entity.HasOne(e => e.Organisation)
                        .WithMany()
                        .HasForeignKey(e => e.OrganisationId)
                        .HasConstraintName("FK_dbo.StatementOrganisations_dbo.Organisations_OrganisationId")
                        .IsRequired(false);

                    entity.Property(e => e.OrganisationName)
                        .HasMaxLength(100)
                        .IsRequired();
                });

            #endregion

            #region StatementSectorType

            modelBuilder.Entity<StatementSectorType>(
                entity =>
                {
                    entity.HasKey(e => e.StatementSectorTypeId)
                        .HasName("PK_dbo.StatementSectorTypes");

                    entity.Property(e => e.Description)
                        .HasMaxLength(250)
                        .IsRequired();
                });

            #endregion

            #region StatementSector

            modelBuilder.Entity<StatementSector>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementSectorTypeId, e.StatementId })
                        .HasName("PK_dbo.StatementSectors");

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.Sectors)
                        .HasForeignKey(e => e.StatementId)
                        .HasConstraintName("FK_dbo.StatementSectors_dbo.Statements_StatementId");
                });

            #endregion

            #region StatementStatus

            modelBuilder.Entity<StatementStatus>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementStatusId, e.StatementId })
                        .HasName("PK_dbo.StatementStatuses");

                    entity.Property(e => e.StatusDetails)
                        .HasMaxLength(255);

                    entity.HasOne(e => e.ByUser)
                        .WithMany()
                        .HasForeignKey(e => e.ByUserId)
                        .HasConstraintName("FK_dbo.StatementStatuses_dbo.Users_UserId");
                });

            #endregion

            #region SicCode

            modelBuilder.Entity<SicCode>(
                entity =>
                {
                    entity.HasKey(e => e.SicCodeId).HasName("PK_dbo.SicCodes");

                    entity.HasIndex(e => e.SicSectionId)
                        .HasName("IX_SicSectionId");

                    entity.Property(e => e.SicCodeId).ValueGeneratedNever();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(250);

                    entity.Property(e => e.SicSectionId)
                        .IsRequired()
                        .HasMaxLength(1);

                    entity.HasOne(d => d.SicSection)
                        .WithMany(p => p.SicCodes)
                        .HasForeignKey(d => d.SicSectionId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_dbo.SicCodes_dbo.SicSections_SicSectionId");
                });

            #endregion

            #region SicSection

            modelBuilder.Entity<SicSection>(
                entity =>
                {
                    entity.HasKey(e => e.SicSectionId).HasName("PK_dbo.SicSections");

                    entity.Property(e => e.SicSectionId)
                        .HasMaxLength(1)
                        .ValueGeneratedNever();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(250);
                });

            #endregion

            #region UserOrganisation

            modelBuilder.Entity<UserOrganisation>(
                entity =>
                {
                    entity.HasKey(e => new { e.UserId, e.OrganisationId }).HasName("PK_dbo.UserOrganisations");

                    entity.HasIndex(e => e.AddressId)
                        .HasName("IX_AddressId");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.UserId)
                        .HasName("IX_UserId");

                    entity.Property(e => e.PINHash)
                        .HasMaxLength(250);

                    entity.Property(e => e.Method).HasColumnName("MethodId").HasDefaultValueSql("((0))");

                    entity.HasOne(d => d.Address)
                        .WithMany(p => p.UserOrganisations)
                        .HasForeignKey(d => d.AddressId)
                        .HasConstraintName("FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId");

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.UserOrganisations)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.UserOrganisations_dbo.Organisations_OrganisationId");

                    entity.HasOne(d => d.User)
                        .WithMany(p => p.UserOrganisations)
                        .HasForeignKey(d => d.UserId)
                        .HasConstraintName("FK_dbo.UserOrganisations_dbo.Users_UserId");
                });

            #endregion

            #region User

            modelBuilder.Entity<User>(
                entity =>
                {
                    entity.HasKey(e => e.UserId).HasName("PK_dbo.Users");

                    entity.HasIndex(e => e.ContactEmailAddress)
                        .HasName("IX_ContactEmailAddress");

                    entity.HasIndex(e => e.ContactPhoneNumber)
                        .HasName("IX_ContactPhoneNumber");

                    entity.HasIndex(e => e.EmailAddress)
                        .HasName("IX_EmailAddress");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.ContactEmailAddress).HasMaxLength(255);

                    entity.Property(e => e.ContactFirstName).HasMaxLength(50);

                    entity.Property(e => e.ContactJobTitle).HasMaxLength(50);

                    entity.Property(e => e.ContactLastName).HasMaxLength(50);

                    entity.Property(e => e.ContactOrganisation).HasMaxLength(100);

                    entity.Property(e => e.ContactPhoneNumber).HasMaxLength(20);

                    entity.Property(e => e.EmailAddress)
                        .IsRequired()
                        .HasMaxLength(255);

                    entity.Property(e => e.EmailVerifyHash).HasMaxLength(250);

                    entity.Property(e => e.Firstname)
                        .IsRequired()
                        .HasMaxLength(50);

                    entity.Property(e => e.JobTitle)
                        .IsRequired()
                        .HasMaxLength(50);

                    entity.Property(e => e.Lastname)
                        .IsRequired()
                        .HasMaxLength(50);

                    entity.Property(e => e.PasswordHash)
                        .IsRequired()
                        .HasMaxLength(250);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);
                });

            #endregion

            #region UserSetting

            modelBuilder.Entity<UserSetting>(
                entity =>
                {
                    entity.HasKey(e => new { e.UserId, e.Key }).HasName("PK_dbo.UserSettings");

                    entity.HasIndex(e => e.UserId)
                        .HasName("IX_UserId");

                    entity.Property(e => e.Value).HasMaxLength(50);

                    entity.HasOne(d => d.User)
                        .WithMany(p => p.UserSettings)
                        .HasForeignKey(d => d.UserId)
                        .HasConstraintName("FK_dbo.UserSettings_dbo.Users_UserId");
                });

            #endregion

            #region UserStatus

            modelBuilder.Entity<UserStatus>(
                entity =>
                {
                    entity.HasKey(e => e.UserStatusId)
                        .HasName("PK_dbo.UserStatus");

                    entity.HasIndex(e => e.ByUserId)
                        .HasName("IX_ByUserId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.HasIndex(e => e.UserId)
                        .HasName("IX_UserId");

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.ByUser)
                        .WithMany(p => p.UserStatusesByUser)
                        .HasForeignKey(d => d.ByUserId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_dbo.UserStatus_dbo.Users_ByUserId");

                    entity.HasOne(d => d.User)
                        .WithMany(p => p.UserStatuses)
                        .HasForeignKey(d => d.UserId)
                        .HasConstraintName("FK_dbo.UserStatus_dbo.Users_UserId");
                });

            #endregion

            #region PublicSectorType

            modelBuilder.Entity<PublicSectorType>(
                entity =>
                {
                    entity.HasKey(e => e.PublicSectorTypeId).HasName("PK_dbo.PublicSectorTypes");

                    entity.Property(e => e.Description)
                        .HasMaxLength(250)
                        .IsRequired();

                    entity.Property(e => e.Created).IsRequired();
                });

            #endregion

            #region OrganisationPublicSectorType

            modelBuilder.Entity<OrganisationPublicSectorType>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationPublicSectorTypeId)
                        .HasName("PK_dbo.OrganisationPublicSectorTypes");

                    entity.HasIndex(e => e.Created).HasName("IX_Created");

                    entity.HasIndex(e => e.Retired).HasName("IX_Retired");

                    entity.HasIndex(e => e.PublicSectorTypeId).HasName("IX_PublicSectorTypeId");

                    entity.HasIndex(e => e.OrganisationId).HasName("IX_OrganisationId");

                    entity.Property(e => e.Source).HasMaxLength(255);
                });

            #endregion

            #region Feedback

            modelBuilder.Entity<Feedback>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("getdate()");

            #endregion

            #region AuditLog

            modelBuilder.Entity<AuditLog>()
                .HasOne(e => e.OriginalUser);
            modelBuilder.Entity<AuditLog>()
                .HasOne(e => e.ImpersonatedUser);
            modelBuilder.Entity<AuditLog>()
                .HasOne(e => e.Organisation);
            modelBuilder.Entity<AuditLog>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("getdate()");

            #endregion

            #region Views

            modelBuilder.Entity<OrganisationAddressInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("OrganisationAddressInfoView");

                entity.Property(e => e.AddressSource).HasMaxLength(255);

                entity.Property(e => e.AddressStatus)
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.Property(e => e.AddressStatusDetails).HasMaxLength(255);

                entity.Property(e => e.FullAddress).HasMaxLength(4000);
            });

            modelBuilder.Entity<OrganisationInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("OrganisationInfoView");

                entity.Property(e => e.CompanyNumber).HasMaxLength(10);

                entity.Property(e => e.Dunsnumber)
                    .HasColumnName("DUNSNumber")
                    .HasMaxLength(10);

                entity.Property(e => e.EmployerReference).HasMaxLength(10);

                entity.Property(e => e.OrganisationId).ValueGeneratedOnAdd();

                entity.Property(e => e.OrganisationName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.OrganisationStatus)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SectorType)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OrganisationRegistrationInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("OrganisationRegistrationInfoView");

                entity.Property(e => e.ContactInfo).HasMaxLength(154);

                entity.Property(e => e.PinconfirmedDate).HasColumnName("PINConfirmedDate");

                entity.Property(e => e.PinsentDate).HasColumnName("PINSentDate");

                entity.Property(e => e.RegistrationMethod)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.UserInfo)
                    .IsRequired()
                    .HasMaxLength(154);
            });

            modelBuilder.Entity<OrganisationScopeAndReturnInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("OrganisationScopeAndReturnInfoView");

                entity.Property(e => e.CompanyNumber).HasMaxLength(10);

                entity.Property(e => e.EmployerReference).HasMaxLength(10);

                entity.Property(e => e.OrganisationName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.OrganisationSize)
                    .HasMaxLength(79)
                    .IsUnicode(false);

                entity.Property(e => e.OrganisationStatus)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.PublicSectorDescription).HasMaxLength(250);

                entity.Property(e => e.ScopeStatus)
                    .HasMaxLength(27)
                    .IsUnicode(false);

                entity.Property(e => e.SectorType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SicCodeSectionDescription).HasMaxLength(250);
            });

            modelBuilder.Entity<OrganisationScopeInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("OrganisationScopeInfoView");

                entity.Property(e => e.RegisterStatus)
                    .HasMaxLength(22)
                    .IsUnicode(false);

                entity.Property(e => e.ScopeStatus)
                    .HasMaxLength(26)
                    .IsUnicode(false);

                entity.Property(e => e.SnapshotYear).HasColumnName("snapshotYear");
            });

            modelBuilder.Entity<OrganisationSearchInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("OrganisationSearchInfoView");

                entity.Property(e => e.CompanyNumber).HasMaxLength(10);

                entity.Property(e => e.Dunsnumber)
                    .HasColumnName("DUNSNumber")
                    .HasMaxLength(10);

                entity.Property(e => e.EmployerReference).HasMaxLength(10);

                entity.Property(e => e.OrganisationId).ValueGeneratedOnAdd();

                entity.Property(e => e.OrganisationName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.OrganisationStatus)
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.Property(e => e.SectorType)
                    .HasMaxLength(12)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OrganisationSicCodeInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("OrganisationSicCodeInfoView");

                entity.Property(e => e.CodeDescription).HasMaxLength(250);

                entity.Property(e => e.SectionDescription).HasMaxLength(250);

                entity.Property(e => e.SicSectionId).HasMaxLength(1);

                entity.Property(e => e.Source).HasMaxLength(255);
            });

            modelBuilder.Entity<OrganisationSubmissionInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("OrganisationSubmissionInfoView");

                entity.Property(e => e.CompanyLinkToGpginfo)
                    .HasColumnName("CompanyLinkToGPGInfo")
                    .HasMaxLength(255);

                entity.Property(e => e.DateFirstReportedInYear).HasColumnName("dateFirstReportedInYear");

                entity.Property(e => e.DiffMeanBonusPercent).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DiffMeanHourlyPayPercent).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DiffMedianBonusPercent).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DiffMedianHourlyPercent).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Ehrcresponse)
                    .IsRequired()
                    .HasColumnName("EHRCResponse")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.FemaleLowerPayBand).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.FemaleMedianBonusPayPercent).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.FemaleMiddlePayBand).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.FemaleUpperPayBand).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.FemaleUpperQuartilePayBand).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.LatestReturnAccountingDate).HasColumnType("date");

                entity.Property(e => e.LatestReturnLateReason).HasMaxLength(200);

                entity.Property(e => e.LatestReturnStatus)
                    .HasColumnName("latestReturnStatus")
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.Property(e => e.LatestReturnStatusDate).HasColumnName("latestReturnStatusDate");

                entity.Property(e => e.LatestReturnStatusDetails).HasMaxLength(255);

                entity.Property(e => e.MaleLowerPayBand).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MaleMedianBonusPayPercent).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MaleMiddlePayBand).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MaleUpperPayBand).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MaleUpperQuartilePayBand).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.OrganisationSize)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.ReportedLate)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.ReturnModifiedFields).HasMaxLength(200);

                entity.Property(e => e.StatusDetails).HasMaxLength(255);

                entity.Property(e => e.StatusId)
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.Property(e => e.SubmittedBy).HasMaxLength(204);
            });

            modelBuilder.Entity<UserLinkedOrganisationsView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("UserLinkedOrganisationsView");

                entity.Property(e => e.CompanyNumber).HasMaxLength(10);

                entity.Property(e => e.Dunsnumber)
                    .HasColumnName("DUNSNumber")
                    .HasMaxLength(10);

                entity.Property(e => e.EmployerReference).HasMaxLength(10);

                entity.Property(e => e.OrganisationName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.SectorTypeId)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.StatusId)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("UserInfoView");

                entity.Property(e => e.ContactFirstName).HasMaxLength(50);

                entity.Property(e => e.ContactJobTitle).HasMaxLength(50);

                entity.Property(e => e.ContactLastName).HasMaxLength(50);

                entity.Property(e => e.ContactOrganisation).HasMaxLength(100);

                entity.Property(e => e.ContactPhoneNumber).HasMaxLength(20);

                entity.Property(e => e.Firstname)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.JobTitle)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Lastname)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.StatusDetails).HasMaxLength(255);

                entity.Property(e => e.StatusId)
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<UserStatusInfoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("UserStatusInfoView");

                entity.Property(e => e.StatusChangedBy)
                    .IsRequired()
                    .HasMaxLength(154);

                entity.Property(e => e.StatusDetails).HasMaxLength(255);

                entity.Property(e => e.StatusId)
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(154);
            });

            #endregion
        }
    }
}