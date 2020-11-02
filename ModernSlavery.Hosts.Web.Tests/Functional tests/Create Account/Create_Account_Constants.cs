using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    internal class Create_Account
    {
        //roger successfull created users
        public const string roger_email = "roger@uat.co";
        public const string roger_first = "Roger";
        public const string roger_last = "Reporter";
        public const string roger_job_title = "Company Reporter";
        public const string roger_password = "Test1234!";

        //existing user
        public const string existing_email = "roger@uat.co";

        //edited user
        public const string edited_email = "edited@test.com";
        public const string edited_first = "EditedFirst";
        public const string edited_last = "EditedLast";
        public const string edited_job_title = "EditedTitle";
        public const string edited_password = "Test2456!";
    }
}
