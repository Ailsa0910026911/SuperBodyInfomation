using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using LokFu.Repositories.Extensions;

namespace LokFu
{
    public class ExcelExport
    {
        /// <summary>
        /// DataTable导出到Excel的MemoryStream
        /// </summary>
        /// <param name="dtSource">源DataTable</param>  
        /// <param name="strHeaderText">表头文本</param>  
        /// <returns></returns>
        public MemoryStream ExportExcelFromDataTable(DataTable dtSource, string strHeaderText = null)
        {
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet();

            var dateStyle = workbook.CreateCellStyle();
            var format = workbook.CreateDataFormat();
            dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");

            //取得列宽  
            var arrColWidth = new int[dtSource.Columns.Count];
            foreach (DataColumn item in dtSource.Columns)
            {
                arrColWidth[item.Ordinal] = Encoding.GetEncoding(936).GetBytes(item.ColumnName.ToString()).Length;
            }
            for (int i = 0; i < dtSource.Rows.Count; i++)
            {
                for (int j = 0; j < dtSource.Columns.Count; j++)
                {
                    int intTemp = Encoding.GetEncoding(936).GetBytes(dtSource.Rows[i][j].ToString()).Length;
                    if (intTemp > arrColWidth[j])
                    {
                        arrColWidth[j] = intTemp;
                    }
                }
            }

            int rowIndex = 0;
            foreach (DataRow row in dtSource.Rows)
            {
                #region 新建表，填充表头，填充列头，样式
                if (rowIndex == 65535 || rowIndex == 0)
                {
                    if (rowIndex != 0)
                    {
                        sheet = workbook.CreateSheet();
                    }

                    #region 表头及样式
                    {
                        var headerRow = sheet.CreateRow(0);
                        headerRow.HeightInPoints = 25;
                        headerRow.CreateCell(0).SetCellValue(strHeaderText);

                        var headStyle = workbook.CreateCellStyle();
                        headStyle.Alignment = HorizontalAlignment.Center;
                        var font = workbook.CreateFont();
                        font.FontHeightInPoints = 20;
                        font.Boldweight = 700;
                        headStyle.SetFont(font);

                        headerRow.GetCell(0).CellStyle = headStyle;

                        sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dtSource.Columns.Count - 1));
                        //headerRow.Dispose();  
                    }
                    #endregion


                    #region 列头及样式
                    {
                        var headerRow = sheet.CreateRow(1);


                        var headStyle = workbook.CreateCellStyle();
                        headStyle.Alignment = HorizontalAlignment.Center;
                        var font = workbook.CreateFont();
                        font.FontHeightInPoints = 10;
                        font.Boldweight = 700;
                        headStyle.SetFont(font);


                        foreach (DataColumn column in dtSource.Columns)
                        {
                            headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                            headerRow.GetCell(column.Ordinal).CellStyle = headStyle;

                            //设置列宽  
                            try
                            {
                                sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);
                            }
                            catch { }

                        }
                        //headerRow.Dispose();  
                    }
                    #endregion

                    rowIndex = 2;
                }
                #endregion

                #region 填充内容
                var dataRow = sheet.CreateRow(rowIndex);

                foreach (DataColumn column in dtSource.Columns)
                {
                    var newCell = dataRow.CreateCell(column.Ordinal);

                    var drValue = row[column].ToString();

                    switch (column.DataType.ToString())
                    {
                        case "System.String"://字符串类型  
                            newCell.SetCellValue(drValue);
                            break;
                        case "System.DateTime"://日期类型  
                            DateTime dateV;
                            DateTime.TryParse(drValue, out dateV);
                            newCell.SetCellValue(dateV);

                            newCell.CellStyle = dateStyle;//格式化显示  
                            break;
                        case "System.Boolean"://布尔型  
                            bool boolV = false;
                            bool.TryParse(drValue, out boolV);
                            newCell.SetCellValue(boolV);
                            break;
                        case "System.Int16"://整型  
                        case "System.Int32":
                        case "System.Int64":
                        case "System.Byte":
                            int intV = 0;
                            int.TryParse(drValue, out intV);
                            newCell.SetCellValue(intV);
                            break;
                        case "System.Decimal"://浮点型  
                        case "System.Double":
                            double doubV = 0;
                            double.TryParse(drValue, out doubV);
                            newCell.SetCellValue(doubV);
                            break;
                        case "System.DBNull"://空值处理  
                            newCell.SetCellValue("");
                            break;
                        default:
                            newCell.SetCellValue("");
                            break;
                    }

                }
                #endregion

                rowIndex++;
            }

            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                workbook.Close();
                return ms;
            }
        }

        /// <summary>
        /// 取得Workbook,按文件路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public HSSFWorkbook GetWorkbook(string filePath)
        {
            HSSFWorkbook workbook = null;
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //把xls文件读入workbook变量里，之后就可以关闭了  
                workbook = new HSSFWorkbook(fs);
                fs.Close();
            }
            return workbook;
        }

        public MemoryStream ToMemoryStream(HSSFWorkbook workbook)
        {
            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                workbook.Close();
                return ms;
            }
        }

        /// <summary>
        /// 预设格式
        /// </summary>
        /// <param name="CellStyle"></param>
        /// <returns></returns>
        public ICellStyle CellStyle(HSSFWorkbook workbook)
        {
            ICellStyle CellStyle = workbook.CreateCellStyle();

            CellStyle.BorderBottom = BorderStyle.Hair;
            CellStyle.BorderLeft = BorderStyle.Hair;
            CellStyle.BorderRight = BorderStyle.Hair;
            CellStyle.BorderTop = BorderStyle.Hair;
            CellStyle.Alignment = HorizontalAlignment.Center;

            var font = workbook.CreateFont();
            font.FontHeightInPoints = 10;
            font.Boldweight = 700;
            CellStyle.SetFont(font);

            return CellStyle;
        }

        /// <summary>
        /// 按类型赋值
        /// </summary>
        /// <param name="Cell"></param>
        /// <param name="DateType"></param>
        /// <param name="Value"></param>
        public ICell Assign(ICell Cell, string DateType, string Value)
        {
            switch (DateType)
            {
                case "System.String"://字符串类型  
                    Cell.SetCellValue(Value);
                    break;
                case "System.DateTime"://日期类型  
                    DateTime dateV;
                    DateTime.TryParse(Value, out dateV);
                    Cell.SetCellValue(dateV);
                    break;
                case "System.Boolean"://布尔型  
                    bool boolV = false;
                    bool.TryParse(Value, out boolV);
                    Cell.SetCellValue(boolV);
                    break;
                case "System.Int16"://整型  
                case "System.Int32":
                case "System.Int64":
                case "System.Byte":
                    int intV = 0;
                    int.TryParse(Value, out intV);
                    Cell.SetCellValue(intV);
                    break;
                case "System.Decimal"://浮点型  
                case "System.Double":
                    double doubV = 0;
                    double.TryParse(Value, out doubV);
                    Cell.SetCellValue(doubV);
                    break;
                case "System.DBNull"://空值处理  
                    Cell.SetCellValue("");
                    break;
                default:
                    Cell.SetCellValue("");
                    break;
            }
            return Cell;
        }

    }
}