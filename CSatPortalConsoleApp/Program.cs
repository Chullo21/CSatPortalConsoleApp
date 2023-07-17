using System;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace CSatPortalConsoleApp
{
    internal class Program
    {

        static void Main(string[] args)
        {
            bool returnToMain = false;

            do
            {
                Console.Clear();
                Console.WriteLine("-------Main Menu-------");
                Console.WriteLine("Enter the corresponding number of your command:");
                Console.WriteLine("1. Enable Notification system (timer)");
                Console.WriteLine("2. Execute Notification system");

                string userInput = Console.ReadLine();
                switch (userInput)
                {
                    case "1":
                        TimerUI();
                        break;
                    case "2":
                        Console.Clear();
                        Console.WriteLine("Notif Executed");
                        ExecuteNotif();
                        Console.ReadKey();
                        break;
                    case "3":
                        returnToMain = true;
                        break;
                    default:
                        Console.WriteLine("Invalid input.");
                        break;
                }
            } while (!returnToMain);
        }

        static void TimerUI()
        {
            bool notifEnabled = false;
            string status = "";

            Console.Clear();

            do
            {
                if (notifEnabled)
                {
                    status = "running...";
                }
                else
                {
                    status = "disabled";
                }

                Console.Clear();

                Console.WriteLine("Enter '1' to enable notification or '0' to disable notification. Enter '3' to access the main menu: ");

                Console.WriteLine("\r\nNotification system is currently " + status + "...");
                string userInput = Console.ReadLine();

                if (int.TryParse(userInput, out int userChoice))
                {
                    if (userChoice == 1)
                    {
                        notifEnabled = true;
                        Console.Clear();

                        Console.WriteLine("Enter '1' to enable notification or '0' to disable notification. Enter '3' to access the main menu: ");

                        Console.WriteLine("\r\nNotification system is currently running...");
                        EnableTimer();
                    }
                    else if (userChoice == 0)
                    {
                        notifEnabled = false;
                        Console.WriteLine("Notification disabled.");
                    }
                    else if (userChoice == 3)
                    {
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                }

            } while (true);
        }

        static void EnableTimer()
        {
            Timer timer = new Timer(CheckForDueDates, null, TimeSpan.Zero, TimeSpan.FromSeconds(86400));

            Console.ReadKey();
            timer.Dispose();
        }
        static void ExecuteNotif()
        {
            using (var _Db = new CSatDbEntities())
            {
                var validations = _Db.IssueDbs.Where(j => !j.ValidatedStatus);

                foreach (var validation in validations)
                {
                    if (DateTime.Now.Date - validation.DateCreated.Date > TimeSpan.FromDays(1) || DateTime.Now.Date - validation.DateCreated.Date > TimeSpan.FromDays(2))
                    {
                        string validationBody = "Please be advised that a pending issue with issue# of " + validation.IssueNo + " is still unvalidated.\r\n\r\nHave a great day!";

                        SendEmail(validationBody, "Validation Reminder", GetValidationEmail(), GetSmtpEmailDet());
                    }
                }

                var actionitems = _Db.ActionDbs.Where(j => j.ActionStatus == "Open");

                foreach (var action in actionitems)
                {
                    if (action.TargetDate < DateTime.Now ||
                        DateTime.Now.Date - action.DateCreated.Date > TimeSpan.FromDays(1) || DateTime.Now.Date - action.DateCreated.Date > TimeSpan.FromDays(2))
                    {
                        string validationBody = "Please be advised that you have an outstanding action item with an action ID of " +
                            action.ControlNo + " is still open.\r\n Refer to our CSat Portal to review and process this action item.\r\n\r\nThank you!";

                        SendEmail(validationBody, "Action Item Reminder", GetActionEmail(action.PIC), GetSmtpEmailDet());
                    }
                }
            }

            Console.WriteLine("Done...");
        }

        static void CheckForDueDates(object state)
        {
            using (var _Db = new CSatDbEntities())
            {
                var validations = _Db.IssueDbs.Where(j => !j.ValidatedStatus);

                foreach (var validation in validations)
                {
                    if (DateTime.Now.Date - validation.DateCreated.Date > TimeSpan.FromDays(1) || DateTime.Now.Date - validation.DateCreated.Date > TimeSpan.FromDays(2))
                    {
                        string validationBody = "Please be advised that a pending issue with issue# of " + validation.IssueNo + " is still unvalidated.\r\n\r\nHave a great day!";

                        SendEmail(validationBody, "Validation Reminder", GetValidationEmail(), GetSmtpEmailDet());
                    }
                }

                var actionitems = _Db.ActionDbs.Where(j => j.ActionStatus == "Open");

                foreach (var action in actionitems)
                {
                    if (action.TargetDate < DateTime.Now ||
                        DateTime.Now.Date - action.DateCreated.Date > TimeSpan.FromDays(1) || DateTime.Now.Date - action.DateCreated.Date > TimeSpan.FromDays(2))
                    {
                        string validationBody = "Please be advised that you have an outstanding action item with an action ID of " +
                            action.ControlNo + " is still open.\r\n Refer to our CSat Portal to review and process this action item.\r\n\r\nThank you!";

                        SendEmail(validationBody, "Action Item Reminder", GetActionEmail(action.PIC), GetSmtpEmailDet());
                    }
                }
            }
        }

        static List<string> GetActionEmail(string pic)
        {
            using (var _Db = new CSatDbEntities())
            {
                List<string> emails = _Db.AccountsDbs.Where(j => j.AccName == pic && !string.IsNullOrEmpty(j.Email)).Select(j => j.Email).ToList();

                return emails;
            }
        }

        static List<string> GetValidationEmail()
        {
            using (var _Db = new CSatDbEntities())
            {
                List<string> emails = _Db.AccountsDbs.Where(j => j.Role == "Admin" && !string.IsNullOrEmpty(j.Email)).Select(j => j.Email).ToList();

                return emails;
            }           
        }

        static string[] GetSmtpEmailDet()
        {
            string[] smtpDet = new string[2];

            using(var _Db = new CSatDbEntities())
            {
                var smtp = _Db.SEDbs.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                smtpDet[0] = smtp.Email;
                smtpDet[1] = smtp.Password;
            }

            return smtpDet;
        }

        static void SendEmail(string body, string subject, List<string> email, string[] smtp)
        {
            if (email == null || email.Count == 0)
            {
                return;
            }

            string senderEmail = smtp[0];
                
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(smtp[0], smtp[1]);
            smtpClient.EnableSsl = true;

            try
            {
                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(senderEmail);
                mail.Subject = subject;
                mail.Body = body;

                foreach (string s in email)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        mail.To.Add(s);
                    }
                }

                if (mail.To.Count > 0)
                {
                    smtpClient.Send(mail);
                    Console.WriteLine("Email sent successfully");
                }
                else
                {
                    Console.WriteLine("No valid email addresses found");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send email: " + ex.Message);
            }
        }
    }
}
