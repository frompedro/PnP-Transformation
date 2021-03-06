﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PeoplePickerRemediation.Console
{
    class Program
    {
        /// <summary>
        // Return a value of TRUE if you want the utility to leverage the App Model for AuthN/AuthZ
        // Return a value of FALSE if you want the utility to leverage User Credentials for AuthN/AuthZ
        /// </summary>
        public static bool UseAppModel
        {
            get
            {
                try
                {
                    return System.Configuration.ConfigurationManager.AppSettings["UseAppModel"].ToBoolean();
                }
                catch
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the domain provided by user
        /// </summary>
        ///
        public static string AdminDomain
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the username provided by user
        /// </summary>
        public static string AdminUsername
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the password provided by user
        /// </summary>
        public static SecureString AdminPassword
        {
            get;
            set;
        }
        public static void Main(string[] args)
        {
            if (UseAppModel == false)
            {
                GetCredentials();
            }

            string inputFilePath = GetInputFilePath();

            if (PeoplePickerRemediation.ValidateLDAPVariable())
            {
                if (ValidateDirectorySearcherUser())
                {
                    PeoplePickerRemediation.DoWork(inputFilePath);
                }

                System.Console.WriteLine("Script execution is completed");
            }
        }

        private static bool ValidateDirectorySearcherUser()
        {
            bool continueStatus = false;
            string accountID = "";
            System.Console.WriteLine("Provide your Credentials in format: domain\\alias: ");
            accountID = System.Console.ReadLine();
            string upn = PeoplePickerRemediation.GetUserPrinicpalNameFromDirectorySeracher(accountID);
            System.Console.WriteLine(string.Format("{0} user's upn value is {1}", accountID, upn));

            System.Console.WriteLine(string.Format("Do you want to continue process InfoPath forms(y/n)?"));
            string continuedStatus = System.Console.ReadLine();
            if (!string.IsNullOrEmpty(upn) && (continuedStatus.Contains("y") || continuedStatus.Contains("Y")))
            {
                continueStatus = true;
            }
            return continueStatus;

        }

        private static string GetInputFilePath()
        {
            bool retryFilePathInput;
            string inputFilePath;

            do
            {
                retryFilePathInput = false;
                System.Console.WriteLine("\n\n" + @"Please enter the Path conataining the InfoPath Form Libraries List: ");
                System.Console.WriteLine(@"- Give the path in the following format [Folder path containing the InfoPath Form Libraries List]\[CSV File Name]");
                System.Console.WriteLine(@"- Example: E:\PeoplePickerRemediation\PeoplePickerReport.csv" + "\n");

                inputFilePath = System.Console.ReadLine();

                if (inputFilePath == "")
                {
                    retryFilePathInput = true;
                    System.Console.WriteLine("\n" + @"Please make sure the File Path is not empty" + "\n");
                }
                else
                {
                    if (!File.Exists(inputFilePath))
                    {
                        retryFilePathInput = true;
                        System.Console.WriteLine("\n" + @"Please make sure the File Path is in the valid format" + "\n");
                    }
                }
            }
            while (retryFilePathInput);

            return inputFilePath;
        }

        private static void GetCredentials()
        {
            ConsoleKeyInfo key;
            bool retryUserNameInput = false;
            string account = String.Empty;
            string password = String.Empty;

            do
            {
                System.Console.WriteLine(@"Please enter the Admin account: ");
                System.Console.WriteLine(@"- Use [domain\alias] for SPO-D & On-Prem farms");
                System.Console.WriteLine(@"- Use [alias@domain.com] for SPO-MT & vNext farms");

                account = System.Console.ReadLine();

                if (account.Contains('\\'))
                {
                    string[] segments = account.Split('\\');
                    AdminDomain = segments[0];
                    AdminUsername = segments[1];
                    break;
                }
                if (account.Contains("@"))
                {
                    AdminUsername = account;
                    break;
                }
            }
            while (retryUserNameInput);

            System.Console.WriteLine("Please enter the Admin password: ");

            do
            {
                key = System.Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    System.Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password = password.Substring(0, password.Length - 1);
                        System.Console.CursorLeft--;
                        System.Console.Write('\0');
                        System.Console.CursorLeft--;
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);

            AdminPassword = Helper.CreateSecureString(password.TrimEnd('\r'));
        }
    }
}
