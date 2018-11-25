﻿using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace excel2json {
    class SQLExporter {
        string mStructSQL;
        string mContentSQL;

        public string structSQL {
            get {
                return mStructSQL;
            }
        }

        public string contentSQL {
            get {
                return mContentSQL;
            }
        }

        /// <summary>
        /// 初始化内部数据
        /// </summary>
        /// <param name="sheet">Excel读取的一个表单</param>
        /// <param name="headerRows">表头有几行</param>
        public SQLExporter(string excelName, DataTable sheet, int headerRows) {
            string tableName = sheet.TableName;

            //-- 转换成SQL语句
            mStructSQL = GetTabelStructSQL(sheet, tableName);
            mContentSQL = GetTableContentSQL(sheet, tableName, headerRows);
        }

        /// <summary>
        /// 转换成SQL字符串，并保存到指定的文件
        /// </summary>
        /// <param name="filePath">存盘文件</param>
        /// <param name="encoding">编码格式</param>
        public void SaveToFile(string filePath, Encoding encoding) {
            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                using (TextWriter writer = new StreamWriter(file, encoding)) {
                    writer.Write(mStructSQL);
                    writer.WriteLine();
                    writer.Write(mContentSQL);
                }
            }
        }

        /// <summary>
        /// 将表单内容转换成INSERT语句
        /// </summary>
        private string GetTableContentSQL(DataTable sheet, string tabelName, int headerRows) {
            StringBuilder sbContent = new StringBuilder();
            StringBuilder sbNames = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();

            //-- 字段名称列表
            foreach (DataColumn column in sheet.Columns) {
                sbNames.Append(column.ToString());
                sbNames.Append(", ");
            }

            //-- 逐行转换数据
            int firstDataRow = headerRows - 1;
            for (int i = firstDataRow; i < sheet.Rows.Count; i++) {
                DataRow row = sheet.Rows[i];
                sbValues.Clear();
                foreach (DataColumn column in sheet.Columns) {
                    if (sbValues.Length > 0)
                        sbValues.Append(", ");
                    sbValues.AppendFormat("'{0}'", row[column].ToString());
                }

#if false
                sbContent.AppendFormat("INSERT INTO `{0}`({1}) VALUES({2});\n",
                    tabelName, sbNames.ToString(), sbValues.ToString());
#else
                sbContent.AppendFormat("INSERT INTO `{0}` VALUES({1});\n",
                    tabelName, sbValues.ToString());
#endif

            }

            return sbContent.ToString();
        }

        /// <summary>
        /// 根据表头构造CREATE TABLE语句
        /// </summary>
        private string GetTabelStructSQL(DataTable sheet, string tabelName) {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("DROP TABLE IF EXISTS `{0}`;\n", tabelName);
            sb.AppendFormat("CREATE TABLE `{0}` (\n", tabelName);

            DataRow typeRow = sheet.Rows[0];

            foreach (DataColumn column in sheet.Columns) {
                string filedName = column.ToString();
                string filedType = typeRow[column].ToString();

                if (filedType == "varchar")
                    sb.AppendFormat("`{0}` {1}(64),", filedName, filedType);
                else if (filedType == "text")
                    sb.AppendFormat("`{0}` {1}(256),", filedName, filedType);
                else
                    sb.AppendFormat("`{0}` {1},", filedName, filedType);
            }

            sb.AppendFormat("PRIMARY KEY (`{0}`) ", sheet.Columns[0].ToString());
            sb.AppendLine("\n) DEFAULT CHARSET=utf8;");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
