using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Web.Models
{
    public class FileUploadViewModel
    {
        public bool ShowResult { get; set; }

        public string ResultMessage { get; set; }

        [Display(Name = "项目文件")]
        public IFormFile ProjectFile { get; set; }

        [Display(Name = "人员文件")]
        public IFormFile StaffFile { get; set; }
    }

    public enum PropertyType
    {
        Text,
        Int,
        Real,
        DateTime,
        Boolean
    }

    public class PropertyMap
    {
        public string PropertyName { get; set; }

        public PropertyType PropertyType { get; set; }

        /// <summary>
        /// 如果文件定义为有Header，该属性为列表头，否则为0开始的索引
        /// </summary>
        /// <value></value>
        public String ColumnIndex { get; set; }
    }

    public class ExcelMap
    {
        /// <summary>
        /// 完整的实体类名（包括完整的命名空间）
        /// </summary>
        /// <value></value>
        public string EntityName { get; set; }

        /// <summary>
        /// 从0开始的工作表索引
        /// </summary>
        /// <value></value>
        public int SheetIndex { get; set; }

        /// <summary>
        /// 文件是否有标题行，
        /// true: 第一行为标题行，使用标题行的列标头作为列索引，导入数据从第二行开始。
        /// false: 没有标题行，使用0开始的整数作为列索引，导入数据从第一行开始。
        /// </summary>
        /// <value></value>
        public bool WithHeader { get; set; }

        /// <summary>
        /// 每个公共可读写属性的定义
        /// </summary>
        /// <value></value>
        public IEnumerable<PropertyMap> Properties { get; set; }

        /// <summary>
        /// 决定使用哪些属性作为唯一键定位记录（增删改）
        /// </summary>
        /// <value></value>
        public IEnumerable<string> Keys { get; set; }

        public bool IsTypeOf(Type entityType) =>
            entityType.FullName.Equals(EntityName, StringComparison.OrdinalIgnoreCase);
    }

    public static class FileUploadHelper
    {
        public static PropertyMap MapProperty(PropertyInfo property, int? index = default(int?)) =>
            new PropertyMap
            {
                PropertyName = property.Name,
                PropertyType = MapSystemType(property.PropertyType),
                ColumnIndex = index == null ? property.Name : index.Value.ToString()
            };

        public static ExcelMap MapEntity(Type entityType, IEnumerable<string> keys) =>
            new ExcelMap
            {
                EntityName = entityType.FullName,
                SheetIndex = 0,
                WithHeader = true,
                Properties = (from p in entityType.GetProperties()
                              select MapProperty(p)).ToArray(),
                Keys = keys
            };

        static void SetProjectPropertyCell(ICell cell, PropertyMap col, ICellStyle dateStyle)
        {
            switch (col.PropertyType)
            {
                case PropertyType.Text:
                    if (col.PropertyName == nameof(Entity.Project.FullName))
                        cell.SetCellValue("(这是示范说明行，切记导入前删除此行)");
                    else if (col.PropertyName == nameof(Entity.Project.ShortName))
                        cell.SetCellValue("简称不要超过10个字");
                    else
                        cell.SetCellValue("一些文本");
                    break;
                case PropertyType.Real:
                    if (col.PropertyName == nameof(Entity.Project.ContractAmount))
                        cell.SetCellValue("以万元为单位");
                    break;
                case PropertyType.Int:
                    if (col.PropertyName == nameof(Entity.Project.Status))
                        cell.SetCellValue("输入数字：0-准备中;1-进行中;2-已竣工;3-已关闭");
                    break;
                case PropertyType.DateTime:
                    cell.CellStyle = dateStyle;
                    cell.SetCellValue(DateTime.Today.Date);
                    break;
                case PropertyType.Boolean:
                    cell.SetCellValue(true);
                    break;
                default:
                    break;
            }
            cell.Sheet.AutoSizeColumn(cell.ColumnIndex);
        }

        static void SetStaffPropertyCell(ICell cell, PropertyMap col, ICellStyle dateStyle)
        {
            switch (col.PropertyType)
            {
                case PropertyType.Text:
                    if (col.PropertyName == nameof(Entity.Employee.Name))
                        cell.SetCellValue("(这是示范说明行，切记导入前删除此行)");
                    else if (col.PropertyName == nameof(Entity.Employee.IdNumber))
                        cell.SetCellValue("注意正确的位数");
                    else
                        cell.SetCellValue("一些文本");
                    break;
                case PropertyType.Real:
                    break;
                case PropertyType.Int:
                    if (col.PropertyName == nameof(Entity.Employee.OrganizationID))
                        cell.SetCellValue("必须输入数字ID（请从单位列表中查看）");
                    else if (col.PropertyName == nameof(Entity.Employee.SpecialtyID))
                        cell.SetCellValue("必须输入数字ID（请从专业列表中查看）");
                    break;
                case PropertyType.DateTime:
                    cell.CellStyle = dateStyle;
                    cell.SetCellValue(DateTime.Today.Date);
                    break;
                case PropertyType.Boolean:
                    cell.SetCellValue(true);
                    break;
                default:
                    break;
            }
        }

        public static void WriteExcelTemplate(Stream stream, ExcelMap map)
        {
            IWorkbook workbook = new XSSFWorkbook();
            IDataFormat dataformat = workbook.CreateDataFormat();
            ICellStyle dateStyle = workbook.CreateCellStyle();
            dateStyle.DataFormat = dataformat.GetFormat("yyyy-MM-dd");

            //【Tips】  
            // 1.yyyy 年份；    yy 年份后两位  
            // 2.MM 月份零起始；M 月份非零起始;  mmm[英文月份简写];mmmm[英文月份全称]  
            // 3.dd   日零起始；d 日非零起始  
            // 4.hh 小时零起始；h 小时非零起始[用于12小时制][12小时制必须在时间后面添加 AM/PM 或 上午/下午]  
            // 5.HH 小时零起始；H 小时非零起始[用于24小时制]  
            // 6.mm 分钟零起始；m 分钟非零起始  
            // 7.ss 秒数零起始；s 秒数非零起始  
            // 8.dddd 星期；ddd 星期缩写【英文】  
            // 9.aaaa 星期；aaa 星期缩写【中文】  

            ISheet sheet = null;
            for (int i = 0; i <= map.SheetIndex; i++)
            {
                sheet = workbook.CreateSheet($"Sheet{i + 1}");
            }
            IRow header = null, data = null;
            if (map.WithHeader)
            {
                header = sheet.CreateRow(0);
                data = sheet.CreateRow(1);
            }
            else
            {
                data = sheet.CreateRow(0);
            }
            int j = 0;
            foreach (var col in map.Properties)
            {
                if (header != null)
                {
                    header.CreateCell(j).SetCellValue(col.ColumnIndex);
                    if (map.IsTypeOf(typeof(Entity.Project)))
                        SetProjectPropertyCell(data.CreateCell(j), col, dateStyle);
                    else if (map.IsTypeOf(typeof(Entity.Employee)))
                        SetStaffPropertyCell(data.CreateCell(j), col, dateStyle);
                    else throw new Exception("not supported importing type.");
                }
                else
                {
                    if (map.IsTypeOf(typeof(Entity.Project)))
                        SetProjectPropertyCell(data.CreateCell(
                            Convert.ToInt32(col.ColumnIndex)), col, dateStyle);
                    else if (map.IsTypeOf(typeof(Entity.Employee)))
                        SetStaffPropertyCell(data.CreateCell(
                            Convert.ToInt32(col.ColumnIndex)), col, dateStyle);
                    else throw new Exception("not supported importing type.");
                }
                j++;
            }
            workbook.Write(stream);
        }

        public static async Task<string> Upload(Stream stream, ExcelMap map,
                IEmployeeService employeeService = null,
                IProjectService projectService = null)
        {
            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(map.SheetIndex);
            if (sheet == null)
                throw new Exception($"上传的文件没有索引为{map.SheetIndex}的工作表");
            IDictionary<string, int> Indecies = new Dictionary<string, int>();
            int startRowIndex;
            if (map.WithHeader)
            {
                IRow header = sheet.GetRow(0);
                if (header == null)
                    throw new Exception($"上传的文件第一行必须是符合格式的标题行");
                foreach (var caption in header.Cells)
                    Indecies.Add(caption.StringCellValue, caption.ColumnIndex);
                startRowIndex = 1;
            }
            else
            {
                IRow firstRow = sheet.GetRow(0);
                if (firstRow == null)
                    throw new Exception("上传文件没有数据，必须从第一行开始有数据");
                for (var i = 0; i <= firstRow.LastCellNum; i++)
                    Indecies.Add(i.ToString(), i);
                startRowIndex = 0;
            }
            //由于现在仅支持项目和人员导入，因此直接使用一个布尔值来区别类型。
            var projectType = typeof(Entity.Project);
            var staffType = typeof(Entity.Employee);
            bool isProject;
            if (map.IsTypeOf(projectType))
                isProject = true;
            else if (map.IsTypeOf(staffType))
                isProject = false;
            else throw new Exception("not supported upload entity type");

            int total = 0, success = 0, failure = 0;
            StringBuilder messageBuilder = new StringBuilder();
            for (var rowIndex = startRowIndex; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                total++;
                IRow data = sheet.GetRow(rowIndex);
                object entity;
                if (isProject)
                    entity = new Entity.Project
                    {
                        ActiveStatus = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = Cnf.CodeBase.Serialize.ValueHelper.DBKEY_NULL
                    };
                else
                    entity = new Entity.Employee
                    {
                        ActiveStatus = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = Cnf.CodeBase.Serialize.ValueHelper.DBKEY_NULL,
                        InDate = Cnf.CodeBase.Serialize.ValueHelper.DbDate_Null,
                        InDutyID = Cnf.CodeBase.Serialize.ValueHelper.DBKEY_NULL,
                        InProjectID = Cnf.CodeBase.Serialize.ValueHelper.DBKEY_NULL
                    };

                try
                {
                    foreach (var propMap in map.Properties)
                    {
                        if (!Indecies.ContainsKey(propMap.ColumnIndex))
                            throw new Exception($"上传的文件无法定位到列索引[{propMap.ColumnIndex}]");
                        object cellValue;

                        switch (propMap.PropertyType)
                        {
                            case PropertyType.Boolean:
                                cellValue = data.GetCell(Indecies[propMap.ColumnIndex]).BooleanCellValue;
                                break;
                            case PropertyType.DateTime:
                                cellValue = data.GetCell(Indecies[propMap.ColumnIndex]).DateCellValue;
                                break;
                            case PropertyType.Int:
                                cellValue = Convert.ToInt32(data.GetCell(Indecies[propMap.ColumnIndex]).NumericCellValue);
                                break;
                            case PropertyType.Real:
                                cellValue = data.GetCell(Indecies[propMap.ColumnIndex]).NumericCellValue;
                                break;
                            case PropertyType.Text:
                                cellValue = data.GetCell(Indecies[propMap.ColumnIndex]).StringCellValue;
                                break;
                            default:
                                throw new Exception("not supported property data type");
                        }
                        PropertyInfo propertyInfo = isProject ?
                            projectType.GetProperty(propMap.PropertyName,
                                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) :
                            staffType.GetProperty(propMap.PropertyName,
                                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (propertyInfo == null)
                            throw new Exception($"Excel映射配置的属性{propMap.PropertyName}不是实体类的成员");
                        propertyInfo.SetValue(entity, cellValue);
                    }
                }
                catch (Exception ex)
                {
                    messageBuilder.AppendLine($"行{rowIndex}读文件错：{ex.Message}<br/>");
                    failure++;
                    continue;  //直接开始处理下一行。
                }
                //保存到数据库
                try
                {
                    if (isProject)
                    {
                        await projectService.SaveProject((Entity.Project)entity);
                    }
                    else
                    {
                        await employeeService.SaveEmployee((Entity.Employee)entity);
                    }
                    success++;
                }
                catch (Exception ex)
                {
                    messageBuilder.AppendLine($"行{rowIndex}写数据库错：{ex.Message}<br/>");
                    failure++;
                }
            }
            messageBuilder.AppendLine($"共处理了{total}行，导入{success}行，错误{failure}行");
            return messageBuilder.ToString();
        }

        /// <summary>
        /// 把一个系统类型映射为一个DataType枚举，系统类型必须是基础类型，其中枚举将被映射成int，
        /// 泛型类被映射成它的基础类型，如int?映射成int。
        /// </summary>
        public static PropertyType MapSystemType(Type systemType)
        {
            string name = "";

            if (systemType.IsValueType && systemType.IsGenericType)
            {
                //consider it is a Nullable value type
                Type t = systemType.GetGenericTypeDefinition();
                if (t.FullName == "System.Nullable`1")
                {
                    Type[] bts = systemType.GetGenericArguments();
                    name = bts[0].FullName;
                }
            }
            else
                name = systemType.FullName;

            switch (name)
            {
                case "System.Boolean":
                    return PropertyType.Boolean;
                case "System.SByte":
                case "System.Byte":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.UInt16":
                case "System.UInt32":
                case "System.UInt64":
                    return PropertyType.Int;
                case "System.Decimal":
                case "System.Double":
                case "System.Single":
                    return PropertyType.Real;
                case "System.String":
                case "System.Char":
                case "System.Guid":
                    return PropertyType.Text;
                case "System.DateTime":
                    return PropertyType.DateTime;
                default:
                    if (systemType.IsEnum == true)
                        return PropertyType.Int;
                    else
                        return PropertyType.Text;
            }
        }

    }
}
