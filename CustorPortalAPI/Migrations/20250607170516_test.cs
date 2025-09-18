using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustorPortalAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Files_FileKey",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Project_ProjectKeyNavigationprojectKey",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Users_UploaderKeyNavigationUserKey",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_ProjectKeyNavigationprojectKey",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_UploaderKeyNavigationUserKey",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Comments_FileKey",
                table: "Comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ProjectKeyNavigationprojectKey",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "UploaderKeyNavigationUserKey",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FileKey",
                table: "Comments");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "Projects");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "Files",
                newName: "Uploaded_At");

            migrationBuilder.RenameColumn(
                name: "IsCurrent",
                table: "Files",
                newName: "Is_Current");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Tasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "To Do",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Tasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Low",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "File_Name",
                table: "Files",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "File_Path",
                table: "Files",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "File_Type",
                table: "Files",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Mentions",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created_at",
                table: "Projects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "creatorKey",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "projectKey");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleKey);
                });

            migrationBuilder.CreateTable(
                name: "TaskAssignees",
                columns: table => new
                {
                    Taskkey = table.Column<int>(type: "int", nullable: false),
                    UserKey = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignees", x => new { x.Taskkey, x.UserKey });
                    table.ForeignKey(
                        name: "FK_TaskAssignees_Tasks_Taskkey",
                        column: x => x.Taskkey,
                        principalTable: "Tasks",
                        principalColumn: "TaskKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskAssignees_Users_UserKey",
                        column: x => x.UserKey,
                        principalTable: "Users",
                        principalColumn: "UserKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProjects",
                columns: table => new
                {
                    UserKey = table.Column<int>(type: "int", nullable: false),
                    ProjectKey = table.Column<int>(type: "int", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjects", x => new { x.UserKey, x.ProjectKey });
                    table.ForeignKey(
                        name: "FK_UserProjects_Projects_ProjectKey",
                        column: x => x.ProjectKey,
                        principalTable: "Projects",
                        principalColumn: "projectKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProjects_Users_UserKey",
                        column: x => x.UserKey,
                        principalTable: "Users",
                        principalColumn: "UserKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleKey",
                table: "Users",
                column: "RoleKey");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatorKey",
                table: "Tasks",
                column: "CreatorKey");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectKey",
                table: "Tasks",
                column: "ProjectKey");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ProjectKey",
                table: "Files",
                column: "ProjectKey");

            migrationBuilder.CreateIndex(
                name: "IX_Files_UploaderKey",
                table: "Files",
                column: "UploaderKey");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_creatorKey",
                table: "Projects",
                column: "creatorKey");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignees_UserKey",
                table: "TaskAssignees",
                column: "UserKey");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_ProjectKey",
                table: "UserProjects",
                column: "ProjectKey");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Projects_ProjectKey",
                table: "Files",
                column: "ProjectKey",
                principalTable: "Projects",
                principalColumn: "projectKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Users_UploaderKey",
                table: "Files",
                column: "UploaderKey",
                principalTable: "Users",
                principalColumn: "UserKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_creatorKey",
                table: "Projects",
                column: "creatorKey",
                principalTable: "Users",
                principalColumn: "UserKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Projects_ProjectKey",
                table: "Tasks",
                column: "ProjectKey",
                principalTable: "Projects",
                principalColumn: "projectKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_CreatorKey",
                table: "Tasks",
                column: "CreatorKey",
                principalTable: "Users",
                principalColumn: "UserKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleKey",
                table: "Users",
                column: "RoleKey",
                principalTable: "Roles",
                principalColumn: "RoleKey",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Projects_ProjectKey",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Users_UploaderKey",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_creatorKey",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Projects_ProjectKey",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_CreatorKey",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleKey",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "TaskAssignees");

            migrationBuilder.DropTable(
                name: "UserProjects");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleKey",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatorKey",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ProjectKey",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Files_ProjectKey",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_UploaderKey",
                table: "Files");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_creatorKey",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "File_Name",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "File_Path",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "File_Type",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Created_at",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "creatorKey",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "description",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Projects");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Project");

            migrationBuilder.RenameColumn(
                name: "Uploaded_At",
                table: "Files",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "Is_Current",
                table: "Files",
                newName: "IsCurrent");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "To Do");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Low");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProjectKeyNavigationprojectKey",
                table: "Files",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UploaderKeyNavigationUserKey",
                table: "Files",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mentions",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FileKey",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Project",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "projectKey");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ProjectKeyNavigationprojectKey",
                table: "Files",
                column: "ProjectKeyNavigationprojectKey");

            migrationBuilder.CreateIndex(
                name: "IX_Files_UploaderKeyNavigationUserKey",
                table: "Files",
                column: "UploaderKeyNavigationUserKey");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_FileKey",
                table: "Comments",
                column: "FileKey");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Files_FileKey",
                table: "Comments",
                column: "FileKey",
                principalTable: "Files",
                principalColumn: "FileKey");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Project_ProjectKeyNavigationprojectKey",
                table: "Files",
                column: "ProjectKeyNavigationprojectKey",
                principalTable: "Project",
                principalColumn: "projectKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Users_UploaderKeyNavigationUserKey",
                table: "Files",
                column: "UploaderKeyNavigationUserKey",
                principalTable: "Users",
                principalColumn: "UserKey",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
