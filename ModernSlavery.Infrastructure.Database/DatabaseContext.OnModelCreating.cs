using Microsoft.EntityFrameworkCore;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Infrastructure.Database
{
    public partial class DatabaseContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Map the correct entity to table names
            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
            modelBuilder.Entity<AddressStatus>().ToTable("AddressStatuses");
            modelBuilder.Entity<Organisation>().ToTable("Organisations");
            modelBuilder.Entity<OrganisationAddress>().ToTable("OrganisationAddresses");
            modelBuilder.Entity<OrganisationName>().ToTable("OrganisationNames");
            modelBuilder.Entity<OrganisationPublicSectorType>().ToTable("OrganisationPublicSectorTypes");
            modelBuilder.Entity<OrganisationReference>().ToTable("OrganisationReferences");
            modelBuilder.Entity<OrganisationScope>().ToTable("OrganisationScopes");
            modelBuilder.Entity<OrganisationSicCode>().ToTable("OrganisationSicCodes");
            modelBuilder.Entity<OrganisationStatus>().ToTable("OrganisationStatuses");
            modelBuilder.Entity<PublicSectorType>().ToTable("PublicSectorTypes");
            modelBuilder.Entity<ReminderEmail>().ToTable("ReminderEmails");
            modelBuilder.Entity<SicCode>().ToTable("SicCodes");
            modelBuilder.Entity<SicSection>().ToTable("SicSections");
            modelBuilder.Entity<Statement>().ToTable("Statements");
            modelBuilder.Entity<StatementDiligence>().ToTable("StatementDiligences");
            modelBuilder.Entity<StatementDiligenceType>().ToTable("StatementDiligenceTypes");
            modelBuilder.Entity<StatementPolicy>().ToTable("StatementPolicies");
            modelBuilder.Entity<StatementPolicyType>().ToTable("StatementPolicyTypes");
            modelBuilder.Entity<StatementRelevantRisk>().ToTable("StatementRelevantRisks");
            modelBuilder.Entity<StatementRiskType>().ToTable("StatementRiskTypes");
            modelBuilder.Entity<StatementHighRisk>().ToTable("StatementHighRisks");
            modelBuilder.Entity<StatementLocationRisk>().ToTable("StatementLocationRisks");
            modelBuilder.Entity<StatementOrganisation>().ToTable("StatementOrganisations");
            modelBuilder.Entity<StatementSector>().ToTable("StatementSectors");
            modelBuilder.Entity<StatementSectorType>().ToTable("StatementSectorTypes");
            modelBuilder.Entity<StatementStatus>().ToTable("StatementStatuses");
            modelBuilder.Entity<StatementTrainingType>().ToTable("StatementTrainingTypes");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserOrganisation>().ToTable("UserOrganisations");
            modelBuilder.Entity<UserStatus>().ToTable("UserStatuses");
            modelBuilder.Entity<UserSetting>().ToTable("UserSettings");

            #region AuditLog

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditLogId);

                entity.HasIndex(e => e.ImpersonatedUserId);

                entity.HasIndex(e => e.OrganisationId);

                entity.HasIndex(e => e.OriginalUserId);

                entity.Property(e => e.Action).HasColumnName("ActionId");

                entity.HasOne(e => e.OriginalUser).WithMany().HasForeignKey(e=>e.OriginalUserId);
                entity.HasOne(e => e.ImpersonatedUser).WithMany().HasForeignKey(e => e.ImpersonatedUserId);
                entity.HasOne(e => e.Organisation).WithMany().HasForeignKey(e => e.OrganisationId);
            });

            #endregion

            #region AddressStatus

            modelBuilder.Entity<AddressStatus>(
                entity =>
                {
                    entity.HasKey(e => e.AddressStatusId);

                    entity.HasIndex(e => e.AddressId);

                    entity.HasIndex(e => e.ByUserId);

                    entity.HasIndex(e => e.StatusDate);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);
                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.HasOne(d => d.Address)
                        .WithMany(p => p.AddressStatuses)
                        .HasForeignKey(d => d.AddressId);

                    entity.HasOne(d => d.ByUser)
                        .WithMany(p => p.AddressStatus)
                        .HasForeignKey(d => d.ByUserId);
                });

            #endregion

            #region Feedback

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.Property(e => e.Difficulty).HasColumnName("DifficultyId");

                entity.Property(e => e.EmailAddress).HasMaxLength(255);

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            });
            #endregion

            #region Organisation

            modelBuilder.Entity<Organisation>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationId);

                    entity.HasIndex(e => e.CompanyNumber)
                        .IsUnique()
                        .HasFilter("([CompanyNumber] IS NOT NULL)");

                    entity.HasIndex(e => e.DUNSNumber)
                        .IsUnique()
                        .HasFilter("([DUNSNumber] IS NOT NULL)");

                    entity.HasIndex(e => e.EmployerReference)
                        .IsUnique()
                        .HasFilter("([EmployerReference] IS NOT NULL)");

                    entity.HasIndex(e => e.LatestAddressId);

                    entity.HasIndex(e => e.LatestPublicSectorTypeId);

                    entity.HasIndex(e => e.LatestScopeId);

                    entity.HasIndex(e => e.LatestStatementId);

                    entity.HasIndex(e => e.OrganisationName);

                    entity.HasIndex(e => e.SectorType);

                    entity.HasIndex(e => e.Status);

                    entity.HasIndex(e => new { e.LatestRegistrationUserId, e.LatestRegistrationOrganisationId });

                    entity.Property(e => e.Status).HasColumnName("StatusId");
                    entity.Property(e => e.SectorType).HasColumnName("SectorTypeId");

                    entity.Property(e => e.CompanyNumber).HasMaxLength(10);

                    entity.Property(e => e.DUNSNumber)
                        .HasMaxLength(10);

                    entity.Property(e => e.EmployerReference).HasMaxLength(10);

                    entity.Property(e => e.LatestRegistrationOrganisationId)
                        .HasColumnName("LatestRegistration_OrganisationId");

                    entity.Property(e => e.LatestRegistrationUserId).HasColumnName("LatestRegistration_UserId");

                    entity.Property(e => e.OptedOutFromCompaniesHouseUpdate)
                      .HasDefaultValue(0);

                    entity.Property(e => e.OrganisationName)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.LatestAddress).WithMany().HasForeignKey(e => e.LatestAddressId);

                    entity.HasOne(d => d.LatestStatement).WithMany().HasForeignKey(e => e.LatestStatementId);

                    entity.HasOne(d => d.LatestScope).WithMany().HasForeignKey(e => e.LatestScopeId);

                    entity.HasOne(d => d.LatestRegistration).WithMany().HasForeignKey(p=> new { p.LatestRegistrationOrganisationId, p.LatestRegistrationUserId });

                    entity.HasOne(d => d.LatestPublicSectorType).WithMany().HasForeignKey(e => e.LatestPublicSectorTypeId);
                });

            #endregion

            #region OrganisationAddress

            modelBuilder.Entity<OrganisationAddress>(
                entity =>
                {
                    entity.HasKey(e => e.AddressId);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.HasIndex(e => e.StatusDate);

                    entity.HasIndex(e => e.Status);

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
                        .HasForeignKey(d => d.OrganisationId);
                });

            #endregion

            #region OrganisationName

            modelBuilder.Entity<OrganisationName>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationNameId);

                    entity.HasIndex(e => e.Created);

                    entity.HasIndex(e => e.Name);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.Property(e => e.Name)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.Source).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationNames)
                        .HasForeignKey(d => d.OrganisationId);
                });

            #endregion

            #region OrganisationPublicSectorType

            modelBuilder.Entity<OrganisationPublicSectorType>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationPublicSectorTypeId);

                    entity.HasIndex(e => e.Created);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.HasIndex(e => e.PublicSectorTypeId);

                    entity.HasIndex(e => e.Retired);

                    entity.Property(e => e.Source).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation).WithMany().HasForeignKey(e => e.OrganisationId);

                    entity.HasOne(d => d.PublicSectorType).WithMany().HasForeignKey(e => e.PublicSectorTypeId);

                });

            #endregion

            #region OrganisationReference

            modelBuilder.Entity<OrganisationReference>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationReferenceId);

                    entity.HasIndex(e => e.Created);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.HasIndex(e => e.ReferenceName);

                    entity.HasIndex(e => e.ReferenceValue);

                    entity.Property(e => e.ReferenceName)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.ReferenceValue)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationReferences)
                        .HasForeignKey(d => d.OrganisationId);
                });

            #endregion

            #region OrganisationScope

            modelBuilder.Entity<OrganisationScope>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationScopeId);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.HasIndex(e => e.RegisterStatus);

                    entity.HasIndex(e => e.ScopeStatusDate);

                    entity.HasIndex(e => e.ScopeStatus);

                    entity.HasIndex(e => e.SubmissionDeadline);

                    entity.HasIndex(e => e.Status);

                    entity.Property(e => e.ScopeStatus).HasColumnName("ScopeStatusId");
                    entity.Property(e => e.RegisterStatus).HasColumnName("RegisterStatusId");
                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.CampaignId).HasMaxLength(50);

                    entity.Property(e => e.ContactEmailAddress).HasMaxLength(255);

                    entity.Property(e => e.ContactFirstname).HasMaxLength(50);

                    entity.Property(e => e.ContactLastname).HasMaxLength(50);

                    entity.Property(e => e.Reason).HasMaxLength(1000);

                    entity.Property(e => e.SubmissionDeadline)
                        .HasColumnType("date");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.Property(e => e.SubmissionDeadline).HasColumnType("date");

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationScopes)
                        .HasForeignKey(d => d.OrganisationId);
                });

            #endregion

            #region OrganisationSicCode

            modelBuilder.Entity<OrganisationSicCode>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationSicCodeId);

                    entity.HasIndex(e => e.Created);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.HasIndex(e => e.Retired);

                    entity.HasIndex(e => e.SicCodeId);

                    entity.Property(e => e.Source).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationSicCodes)
                        .HasForeignKey(d => d.OrganisationId);

                    entity.HasOne(d => d.SicCode)
                        .WithMany(p => p.OrganisationSicCodes)
                        .HasForeignKey(d => d.SicCodeId);
                });

            #endregion

            #region OrganisationStatus

            modelBuilder.Entity<OrganisationStatus>(
                entity =>
                {
                    entity.HasKey(e => e.OrganisationStatusId);
                    entity.HasIndex(e => e.ByUserId);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.HasIndex(e => e.StatusDate);

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.ByUser)
                        .WithMany(p => p.OrganisationStatus)
                        .HasForeignKey(d => d.ByUserId);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationStatuses)
                        .HasForeignKey(d => d.OrganisationId);
                });

            #endregion

            #region PublicSectorType

            modelBuilder.Entity<PublicSectorType>(
                entity =>
                {
                    entity.HasKey(e => e.PublicSectorTypeId);

                    entity.Property(e => e.Description)
                        .HasMaxLength(255)
                        .IsRequired();

                    entity.Property(e => e.Created).IsRequired();
                });

            #endregion

            #region ReminderEmails
            modelBuilder.Entity<ReminderEmail>(entity =>
            {
                entity.HasKey(e => e.ReminderEmailId);
                entity.Property(e => e.SectorType).HasColumnName("SectorTypeId");
            });
            #endregion

            #region SicCode

            modelBuilder.Entity<SicCode>(
                entity =>
                {
                    entity.HasKey(e => e.SicCodeId);

                    entity.HasIndex(e => e.SicSectionId);

                    entity.Property(e => e.SicCodeId).ValueGeneratedNever();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(255);

                    entity.Property(e => e.SicSectionId)
                        .IsRequired()
                        .HasMaxLength(1);

                    entity.HasOne(d => d.SicSection)
                        .WithMany(p => p.SicCodes)
                        .HasForeignKey(d => d.SicSectionId)
                        .OnDelete(DeleteBehavior.ClientSetNull);
                });

            #endregion

            #region SicSection

            modelBuilder.Entity<SicSection>(
                entity =>
                {
                    entity.HasKey(e => e.SicSectionId);

                    entity.Property(e => e.SicSectionId)
                        .HasMaxLength(1)
                        .ValueGeneratedNever();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(255);
                });

            #endregion

            #region Statement

            modelBuilder.Entity<Statement>(
                entity =>
                {
                    entity.HasKey(e => e.StatementId);

                    entity.HasIndex(e => e.IncludesDueDiligence);

                    entity.HasIndex(e => e.IncludesGoals);

                    entity.HasIndex(e => e.IncludesPolicies);

                    entity.HasIndex(e => e.IncludesRisks);

                    entity.HasIndex(e => e.IncludesStructure);

                    entity.HasIndex(e => e.IncludesTraining);

                    entity.HasIndex(e => e.MaxTurnover);

                    entity.HasIndex(e => e.MinTurnover);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.HasIndex(e => e.StatementEndDate);

                    entity.HasIndex(e => e.StatementStartDate);

                    entity.HasIndex(e => e.Status);

                    entity.HasIndex(e => e.SubmissionDeadline);

                    entity.Property(e => e.ApprovedDate).HasColumnType("Date");

                    entity.Property(e => e.ApproverFirstName)
                        .HasMaxLength(50);

                    entity.Property(e => e.ApproverJobTitle)
                        .HasMaxLength(100);

                    entity.Property(e => e.ApproverLastName)
                        .HasMaxLength(50);

                    entity.Property(e => e.LateReason)
                        .HasMaxLength(255);

                    entity.Property(e => e.StatementEndDate)
                        .HasColumnType("Date");

                    entity.Property(e => e.StatementStartDate)
                        .HasColumnType("Date");

                    entity.Property(e => e.StatementUrl)
                        .HasMaxLength(255);

                    entity.Property(e => e.StatusDetails)
                        .HasMaxLength(255);

                    entity.Property(e => e.SubmissionDeadline)
                        .HasColumnType("Date");

                    entity.Property(e => e.Status)
                        .HasColumnName("StatusId");

                    entity.Property(e => e.EHRCResponse)
                        .HasDefaultValue(0);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.Statements)
                        .HasForeignKey(d => d.OrganisationId);

                    entity.Property(e => e.IncludedOrganisationCount)
                        .HasDefaultValue(0);

                    entity.Property(e => e.ExcludedOrganisationCount)
                        .HasDefaultValue(0);
                });

            #endregion

            #region StatementTrainingType

            modelBuilder.Entity<StatementTrainingType>(
                entity =>
                {
                    entity.HasKey(e => e.StatementTrainingTypeId);
                    entity.Property(e => e.StatementTrainingTypeId).ValueGeneratedNever();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(255);
                });

            #endregion

            #region StatementTraining

            modelBuilder.Entity<StatementTraining>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementTrainingTypeId, e.StatementId });

                    entity.HasIndex(e => e.StatementId);

                    entity.HasOne(e => e.StatementTrainingType)
                        .WithMany(e=>e.StatementTraining)
                        .HasForeignKey(e => e.StatementTrainingTypeId);

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.Training)
                        .HasForeignKey(e => e.StatementId);
                });

            #endregion

            #region StatementDiligence

            modelBuilder.Entity<StatementDiligence>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementDiligenceTypeId, e.StatementId });

                    entity.HasIndex(e => e.StatementId);

                    entity.HasOne(e => e.StatementDiligenceType)
                        .WithMany()
                        .HasForeignKey(e => e.StatementDiligenceTypeId);

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.Diligences)
                        .HasForeignKey(e => e.StatementId);

                    entity.HasOne(e => e.StatementDiligenceType)
                        .WithMany(e => e.StatementDiligences)
                        .HasForeignKey(e => e.StatementDiligenceTypeId);
                });

            #endregion

            #region StatementDiligenceType

            modelBuilder.Entity<StatementDiligenceType>(
                entity =>
                {
                    entity.HasKey(e => e.StatementDiligenceTypeId);
                    entity.Property(e => e.StatementDiligenceTypeId).ValueGeneratedNever();
                    
                    entity.HasIndex(e => e.ParentDiligenceTypeId);

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(255);

                    entity.HasOne(d => d.ParentDiligenceType)
                    .WithMany(p => p.ChildDiligenceTypes)
                    .HasForeignKey(d => d.ParentDiligenceTypeId);
                });

            #endregion

            #region StatementPolicyType

            modelBuilder.Entity<StatementPolicyType>(
                entity =>
                {
                    entity.HasKey(e => e.StatementPolicyTypeId);
                    entity.Property(e => e.StatementPolicyTypeId).ValueGeneratedNever();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(255);
                });

            #endregion

            #region StatementPolicy

            modelBuilder.Entity<StatementPolicy>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementPolicyTypeId, e.StatementId });

                    entity.HasIndex(e => e.StatementId);

                    entity.HasOne(e => e.StatementPolicyType)
                        .WithMany(e=>e.StatementPolicies)
                        .HasForeignKey(e => e.StatementPolicyTypeId);

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.Policies)
                        .HasForeignKey(e => e.StatementId);
                });

            #endregion

            #region StatementRelevantRisk

            modelBuilder.Entity<StatementRelevantRisk>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementRiskTypeId, e.StatementId });

                    entity.HasIndex(e => e.StatementId);

                    entity.HasOne(e => e.StatementRiskType)
                        .WithMany(e=> e.StatementRelevantRisks)
                        .HasForeignKey(e => e.StatementRiskTypeId);

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.RelevantRisks)
                        .HasForeignKey(e => e.StatementId);
                });

            #endregion

            #region StatementHighRisk

            modelBuilder.Entity<StatementHighRisk>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementRiskTypeId, e.StatementId });

                    entity.HasIndex(e => e.StatementId);

                    entity.HasOne(e => e.StatementRiskType)
                        .WithMany(e=> e.StatementHighRisks)
                        .HasForeignKey(e => e.StatementRiskTypeId);

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.HighRisks)
                        .HasForeignKey(e => e.StatementId);
                });

            #endregion

            #region StatementRiskType

            modelBuilder.Entity<StatementRiskType>(
                entity =>
                {

                    entity.Property(e => e.Category).HasColumnName("RiskCategoryId");

                    entity.HasKey(e => e.StatementRiskTypeId);
                    entity.Property(e => e.StatementRiskTypeId).ValueGeneratedNever();

                    entity.HasIndex(e => e.ParentRiskTypeId);

                    entity.Property(e => e.Description).HasMaxLength(255);

                    entity.HasOne(d => d.ParentRiskType)
                   .WithMany(p => p.ChildRiskType)
                   .HasForeignKey(d => d.ParentRiskTypeId);
                });

            #endregion

            #region StatementLocationRisk

            modelBuilder.Entity<StatementLocationRisk>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementRiskTypeId, e.StatementId });

                    entity.HasIndex(e => e.StatementId);

                    entity.HasOne(e => e.StatementRiskType)
                        .WithMany(e=>e.StatementLocationRisks)
                        .HasForeignKey(e => e.StatementRiskTypeId);

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.LocationRisks)
                        .HasForeignKey(e => e.StatementId);
                });

            #endregion

            #region StatementOrganisation

            modelBuilder.Entity<StatementOrganisation>(
                entity =>
                {
                    entity.HasKey(e => e.StatementOrganisationId);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.HasIndex(e => e.StatementId);

                    entity.HasOne(e => e.Statement)
                        .WithMany()
                        .HasForeignKey(e => e.StatementId);

                    entity.HasOne(e => e.Organisation)
                        .WithMany()
                        .HasForeignKey(e => e.OrganisationId)
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
                    entity.HasKey(e => e.StatementSectorTypeId);
                    entity.Property(e => e.StatementSectorTypeId).ValueGeneratedNever();

                    entity.Property(e => e.Description)
                        .HasMaxLength(255)
                        .IsRequired();
                });

            #endregion

            #region StatementSector

            modelBuilder.Entity<StatementSector>(
                entity =>
                {
                    entity.HasKey(e => new { e.StatementSectorTypeId, e.StatementId });

                    entity.HasIndex(e => e.StatementId);

                    entity.HasOne(d => d.Statement)
                        .WithMany(p => p.Sectors)
                        .HasForeignKey(d => d.StatementId);

                    entity.HasOne(d => d.StatementSectorType)
                        .WithMany(d=>d.StatementSectors)
                        .HasForeignKey(d => d.StatementSectorTypeId);
                });

            #endregion

            #region StatementStatus

            modelBuilder.Entity<StatementStatus>(
                entity =>
                {
                    entity.HasKey(e => e.StatementStatusId);

                    entity.HasIndex(e => e.ByUserId);

                    entity.HasIndex(e => e.StatementId);

                    entity.Property(e => e.StatusDetails)
                        .HasMaxLength(255);

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.HasOne(e => e.ByUser)
                        .WithMany()
                        .HasForeignKey(e => e.ByUserId);

                    entity.HasOne(e => e.Statement)
                        .WithMany(e => e.Statuses)
                        .HasForeignKey(e => e.StatementId);

                });

            #endregion

            #region UserOrganisation

            modelBuilder.Entity<UserOrganisation>(
                entity =>
                {
                    entity.HasKey(e => new { e.UserId, e.OrganisationId });

                    entity.HasIndex(e => e.AddressId);

                    entity.HasIndex(e => e.OrganisationId);

                    entity.HasIndex(e => e.UserId);

                    entity.Property(e => e.PITPNotifyLetterId)
                        .HasMaxLength(255);

                    entity.Property(e => e.PIN)
                        .HasMaxLength(255);

                    entity.Property(e => e.PINHash)
                        .HasMaxLength(255);

                    entity.Property(e => e.Method).HasColumnName("MethodId");

                    entity.HasOne(d => d.Address).WithMany(e=>e.UserOrganisations).HasForeignKey(e => e.AddressId);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.UserOrganisations)
                        .HasForeignKey(d => d.OrganisationId);

                    entity.HasOne(d => d.User)
                        .WithMany(p => p.UserOrganisations)
                        .HasForeignKey(d => d.UserId);
                });

            #endregion

            #region User

            modelBuilder.Entity<User>(
                entity =>
                {
                    entity.HasKey(e => e.UserId);

                    entity.HasIndex(e => e.ContactEmailAddress);

                    entity.HasIndex(e => e.ContactPhoneNumber);

                    entity.HasIndex(e => e.EmailAddress);

                    entity.HasIndex(e => e.Status);

                    entity.Property(e => e.Salt).HasMaxLength(255);

                    entity.Property(e => e.Status).HasColumnName("StatusId");
                    entity.Property(e => e.HashingAlgorithm).HasColumnName("HashingAlgorithmId");

                    entity.Property(e => e.ContactEmailAddress).HasMaxLength(255);

                    entity.Property(e => e.ContactFirstName).HasMaxLength(50);

                    entity.Property(e => e.ContactJobTitle).HasMaxLength(50);

                    entity.Property(e => e.ContactLastName).HasMaxLength(50);

                    entity.Property(e => e.ContactOrganisation).HasMaxLength(100);

                    entity.Property(e => e.ContactPhoneNumber).HasMaxLength(20);

                    entity.Property(e => e.EmailAddress)
                        .IsRequired()
                        .HasMaxLength(255);

                    entity.Property(e => e.EmailVerifyHash).HasMaxLength(255);

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
                        .HasMaxLength(255);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);
                });

            #endregion

            #region UserSetting

            modelBuilder.Entity<UserSetting>(
                entity =>
                {
                    entity.HasKey(e => new { e.UserId, e.Key });

                    entity.HasIndex(e => e.UserId);

                    entity.Property(e => e.Key).HasColumnName("KeyId");
                    entity.Property(e => e.Value).HasMaxLength(50);

                    entity.HasOne(d => d.User)
                        .WithMany(p => p.UserSettings)
                        .HasForeignKey(d => d.UserId);
                });

            #endregion

            #region UserStatus

            modelBuilder.Entity<UserStatus>(
                entity =>
                {
                    entity.HasKey(e => e.UserStatusId);

                    entity.HasIndex(e => e.ByUserId);

                    entity.HasIndex(e => e.StatusDate);

                    entity.HasIndex(e => e.UserId);

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.ByUser)
                        .WithMany(p => p.UserStatusesByUser)
                        .HasForeignKey(d => d.ByUserId)
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    entity.HasOne(d => d.User)
                        .WithMany(p => p.UserStatuses)
                        .HasForeignKey(d => d.UserId);
                });

            #endregion

        }
    }
}