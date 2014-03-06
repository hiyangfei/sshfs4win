﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using Renci.SshNet;

#endregion

namespace Sshfs
{
    internal static class Utilities
    {
        private static readonly IsolatedStorageFile _userStoreForAssembly =
            IsolatedStorageFile.GetUserStoreForAssembly();


        public static void Load<T>(this List<T> list, string file) where T : ISerializable
        {
            if (!_userStoreForAssembly.FileExists(file)) return;

            var xmlSerializer = new DataContractSerializer(typeof(IEnumerable<T>));
            using (
                var stream = _userStoreForAssembly.OpenFile(file, FileMode.OpenOrCreate,
                                                            FileAccess.Read))
            {
                list.Clear();
               
                list.AddRange(xmlSerializer.ReadObject(stream) as IEnumerable<T>);
            }
        }

        public static void Presist<T>(this List<T> list, string file, bool delete = false) where T : ISerializable

        {
            if (delete)
            {
                _userStoreForAssembly.DeleteFile(file);
            }
            else
            {
                var xmlSerializer = new DataContractSerializer(typeof (List<T>));
                using (
                    var stream = _userStoreForAssembly.OpenFile(file, FileMode.Create,
                                                                FileAccess.Write))
                {
                    xmlSerializer.WriteObject(stream, list);
                }
            }
        }

        public static string ProtectString(string stringToProtect)
        {
            return stringToProtect != null
                       ? Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(stringToProtect), null,
                                                                      DataProtectionScope.CurrentUser))
                       : null;
        }

        public static string UnprotectString(string stringToUnprotect)
        {
            return stringToUnprotect != null
                       ? Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(stringToUnprotect),
                                                                         null,
                                                                         DataProtectionScope.CurrentUser))
                       : null;
        }

        public static IEnumerable<char> GetAvailableDrives()
        {
            return Enumerable.Range('D', 23).Select(value => (char) value).Except(
                Directory.GetLogicalDrives().Select(drive => drive[0]));
        }


        public static void RegisterForStartup()
        {
            Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).SetValue(Application.ProductName,
                                                                                     Application.ExecutablePath);
        }

        public static void UnregisterForStarup()
        {
            Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).DeleteValue(Application.ProductName);
        }


        public static bool IsAppRegistredForStarup()
        {
            return (Registry.CurrentUser.OpenSubKey
                        ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false).GetValue(Application.ProductName)
                    as string) == Application.ExecutablePath;
        }


        public static bool IsValidUnixPath(string value)
        {
            return !string.IsNullOrEmpty(value) && (value[0] == '.' || value[0] == '/') && (value.IndexOf('\\') == -1);
        }

       /* public static bool IsValidPrivateKey(string path)
        {
            try
            {
                new PrivateKeyFile(path);
                return true;
            }
            catch
            {
                return false;
            }
        }*/

        public static void SetNetworkDriveName(string connection,string name)
        {
           var drive= Registry.CurrentUser.OpenSubKey
               ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MountPoints2", false).OpenSubKey(connection.Replace("\\","#"),true);
            if(drive!=null)
            {
                drive.SetValue("_LabelFromReg", name);
            }
        }
    }
}