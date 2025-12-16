using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_FormBuilder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcessEntries_ProcessSteps_CurrentStepId",
                table: "ProcessEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessEntries_Processes_ProcessId",
                table: "ProcessEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessEntries_WebUsers_InitiatorUserId",
                table: "ProcessEntries");

            migrationBuilder.DropTable(
                name: "ProcessEntryHistories");

            migrationBuilder.DropIndex(
                name: "IX_ProcessEntries_CurrentStepId",
                table: "ProcessEntries");

            migrationBuilder.DropIndex(
                name: "IX_ProcessEntries_EntryNumber",
                table: "ProcessEntries");

            migrationBuilder.DropIndex(
                name: "IX_ProcessEntries_InitiatorUserId",
                table: "ProcessEntries");

            migrationBuilder.DropIndex(
                name: "IX_ProcessEntries_ProcessId",
                table: "ProcessEntries");

            migrationBuilder.DropColumn(
                name: "CurrentStepId",
                table: "ProcessEntries");

            migrationBuilder.DropColumn(
                name: "EntryNumber",
                table: "ProcessEntries");

            migrationBuilder.DropColumn(
                name: "InitiatorUserId",
                table: "ProcessEntries");

            migrationBuilder.DropColumn(
                name: "ProcessId",
                table: "ProcessEntries");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ProcessEntries",
                newName: "EntryType");

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "ProcessEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "ProcessEntries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "ProcessEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ProcessEntries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ValidationRegex",
                table: "ProcessEntries",
                type: "text",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntries_Key",
                table: "ProcessEntries",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_PePsConnections_ProcessEntryId",
                table: "PePsConnections",
                column: "ProcessEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PePsConnections_ProcessStepId",
                table: "PePsConnections",
                column: "ProcessStepId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PePsConnections");

            migrationBuilder.DropTable(
                name: "ProcessRequestHistories");

            migrationBuilder.DropTable(
                name: "ProcessRequestValues");

            migrationBuilder.DropTable(
                name: "ProcessRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProcessEntries_Key",
                table: "ProcessEntries");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "ProcessEntries");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "ProcessEntries");

            migrationBuilder.DropColumn(
                name: "Options",
                table: "ProcessEntries");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ProcessEntries");

            migrationBuilder.DropColumn(
                name: "ValidationRegex",
                table: "ProcessEntries");

            migrationBuilder.RenameColumn(
                name: "EntryType",
                table: "ProcessEntries",
                newName: "Status");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentStepId",
                table: "ProcessEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "EntryNumber",
                table: "ProcessEntries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "InitiatorUserId",
                table: "ProcessEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessId",
                table: "ProcessEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ProcessEntryHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessEntryHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessEntryHistories_ProcessActions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "ProcessActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessEntryHistories_ProcessEntries_ProcessEntryId",
                        column: x => x.ProcessEntryId,
                        principalTable: "ProcessEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessEntryHistories_ProcessSteps_FromStepId",
                        column: x => x.FromStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessEntryHistories_ProcessSteps_ToStepId",
                        column: x => x.ToStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessEntryHistories_WebUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "WebUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntries_CurrentStepId",
                table: "ProcessEntries",
                column: "CurrentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntries_EntryNumber",
                table: "ProcessEntries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntries_InitiatorUserId",
                table: "ProcessEntries",
                column: "InitiatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntries_ProcessId",
                table: "ProcessEntries",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntryHistories_ActionId",
                table: "ProcessEntryHistories",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntryHistories_ActorUserId",
                table: "ProcessEntryHistories",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntryHistories_FromStepId",
                table: "ProcessEntryHistories",
                column: "FromStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntryHistories_ProcessEntryId",
                table: "ProcessEntryHistories",
                column: "ProcessEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEntryHistories_ToStepId",
                table: "ProcessEntryHistories",
                column: "ToStepId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessEntries_ProcessSteps_CurrentStepId",
                table: "ProcessEntries",
                column: "CurrentStepId",
                principalTable: "ProcessSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessEntries_Processes_ProcessId",
                table: "ProcessEntries",
                column: "ProcessId",
                principalTable: "Processes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessEntries_WebUsers_InitiatorUserId",
                table: "ProcessEntries",
                column: "InitiatorUserId",
                principalTable: "WebUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
