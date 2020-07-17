using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    FeedbackId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DifficultyId = table.Column<byte>(nullable: true),
                    Details = table.Column<string>(nullable: true),
                    EmailAddress = table.Column<string>(maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(maxLength: 20, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    NewsArticle = table.Column<bool>(nullable: true),
                    SocialMedia = table.Column<bool>(nullable: true),
                    CompanyIntranet = table.Column<bool>(nullable: true),
                    EmployerUnion = table.Column<bool>(nullable: true),
                    InternetSearch = table.Column<bool>(nullable: true),
                    Charity = table.Column<bool>(nullable: true),
                    LobbyGroup = table.Column<bool>(nullable: true),
                    Report = table.Column<bool>(nullable: true),
                    OtherSource = table.Column<bool>(nullable: true),
                    OtherSourceText = table.Column<string>(nullable: true),
                    FindOutAboutGpg = table.Column<bool>(nullable: true),
                    ReportOrganisationGpgData = table.Column<bool>(nullable: true),
                    CloseOrganisationGpg = table.Column<bool>(nullable: true),
                    ViewSpecificOrganisationGpg = table.Column<bool>(nullable: true),
                    ActionsToCloseGpg = table.Column<bool>(nullable: true),
                    OtherReason = table.Column<bool>(nullable: true),
                    OtherReasonText = table.Column<string>(nullable: true),
                    EmployeeInterestedInOrganisationData = table.Column<bool>(nullable: true),
                    ManagerInvolvedInGpgReport = table.Column<bool>(nullable: true),
                    ResponsibleForReportingGpg = table.Column<bool>(nullable: true),
                    PersonInterestedInGeneralGpg = table.Column<bool>(nullable: true),
                    PersonInterestedInSpecificOrganisationGpg = table.Column<bool>(nullable: true),
                    OtherPerson = table.Column<bool>(nullable: true),
                    OtherPersonText = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.FeedbackId);
                });

            migrationBuilder.CreateTable(
                name: "PublicSectorTypes",
                columns: table => new
                {
                    PublicSectorTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicSectorTypes", x => x.PublicSectorTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ReminderEmails",
                columns: table => new
                {
                    ReminderEmailId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(nullable: false),
                    SectorTypeId = table.Column<byte>(nullable: false),
                    DateSent = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderEmails", x => x.ReminderEmailId);
                });

            migrationBuilder.CreateTable(
                name: "SicSections",
                columns: table => new
                {
                    SicSectionId = table.Column<string>(maxLength: 1, nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SicSections", x => x.SicSectionId);
                });

            migrationBuilder.CreateTable(
                name: "StatementDiligenceTypes",
                columns: table => new
                {
                    StatementDiligenceTypeId = table.Column<short>(nullable: false),
                    ParentDiligenceTypeId = table.Column<short>(nullable: true),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementDiligenceTypes", x => x.StatementDiligenceTypeId);
                    table.ForeignKey(
                        name: "FK_StatementDiligenceTypes_StatementDiligenceTypes_ParentDiligenceTypeId",
                        column: x => x.ParentDiligenceTypeId,
                        principalTable: "StatementDiligenceTypes",
                        principalColumn: "StatementDiligenceTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StatementPolicyTypes",
                columns: table => new
                {
                    StatementPolicyTypeId = table.Column<short>(nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementPolicyTypes", x => x.StatementPolicyTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementRiskTypes",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(nullable: false),
                    ParentRiskTypeId = table.Column<short>(nullable: true),
                    RiskCategoryId = table.Column<byte>(nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementRiskTypes", x => x.StatementRiskTypeId);
                    table.ForeignKey(
                        name: "FK_StatementRiskTypes_StatementRiskTypes_ParentRiskTypeId",
                        column: x => x.ParentRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StatementSectorTypes",
                columns: table => new
                {
                    StatementSectorTypeId = table.Column<short>(nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementSectorTypes", x => x.StatementSectorTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementTrainingTypes",
                columns: table => new
                {
                    StatementTrainingTypeId = table.Column<short>(nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementTrainingTypes", x => x.StatementTrainingTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobTitle = table.Column<string>(maxLength: 50, nullable: false),
                    Firstname = table.Column<string>(maxLength: 50, nullable: false),
                    Lastname = table.Column<string>(maxLength: 50, nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 255, nullable: false),
                    ContactJobTitle = table.Column<string>(maxLength: 50, nullable: true),
                    ContactFirstName = table.Column<string>(maxLength: 50, nullable: true),
                    ContactLastName = table.Column<string>(maxLength: 50, nullable: true),
                    ContactOrganisation = table.Column<string>(maxLength: 100, nullable: true),
                    ContactEmailAddress = table.Column<string>(maxLength: 255, nullable: true),
                    ContactPhoneNumber = table.Column<string>(maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(maxLength: 255, nullable: false),
                    Salt = table.Column<string>(maxLength: 255, nullable: true),
                    HashingAlgorithmId = table.Column<int>(nullable: false),
                    EmailVerifyHash = table.Column<string>(maxLength: 255, nullable: true),
                    EmailVerifySendDate = table.Column<DateTime>(nullable: true),
                    EmailVerifiedDate = table.Column<DateTime>(nullable: true),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    LoginAttempts = table.Column<int>(nullable: false),
                    LoginDate = table.Column<DateTime>(nullable: true),
                    ResetSendDate = table.Column<DateTime>(nullable: true),
                    ResetAttempts = table.Column<int>(nullable: false),
                    VerifyAttemptDate = table.Column<DateTime>(nullable: true),
                    VerifyAttempts = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SicCodes",
                columns: table => new
                {
                    SicCodeId = table.Column<int>(nullable: false),
                    SicSectionId = table.Column<string>(maxLength: 1, nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SicCodes", x => x.SicCodeId);
                    table.ForeignKey(
                        name: "FK_SicCodes_SicSections_SicSectionId",
                        column: x => x.SicSectionId,
                        principalTable: "SicSections",
                        principalColumn: "SicSectionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    KeyId = table.Column<byte>(nullable: false),
                    Value = table.Column<string>(maxLength: 50, nullable: true),
                    Modified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => new { x.UserId, x.KeyId });
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStatuses",
                columns: table => new
                {
                    UserStatusId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatuses", x => x.UserStatusId);
                    table.ForeignKey(
                        name: "FK_UserStatuses_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserStatuses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AddressStatuses",
                columns: table => new
                {
                    AddressStatusId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressStatuses", x => x.AddressStatusId);
                    table.ForeignKey(
                        name: "FK_AddressStatuses_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Organisations",
                columns: table => new
                {
                    OrganisationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyNumber = table.Column<string>(maxLength: 10, nullable: true),
                    OrganisationName = table.Column<string>(maxLength: 100, nullable: false),
                    SectorTypeId = table.Column<byte>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    DUNSNumber = table.Column<string>(maxLength: 10, nullable: true),
                    EmployerReference = table.Column<string>(maxLength: 10, nullable: true),
                    DateOfCessation = table.Column<DateTime>(nullable: true),
                    LatestAddressId = table.Column<long>(nullable: true),
                    LatestStatementId = table.Column<long>(nullable: true),
                    LatestScopeId = table.Column<long>(nullable: true),
                    LatestRegistration_UserId = table.Column<long>(nullable: true),
                    LatestRegistration_OrganisationId = table.Column<long>(nullable: true),
                    LatestPublicSectorTypeId = table.Column<long>(nullable: true),
                    LastCheckedAgainstCompaniesHouse = table.Column<DateTime>(nullable: true),
                    OptedOutFromCompaniesHouseUpdate = table.Column<bool>(nullable: false, defaultValue: false),
                    SecurityCode = table.Column<string>(nullable: true),
                    SecurityCodeExpiryDateTime = table.Column<DateTime>(nullable: true),
                    SecurityCodeCreatedDateTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisations", x => x.OrganisationId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionId = table.Column<byte>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: true),
                    OriginalUserId = table.Column<long>(nullable: true),
                    ImpersonatedUserId = table.Column<long>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ImpersonatedUserId",
                        column: x => x.ImpersonatedUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_OriginalUserId",
                        column: x => x.OriginalUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationAddresses",
                columns: table => new
                {
                    AddressId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<long>(nullable: false),
                    Address1 = table.Column<string>(maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(maxLength: 100, nullable: true),
                    Address3 = table.Column<string>(maxLength: 100, nullable: true),
                    TownCity = table.Column<string>(maxLength: 100, nullable: true),
                    County = table.Column<string>(maxLength: 100, nullable: true),
                    Country = table.Column<string>(maxLength: 100, nullable: true),
                    PoBox = table.Column<string>(maxLength: 30, nullable: true),
                    PostCode = table.Column<string>(maxLength: 20, nullable: true),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    IsUkAddress = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationAddresses", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_OrganisationAddresses_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationNames",
                columns: table => new
                {
                    OrganisationNameId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationNames", x => x.OrganisationNameId);
                    table.ForeignKey(
                        name: "FK_OrganisationNames_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationPublicSectorTypes",
                columns: table => new
                {
                    OrganisationPublicSectorTypeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicSectorTypeId = table.Column<int>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Retired = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationPublicSectorTypes", x => x.OrganisationPublicSectorTypeId);
                    table.ForeignKey(
                        name: "FK_OrganisationPublicSectorTypes_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganisationPublicSectorTypes_PublicSectorTypes_PublicSectorTypeId",
                        column: x => x.PublicSectorTypeId,
                        principalTable: "PublicSectorTypes",
                        principalColumn: "PublicSectorTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationReferences",
                columns: table => new
                {
                    OrganisationReferenceId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    ReferenceName = table.Column<string>(maxLength: 100, nullable: false),
                    ReferenceValue = table.Column<string>(maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationReferences", x => x.OrganisationReferenceId);
                    table.ForeignKey(
                        name: "FK_OrganisationReferences_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationScopes",
                columns: table => new
                {
                    OrganisationScopeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    ScopeStatusId = table.Column<byte>(nullable: false),
                    ScopeStatusDate = table.Column<DateTime>(nullable: false),
                    RegisterStatusId = table.Column<byte>(nullable: false),
                    RegisterStatusDate = table.Column<DateTime>(nullable: false),
                    ContactFirstname = table.Column<string>(maxLength: 50, nullable: true),
                    ContactLastname = table.Column<string>(maxLength: 50, nullable: true),
                    ContactEmailAddress = table.Column<string>(maxLength: 255, nullable: true),
                    ReadGuidance = table.Column<bool>(nullable: true),
                    Reason = table.Column<string>(maxLength: 1000, nullable: true),
                    CampaignId = table.Column<string>(maxLength: 50, nullable: true),
                    SubmissionDeadline = table.Column<DateTime>(type: "date", nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationScopes", x => x.OrganisationScopeId);
                    table.ForeignKey(
                        name: "FK_OrganisationScopes_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationSicCodes",
                columns: table => new
                {
                    OrganisationSicCodeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SicCodeId = table.Column<int>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Retired = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationSicCodes", x => x.OrganisationSicCodeId);
                    table.ForeignKey(
                        name: "FK_OrganisationSicCodes_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganisationSicCodes_SicCodes_SicCodeId",
                        column: x => x.SicCodeId,
                        principalTable: "SicCodes",
                        principalColumn: "SicCodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationStatuses",
                columns: table => new
                {
                    OrganisationStatusId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationStatuses", x => x.OrganisationStatusId);
                    table.ForeignKey(
                        name: "FK_OrganisationStatuses_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganisationStatuses_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Statements",
                columns: table => new
                {
                    StatementId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    SubmissionDeadline = table.Column<DateTime>(type: "Date", nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Modifications = table.Column<string>(nullable: true),
                    Modified = table.Column<DateTime>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    LateReason = table.Column<string>(maxLength: 255, nullable: true),
                    EHRCResponse = table.Column<bool>(nullable: false, defaultValue: false),
                    StatementUrl = table.Column<string>(maxLength: 255, nullable: true),
                    StatementStartDate = table.Column<DateTime>(type: "Date", nullable: false),
                    StatementEndDate = table.Column<DateTime>(type: "Date", nullable: false),
                    ApproverFirstName = table.Column<string>(maxLength: 50, nullable: true),
                    ApproverLastName = table.Column<string>(maxLength: 50, nullable: true),
                    ApproverJobTitle = table.Column<string>(maxLength: 100, nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "Date", nullable: false),
                    IncludesStructure = table.Column<bool>(nullable: false),
                    StructureDetails = table.Column<string>(nullable: true),
                    IncludesPolicies = table.Column<bool>(nullable: false),
                    PolicyDetails = table.Column<string>(nullable: true),
                    IncludesRisks = table.Column<bool>(nullable: false),
                    RisksDetails = table.Column<string>(nullable: true),
                    IncludesDueDiligence = table.Column<bool>(nullable: false),
                    DueDiligenceDetails = table.Column<string>(nullable: true),
                    IncludesTraining = table.Column<bool>(nullable: false),
                    TrainingDetails = table.Column<string>(nullable: true),
                    IncludesGoals = table.Column<bool>(nullable: false),
                    GoalsDetails = table.Column<string>(nullable: true),
                    OtherSector = table.Column<string>(nullable: true),
                    MinTurnover = table.Column<int>(nullable: false),
                    MaxTurnover = table.Column<int>(nullable: true),
                    OtherPolicies = table.Column<string>(nullable: true),
                    OtherRelevantRisks = table.Column<string>(nullable: true),
                    OtherHighRisks = table.Column<string>(nullable: true),
                    ForcedLabourDetails = table.Column<string>(nullable: true),
                    SlaveryInstanceDetails = table.Column<string>(nullable: true),
                    SlaveryInstanceRemediation = table.Column<string>(nullable: true),
                    OtherTraining = table.Column<string>(nullable: true),
                    IncludesMeasuringProgress = table.Column<bool>(nullable: false),
                    ProgressMeasures = table.Column<string>(nullable: true),
                    KeyAchievements = table.Column<string>(nullable: true),
                    MinStatementYears = table.Column<decimal>(nullable: false),
                    MaxStatementYears = table.Column<decimal>(nullable: true),
                    IncludedOrganisationCount = table.Column<short>(nullable: false, defaultValue: (short)0),
                    ExcludedOrganisationCount = table.Column<short>(nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statements", x => x.StatementId);
                    table.ForeignKey(
                        name: "FK_Statements_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOrganisations",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    PIN = table.Column<string>(maxLength: 255, nullable: true),
                    PINHash = table.Column<string>(maxLength: 255, nullable: true),
                    PINSentDate = table.Column<DateTime>(nullable: true),
                    PITPNotifyLetterId = table.Column<string>(maxLength: 255, nullable: true),
                    PINConfirmedDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttemptDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttempts = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    AddressId = table.Column<long>(nullable: true),
                    MethodId = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrganisations", x => new { x.UserId, x.OrganisationId });
                    table.ForeignKey(
                        name: "FK_UserOrganisations_OrganisationAddresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "OrganisationAddresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserOrganisations_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOrganisations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementDiligences",
                columns: table => new
                {
                    StatementDiligenceTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Details = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementDiligences", x => new { x.StatementDiligenceTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementDiligences_StatementDiligenceTypes_StatementDiligenceTypeId",
                        column: x => x.StatementDiligenceTypeId,
                        principalTable: "StatementDiligenceTypes",
                        principalColumn: "StatementDiligenceTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementDiligences_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementHighRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementHighRisks", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementHighRisks_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementHighRisks_StatementRiskTypes_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementLocationRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementLocationRisks", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementLocationRisks_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementLocationRisks_StatementRiskTypes_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementOrganisations",
                columns: table => new
                {
                    StatementOrganisationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatementId = table.Column<long>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: true),
                    Included = table.Column<bool>(nullable: false),
                    OrganisationName = table.Column<string>(maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementOrganisations", x => x.StatementOrganisationId);
                    table.ForeignKey(
                        name: "FK_StatementOrganisations_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StatementOrganisations_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementPolicies",
                columns: table => new
                {
                    StatementPolicyTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementPolicies", x => new { x.StatementPolicyTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementPolicies_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementPolicies_StatementPolicyTypes_StatementPolicyTypeId",
                        column: x => x.StatementPolicyTypeId,
                        principalTable: "StatementPolicyTypes",
                        principalColumn: "StatementPolicyTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementRelevantRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementRelevantRisks", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementRelevantRisks_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementRelevantRisks_StatementRiskTypes_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementSectors",
                columns: table => new
                {
                    StatementSectorTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementSectors", x => new { x.StatementSectorTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementSectors_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementSectors_StatementSectorTypes_StatementSectorTypeId",
                        column: x => x.StatementSectorTypeId,
                        principalTable: "StatementSectorTypes",
                        principalColumn: "StatementSectorTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementStatuses",
                columns: table => new
                {
                    StatementStatusId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatementId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementStatuses", x => x.StatementStatusId);
                    table.ForeignKey(
                        name: "FK_StatementStatuses_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementStatuses_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementTrainings",
                columns: table => new
                {
                    StatementTrainingTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementTrainings", x => new { x.StatementTrainingTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementTrainings_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementTrainings_StatementTrainingTypes_StatementTrainingTypeId",
                        column: x => x.StatementTrainingTypeId,
                        principalTable: "StatementTrainingTypes",
                        principalColumn: "StatementTrainingTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressStatuses_AddressId",
                table: "AddressStatuses",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressStatuses_ByUserId",
                table: "AddressStatuses",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressStatuses_StatusDate",
                table: "AddressStatuses",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ImpersonatedUserId",
                table: "AuditLogs",
                column: "ImpersonatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganisationId",
                table: "AuditLogs",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OriginalUserId",
                table: "AuditLogs",
                column: "OriginalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationAddresses_OrganisationId",
                table: "OrganisationAddresses",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationAddresses_StatusId",
                table: "OrganisationAddresses",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationAddresses_StatusDate",
                table: "OrganisationAddresses",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationNames_Created",
                table: "OrganisationNames",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationNames_Name",
                table: "OrganisationNames",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationNames_OrganisationId",
                table: "OrganisationNames",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationPublicSectorTypes_Created",
                table: "OrganisationPublicSectorTypes",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationPublicSectorTypes_OrganisationId",
                table: "OrganisationPublicSectorTypes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationPublicSectorTypes_PublicSectorTypeId",
                table: "OrganisationPublicSectorTypes",
                column: "PublicSectorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationPublicSectorTypes_Retired",
                table: "OrganisationPublicSectorTypes",
                column: "Retired");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_Created",
                table: "OrganisationReferences",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_OrganisationId",
                table: "OrganisationReferences",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_ReferenceName",
                table: "OrganisationReferences",
                column: "ReferenceName");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_ReferenceValue",
                table: "OrganisationReferences",
                column: "ReferenceValue");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_CompanyNumber",
                table: "Organisations",
                column: "CompanyNumber",
                unique: true,
                filter: "([CompanyNumber] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_DUNSNumber",
                table: "Organisations",
                column: "DUNSNumber",
                unique: true,
                filter: "([DUNSNumber] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_EmployerReference",
                table: "Organisations",
                column: "EmployerReference",
                unique: true,
                filter: "([EmployerReference] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_LatestAddressId",
                table: "Organisations",
                column: "LatestAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_LatestPublicSectorTypeId",
                table: "Organisations",
                column: "LatestPublicSectorTypeId",
                unique: true,
                filter: "[LatestPublicSectorTypeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_LatestScopeId",
                table: "Organisations",
                column: "LatestScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_LatestStatementId",
                table: "Organisations",
                column: "LatestStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_OrganisationName",
                table: "Organisations",
                column: "OrganisationName");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_SectorTypeId",
                table: "Organisations",
                column: "SectorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_StatusId",
                table: "Organisations",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_LatestRegistration_OrganisationId_LatestRegistration_UserId",
                table: "Organisations",
                columns: new[] { "LatestRegistration_OrganisationId", "LatestRegistration_UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                table: "Organisations",
                columns: new[] { "LatestRegistration_UserId", "LatestRegistration_OrganisationId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_OrganisationId",
                table: "OrganisationScopes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_RegisterStatusId",
                table: "OrganisationScopes",
                column: "RegisterStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_ScopeStatusId",
                table: "OrganisationScopes",
                column: "ScopeStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_ScopeStatusDate",
                table: "OrganisationScopes",
                column: "ScopeStatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_StatusId",
                table: "OrganisationScopes",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_SubmissionDeadline",
                table: "OrganisationScopes",
                column: "SubmissionDeadline");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSicCodes_Created",
                table: "OrganisationSicCodes",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSicCodes_OrganisationId",
                table: "OrganisationSicCodes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSicCodes_Retired",
                table: "OrganisationSicCodes",
                column: "Retired");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSicCodes_SicCodeId",
                table: "OrganisationSicCodes",
                column: "SicCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationStatuses_ByUserId",
                table: "OrganisationStatuses",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationStatuses_OrganisationId",
                table: "OrganisationStatuses",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationStatuses_StatusDate",
                table: "OrganisationStatuses",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_SicCodes_SicSectionId",
                table: "SicCodes",
                column: "SicSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementDiligences_StatementId",
                table: "StatementDiligences",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementDiligenceTypes_ParentDiligenceTypeId",
                table: "StatementDiligenceTypes",
                column: "ParentDiligenceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementHighRisks_StatementId",
                table: "StatementHighRisks",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementLocationRisks_StatementId",
                table: "StatementLocationRisks",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementOrganisations_OrganisationId",
                table: "StatementOrganisations",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementOrganisations_StatementId",
                table: "StatementOrganisations",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementPolicies_StatementId",
                table: "StatementPolicies",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRelevantRisks_StatementId",
                table: "StatementRelevantRisks",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRiskTypes_ParentRiskTypeId",
                table: "StatementRiskTypes",
                column: "ParentRiskTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_IncludesDueDiligence",
                table: "Statements",
                column: "IncludesDueDiligence");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_IncludesGoals",
                table: "Statements",
                column: "IncludesGoals");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_IncludesPolicies",
                table: "Statements",
                column: "IncludesPolicies");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_IncludesRisks",
                table: "Statements",
                column: "IncludesRisks");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_IncludesStructure",
                table: "Statements",
                column: "IncludesStructure");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_IncludesTraining",
                table: "Statements",
                column: "IncludesTraining");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_MaxTurnover",
                table: "Statements",
                column: "MaxTurnover");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_MinTurnover",
                table: "Statements",
                column: "MinTurnover");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_OrganisationId",
                table: "Statements",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_StatementEndDate",
                table: "Statements",
                column: "StatementEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_StatementStartDate",
                table: "Statements",
                column: "StatementStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_StatusId",
                table: "Statements",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_SubmissionDeadline",
                table: "Statements",
                column: "SubmissionDeadline");

            migrationBuilder.CreateIndex(
                name: "IX_StatementSectors_StatementId",
                table: "StatementSectors",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementStatuses_ByUserId",
                table: "StatementStatuses",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementStatuses_StatementId",
                table: "StatementStatuses",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementTrainings_StatementId",
                table: "StatementTrainings",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisations_AddressId",
                table: "UserOrganisations",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisations_OrganisationId",
                table: "UserOrganisations",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisations_UserId",
                table: "UserOrganisations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ContactEmailAddress",
                table: "Users",
                column: "ContactEmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ContactPhoneNumber",
                table: "Users",
                column: "ContactPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress",
                table: "Users",
                column: "EmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StatusId",
                table: "Users",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatuses_ByUserId",
                table: "UserStatuses",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatuses_StatusDate",
                table: "UserStatuses",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatuses_UserId",
                table: "UserStatuses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AddressStatuses_OrganisationAddresses_AddressId",
                table: "AddressStatuses",
                column: "AddressId",
                principalTable: "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_OrganisationAddresses_LatestAddressId",
                table: "Organisations",
                column: "LatestAddressId",
                principalTable: "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_OrganisationPublicSectorTypes_LatestPublicSectorTypeId",
                table: "Organisations",
                column: "LatestPublicSectorTypeId",
                principalTable: "OrganisationPublicSectorTypes",
                principalColumn: "OrganisationPublicSectorTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_OrganisationScopes_LatestScopeId",
                table: "Organisations",
                column: "LatestScopeId",
                principalTable: "OrganisationScopes",
                principalColumn: "OrganisationScopeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_Statements_LatestStatementId",
                table: "Organisations",
                column: "LatestStatementId",
                principalTable: "Statements",
                principalColumn: "StatementId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_UserOrganisations_LatestRegistration_OrganisationId_LatestRegistration_UserId",
                table: "Organisations",
                columns: new[] { "LatestRegistration_OrganisationId", "LatestRegistration_UserId" },
                principalTable: "UserOrganisations",
                principalColumns: new[] { "UserId", "OrganisationId" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_OrganisationAddresses_LatestAddressId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserOrganisations_OrganisationAddresses_AddressId",
                table: "UserOrganisations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserOrganisations_Users_UserId",
                table: "UserOrganisations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationPublicSectorTypes_Organisations_OrganisationId",
                table: "OrganisationPublicSectorTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationScopes_Organisations_OrganisationId",
                table: "OrganisationScopes");

            migrationBuilder.DropForeignKey(
                name: "FK_Statements_Organisations_OrganisationId",
                table: "Statements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserOrganisations_Organisations_OrganisationId",
                table: "UserOrganisations");

            migrationBuilder.DropTable(
                name: "AddressStatuses");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "OrganisationNames");

            migrationBuilder.DropTable(
                name: "OrganisationReferences");

            migrationBuilder.DropTable(
                name: "OrganisationSicCodes");

            migrationBuilder.DropTable(
                name: "OrganisationStatuses");

            migrationBuilder.DropTable(
                name: "ReminderEmails");

            migrationBuilder.DropTable(
                name: "StatementDiligences");

            migrationBuilder.DropTable(
                name: "StatementHighRisks");

            migrationBuilder.DropTable(
                name: "StatementLocationRisks");

            migrationBuilder.DropTable(
                name: "StatementOrganisations");

            migrationBuilder.DropTable(
                name: "StatementPolicies");

            migrationBuilder.DropTable(
                name: "StatementRelevantRisks");

            migrationBuilder.DropTable(
                name: "StatementSectors");

            migrationBuilder.DropTable(
                name: "StatementStatuses");

            migrationBuilder.DropTable(
                name: "StatementTrainings");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "UserStatuses");

            migrationBuilder.DropTable(
                name: "SicCodes");

            migrationBuilder.DropTable(
                name: "StatementDiligenceTypes");

            migrationBuilder.DropTable(
                name: "StatementPolicyTypes");

            migrationBuilder.DropTable(
                name: "StatementRiskTypes");

            migrationBuilder.DropTable(
                name: "StatementSectorTypes");

            migrationBuilder.DropTable(
                name: "StatementTrainingTypes");

            migrationBuilder.DropTable(
                name: "SicSections");

            migrationBuilder.DropTable(
                name: "OrganisationAddresses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Organisations");

            migrationBuilder.DropTable(
                name: "OrganisationPublicSectorTypes");

            migrationBuilder.DropTable(
                name: "OrganisationScopes");

            migrationBuilder.DropTable(
                name: "Statements");

            migrationBuilder.DropTable(
                name: "UserOrganisations");

            migrationBuilder.DropTable(
                name: "PublicSectorTypes");
        }
    }
}
