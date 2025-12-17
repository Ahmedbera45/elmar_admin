using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileMetadatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    UploadedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileMetadatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    Options = table.Column<string>(type: "text", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ValidationRegex = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentProcessId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Processes_Processes_ParentProcessId",
                        column: x => x.ParentProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Permissions = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StepType = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessSteps_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PePsConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    PermissionType = table.Column<int>(type: "integer", nullable: false),
                    VisibilityRule = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PePsConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PePsConnections_ProcessEntries_ProcessEntryId",
                        column: x => x.ProcessEntryId,
                        principalTable: "ProcessEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PePsConnections_ProcessSteps_ProcessStepId",
                        column: x => x.ProcessStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InitiatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessRequests_ProcessSteps_CurrentStepId",
                        column: x => x.CurrentStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRequests_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRequests_WebUsers_InitiatorUserId",
                        column: x => x.InitiatorUserId,
                        principalTable: "WebUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessRequestValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    StringValue = table.Column<string>(type: "text", nullable: true),
                    IntValue = table.Column<int>(type: "integer", nullable: true),
                    DecimalValue = table.Column<decimal>(type: "numeric", nullable: true),
                    DateValue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BoolValue = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRequestValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessRequestValues_ProcessEntries_ProcessEntryId",
                        column: x => x.ProcessEntryId,
                        principalTable: "ProcessEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRequestValues_ProcessRequests_ProcessRequestId",
                        column: x => x.ProcessRequestId,
                        principalTable: "ProcessRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessActionConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleExpression = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessActionConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessActionConditions_ProcessSteps_TargetStepId",
                        column: x => x.TargetStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    IsCommentRequired = table.Column<bool>(type: "boolean", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: true),
                    TimeoutActionId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultConditionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessActions_ProcessActionConditions_DefaultConditionId",
                        column: x => x.DefaultConditionId,
                        principalTable: "ProcessActionConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessActions_ProcessActions_TimeoutActionId",
                        column: x => x.TimeoutActionId,
                        principalTable: "ProcessActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessActions_ProcessSteps_ProcessStepId",
                        column: x => x.ProcessStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessActions_ProcessSteps_TargetStepId",
                        column: x => x.TargetStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessRequestHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRequestHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessRequestHistories_ProcessActions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "ProcessActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRequestHistories_ProcessRequests_ProcessRequestId",
                        column: x => x.ProcessRequestId,
                        principalTable: "ProcessRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessRequestHistories_ProcessSteps_FromStepId",
                        column: x => x.FromStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRequestHistories_ProcessSteps_ToStepId",
                        column: x => x.ToStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRequestHistories_WebUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "WebUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PePsConnections_ProcessEntryId",
                table: "PePsConnections",
                column: "ProcessEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PePsConnections_ProcessStepId",
                table: "PePsConnections",
                column: "ProcessStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessActionConditions_ProcessActionId",
                table: "ProcessActionConditions",
                column: "ProcessActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessActionConditions_TargetStepId",
                table: "ProcessActionConditions",
                column: "TargetStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessActions_DefaultConditionId",
                table: "ProcessActions",
                column: "DefaultConditionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessActions_ProcessStepId",
                table: "ProcessActions",
                column: "ProcessStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessActions_TargetStepId",
                table: "ProcessActions",
                column: "TargetStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessActions_TimeoutActionId",
                table: "ProcessActions",
                column: "TimeoutActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntries_Key",
                table: "ProcessEntries",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_Code",
                table: "Processes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Processes_ParentProcessId",
                table: "Processes",
                column: "ParentProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequestHistories_ActionId",
                table: "ProcessRequestHistories",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequestHistories_ActorUserId",
                table: "ProcessRequestHistories",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequestHistories_FromStepId",
                table: "ProcessRequestHistories",
                column: "FromStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequestHistories_ProcessRequestId",
                table: "ProcessRequestHistories",
                column: "ProcessRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequestHistories_ToStepId",
                table: "ProcessRequestHistories",
                column: "ToStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequests_CurrentStepId",
                table: "ProcessRequests",
                column: "CurrentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequests_InitiatorUserId",
                table: "ProcessRequests",
                column: "InitiatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequests_ProcessId",
                table: "ProcessRequests",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequests_RequestNumber",
                table: "ProcessRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequestValues_ProcessEntryId",
                table: "ProcessRequestValues",
                column: "ProcessEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequestValues_ProcessRequestId",
                table: "ProcessRequestValues",
                column: "ProcessRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessSteps_ProcessId",
                table: "ProcessSteps",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WebUsers_Username",
                table: "WebUsers",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessActionConditions_ProcessActions_ProcessActionId",
                table: "ProcessActionConditions",
                column: "ProcessActionId",
                principalTable: "ProcessActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcessActionConditions_ProcessSteps_TargetStepId",
                table: "ProcessActionConditions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessActions_ProcessSteps_ProcessStepId",
                table: "ProcessActions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessActions_ProcessSteps_TargetStepId",
                table: "ProcessActions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessActionConditions_ProcessActions_ProcessActionId",
                table: "ProcessActionConditions");

            migrationBuilder.DropTable(
                name: "FileMetadatas");

            migrationBuilder.DropTable(
                name: "PePsConnections");

            migrationBuilder.DropTable(
                name: "ProcessRequestHistories");

            migrationBuilder.DropTable(
                name: "ProcessRequestValues");

            migrationBuilder.DropTable(
                name: "ProcessEntries");

            migrationBuilder.DropTable(
                name: "ProcessRequests");

            migrationBuilder.DropTable(
                name: "WebUsers");

            migrationBuilder.DropTable(
                name: "ProcessSteps");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropTable(
                name: "ProcessActions");

            migrationBuilder.DropTable(
                name: "ProcessActionConditions");
        }
    }
}
