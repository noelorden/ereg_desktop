using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERegEmail
{
    public class Formatter
    {
        public static string FormatEmailBody(string body, string roomNo, string checkinDate, string guestName, string email)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            guestName = textInfo.ToTitleCase(guestName.ToLower());

            body = body.Replace("{@RoomNo}", roomNo);
            body = body.Replace("{@CheckInDate}", checkinDate);
            body = body.Replace("{@GuestName}", guestName);
            body = body.Replace("{@GuestEMail}", email);
            return body;
        }
    }
}
