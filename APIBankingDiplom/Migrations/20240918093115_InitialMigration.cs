using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIBankingDiplom.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    MoneyEncrypt = table.Column<string>(type: "nvarchar(82)", maxLength: 82, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfilePicture = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    NameEncrypt = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(72)", maxLength: 72, nullable: false),
                    EmailEncrypt = table.Column<string>(type: "nvarchar(319)", maxLength: 319, nullable: false),
                    EmailHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    PhoneNumberEncrypt = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PhoneNumberHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    PINEncrypt = table.Column<string>(type: "nvarchar(67)", maxLength: 67, nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsInvalid = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardNumberHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    CardNumberEncrypt = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CardType = table.Column<int>(type: "int", nullable: false),
                    ExpireDateHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    ExpireDateEncrypt = table.Column<string>(type: "nvarchar(74)", maxLength: 74, nullable: false),
                    CVVEncrypt = table.Column<string>(type: "nvarchar(67)", maxLength: 67, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailChangeTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailEncrypt = table.Column<string>(type: "nvarchar(319)", maxLength: 319, nullable: false),
                    EmailHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsInvalid = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailChangeTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailChangeTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsInvalid = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionTypeEncrypt = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    ActionDescriptionEncrypt = table.Column<string>(type: "nvarchar(576)", maxLength: 576, nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeEncrypt = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    MessageEncrypt = table.Column<string>(type: "nvarchar(576)", maxLength: 576, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    TimeSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsInvalid = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsInvalid = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CardBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ATM_DepositedToday = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    ATM_DepositLimit = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    ATM_WithdrawnToday = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    ATM_WithdrawalLimit = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    TransactedToday = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    TransactionLimit = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    LastDepositDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastWithdrawalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastTransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardBalances_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Commissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AmountEncrypt = table.Column<string>(type: "nvarchar(82)", maxLength: 82, nullable: false),
                    CommissionPercentage = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    IsSentToBankAccount = table.Column<bool>(type: "bit", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commissions_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ATMOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Money = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    ATMOperationType = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    CardBalanceId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATMOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ATMOperations_CardBalances_CardBalanceId",
                        column: x => x.CardBalanceId,
                        principalTable: "CardBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ATMOperations_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionTypeEncrypt = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    AmountSentEncrypt = table.Column<string>(type: "nvarchar(82)", maxLength: 82, nullable: false),
                    AmountReceivedEncrypt = table.Column<string>(type: "nvarchar(82)", maxLength: 82, nullable: false),
                    SenderCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ReceiverCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DescriptionEncrypt = table.Column<string>(type: "nvarchar(576)", maxLength: 576, nullable: false),
                    SenderCardId = table.Column<int>(type: "int", nullable: false),
                    SenderBalanceId = table.Column<int>(type: "int", nullable: false),
                    ReceiverCardId = table.Column<int>(type: "int", nullable: false),
                    ReceiverBalanceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_CardBalances_ReceiverBalanceId",
                        column: x => x.ReceiverBalanceId,
                        principalTable: "CardBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_CardBalances_SenderBalanceId",
                        column: x => x.SenderBalanceId,
                        principalTable: "CardBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Cards_ReceiverCardId",
                        column: x => x.ReceiverCardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Cards_SenderCardId",
                        column: x => x.SenderCardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessTokens_UserId",
                table: "AccessTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ATMOperations_CardBalanceId",
                table: "ATMOperations",
                column: "CardBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ATMOperations_CardId",
                table: "ATMOperations",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardBalances_CardId",
                table: "CardBalances",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_UserId",
                table: "Cards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_CardId",
                table: "Commissions",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailChangeTokens_UserId",
                table: "EmailChangeTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTokens_UserId",
                table: "EmailTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserId",
                table: "Logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_UserId",
                table: "Settings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReceiverBalanceId",
                table: "Transactions",
                column: "ReceiverBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReceiverCardId",
                table: "Transactions",
                column: "ReceiverCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SenderBalanceId",
                table: "Transactions",
                column: "SenderBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SenderCardId",
                table: "Transactions",
                column: "SenderCardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessTokens");

            migrationBuilder.DropTable(
                name: "ATMOperations");

            migrationBuilder.DropTable(
                name: "BankBalances");

            migrationBuilder.DropTable(
                name: "Commissions");

            migrationBuilder.DropTable(
                name: "EmailChangeTokens");

            migrationBuilder.DropTable(
                name: "EmailTokens");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "CardBalances");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
