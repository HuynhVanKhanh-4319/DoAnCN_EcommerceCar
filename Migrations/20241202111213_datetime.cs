﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceCar.Migrations
{
    /// <inheritdoc />
    public partial class datetime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Bills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Bills",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Bills");
        }
    }
}
