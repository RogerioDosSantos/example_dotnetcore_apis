using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DotNetCoreApis.Models;
using Microsoft.AspNetCore.Http;

namespace dotnetcore_apis.Tools
{
    public class FileTools
    {
        public static bool SaveFilesToDisk(string savingDirectory, IFormFileCollection files, out List<FileUploadResponseModel> savedProperties)
        {
            savedProperties = new List<FileUploadResponseModel>();
            try
            {
                if (string.IsNullOrEmpty(savingDirectory))
                {
                    savedProperties.Add(new FileUploadResponseModel
                    {
                        error = "Invalid sub directory"
                    });
                    return false;
                }

                if (files.Count == 0)
                {
                    savedProperties.Add(new FileUploadResponseModel
                    {
                        error = "Received no file"
                    });
                    return false;
                }

                if (!Directory.Exists(savingDirectory))
                    Directory.CreateDirectory(savingDirectory);

                foreach (IFormFile postedFile in files)
                {
                    string fileName = Path.GetFileName(postedFile.FileName);
                    string filePath = Path.Combine(savingDirectory, fileName);
                    bool fileExist = System.IO.File.Exists(filePath);
                    float fileSize = ((float)postedFile.Length) / (1024 * 1024);
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        postedFile.CopyTo(fileStream);
                        savedProperties.Add(new FileUploadResponseModel
                        {
                            file = fileName,
                            uploadPath = filePath,
                            fileReplaced = fileExist,
                            fileSize = fileSize
                        });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                savedProperties.Add(new FileUploadResponseModel
                {
                    error = $"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}"
                });
                return false;
            }
        }

        public static string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uriBuilder = new UriBuilder(codeBase);
            string assemblyPath = Uri.UnescapeDataString(uriBuilder.Path);
            return Path.GetDirectoryName(assemblyPath);
        }
    }
}
