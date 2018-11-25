using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace excel2json {
    /// <summary>
    /// 根据表头，生成C#类定义数据结构
    /// 表头使用三行定义：字段名称、字段类型、注释
    /// </summary>
    class CSDefineGenerator {
        struct FieldDef {
            public string name;
            public string type;
            public string comment;
        }

        string mCode;

        public string code {
            get {
                return this.mCode;
            }
        }

        public CSDefineGenerator(string excelName, DataTable sheet) {
            //-- First Row as Column Name
            if (sheet.Rows.Count < 2)
                return;

            List<FieldDef> m_fieldList = new List<FieldDef>();
            DataRow typeRow = sheet.Rows[0];
            DataRow commentRow = sheet.Rows[1];

            foreach (DataColumn column in sheet.Columns) {
                FieldDef field;
                field.name = column.ToString();
                field.type = typeRow[column].ToString();
                field.comment = commentRow[column].ToString();

                m_fieldList.Add(field);
            }

            //-- 创建代码字符串
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Auto Generated Code By excel2json");
            sb.AppendFormat("/// Generate From {0}.xlsx", excelName);
            sb.AppendLine();
            sb.AppendLine("/// </summary>");
            sb.AppendFormat("public class {0}ConfigVO\r\n{{", sheet.TableName);
            sb.AppendLine();

            for (int i = 0; i < m_fieldList.Count; i++) 
            {
                FieldDef field = m_fieldList[i];

                sb.AppendFormat("\t/// <summary>\n");
                sb.AppendFormat("\t/// {0}\n", field.comment);
                sb.AppendFormat("\t/// </summary>\n");
                sb.AppendFormat("\tpublic {0} {1};", field.type, field.name);
                sb.AppendLine();
                if (i < m_fieldList.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            sb.Append('}');
            sb.AppendLine();
            sb.AppendLine("// End of Auto Generated Code");

            mCode = sb.ToString();
        }

        public void SaveToFile(string filePath, Encoding encoding) {
            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                using (TextWriter writer = new StreamWriter(file, encoding))
                    writer.Write(mCode);
            }
        }
    }
}
