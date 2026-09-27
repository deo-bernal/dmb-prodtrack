using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ord");

            migrationBuilder.EnsureSchema(
                name: "sec");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "md");

            migrationBuilder.EnsureSchema(
                name: "mes");

            migrationBuilder.EnsureSchema(
                name: "qc");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "sec",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "sec",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BadgeNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HomeStationCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEntry",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColorScheme",
                schema: "md",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TextColor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorScheme", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                schema: "sec",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NumberSequence",
                schema: "ord",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastValue = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberSequence", x => new { x.Name, x.Year });
                });

            migrationBuilder.CreateTable(
                name: "Product",
                schema: "md",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sku = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequiresArtworkApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DefaultSpec = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.CheckConstraint("CK_Product_DefaultSpec_IsJson", "ISJSON([DefaultSpec]) = 1");
                });

            migrationBuilder.CreateTable(
                name: "QcChecklistTemplate",
                schema: "qc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QcChecklistTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReasonCode",
                schema: "md",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReasonCode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Routing",
                schema: "md",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routing", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrder",
                schema: "ord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PoNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SignalWord",
                schema: "md",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Word = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    HeaderColor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TextColor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalWord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Station",
                schema: "md",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    WorkCenter = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Station", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "sec",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "sec",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "sec",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "sec",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "sec",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "sec",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "sec",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "sec",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "sec",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "sec",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "sec",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QcChecklistItem",
                schema: "qc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QcChecklistItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QcChecklistItem_QcChecklistTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "qc",
                        principalTable: "QcChecklistTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderLine",
                schema: "ord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Legend = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Spec = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderLine", x => x.Id);
                    table.CheckConstraint("CK_SalesOrderLine_Spec_IsJson", "ISJSON([Spec]) = 1");
                    table.ForeignKey(
                        name: "FK_SalesOrderLine_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "md",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderLine_SalesOrder_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalSchema: "ord",
                        principalTable: "SalesOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutingStep",
                schema: "md",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoutingId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    SetupMinutes = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    StdMinutesPerUnit = table.Column<decimal>(type: "decimal(9,3)", precision: 9, scale: 3, nullable: false),
                    AllowOverlap = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingStep_Routing_RoutingId",
                        column: x => x.RoutingId,
                        principalSchema: "md",
                        principalTable: "Routing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoutingStep_Station_StationId",
                        column: x => x.StationId,
                        principalSchema: "md",
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrder",
                schema: "ord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SalesOrderLineId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequiresArtworkApproval = table.Column<bool>(type: "bit", nullable: false),
                    RoutingId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CompletedQuantity = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Legend = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HoldReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReleasedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Spec = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrder", x => x.Id);
                    table.CheckConstraint("CK_WorkOrder_Spec_IsJson", "ISJSON([Spec]) = 1");
                    table.ForeignKey(
                        name: "FK_WorkOrder_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "md",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrder_Routing_RoutingId",
                        column: x => x.RoutingId,
                        principalSchema: "md",
                        principalTable: "Routing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrder_SalesOrderLine_SalesOrderLineId",
                        column: x => x.SalesOrderLineId,
                        principalSchema: "ord",
                        principalTable: "SalesOrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtworkProof",
                schema: "ord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    FileKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DecisionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DecidedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtworkProof", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtworkProof_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "ord",
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Operation",
                schema: "ord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InputQuantity = table.Column<int>(type: "int", nullable: false),
                    GoodQuantity = table.Column<int>(type: "int", nullable: false),
                    ScrapQuantity = table.Column<int>(type: "int", nullable: false),
                    SetupMinutes = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    StdMinutesPerUnit = table.Column<decimal>(type: "decimal(9,3)", precision: 9, scale: 3, nullable: false),
                    AllowOverlap = table.Column<bool>(type: "bit", nullable: false),
                    IsRework = table.Column<bool>(type: "bit", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operation_Station_StationId",
                        column: x => x.StationId,
                        principalSchema: "md",
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operation_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "ord",
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperationEvent",
                schema: "mes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReasonCodeId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationEvent_Operation_OperationId",
                        column: x => x.OperationId,
                        principalSchema: "ord",
                        principalTable: "Operation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperationEvent_ReasonCode_ReasonCodeId",
                        column: x => x.ReasonCodeId,
                        principalSchema: "md",
                        principalTable: "ReasonCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QcInspection",
                schema: "qc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationId = table.Column<int>(type: "int", nullable: false),
                    SampleSize = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Disposition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InspectorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InspectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QcInspection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QcInspection_Operation_OperationId",
                        column: x => x.OperationId,
                        principalSchema: "ord",
                        principalTable: "Operation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScrapRecord",
                schema: "mes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReasonCodeId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhotoFileKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapRecord_Operation_OperationId",
                        column: x => x.OperationId,
                        principalSchema: "ord",
                        principalTable: "Operation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScrapRecord_ReasonCode_ReasonCodeId",
                        column: x => x.ReasonCodeId,
                        principalSchema: "md",
                        principalTable: "ReasonCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QcResultItem",
                schema: "qc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    ChecklistItemId = table.Column<int>(type: "int", nullable: false),
                    Passed = table.Column<bool>(type: "bit", nullable: false),
                    MeasuredValue = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QcResultItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QcResultItem_QcChecklistItem_ChecklistItemId",
                        column: x => x.ChecklistItemId,
                        principalSchema: "qc",
                        principalTable: "QcChecklistItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QcResultItem_QcInspection_InspectionId",
                        column: x => x.InspectionId,
                        principalSchema: "qc",
                        principalTable: "QcInspection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtworkProof_WorkOrderId_Version",
                schema: "ord",
                table: "ArtworkProof",
                columns: new[] { "WorkOrderId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "sec",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "sec",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "sec",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "sec",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "sec",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "sec",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BadgeNumber",
                schema: "sec",
                table: "AspNetUsers",
                column: "BadgeNumber",
                unique: true,
                filter: "[BadgeNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "sec",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntry_EntityName_EntityKey_OccurredAtUtc",
                schema: "audit",
                table: "AuditEntry",
                columns: new[] { "EntityName", "EntityKey", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ColorScheme_Code",
                schema: "md",
                table: "ColorScheme",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operation_StationId_Status",
                schema: "ord",
                table: "Operation",
                columns: new[] { "StationId", "Status" })
                .Annotation("SqlServer:Include", new[] { "WorkOrderId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_Operation_WorkOrderId_Sequence",
                schema: "ord",
                table: "Operation",
                columns: new[] { "WorkOrderId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_OperationEvent_OperationId_OccurredAtUtc",
                schema: "mes",
                table: "OperationEvent",
                columns: new[] { "OperationId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OperationEvent_ReasonCodeId",
                schema: "mes",
                table: "OperationEvent",
                column: "ReasonCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationEvent_RequestId",
                schema: "mes",
                table: "OperationEvent",
                column: "RequestId",
                unique: true,
                filter: "[RequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Sku",
                schema: "md",
                table: "Product",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QcChecklistItem_TemplateId",
                schema: "qc",
                table: "QcChecklistItem",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspection_OperationId",
                schema: "qc",
                table: "QcInspection",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_QcResultItem_ChecklistItemId",
                schema: "qc",
                table: "QcResultItem",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QcResultItem_InspectionId",
                schema: "qc",
                table: "QcResultItem",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReasonCode_Category_Code",
                schema: "md",
                table: "ReasonCode",
                columns: new[] { "Category", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routing_ProductType_Version",
                schema: "md",
                table: "Routing",
                columns: new[] { "ProductType", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoutingStep_RoutingId_Sequence",
                schema: "md",
                table: "RoutingStep",
                columns: new[] { "RoutingId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoutingStep_StationId",
                schema: "md",
                table: "RoutingStep",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrder_Number",
                schema: "ord",
                table: "SalesOrder",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderLine_ProductId",
                schema: "ord",
                table: "SalesOrderLine",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderLine_SalesOrderId_LineNumber",
                schema: "ord",
                table: "SalesOrderLine",
                columns: new[] { "SalesOrderId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScrapRecord_OccurredAtUtc",
                schema: "mes",
                table: "ScrapRecord",
                column: "OccurredAtUtc")
                .Annotation("SqlServer:Include", new[] { "Quantity", "ReasonCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapRecord_OperationId",
                schema: "mes",
                table: "ScrapRecord",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapRecord_ReasonCodeId",
                schema: "mes",
                table: "ScrapRecord",
                column: "ReasonCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_SignalWord_Word",
                schema: "md",
                table: "SignalWord",
                column: "Word",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Station_Code",
                schema: "md",
                table: "Station",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_Number",
                schema: "ord",
                table: "WorkOrder",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_ProductId",
                schema: "ord",
                table: "WorkOrder",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_RoutingId",
                schema: "ord",
                table: "WorkOrder",
                column: "RoutingId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_SalesOrderLineId",
                schema: "ord",
                table: "WorkOrder",
                column: "SalesOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_Status_DueDate",
                schema: "ord",
                table: "WorkOrder",
                columns: new[] { "Status", "DueDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtworkProof",
                schema: "ord");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "AuditEntry",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ColorScheme",
                schema: "md");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "NumberSequence",
                schema: "ord");

            migrationBuilder.DropTable(
                name: "OperationEvent",
                schema: "mes");

            migrationBuilder.DropTable(
                name: "QcResultItem",
                schema: "qc");

            migrationBuilder.DropTable(
                name: "RoutingStep",
                schema: "md");

            migrationBuilder.DropTable(
                name: "ScrapRecord",
                schema: "mes");

            migrationBuilder.DropTable(
                name: "SignalWord",
                schema: "md");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "QcChecklistItem",
                schema: "qc");

            migrationBuilder.DropTable(
                name: "QcInspection",
                schema: "qc");

            migrationBuilder.DropTable(
                name: "ReasonCode",
                schema: "md");

            migrationBuilder.DropTable(
                name: "QcChecklistTemplate",
                schema: "qc");

            migrationBuilder.DropTable(
                name: "Operation",
                schema: "ord");

            migrationBuilder.DropTable(
                name: "Station",
                schema: "md");

            migrationBuilder.DropTable(
                name: "WorkOrder",
                schema: "ord");

            migrationBuilder.DropTable(
                name: "Routing",
                schema: "md");

            migrationBuilder.DropTable(
                name: "SalesOrderLine",
                schema: "ord");

            migrationBuilder.DropTable(
                name: "Product",
                schema: "md");

            migrationBuilder.DropTable(
                name: "SalesOrder",
                schema: "ord");
        }
    }
}
