using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LightDeploy.Server.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeployHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    PublishTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeployHistory", x => x.Id);
                });
            
            migrationBuilder.CreateTable(
                name: "DeployService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupName = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DeployMode = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectPath = table.Column<string>(type: "TEXT", nullable: false),
                    IsSelfContained = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableHealthCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableNotify = table.Column<bool>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    EnvironmentName = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: true),
                    IgnoreRules = table.Column<string>(type: "TEXT", nullable: true),
                    TargetDir = table.Column<string>(type: "TEXT", nullable: true),
                    OnlyCopyFiles = table.Column<bool>(type: "INTEGER", nullable: true),
                    BeforeScript = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeployService", x => x.Id);
                });
            
            migrationBuilder.CreateTable(
                name: "DeployTarget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Host = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<string>(type: "TEXT", nullable: false),
                    HealthCheckUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AuthKey = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeployTarget", x => x.Id);
                });
            
            migrationBuilder.CreateTable(
                name: "GlobalSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QiyeWeChatKey = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalSetting", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeployHistory");

            migrationBuilder.DropTable(
                name: "DeployService");

            migrationBuilder.DropTable(
                name: "DeployTarget");

            migrationBuilder.DropTable(
                name: "GlobalSetting");
        }
    }
}
