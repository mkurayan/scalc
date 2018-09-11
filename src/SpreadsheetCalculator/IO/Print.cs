﻿using System.Collections.Generic;
using SpreadsheetCalculator.Spreadsheet;
using SpreadsheetCalculator.Utils;
using System.Linq;
using System.Text;
using System;

namespace SpreadsheetCalculator.IO
{
    internal static class Print
    {
        public static string PrintSpreadsheet(IViewSpreadsheet spreadsheet)
        {
            var view = new VirtualSpreadsheet(spreadsheet);

            return PrintContent(view);
        }

        private static string PrintContent(IViewSpreadsheet spreadsheet)
        {
            var columnsLength =
              Enumerable.Range(1, spreadsheet.ColumnsCount)
                  .Select(columnIndex =>
                      GetColumn(spreadsheet, columnIndex).Max(value => value.Length)
                  ).ToArray();

            // create the string format with padding
            var format = Enumerable.Range(0, spreadsheet.ColumnsCount)
                .Select(i => " | {" + i + ",-" + columnsLength[i] + "}")
                .Aggregate((s, a) => s + a) + " |" + Environment.NewLine;

            // create the divider            
            var divider = " " + new string('-', columnsLength.Sum() + columnsLength.Length * 3 + 1) + " ";

            var builder = new StringBuilder();

            builder.AppendLine(divider);

            for (var rowIndex = 1; rowIndex <= spreadsheet.RowsCount; rowIndex++)
            {
                // ReSharper disable once CoVariantArrayConversion
                object[] row = GetRow(spreadsheet, rowIndex).ToArray();
                
                builder.AppendFormat(format, row);
                builder.AppendLine(divider);
            }

            return builder.ToString();
        }

        private static IEnumerable<string> GetRow(IViewSpreadsheet spreadsheet, int rowIndex)
        {
            return Enumerable
                .Range(1, spreadsheet.ColumnsCount)
                .Select(columnIndex => spreadsheet.GetValue(columnIndex, rowIndex));

        }

        private static IEnumerable<string> GetColumn(IViewSpreadsheet spreadsheet, int columnIndex)
        {
            return Enumerable
                .Range(1, spreadsheet.RowsCount)
                .Select(rowIndex => spreadsheet.GetValue(columnIndex, rowIndex));
        }

        private class VirtualSpreadsheet : IViewSpreadsheet
        {
            private IViewSpreadsheet View { get; }

            public int ColumnsCount => View.ColumnsCount + 1;

            public int RowsCount => View.RowsCount + 1;

            public VirtualSpreadsheet(IViewSpreadsheet view)
            {
                View = view;
            }

            public string GetValue(int column, int row)
            {
                if (column == 1 && row == 1)
                {
                    return "/";
                }

                if (row == 1)
                {
                    return AlphabetConverter.IntToLetters(column - 1);
                }

                if (column == 1)
                {
                    return (row - 1).ToString();
                }

                return View.GetValue(column - 1, row - 1);
            }
        }
    }
}
