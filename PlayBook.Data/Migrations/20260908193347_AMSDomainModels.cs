using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayBook.Data.Migrations
{
    /// <inheritdoc />
    public partial class AMSDomainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AutoGenratedProductId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationType",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlexPrice",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LongDescription",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ValidityDuration",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountManagerId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountManagerSnapshotSource",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AmsOrderStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutoGenratedId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactPersonId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertCurrencySymbol",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultCurrencySymbol",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateApplied",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpansionTypesSummary",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCustomProducts",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsContractionOpportunity",
                table: "Orders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpansionOpportunity",
                table: "Orders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOldPurchase",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPackageModified",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReactivation",
                table: "Orders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmountInDefaultCurrency",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OpportunityId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderAmountInDefaultCurrency",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderDiscountPercentage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OrderStatusInBool",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PackageId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmountInDefaultCurrency",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "OrderProducts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertCurrencySymbol",
                table: "OrderProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConvertedOpportunityId",
                table: "OrderProducts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "OrderProducts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DefaultCurrencySymbol",
                table: "OrderProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "OrderProducts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedRate",
                table: "OrderProducts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationType",
                table: "OrderProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExtendedGraceDays",
                table: "OrderProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConvertedRenewalToOpportunity",
                table: "OrderProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OrderStatus",
                table: "OrderProducts",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductTotalAmountInDefaultCurrency",
                table: "OrderProducts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RenewalDate",
                table: "OrderProducts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "OrderProducts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Term",
                table: "OrderProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValidityDuration",
                table: "OrderProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Opportunities",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountManagerId",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountManagerSnapshotSource",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutoGenratedId",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                table: "Opportunities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedStatus",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactPersonId",
                table: "Opportunities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertCurrencySymbol",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Opportunities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DefaultCurrencySymbol",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateApplied",
                table: "Opportunities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpansionTypesSummary",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedClosureDate",
                table: "Opportunities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "HasCustomProducts",
                table: "Opportunities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Opportunities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsContractionOpportunity",
                table: "Opportunities",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCreatedFromQbr",
                table: "Opportunities",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Opportunities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpansionOpportunity",
                table: "Opportunities",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLost",
                table: "Opportunities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "IsLostDate",
                table: "Opportunities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPackageModified",
                table: "Opportunities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReactivation",
                table: "Opportunities",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReferral",
                table: "Opportunities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRenewal",
                table: "Opportunities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "Opportunities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "Opportunities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmountInDefaultCurrency",
                table: "Opportunities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OpportunityAmount",
                table: "Opportunities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OpportunityAmountInDefaultCurrency",
                table: "Opportunities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpportunityName",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OverAllDiscountPercentage",
                table: "Opportunities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PackageId",
                table: "Opportunities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProposalId",
                table: "Opportunities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceQbrId",
                table: "Opportunities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Opportunities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmountInDefaultCurrency",
                table: "Opportunities",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountProfileImg",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccountSince",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutoGenrateAccountId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertCurrencySymbol",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultCurrencySymbol",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCount",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IncorporationDate",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "KeyAccount",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ParentAccountId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralAccountId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RegionId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredMobileNumber",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondMobileNumber",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AutoGenrateAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RegisteredMobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondMobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AccountProfileImg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IncorporationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountSince = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeCount = table.Column<int>(type: "int", nullable: true),
                    KeyAccount = table.Column<bool>(type: "bit", nullable: false),
                    ReferralAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralAccountContactsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountTypesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IndustryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefaultCurrencySymbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConvertCurrencySymbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountManagerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "AccountAddresses",
                columns: table => new
                {
                    AccountAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MapUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryAddress = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAddresses", x => x.AccountAddressId);
                    table.ForeignKey(
                        name: "FK_AccountAddresses_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountContacts",
                columns: table => new
                {
                    AccountContactsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedInProfile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountContacts", x => x.AccountContactsId);
                    table.ForeignKey(
                        name: "FK_AccountContacts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountAddressTypes",
                columns: table => new
                {
                    AccountAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAddressTypes", x => new { x.AccountAddressId, x.AddressType });
                    table.ForeignKey(
                        name: "FK_AccountAddressTypes_AccountAddresses_AccountAddressId",
                        column: x => x.AccountAddressId,
                        principalTable: "AccountAddresses",
                        principalColumn: "AccountAddressId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AccountId",
                table: "Orders",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ContactPersonId",
                table: "Orders",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OpportunityId",
                table: "Orders",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_AccountId",
                table: "OrderProducts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_ConvertedOpportunityId",
                table: "OrderProducts",
                column: "ConvertedOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_AccountId",
                table: "Opportunities",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_ContactPersonId",
                table: "Opportunities",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountAddresses_AccountId",
                table: "AccountAddresses",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountContacts_AccountId",
                table: "AccountContacts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountName",
                table: "Accounts",
                column: "AccountName");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_AccountContacts_ContactPersonId",
                table: "Opportunities",
                column: "ContactPersonId",
                principalTable: "AccountContacts",
                principalColumn: "AccountContactsId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProducts_Accounts_AccountId",
                table: "OrderProducts",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProducts_Opportunities_ConvertedOpportunityId",
                table: "OrderProducts",
                column: "ConvertedOpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AccountContacts_ContactPersonId",
                table: "Orders",
                column: "ContactPersonId",
                principalTable: "AccountContacts",
                principalColumn: "AccountContactsId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Accounts_AccountId",
                table: "Orders",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Opportunities_OpportunityId",
                table: "Orders",
                column: "OpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_AccountContacts_ContactPersonId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Accounts_AccountId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderProducts_Accounts_AccountId",
                table: "OrderProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderProducts_Opportunities_ConvertedOpportunityId",
                table: "OrderProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AccountContacts_ContactPersonId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Accounts_AccountId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Opportunities_OpportunityId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "AccountAddressTypes");

            migrationBuilder.DropTable(
                name: "AccountContacts");

            migrationBuilder.DropTable(
                name: "AccountAddresses");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AccountId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ContactPersonId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OpportunityId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderProducts_AccountId",
                table: "OrderProducts");

            migrationBuilder.DropIndex(
                name: "IX_OrderProducts_ConvertedOpportunityId",
                table: "OrderProducts");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_AccountId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_ContactPersonId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AutoGenratedProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DurationType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFlexPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LongDescription",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ValidityDuration",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AccountManagerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AccountManagerSnapshotSource",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AmsOrderStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AutoGenratedId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ContactPersonId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ConvertCurrencySymbol",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DefaultCurrencySymbol",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExchangeRateApplied",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExpansionTypesSummary",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HasCustomProducts",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsContractionOpportunity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsExpansionOpportunity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsOldPurchase",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsPackageModified",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsReactivation",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LastUpdatedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NetAmountInDefaultCurrency",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OpportunityId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderAmountInDefaultCurrency",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderDiscountPercentage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderStatusInBool",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PurchaseAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TaxAmountInDefaultCurrency",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "ConvertCurrencySymbol",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "ConvertedOpportunityId",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "DefaultCurrencySymbol",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "DiscountedRate",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "DurationType",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "ExtendedGraceDays",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "IsConvertedRenewalToOpportunity",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "OrderStatus",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "ProductTotalAmountInDefaultCurrency",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "RenewalDate",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "Term",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "ValidityDuration",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AccountManagerId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AccountManagerSnapshotSource",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AutoGenratedId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ClosedDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CompletedStatus",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ContactPersonId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ConvertCurrencySymbol",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DefaultCurrencySymbol",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExchangeRateApplied",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExpansionTypesSummary",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExpectedClosureDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "HasCustomProducts",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsContractionOpportunity",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsCreatedFromQbr",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsExpansionOpportunity",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsLost",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsLostDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsPackageModified",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsReactivation",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsReferral",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IsRenewal",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LastUpdatedDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "NetAmountInDefaultCurrency",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OpportunityAmount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OpportunityAmountInDefaultCurrency",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OpportunityName",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OverAllDiscountPercentage",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ProposalId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "SourceQbrId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "TaxAmountInDefaultCurrency",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AccountProfileImg",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AccountSince",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AutoGenrateAccountId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ConvertCurrencySymbol",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DefaultCurrencySymbol",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IncorporationDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "KeyAccount",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ParentAccountId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ReferralAccountId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RegisteredMobileNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SecondMobileNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Customers");
        }
    }
}
