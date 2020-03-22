using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;

namespace Cnf.Project.Employee.Entity
{
    /// <summary>
    /// 使用标签表达的人员信息，EmployeeID|Name|DutyID
    /// </summary>
    public class Staff
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int DutyID { get; set; }

        public static implicit operator Staff(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return default(Staff);
            var parts = label.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            return new Staff
            {
                EmployeeID = Convert.ToInt32(parts[0]),
                EmployeeName = parts[1],
                DutyID = parts.Length > 2 ? Convert.ToInt32(parts[2]) : default(int),
            };
        }
    }

    /// <summary>
    /// 表示经过分组后的一行汇总人员，每个字典项包含若干Staff
    /// </summary>
    public class StaffRow
    {
        /// <summary>
        /// 汇总的名称，例如：项目名称、单位名称
        /// </summary>
        /// <value></value>
        public string GroupName { get; set; }

        /// <summary>
        /// Key是经过PIVOT的某个维度的值列表。
        /// Value是这个维度值包括的若干人员。
        /// </summary>
        /// <value></value>
        public Dictionary<string, List<Staff>> Staffs { get; set; }

        /// <summary>
        /// dataRow 的第一列是GroupName的值，后面列是keys指定的Staff Label
        /// </summary>
        /// <param name="dataRow"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static StaffRow Create(DataRow dataRow, List<string> keys)
        {
            var staffRow = new StaffRow();
            staffRow.GroupName = Convert.ToString(dataRow[0]);
            staffRow.Staffs = new Dictionary<string, List<Staff>>();
            foreach (string key in keys)
            {
                var staffs = new List<Staff>(
                    Convert.ToString(dataRow[key])?.Split(
                        new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => (Staff)s)
                );
                staffRow.Staffs.Add(key, staffs);
            }
            return staffRow;
        }
    }

    public class GroupPivot
    {
        /// <summary>
        /// Keys是PIVOT后列值转为列标头的唯一值清单, 例如：所有职责、所有资格类型，等
        /// </summary>
        /// <value></value>
        public List<string> PivotValueKeys { get; set; }

        /// <summary>
        /// 汇总后的数据集合，每个元素代表一个汇总行：包括一个组名称，然后是一个Staff字典
        /// </summary>
        /// <value></value>
        public List<StaffRow> StaffRows { get; set; }

        /// <summary>
        /// 从DataTable隐式转换，必须是符合格式要求的记录集，要求如下：
        /// 1. 表列： 第一列是分组名称，如：“项目名称”、“单位名称”， 后面每一个列名称对应一个维度名称，如“职责名称“、资格类型；
        /// 2. 每一行第二列起：值要么是NULL， 要么是逗号（，）分割的字符串，每个分割节表示一个Staff
        /// 3. 每个分割节表示的Staff都是有ID、Name和InDutyID拼接成的，中间用竖线（|）分割。
        /// </summary>
        /// <param name="table"></param>
        public static implicit operator GroupPivot(DataTable table)
        {
            if (table == null)
                return null;

            var pivot = new GroupPivot();
            pivot.PivotValueKeys = new List<string>();
            for (var i = 1; i < table.Columns.Count; i++)
            {
                pivot.PivotValueKeys.Add(table.Columns[i].ColumnName);
            }

            pivot.StaffRows = new List<StaffRow>();

            foreach (DataRow dataRow in table.Rows)
            {
                StaffRow staffRow = StaffRow.Create(dataRow, pivot.PivotValueKeys);
                pivot.StaffRows.Add(staffRow);
            }
            return pivot;
        }
    }
}
