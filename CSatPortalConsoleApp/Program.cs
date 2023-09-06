using System;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using CSatPortalConsoleApp.AppDbContext;

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
            using (CSATDbEntities _Db = new CSATDbEntities())
            {
                Console.WriteLine("Processing Validation Notification System...");

                CheckVals(_Db);

                Console.WriteLine("DONE...");

                Console.WriteLine("Processing Action Items Notification System...");

                CheckActs(_Db);

                Console.WriteLine("DONE...");

                Console.WriteLine("Processing CAPA or 8D Notification System...");

                CheckCAPAOr8D(_Db);

                Console.WriteLine("DONE...");

            }

            Console.WriteLine("Action Complete...");
        }

        static void CheckForDueDates(object state)
        {
            using (CSATDbEntities _Db = new CSATDbEntities())
            {
                Console.WriteLine("Processing Validation Notification System...");

                CheckVals(_Db);

                Console.WriteLine("DONE...");

                Console.WriteLine("Processing Action Items Notification System...");

                CheckActs(_Db);

                Console.WriteLine("DONE...");

                Console.WriteLine("Processing CAPA or 8D Notification System...");

                CheckCAPAOr8D(_Db);

                Console.WriteLine("DONE...");
            }

            Console.WriteLine("Action Complete...");
        }

        private static TimeSpan GetWeekdaysCount(DateTime to)
        {
            int weekdaysCount = 0;

            for (DateTime date = to.Date; date <= DateTime.Now; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    weekdaysCount++;
                }
            }

            if (weekdaysCount >= 1) weekdaysCount--;

            return TimeSpan.FromDays(weekdaysCount);
        }

        private static DateTime ExcludeWeekendsForVal(DateTime created)
        {
            int days = 0;

            while (days != 3)
            {
                if (created.DayOfWeek != DayOfWeek.Saturday && created.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;                  
                }

                created = created.AddDays(1);
            }

            return created;
        }

        private static void CheckCAPAOr8D(CSATDbEntities _Db)
        {
            var reps = _Db.IssueDb.Where(j => !j.Has8D && j.ValRes == "Valid");

            foreach (var issue in reps)
            {
                TimeSpan weekdaysCount = GetWeekdaysCount(issue.DateVdal);

                if (weekdaysCount > TimeSpan.FromDays(5))
                {
                    string validationBody = 
                        "Good day!\r\n" +
                        "Please be advised that an Issue with Control Number of " +
                        issue.ControlNumber + " is still awaiting for 8D attachment.\r\n" +
                        "Refer to our CSat Portal to review and upload your 8D attachment.\r\n" +
                        "Thank you!" + FooterBody();
                    Console.WriteLine("Sending...");
                    SendEmail(validationBody, issue.IssueNo + " Awaiting 8D Report", ListOfQA(), GetSmtpEmailDet());
                }
            }         
        }

        private static List<string> ListOfQA()
        {
            List<string> Emails = new List<string>();

            using(CSATDbEntities _Db = new CSATDbEntities())
            {
                Emails = _Db.AccountsDb.Where(j => j.Section == "QA" || j.Section == "QAManager").Select(j => j.Email).ToList();
            }

            return Emails;
        }

        private static void CheckVals(CSATDbEntities _Db)
        {
            var validations = _Db.IssueDb.Where(j => !j.ValidatedStatus);

            foreach (var validation in validations)
            {
                List<string> emails = new List<string>();
                TimeSpan timeDiff = ExcludeWeekendsForVal(validation.DateCreated) - (DateTime.Now);
                int Days = timeDiff.Days;

                if (Days == 1)
                {
                    string validationBody =
                        "Good day!\r\n" +
                        "Please be advised that a pending issue with issue# of " + validation.IssueNo + " is still unvalidated.\r\n" +
                        "You only have until tommorrow to validate this issue.\r\n\r\n" +
                        "Have a great day!" + FooterBody();

                    emails.AddRange(_Db.AccountsDb.Where(j => j.Section == "QA" || j.Section == "QAManager" || j.Section == "VicePresident").Select(j => j.Email));

                    Console.WriteLine("Sending...");

                    SendEmail(validationBody, "Validation Reminder", emails, GetSmtpEmailDet());
                }
                else if (Days < 1)
                {
                    string validationBody =
                        "Good day!\r\n" +
                        "Please be advised that a pending issue with issue# of " + validation.IssueNo + " is still unvalidated.\r\n" +
                        "This is an overdue reminder. Please process your validation report as soon as possible.\r\n" +
                        "Have a great day!" + FooterBody();

                    emails.AddRange(_Db.AccountsDb.Where(j => j.Section == "QA" || j.Section == "QAManager" || j.Section == "VicePresident").Select(j => j.Email));

                    Console.WriteLine("Sending...");

                    SendEmail(validationBody, "Validation Reminder", emails, GetSmtpEmailDet());
                }
                else if (Days > 1 && DateTime.Now.DayOfWeek != DayOfWeek.Saturday && DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
                {
                    string validationBody =
                        "Good day!" +
                        "\r\nPlease be advised that a pending issue with issue# of " + validation.IssueNo + " is still unvalidated.\r\n" +
                        "This is a daily reminder to process your QI validation report as soon as possible. \r\n" +
                        "Have a great day!" + FooterBody();

                    emails.AddRange(_Db.AccountsDb.Where(j => j.Section == "QA" || j.Section == "QAManager").Select(j => j.Email));

                    Console.WriteLine("Sending...");

                    SendEmail(validationBody, "Validation Reminder", emails, GetSmtpEmailDet());
                }
            }
        }

        private static void CheckActs(CSATDbEntities _Db)
        {
            var actionitems = _Db.ActionDb.Where(j => j.ActionStatus == "Open");

            foreach (var action in actionitems)
            {
                TimeSpan timeDifference = action.TargetDate.Date - DateTime.Now;
                int Days = timeDifference.Days;

                if (Days <= 3)
                {
                    string validationBody =
                        "Good day!\r\n" +
                        "Please be advised that you have an outstanding action item with Control Number of " +
                        action.ControlNo + ".\r\n" +
                        "\r\nRefer to our CSat Portal to review and process this action item.\r\n" +
                        "Thank you!" + FooterBody();

                    Console.WriteLine("Sending...");

                    SendEmail(validationBody, "Action Item Reminder", GetActionEmail(action.PIC), GetSmtpEmailDet());
                }
            }
        }

        static List<string> GetActionEmail(string pic)
        {
            using (CSATDbEntities _Db = new CSATDbEntities())
            {
                List<string> emails = new List<string>();
                var getPIC = _Db.AccountsDb.FirstOrDefault(j => j.AccName == pic);
                string sectionManager;

                switch (getPIC.Section)
                {
                    case "QA":
                        {
                            sectionManager =  "QAManager";
                            break;
                        }
                    case "BC":
                        {
                            sectionManager = "BCManager";
                            break;
                        }
                    case "Purchasing":
                        {
                            sectionManager = "PurchasingManager";
                            break;
                        }
                    case "NPI":
                        {
                            sectionManager = "NPIManager";
                            break;
                        }
                    case "QE":
                        {
                            sectionManager = "QEManager";
                            break;
                        }
                    case "PPIC":
                        {
                            sectionManager = "PPICManager";
                            break;
                        }
                    case "Production":
                        {
                            sectionManager = "ProductionManager";
                            break;
                        }
                    case "Engineering":
                        {
                            sectionManager = "EngineeringManager";
                            break;
                        }
                    default:
                        {
                            sectionManager = "";
                            break;
                        }
                }
                var Manager = _Db.AccountsDb.FirstOrDefault(j => j.Section == sectionManager);
                
                emails.Add(getPIC.Email);
                
                if (Manager != null && !string.IsNullOrEmpty(Manager.Email))
                {
                    emails.Add(Manager.Email);
                }

                return emails;
            }
        }

        static string[] GetSmtpEmailDet()
        {
            string[] smtpDet = new string[2];

            using(CSATDbEntities _Db = new CSATDbEntities())
            {
                var smtp = _Db.SEDb.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                smtpDet[0] = smtp.Email;
                smtpDet[1] = smtp.Password;
            }

            return smtpDet;
        }

        static string FooterBody()
        {
            string link = "http://192.168.6.144:8080";
            string withLink = "\r\n\r\nThis is an email generated by our CSat Portal, please refrain from replying.\r\n" +
                $"Click this link \"{link}\" to access our CSAT Portal";
            return withLink;
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
