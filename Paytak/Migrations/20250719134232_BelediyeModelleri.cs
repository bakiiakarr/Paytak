using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Paytak.Migrations
{
    /// <inheritdoc />
    public partial class BelediyeModelleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BelediyeRaporlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RaporTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RaporTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DosyaYolu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notlar = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BelediyeRaporlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BelediyeRaporlar_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VatandasTalepleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TalepKonusu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TalepDetayi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TalepTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Oncelik = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TalepTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Yanit = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IslemYapanUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatandasTalepleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VatandasTalepleri_AspNetUsers_IslemYapanUserId",
                        column: x => x.IslemYapanUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BelediyeRaporlar_UserId",
                table: "BelediyeRaporlar",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VatandasTalepleri_IslemYapanUserId",
                table: "VatandasTalepleri",
                column: "IslemYapanUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BelediyeRaporlar");

            migrationBuilder.DropTable(
                name: "VatandasTalepleri");
        }
    }
}
