using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText;
using Spire.Pdf;

namespace PastQuestion
{
    public class ProcessData
    {

        public static void Process(string folderPath, string saveFilePath)
        {
            string source = string.Empty;
            string _fileName = string.Empty;
            string errorDestFile = saveFilePath + "\\Error";
            string fileErrorDest = saveFilePath + "\\FileError";
            try
            {
                string[] files = Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly);
                foreach (string s in files)
                {
                    try
                    {
                        //test
                        source = s;
                        PdfDocument document = new PdfDocument();
                        document.LoadFromFile(s);
                        _fileName = Path.GetFileName(s);
                        _fileName = string.Join(" ", _fileName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                        StringBuilder content = new StringBuilder();
                        content.Append(document.Pages[0].ExtractText());
                        string data = content.ToString().Trim();

                        var datas = data.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        string[] dataArr = datas.Where(a => a.Trim() != "").ToArray();
                        string academicYear = GetLastN(dataArr[2].Trim().ToString(), 4);
                        string courseCode = dataArr[3].ToString();//GetLastN(dataArr[3].Trim().Trim().Replace(" ",string.Empty).Replace(":",string.Empty).ToString(),5);
                        string courseName = dataArr[4].Replace("COURSE NAME", string.Empty).Replace(":", string.Empty).Trim().ToString();
                        courseCode = string.Join(" ", courseCode.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                        if (!IsMultipleClass(courseCode))
                        {
                            ProcessSingleClass(saveFilePath, source, _fileName, fileErrorDest, s, dataArr, academicYear, courseName);
                        }
                        else
                        {
                            bool deleteFile = false;
                            string[] nameArr;
                            nameArr = courseCode.Replace("COURSE NO", string.Empty).Replace(":", string.Empty).Trim().Split('/');
                            if (nameArr.Count() <= 1)
                            {
                                nameArr = courseCode.Replace("COURSE NO", string.Empty).Replace(":", string.Empty).Trim().Split(',');
                            }

                            string code = string.Empty;
                            string lastCode = GetFirstN(nameArr[nameArr.Length - 1].Trim(), 3);
                            code = GetLastN(nameArr[nameArr.Length - 1].Trim(), 3);
                            foreach (var item in nameArr)
                            {
                                string finalName = item.Trim();
                                finalName = GetFirstN(finalName, 3).Trim();
                                string finalCourseCode = finalName + "-" + code;

                                if (finalName.Trim().ToUpper() == lastCode.Trim().ToUpper())
                                {
                                    deleteFile = true;
                                }
                                ProcessMultipleClass(saveFilePath, source, _fileName, fileErrorDest, s, dataArr, academicYear, courseName, finalCourseCode, deleteFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Source == "Spire.Pdf")
                        {
                            CreateFolder(errorDestFile);
                            string destFile = Path.Combine(errorDestFile, _fileName);

                            File.Copy(source, destFile, true);
                            File.Delete(source);
                        }
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {

                if (ex.Source == "Spire.Pdf")
                {
                    CreateFolder(errorDestFile);
                    string destFile = Path.Combine(errorDestFile, _fileName);

                    File.Copy(source, destFile, true);
                    File.Delete(source);
                }
                else
                {
                 throw;

                }
               //return result;
            }
        }

        private static void ProcessSingleClass(string saveFilePath, string source, string _fileName, string fileErrorDest, string s, string[] dataArr, string academicYear, string courseName)
        {
            string _courseCode = GetLastN(dataArr[3].Trim().Trim().Replace(" ", string.Empty).Replace(":", string.Empty).ToString(), 5);
            int code; // = Convert.ToInt32(GetLastN(courseCode,3));
            int year;// = Convert.ToInt32(academicYear);

            if (int.TryParse(academicYear, out year))
            {
                if (int.TryParse(GetLastN(_courseCode, 3), out code))
                {
                    int levelCode = Convert.ToInt32(GetFirstN(GetLastN(_courseCode, 3), 1));
                    string level = string.Empty;
                    if (levelCode == 1) { level = "100"; }
                    else if (levelCode == 2) { level = "200"; }
                    else if (levelCode == 3) { level = "300"; }
                    else if (levelCode == 4) { level = "400"; }

                    string programme = string.Empty;
                    string fileName = string.Empty;
                    string Semester = code % 2 == 0 ? "Semester Two" : "Semester One";

                    string programmeCode = _courseCode.Remove(2);

                    programme = GetProgramme(programmeCode);

                    if (Semester == "Semester One")
                    {
                        SaveFile(year, academicYear, saveFilePath, programme, level, Semester, fileName, programmeCode, code, courseName, s, source, true);
                    }
                    else
                    {
                        SaveFile(year, academicYear, saveFilePath, programme, level, Semester, fileName, programmeCode, code, courseName, s, source, true);
                    }
                }
                else
                {
                    MoveFile(source, fileErrorDest, _fileName);
                }
            }
            else
            {
                MoveFile(source, fileErrorDest, _fileName);
            }
        }

        private static void ProcessMultipleClass(string saveFilePath, string source, string _fileName, string fileErrorDest, string s, string[] dataArr, string academicYear, string courseName, string _code, bool deleteFile)
        {
            string _courseCode = _code; ;//GetLastN(dataArr[3].Trim().Trim().Replace(" ", string.Empty).Replace(":", string.Empty).ToString(), 5);
            int code; // = Convert.ToInt32(GetLastN(courseCode,3));
            int year;// = Convert.ToInt32(academicYear);

            if (int.TryParse(academicYear, out year))
            {
                if (int.TryParse(GetLastN(_courseCode, 3), out code))
                {
                    int levelCode = Convert.ToInt32(GetFirstN(GetLastN(_courseCode, 3), 1));
                    string level = string.Empty;
                    if (levelCode == 1) { level = "100"; }
                    else if (levelCode == 2) { level = "200"; }
                    else if (levelCode == 3) { level = "300"; }
                    else if (levelCode == 4) { level = "400"; }

                    string programme = string.Empty;
                    string fileName = string.Empty;
                    string Semester = code % 2 == 0 ? "Semester Two" : "Semester One";

                    string programmeCode = _courseCode.Remove(2);

                    programme = GetProgramme(programmeCode);

                    if (Semester == "Semester One")
                    {
                        SaveFile(year, academicYear, saveFilePath, programme, level, Semester, fileName, programmeCode, code, courseName, s, source, deleteFile);
                    }
                    else
                    {
                        SaveFile(year, academicYear, saveFilePath, programme, level, Semester, fileName, programmeCode, code, courseName, s, source, deleteFile);
                    }
                }
                else
                {
                    MoveFile(source, fileErrorDest, _fileName);
                }
            }
            else
            {
                MoveFile(source, fileErrorDest, _fileName);
            }
        }

        private static string GetLastN(string text, int length)
        {
            if (length >= text.Length)
                return text;
            return text.Substring(text.Length - length);
        }

        private static string GetFirstN(string text, int length)
        {
            if (length >= text.Length)
                return text;
            return text.Substring(0,length);
        }

        private static void CreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private static string GetProgramme(string programmeCode)
        {
            string programme = string.Empty;
            if (programmeCode.ToUpper() == "CE")
            {
                return programme = "1-CE";
            }
            else if (programmeCode.ToUpper() == "EL")
            {
                return programme = "2-EL";
            }
            else if (programmeCode.ToUpper() == "ES")
            {
                return programme = "3-ES";
            }
            else if (programmeCode.ToUpper() == "GD")
            {
                return programme = "4-GD";
            }
            else if (programmeCode.ToUpper() == "GL")
            {
                return programme = "5-GL";
            }
            else if (programmeCode.ToUpper() == "GM")
            {
                return programme = "6-GM";
            }
            else if (programmeCode.ToUpper() == "MA")
            {
                return programme = "7-MA";
            }
            else if (programmeCode.ToUpper() == "MC")
            {
                return programme = "8-MC";
            }
            else if (programmeCode.ToUpper() == "MN")
            {
                return programme = "9-MN";
            }
            else if (programmeCode.ToUpper() == "MR")
            {
                return programme = "10-MR";
            }
            else if (programmeCode.ToUpper() == "PE")
            {
                return programme = "12-PE";
            }
            else if (programmeCode.ToUpper() == "RN")
            {
                return programme = "13-RN";
            }
            else if (programmeCode.ToUpper() == "TC")
            {
                return programme = "TC";
            }
            return programme;
        }

        private static void MoveFile(string fileSource, string saveFilePath, string fileName)
        {
            CreateFolder(saveFilePath);
            string destFile = Path.Combine(saveFilePath, fileName);
            File.Move(fileSource, destFile);
        }

        private static void SaveFile(int year, string academicYear, string saveFilePath, string programme, string level, string Semester, string fileName, string programmeCode, int code, string courseName, string file, string source, bool deleteFile)
        {
            int nextYear = year + 1;
            academicYear = (year + "-" + nextYear);
            string destPath = @saveFilePath + "\\" + programme + "\\" + academicYear + "\\" + level + "\\" + Semester;
            CreateFolder(destPath);//
            fileName = programmeCode + "-" + code + "-" + courseName + ".pdf";
            string destFile = Path.Combine(destPath, fileName);

            if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(destFile))
            {
                File.Copy(file, destFile, true);
                if (deleteFile == true)
                {
                    File.Delete(source);
                }
            }
        }

        private static bool IsMultipleClass(string name)
        {
            bool result = false;
            string _name = string.Empty;
            _name = name.Replace("COURSE NO", string.Empty).Replace(":", string.Empty).Trim();
            string finName = _name.Replace("CLASSES", string.Empty).Replace(":", string.Empty).Trim();
            if (finName.Length >7)
            {
                result = true;
            }
            return result;
        }

    }
}
