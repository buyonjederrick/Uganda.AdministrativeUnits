using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uganda.AdministrativeUnits.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DatasetMeta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Edition = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PublishedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    SourceNote = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    CorrectionsApplied = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatasetMeta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    FullCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Breadcrumb = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Constituencies",
                columns: table => new
                {
                    FullCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Breadcrumb = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    DistrictCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Constituencies", x => x.FullCode);
                    table.ForeignKey(
                        name: "FK_Constituencies_Districts_DistrictCode",
                        column: x => x.DistrictCode,
                        principalTable: "Districts",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subcounties",
                columns: table => new
                {
                    FullCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Breadcrumb = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ConstituencyFullCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcounties", x => x.FullCode);
                    table.ForeignKey(
                        name: "FK_Subcounties_Constituencies_ConstituencyFullCode",
                        column: x => x.ConstituencyFullCode,
                        principalTable: "Constituencies",
                        principalColumn: "FullCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parishes",
                columns: table => new
                {
                    FullCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Breadcrumb = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    SubcountyFullCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parishes", x => x.FullCode);
                    table.ForeignKey(
                        name: "FK_Parishes_Subcounties_SubcountyFullCode",
                        column: x => x.SubcountyFullCode,
                        principalTable: "Subcounties",
                        principalColumn: "FullCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Villages",
                columns: table => new
                {
                    FullCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Breadcrumb = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ParishFullCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Villages", x => x.FullCode);
                    table.ForeignKey(
                        name: "FK_Villages_Parishes_ParishFullCode",
                        column: x => x.ParishFullCode,
                        principalTable: "Parishes",
                        principalColumn: "FullCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Constituencies_DistrictCode",
                table: "Constituencies",
                column: "DistrictCode");

            migrationBuilder.CreateIndex(
                name: "IX_Constituencies_Name",
                table: "Constituencies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_FullCode",
                table: "Districts",
                column: "FullCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_Name",
                table: "Districts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Parishes_Name",
                table: "Parishes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Parishes_SubcountyFullCode",
                table: "Parishes",
                column: "SubcountyFullCode");

            migrationBuilder.CreateIndex(
                name: "IX_Subcounties_ConstituencyFullCode",
                table: "Subcounties",
                column: "ConstituencyFullCode");

            migrationBuilder.CreateIndex(
                name: "IX_Subcounties_Name",
                table: "Subcounties",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Villages_Name",
                table: "Villages",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Villages_ParishFullCode",
                table: "Villages",
                column: "ParishFullCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatasetMeta");

            migrationBuilder.DropTable(
                name: "Villages");

            migrationBuilder.DropTable(
                name: "Parishes");

            migrationBuilder.DropTable(
                name: "Subcounties");

            migrationBuilder.DropTable(
                name: "Constituencies");

            migrationBuilder.DropTable(
                name: "Districts");
        }
    }
}
