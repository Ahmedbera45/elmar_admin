using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRuntimeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InitiatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessEntries_ProcessSteps_CurrentStepId",
                        column: x => x.CurrentStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessEntries_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessEntries_WebUsers_InitiatorUserId",
                        column: x => x.InitiatorUserId,
                        principalTable: "WebUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessEntryHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessEntryId = table.Column<Guid>(type: "uuid", nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessEntryHistories");

            migrationBuilder.DropTable(
                name: "ProcessEntries");
        }
    }
}
