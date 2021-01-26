using CsvHelper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SFA.DAS.EmployerIncentives.ManualPaymentsTemplate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter path to CSV containing query results:");
            var csvPath = Console.ReadLine();
            Console.WriteLine("Enter path to template EUPS spreadsheet:");
            var xlsPath = Console.ReadLine();

            var paymentRecords = LoadCsvFile(csvPath);
            var templateSpreadsheet = LoadWorkbook(xlsPath);

            var updatedSpreadsheet = PopulateSpreadsheet(templateSpreadsheet, paymentRecords);
            Console.WriteLine("Enter path to save updated EUPS spreadsheet:");
            var savePath = Console.ReadLine();
            updatedSpreadsheet.SaveAs(new FileInfo(savePath));
        }

        private static List<PaymentRecord> LoadCsvFile(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                var paymentRecords = new List<PaymentRecord>();

                using (var csvReader = new CsvReader(reader, CultureInfo.CurrentCulture))
                {
                    while (csvReader.Read())
                    {
                        var record = new PaymentRecord
                        {
                            DocumentType = csvReader.GetField(0),
                            AccountNumber = csvReader.GetField(1),
                            FundingTypeCode = csvReader.GetField(2),
                            Values = Convert.ToDecimal(csvReader.GetField(3)),
                            PostingDate = Convert.ToDateTime(csvReader.GetField(4)),
                            PaymentDate = Convert.ToDateTime(csvReader.GetField(5)),
                            GLAccountCode = Convert.ToInt64(csvReader.GetField(6)),
                            ExtRef4 = csvReader.GetField(7),
                            CostCentreCodeDimension2 = csvReader.GetField(8),
                            ExtRef3Submitter = csvReader.GetField(9),
                            RemittanceDescription = csvReader.GetField(10)
                        };
                        paymentRecords.Add(record);
                    }
                }

                return paymentRecords;
            }
        }

        private static ExcelPackage LoadWorkbook(string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage(new FileInfo(fileName));
        }

        private static ExcelPackage PopulateSpreadsheet(ExcelPackage excelPackage, List<PaymentRecord> paymentRecords)
        {
            var templateSheet = excelPackage.Workbook.Worksheets["Template"];

            var row = 10;

            foreach(var paymentRecord in paymentRecords)
            {
                templateSheet.Cells[$"C{row}"].Value = paymentRecord.DocumentType;
                templateSheet.Cells[$"D{row}"].Value = paymentRecord.AccountNumber;
                templateSheet.Cells[$"I{row}"].Value = paymentRecord.FundingTypeCode;
                templateSheet.Cells[$"J{row}"].Value = paymentRecord.Values;
                templateSheet.Cells[$"K{row}"].Value = paymentRecord.PostingDate.ToString("dd/MM/yyyy");
                templateSheet.Cells[$"P{row}"].Value = paymentRecord.PaymentDate.ToString("dd/MM/yyyy");
                templateSheet.Cells[$"Q{row}"].Value = paymentRecord.GLAccountCode;
                templateSheet.Cells[$"T{row}"].Value = paymentRecord.ExtRef4;
                templateSheet.Cells[$"U{row}"].Value = paymentRecord.CostCentreCodeDimension2;
                templateSheet.Cells[$"Z{row}"].Value = paymentRecord.ExtRef3Submitter;
                templateSheet.Cells[$"AB{row}"].Value = paymentRecord.RemittanceDescription;
                
                row++;
            }
            return excelPackage;
        }
    }
}
